
//  描  述: 	每次直接下载最新资源包，先将资源按照一定规则划分为一定数量的小资源包(zip)，
//            客户端一个新版本的生成，则生成相应的资源包，旧版本的客户端直接到服务器上面下载需要更新的资源包即可
//            如资源包分为代码资源包、UI图片资源包、UI窗口资源包、场景资源包、声音资源包，新版本生成时，产生这些资源包，并记录到配置表中
//            客户端根据本地记录计算需要更新的资源包，进行更新

// 2018.5.22更新流程：
// 1.检测本地缓存程序版本号与安装包中的程序版本号->以安装包的版本号大为准->更新缓存路径
// 2.检测本地缓存程序版本号与服务器程序版本号->以服务器版本号为准->提示是否更新整包
// 3.比较本地缓存资源包与服务器资源包，记录需要更新的资源包，以及是否有新加的资源包
// 4.判断网路情况,下载资源包->做资源的MD5对比->替换资源
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Security;
using Mono.Xml;

public class VersionManager : Singleton<VersionManager>
{
    static public string VersionInfoFilePath = "VersionInfo.xml"; //版本配置文件路径
    static public VersionInfo serverVersionInfo;

    //获取本地版本信息
    public VersionInfo GetLocalVersionInfo()
    {
        if(m_LocalVersionInfo != null)
        {
            return m_LocalVersionInfo;
        }
        //获取安装包中版本信息
        string innerText = MyFileUtil.ReadConfigDataInStreamingAssets(VersionInfoFilePath);
        VersionInfo innerVersionInfo = VersionInfo.ParseData(innerText);

        //外部版本信息
        string outText = MyFileUtil.ReadConfigDataInCacheDir(VersionInfoFilePath);
        if (outText != null)
        {
            VersionInfo outVersionInfo = VersionInfo.ParseData(outText);
            outVersionInfo.ProgramVersion = innerVersionInfo.ProgramVersion;
            return outVersionInfo;
        }

        return innerVersionInfo;
    }

    //异步获取本地版本信息
    public void GetLocalVersionInfoAsync(Action<VersionInfo> callback)
    {
        if (m_LocalVersionInfo != null)
        {
            callback(m_LocalVersionInfo);
            return;
        }
        //获取安装包中版本信息
        MyFileUtil.ReadConfigDataInStreamingAssetsAsync(VersionInfoFilePath, (innerText)=>
        {
            VersionInfo innerVersionInfo = VersionInfo.ParseData(innerText);

            //外部版本信息
            MyFileUtil.ReadConfigDataInCacheDirAsync(VersionInfoFilePath, (outText)=>
            {
                if (outText != null)
                {
                    VersionInfo outVersionInfo = VersionInfo.ParseData(outText);
                    outVersionInfo.ProgramVersion = innerVersionInfo.ProgramVersion;
                    callback(outVersionInfo);
                    return;
                }

                callback(innerVersionInfo);
            });
        });
    }

    public VersionInfo GetInnerVersionInfo()
    {
        string innerText = MyFileUtil.ReadConfigDataInStreamingAssets(VersionInfoFilePath);
        VersionInfo innerVersionInfo = VersionInfo.ParseData(innerText);
        return innerVersionInfo;
    }

    private VersionInfo m_InnerVersionInfo = null;

    public VersionInfo InnerVersionInfo
    {
        get
        {
            if (m_InnerVersionInfo == null)
            {
                m_InnerVersionInfo = GetInnerVersionInfo();
            }
            return m_InnerVersionInfo;
        }
    }

    //-----------------------------------------------------------------------------------//

