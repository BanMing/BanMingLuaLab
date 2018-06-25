
using UnityEngine;
using System;
using System.Collections;

//本地配置
public class ServerConfig : GameData<ServerConfig>
{
    static public readonly string fileName = "ServerConfig.xml";

    public string CfgMapURL { set; get; } //服务器入口CFG URL

    public string Version { set; get; }   //客户端当前版本号

    static bool IsInit = false;

    static public ServerConfig Instance
    {
        get
        {
            /*
            if(IsInit == false)
            {
                string data = MyFileUtil.ReadConfigDataInStreamingAssets(fileName);
                ServerConfig.LoadFromText(data);
                IsInit = true;
            }
            */

            return ServerConfig.dataMap[0];
        }
    }
}

//服务器CFG分发--内部使用
public class CfgURLConfig : GameData<CfgURLConfig>
{
    public string version { protected set; get; }

    public string url { protected set; get; }

    static public string GetCfgURL(string version)
    {
        foreach (var item in dataMap)
        {
            if (string.Compare(version, item.Value.version, true) == 0)
            {
                return item.Value.url;
            }
        }

        if (dataMap.Count > 0)
        {
            string str = string.Format("CfgURLConfig.GetCfgURL:匹配版本{0}失败，默认返回第一个服务器配置项", version);
            Debug.LogError(str);

            foreach (var item in dataMap)
            {
                return item.Value.url;
            }
        }

        return null;
    }
}

//内部使用
class ServerURLConfig : GameData<ServerURLConfig>
{
    public string name { protected set; get; }

    public string url { protected set; get; }

    public string text { protected set; get; }
}
