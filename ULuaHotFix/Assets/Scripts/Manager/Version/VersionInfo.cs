using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Security;
using Mono.Xml;
public class ResInfo
{
    public string resName;
    public string resMD5;
    public int resSize; //byte为单位
    public string resURL;

    //true：本地没有这个资源包则会下载这个资源包，需要更新则更新
    //false:本地没有这个资源包则不会下载，如果本地有，需要更新则会更新
    public bool isResRequire = true;

    public int resRequireID = 0;
}

public class VersionInfo
{
    public float ProgramVersion { set; get; }   //C#代码版本号

    public string ApkUrl { set; get; }          //最新的apk安装包路径
    public string ApkMd5 { set; get; }          //最新的apk安装包md5

    public string IOSAppUrl { set; get; }       //IOS更新URL，企业版
    public string IOSAppStoreUrl { set; get; }  //IOS更新URL，商店版
    public bool IsAppleAppStore { set; get; }   //是否属于IOS App Store版本
    public bool IsOpenAutoUpdateInAppStore { set; get; } //是否开启App Store版本的自动更新，服务器开关，动态开启关闭自动更新

    public bool IsForceToUpdate { set; get; }	//是否强制更新(由服务器控制)

    public Dictionary<string, ResInfo> dictRes = new Dictionary<string, ResInfo>();

    //暴露给Lua使用
    public void AddRes(ResInfo info)
    {
        dictRes.Add(info.resName, info);
    }

    //-----------------------------------------------------------------------------------