    //检查安装包中版本号和本地版本号--可能在游戏重新安装时缓存文件没被清理
    public void CheckInstallationPackageVersionWithLocal(Action callback = null)
    {
        MyFileUtil.ReadConfigDataInCacheDirAsync(VersionInfoFilePath, (outText)=>
        {
            if (outText == null)
            {
                if (callback != null)
                {
                    callback();
                }
                return;
            }

            //判断本地版本号和包体内部版本号
            MyFileUtil.ReadConfigDataInStreamingAssetsAsync(VersionInfoFilePath, (innerText)=>
            {
                VersionInfo innerVersionInfo = VersionInfo.ParseData(innerText);

                VersionInfo outVersionInfo = VersionInfo.ParseData(outText);

                if (innerVersionInfo.ProgramVersion > outVersionInfo.ProgramVersion)
                {
                    //清空本地资源
                    MyFileUtil.DeleteDir(MyFileUtil.CacheDir);
                    MyFileUtil.CreateDir(MyFileUtil.CacheDir);
                }

                /*
                foreach(var item in innerVersionInfo.dictRes)
                {
                    if(outVersionInfo.dictRes.ContainsKey(item.Key))
                    {
                        ResInfo outResInfo = outVersionInfo.dictRes[item.Key];
                    }
                }
                */

                Debug.Log("VersionManager.CheckLocalLuaCodeVersion");
                if(callback != null)
                {
                    callback();
                }
            });
        });
    }

    //检查服务器和本地版本号
    public void CheckLocalVersionInfoWithServer(System.Action<bool> updateFinish)
    {
        System.Action<string> getServerVersionFinish = delegate (string data)
        {
            //服务器版本
            VersionInfo serverVersionInfo = VersionInfo.ParseData(data);
            m_serverVersionInfo = serverVersionInfo;
			if(serverVersionInfo.IsForceToUpdate)
				SystemConfig.Instance.IsAutoUpdate = true;
			if(!SystemConfig.Instance.IsAutoUpdate)
			{
				updateFinish(true);
				return;
			}
            //本地版本
            GetLocalVersionInfoAsync((localVersionInfo) =>
            {
                //苹果商店版本
					if (InnerVersionInfo.IsAppleAppStore && serverVersionInfo.IsOpenAutoUpdateInAppStore == false)
                {
                    updateFinish(true);
                    return;
                }

                if (localVersionInfo.ProgramVersion < serverVersionInfo.ProgramVersion)
                {
                    //整个客户端需要更新
                    System.Action clickAction = delegate ()
                    {
                        if (Application.platform == RuntimePlatform.IPhonePlayer)
                        {
                            if (InnerVersionInfo.IsAppleAppStore)
                            {
                                Application.OpenURL(serverVersionInfo.IOSAppStoreUrl);
                            }
                            else
                            {
                                Application.OpenURL(serverVersionInfo.IOSAppUrl);
                            }
                        }
                        else
                        {
                            Application.OpenURL(serverVersionInfo.ApkUrl);
                        }
                    };

                    //UIMsgBox.Instance.ShowMsgBoxOK(LanguageConfig.WordUpdate, "有新客户端发布了，点击按钮进行更新", "更新", clickAction, false);
                    UIMsgBox.Instance.ShowMsgBoxOK(LanguageConfig.WordUpdate, LanguageConfig.GetText(3), LanguageConfig.WordUpdate, clickAction, false);
                    Debug.Log("提示更新整包");
                    return;
                }

                //计算需要更新的资源
                List<ResInfo> listResInfo = new List<ResInfo>();
                foreach(KeyValuePair<string, ResInfo> item in serverVersionInfo.dictRes)
                {
                    string key = item.Key;
                    var resinfo = item.Value;
                    if(localVersionInfo.dictRes.ContainsKey(item.Key))
                    {
                        ResInfo localResInfo = localVersionInfo.dictRes[item.Key];
                        if(string.Compare(item.Value.resMD5, localResInfo.resMD5, true) != 0 && item.Value.resRequireID == 0)
                        {
                            listResInfo.Add(item.Value);
                        }
                    }
                    else
                    {
                        //本地没有
                        if(item.Value.isResRequire && item.Value.resRequireID == 0)
                        {
                            listResInfo.Add(item.Value);
                        }
                    }
                }

                if (listResInfo.Count != 0)
                {
                    //更新Lua脚本和资源
                    DownLoadRes(listResInfo, updateFinish);
                }
                else
                {
                    updateFinish(true);
                }
            });
        };

        //获取服务器版本信息
        ServerURLManager.GetVersionData(getServerVersionFinish);
        Debug.Log("获取服务器版本信息");
    }

