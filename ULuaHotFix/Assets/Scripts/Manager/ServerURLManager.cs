using UnityEngine;
using System;
using System.Collections;

//各种服务器业务对应的URL——外部使用
public class ServerURLManager
{
    static string ServerListURL { set; get; }

    static string NoticeURL { set; get; }

    static string PaoMaDengURL { set; get; }

    static string ContactURL { set; get; }

    static string HelpURL { set; get; }

    static string AndroidVersionURL { set; get; }

    static string IOSVersionURL { set; get; }

    static string WindowsVersionURL { set; get; }

    static string GameConfigURL { set; get; }

    static string AgreementURL { set; get; }

    static bool IsInit = false;

    //------------------------------------------------------------------------//

    static private void Init(Action<bool> initFinish)
    {
        //Debug.Log("ServerConfig-->Init()-->ServerConfig.Instance.CfgMapURL:" + ServerConfig.Instance.CfgMapURL);
        Action<string> getMapCfgAction = delegate (string cfgMapContent)
        {
            if (string.IsNullOrEmpty(cfgMapContent) || cfgMapContent == "")
            {
                UIWindowUpdate.Instance.ShowTips(LanguageConfig.GetText(15));
                Init(initFinish);
                return;
            }

            CfgURLConfig.LoadFromText(cfgMapContent, "CfgURLConfig");

            string cfgURL = CfgURLConfig.GetCfgURL(ServerConfig.Instance.Version);
            //Debug.Log("ServerConfig-->Init()-->getMapCfgAction-->cfgURL:" + cfgURL);

            Action<string> getCfgAction = delegate (string cfgContent)
            {
                ServerURLConfig.LoadFromText(cfgContent, "ServerURLConfig");
                foreach (var item in ServerURLConfig.dataMap)
                {
                    //Debug.Log("ServerConfig-->Init()-->getMapCfgAction-->getCfgAction-->cfg name:"+item.Value.name + " url:" + item.Value.url);
                    switch (item.Value.name.ToLower())
                    {
                        case "noticedata": NoticeURL = item.Value.url; break;
                        case "paomadeng": PaoMaDengURL = item.Value.url; break;
                        case "contact": ContactURL = item.Value.url; break;
                        case "help": HelpURL = item.Value.url; break;
                        case "serverlist": ServerListURL = item.Value.url; break;
                        case "androidversion": AndroidVersionURL = item.Value.url; break;
                        case "iosversion": IOSVersionURL = item.Value.url; break;
                        case "windowsversion": WindowsVersionURL = item.Value.url; break;
                        case "game": GameConfigURL = item.Value.url; break;
                        case "agreement": AgreementURL = item.Value.url; break;
                    }
                }
                IsInit = true;
                initFinish(true);
            };

            HTTPTool.GetText(cfgURL, getCfgAction);
        };
        HTTPTool.GetText(ServerConfig.Instance.CfgMapURL, getMapCfgAction);
    }

    //根据配置项 获得配置项目对应的URL
    static public void GetURL(string cfgItemName, Action<string> onLoad)
    {
        if (IsInit)
        {
            GetURLImp(cfgItemName, onLoad);
        }
        else
        {
            Action<bool> initAction = delegate (bool result)
            {
                GetURLImp(cfgItemName, onLoad);
            };
            Init(initAction);
        }
    }

    static private void GetURLImp(string cfgItemName, Action<string> onLoad)
    {
        foreach (var item in ServerURLConfig.dataMap)
        {
            if (string.Compare(item.Value.name, cfgItemName, true) == 0)
            {
                onLoad(item.Value.url);
                return;
            }
        }

        onLoad("");
    }

    //根据配置项 获得配置项目对应的URL页面内容
    static public void GetURLPageContent(string cfgItemName, Action<string> onLoad)
    {
        Action<string> loadURL = delegate (string url)
        {
            HTTPTool.GetText(url, onLoad);
        };

        GetURL(cfgItemName, loadURL);
    }

    //------------------------------------------------------------------------//