    //解析xml数据
    static public VersionInfo ParseData(string xmlContent)
    {
        VersionInfo versionInfo = new VersionInfo();

        try
        {
            SecurityParser securityParser = new SecurityParser();
            securityParser.LoadXml(xmlContent);
            SecurityElement xml = securityParser.ToXml();

            if (xml == null)
            {
                Debug.LogError("VersionInfo.ParseData:XML Data Error");
                return versionInfo;
            }

            if (xml.Children == null || xml.Children.Count == 0)
            {
                return versionInfo;
            }

            foreach (SecurityElement se in xml.Children)
            {
                string tag = se.Tag.ToLower();
                switch (tag)
                {
                    case "programversion": versionInfo.ProgramVersion = float.Parse(se.Text); break;
                    case "apkurl": versionInfo.ApkUrl = se.Text; break;
                    case "apkmd5": versionInfo.ApkMd5 = se.Text; break;
                    case "iosappurl": versionInfo.IOSAppUrl = se.Text; break;
                    case "iosappstoreurl": versionInfo.IOSAppStoreUrl = se.Text; break;
                    case "isappleappstore": versionInfo.IsAppleAppStore = StrBoolParse(se.Text); break;
                    case "isopenautoupdateinappstore": versionInfo.IsOpenAutoUpdateInAppStore = StrBoolParse(se.Text); break;
                    case "isforcetoupdate": versionInfo.IsForceToUpdate = StrBoolParse(se.Text); break;
                    case "resinfo":
                        {
                            if (se.Children == null || se.Children.Count == 0)
                            {
                                continue;
                            }

                            foreach (SecurityElement record in se.Children)
                            {
                                if (record.Children == null || record.Children.Count == 0)
                                {
                                    continue;
                                }

                                ResInfo resInfo = new ResInfo();
                                foreach (SecurityElement node in record.Children)
                                {
                                    string resTag = node.Tag.ToLower();
                                    switch (resTag)
                                    {
                                        case "resname": resInfo.resName = node.Text; break;
                                        case "resmd5": resInfo.resMD5 = node.Text; break;
                                        case "ressize": resInfo.resSize = int.Parse(node.Text); break;
                                        case "resurl": resInfo.resURL = node.Text; break;
                                        case "resrequire":
                                            {
                                                if (node.Text == "0")
                                                    resInfo.isResRequire = false;
                                                else if (node.Text == "1")
                                                    resInfo.isResRequire = true;
                                                else
                                                    resInfo.isResRequire = bool.Parse(node.Text);
                                            }
                                            break;
                                        case "resrequireid":
                                            resInfo.resRequireID = int.Parse(node.Text);
                                            break;
                                    }
                                }

                                if (versionInfo.dictRes.ContainsKey(resInfo.resName) == false)
                                {
                                    versionInfo.dictRes.Add(resInfo.resName, resInfo);
                                }
                                else
                                {
                                    string strError = string.Format("VersionInfo.ParseData:更新资源包名{0}重复", resInfo.resName);
                                    Debug.LogError(strError);
                                }
                            }
                        }
                        break;
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
        }

        return versionInfo;
    }

    static bool StrBoolParse(string str)
    {
        if (str == "0")
            return false;
        else if (str == "1")
            return true;
        else
            return bool.Parse(str);
    }

    //序列化为字符串
    static public string Serialize(VersionInfo versionInfo)
    {
        var root = new System.Security.SecurityElement("root");
        root.AddChild(new System.Security.SecurityElement("ProgramVersion", versionInfo.ProgramVersion.ToString()));
        var resInfoNode = new System.Security.SecurityElement("ResInfo");
        root.AddChild(resInfoNode);

        foreach (var item in versionInfo.dictRes)
        {
            var recordNode = new System.Security.SecurityElement("Record");
            resInfoNode.AddChild(recordNode);

            recordNode.AddChild(new System.Security.SecurityElement("ResName", item.Value.resName));
            recordNode.AddChild(new System.Security.SecurityElement("ResMD5", item.Value.resMD5));
        }

        return root.ToString();
    }

    //在生成资源包时使用
    static public string SerializeInEditor(List<ResInfo> listResInfo)
    {
        string innerText = MyFileUtil.ReadConfigDataInStreamingAssets(VersionManager.VersionInfoFilePath);
        VersionInfo innerVersionInfo = VersionInfo.ParseData(innerText);

        var root = new System.Security.SecurityElement("root");
        root.AddChild(new System.Security.SecurityElement("ProgramVersion", innerVersionInfo.ProgramVersion.ToString()));
        root.AddChild(new System.Security.SecurityElement("ApkUrl", innerVersionInfo.ApkUrl));
        root.AddChild(new System.Security.SecurityElement("ApkMd5"));
        root.AddChild(new System.Security.SecurityElement("IOSAppUrl", innerVersionInfo.IOSAppUrl));
        root.AddChild(new System.Security.SecurityElement("IOSAppStoreUrl", innerVersionInfo.IOSAppStoreUrl));
        root.AddChild(new System.Security.SecurityElement("IsAppleAppStore", innerVersionInfo.IsAppleAppStore.ToString()));
        root.AddChild(new System.Security.SecurityElement("IsOpenAutoUpdateInAppStore", innerVersionInfo.IsOpenAutoUpdateInAppStore.ToString()));
        root.AddChild(new System.Security.SecurityElement("IsForceToUpdate", innerVersionInfo.IsForceToUpdate.ToString()));
        var resInfoNode = new System.Security.SecurityElement("ResInfo");
        root.AddChild(resInfoNode);

        foreach (var item in listResInfo)
        {
            var recordNode = new System.Security.SecurityElement("Record");
            resInfoNode.AddChild(recordNode);

            recordNode.AddChild(new System.Security.SecurityElement("ResName", item.resName));
            recordNode.AddChild(new System.Security.SecurityElement("ResMD5", item.resMD5));
            recordNode.AddChild(new System.Security.SecurityElement("ResURL", item.resURL));
            recordNode.AddChild(new System.Security.SecurityElement("ResSize", item.resSize.ToString()));
            recordNode.AddChild(new System.Security.SecurityElement("ResRequire", "true"));
            recordNode.AddChild(new System.Security.SecurityElement("resRequireID", item.resRequireID.ToString()));
        }

        return root.ToString();
    }
}