    //资源下载
    public void DownLoadRes(List<ResInfo> listResInfo, System.Action<bool> updateFinish)
    {
        if(Application.internetReachability != NetworkReachability.ReachableViaLocalAreaNetwork)
        {
            //提示非wifi情况下更新 提示下载数据大小
            long totalSize = 0;
            foreach (var item in listResInfo)
            {
                totalSize += item.resSize;
            }

            System.Action<bool> onClick = delegate (bool result)
            {
                if(result)
                {
                    //下载更新
                    StartDownLoadResFile(listResInfo, updateFinish);
                }
                else
                {
                    //退出游戏
                    Application.Quit();
                }
            };

            //提示处于移动网络，是否继续更新
            float downSize = totalSize / (1024.0f * 1024.0f); //换算为mb
            string tips = string.Format(LanguageConfig.GetText(4), downSize);
            UIMsgBox.Instance.ShowMsgBoxOKCancel(LanguageConfig.WordUpdate, tips, LanguageConfig.WordUpdate, LanguageConfig.WordCancel, onClick);
        }
        else
        {
            //wifi下自动更新
            StartDownLoadResFile(listResInfo, updateFinish);
        }
    }

    private int m_Current = 0;  //当前下载项目
    private List<ResInfo> m_ListResInfo = null;
    private System.Action<bool> m_UpdateFinish = null;
    private VersionInfo m_LocalVersionInfo = null;
    private VersionInfo m_serverVersionInfo = null;

    void StartDownLoadResFile(List<ResInfo> listResInfo, System.Action<bool> updateFinish)
    {
        m_Current = 0;
        m_ListResInfo = listResInfo;
        m_UpdateFinish = updateFinish;

        //本地版本信息
        m_LocalVersionInfo = GetLocalVersionInfo();

        DownLoadResItem();
    }

    void DownLoadResItem()
    {
        if (m_Current < m_ListResInfo.Count)
        {
            ResInfo item = m_ListResInfo[m_Current];
            ScriptThread.Instance.StartCoroutine(DownLoadResItemImp(item, m_ListResInfo.Count, m_Current, DownLoadResItemFinish));
        }
        else
        {
            m_UpdateFinish(true);
        }
    }

    IEnumerator DownLoadResItemImp(ResInfo item, int totalCount, int current, System.Action<bool, string, ResInfo> updateFinish)
    {
        //WWW www = new WWW(item.resURL);
        WWW www = HTTPTool.GetWWW(item.resURL);

        //ui提示
        UIWindowUpdate.Instance.ShowDownloadTips(totalCount, current + 1, item.resName, www, item.resSize);
        UIWindowFirstLoading.Hide();

        yield return www;

        if (string.IsNullOrEmpty(www.error) == false)
        {
            //下载出错
            Debug.LogError(www.error + item.resURL);
            // updateFinish(false, "资源下载失败，请点击重试", item);
            updateFinish(false, LanguageConfig.GetText(5), item);
            yield break;
        }
        else
        {
            UIWindowUpdate.Instance.ShowVerifyTips();
            UIWindowFirstLoading.Hide();
            if (MD5Tool.Verify(www.bytes, item.resMD5))
            {
                //解压文件--下载成功
                UIWindowUpdate.Instance.ShowUnZipTips();
                UIWindowFirstLoading.Hide();
                ZIPTool.DecompressToDirectory(www.bytes, MyFileUtil.CacheDir);
                updateFinish(true, "", item);
            }
            else
            {
                //md5 匹配不上
                string str = string.Format("VersionManager.DownLoadResImp:资源{0} md5出错", item.resURL);
                Debug.LogError(str);

                // updateFinish(false, "资源校验失败，md5值不匹配，请点击重新下载", item);
                updateFinish(false, LanguageConfig.GetText(6), item);
                yield break;
            }
        }
    }