    static public void GetNoticeData(Action<string> onLoad)
    {
        if (IsInit)
        {
            HTTPTool.GetText(NoticeURL, onLoad);
        }
        else
        {
            Action<bool> initAction = delegate (bool result)
            {
                HTTPTool.GetText(NoticeURL, onLoad);
            };
            Init(initAction);
        }
    }

    static public void GetPaoMaDengData(Action<string> onLoad)
    {
        if (IsInit)
        {
            HTTPTool.GetText(PaoMaDengURL, onLoad);
        }
        else
        {
            Action<bool> initAction = delegate (bool result)
            {
                HTTPTool.GetText(PaoMaDengURL, onLoad);
            };
            Init(initAction);
        }
    }

    static public void GetContactData(Action<string> onLoad)
    {
        if (IsInit)
        {
            HTTPTool.GetText(ContactURL, onLoad);
        }
        else
        {
            Action<bool> initAction = delegate (bool result)
            {
                HTTPTool.GetText(ContactURL, onLoad);
            };
            Init(initAction);
        }
    }

    static public void GetHelpData(Action<string> onLoad)
    {
        if (IsInit)
        {
            HTTPTool.GetText(HelpURL, onLoad);
        }
        else
        {
            Action<bool> initAction = delegate (bool result)
            {
                HTTPTool.GetText(HelpURL, onLoad);
            };
            Init(initAction);
        }
    }

    static public void GetSeverListData(Action<string> onLoad)
    {
        if (IsInit)
        {
            HTTPTool.GetText(ServerListURL, onLoad);
        }
        else
        {
            Action<bool> initAction = delegate (bool result)
            {
                HTTPTool.GetText(ServerListURL, onLoad);
            };
            Init(initAction);
        }
    }

    static private string GetCurrentVersionURL()
    {
        string versionURL = "";
#if UNITY_ANDROID
        versionURL = AndroidVersionURL;
#elif UNITY_IOS
        versionURL = IOSVersionURL;
#elif UNITY_STANDALONE_WIN
        versionURL = WindowsVersionURL;
#else
        versionURL = WindowsVersionURL;
#endif
        return versionURL;
    }

    static public void GetVersionData(Action<string> onLoad)
    {
        if (IsInit)
        {
            HTTPTool.GetText(GetCurrentVersionURL(), onLoad);
        }
        else
        {
            Action<bool> initAction = delegate (bool result)
            {
                HTTPTool.GetText(GetCurrentVersionURL(), onLoad);
            };
            Init(initAction);
        }
    }

    static public void GetGameConfigData(Action<string> onLoad)
    {
        //Debug.Log("ServerURLManager.cs-->GetGameConfigData-->GameConfigURL:" + GameConfigURL + " Init:" + IsInit);
        if (IsInit)
        {
            HTTPTool.GetText(GameConfigURL, onLoad);
        }
        else
        {
            Action<bool> initAction = delegate (bool result)
            {
                HTTPTool.GetText(GameConfigURL, onLoad);
            };
            Init(initAction);
        }
    }

    static public void GetAgreementData(Action<string> onLoad)
    {
        if (IsInit)
        {
            HTTPTool.GetText(AgreementURL, onLoad);
        }
        else
        {
            Action<bool> initAction = delegate (bool result)
            {
                HTTPTool.GetText(AgreementURL, onLoad);
            };
            Init(initAction);
        }
    }
}

//登陆服务器配置
public class LoginServerConfig : GameData<LoginServerConfig>
{
    public string ip { protected set; get; }
    public int port { protected set; get; }

    //获取登陆服务器IP地址
    static public void GetLoginServerConfig(Action<string, int> getServerConfigCB)
    {
        Action<string> getServerListAction = delegate (string serverListContent)
        {
            LoginServerConfig.LoadFromText(serverListContent, "LoginServerConfig");

            string ip = "";
            int port = 0;
            foreach (var item in dataMap)
            {
                ip = item.Value.ip;
                port = item.Value.port;
                break;
            }
            getServerConfigCB(ip, port);
        };
        ServerURLManager.GetSeverListData(getServerListAction);
    }
}