    void DownLoadResItemFinish(bool result, string errorInfo, ResInfo item)
    {
        if (result)
        {
            //更新版本信息
            if (m_LocalVersionInfo.dictRes.ContainsKey(item.resName))
            {
                m_LocalVersionInfo.dictRes[item.resName].resMD5 = item.resMD5;
            }
            else
            {
                m_LocalVersionInfo.dictRes.Add(item.resName, item);
            }

            //保存版本信息
            SaveLocalVersionInfo(m_LocalVersionInfo);

            ++m_Current;
            if (m_Current == m_ListResInfo.Count)
            {
                m_UpdateFinish(true);
            }
            else
            {
                DownLoadResItem();
            }
        }
        else
        {
            System.Action<bool> onClick = delegate (bool isClickOK)
            {
                if(isClickOK)
                {
                    if (Application.internetReachability == NetworkReachability.NotReachable)
                    {
                        //更新失败，先检查网络
                        UITextTips.Instance.ShowText(LanguageConfig.GetText(12));
                    }
                    else
                    {
                        UIMsgBox.Instance.HideMsgBox();
                        DownLoadResItem();
                    }
                }
                else
                {
                    Application.Quit();
                }
            };

            //错误提示
            UIMsgBox.Instance.ShowMsgBoxOKCancel(LanguageConfig.WordUpdate, errorInfo, LanguageConfig.GetText(13), LanguageConfig.GetText(14), onClick, false);
        }
    }

    //保存版本信息
    public void SaveLocalVersionInfo(VersionInfo versionInfo)
    {
        m_LocalVersionInfo = versionInfo;
        string data = VersionInfo.Serialize(versionInfo);
        MyFileUtil.WriteConfigDataInCacheDir(VersionInfoFilePath, data);
    }

    //检查是否需要更新--外部接口
    public void UpdateGame(System.Action<bool> updateFinish)
    {
        //先检查安装包中的版本号和本地版本号
        CheckInstallationPackageVersionWithLocal(()=>
        {
            //检查本地版本号和服务器版本号
            CheckLocalVersionInfoWithServer(updateFinish);
        });
    }

    //-----------------------------------------------------------------------------------//

    public void GetServerVersionInfo(Action<VersionInfo> onLoad)
    {
        System.Action<string> onLoadText = delegate (string data)
        {
            //服务器版本
            VersionInfo serverVersionInfo = VersionInfo.ParseData(data);
            onLoad(serverVersionInfo);
        };
        // ServerURLManager.GetVersionData(onLoadText);
    }

    public void DownLoadSingleResItem(ResInfo info, Action<bool> onLoad)
    {
        List<ResInfo> list = new List<ResInfo>();
        list.Add(info);
        DownLoadRes(list, onLoad);
    }

    public List<ResInfo> GetServerResInfoList()
    {
        List<ResInfo> resInfoList = new List<ResInfo>();
        m_LocalVersionInfo = GetLocalVersionInfo();
        
        if(m_serverVersionInfo!= null && m_LocalVersionInfo != null)
        {
            foreach(var item in m_serverVersionInfo.dictRes)
            {
                if(!m_LocalVersionInfo.dictRes.ContainsKey(item.Key))
                {
                    resInfoList.Add(item.Value);
                }
                else
                {                    
                    ResInfo localResInfo = m_LocalVersionInfo.dictRes[item.Key];
                    Debug.Log(item.Value.resMD5 + "----" + localResInfo.resMD5);
                    if(string.Compare(item.Value.resMD5, localResInfo.resMD5, true) != 0)
                    {
                        resInfoList.Add(item.Value);
                    }
                }
            }
        }
        return resInfoList;
    }
}
