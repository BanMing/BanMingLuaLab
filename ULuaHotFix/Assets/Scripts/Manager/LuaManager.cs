

using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;

public class LuaManager : LuaClient
{
    void Start()
    {
        Debug.Log("LuaManager.Start(MonoBehavior)");
    }

    static public System.Action m_InitFinishCB = null; //初始化结束

    protected override LuaFileUtils InitLoader()
    {
        return new MyLuaResLoader();
        //return new LuaResLoader();
    }

    protected override void OpenLibs()
    {
        base.OpenLibs();
        OpenCJson();
    }

    protected override void OnLoadFinished()
    {
        /*
        //添加Lua脚本目录, Android不添加目录APK中的路径，否则会报错
        if(Application.platform != RuntimePlatform.Android)
        {
            luaState.AddSearchPath(LuaConst.luaDir);
            luaState.AddSearchPath(LuaConst.toluaDir);
        }
        */

        base.OnLoadFinished();
        Debug.Log("LuaManager.OnLoadFinished");
    }

    protected override void StartMain()
    {
        // luaState.DoFile("Main.lua", (objects) =>
        // {


        //     levelLoaded = luaState.GetFunction("OnLevelWasLoaded");
        //     //DispatchSocketMsgAction = luaState.GetFunction("MsgManager.DispatchMsg");

        //     Debug.Log("LuaManager.StartMain:准备执行Lua主函数");
        //     CallMain();
        //     Debug.Log("LuaManager.StartMain:执行Lua主函数结束");

        //     if (m_InitFinishCB != null)
        //     {
        //         m_InitFinishCB();
        //     }

        //     Debug.Log("LuaManager.StartMain");
        // });
        luaState.DoFile("Main.lua");
        CallMain();
        // if (m_InitFinishCB != null)
        // {
        //     m_InitFinishCB();
        // }
    }

    static private LuaInterface.LuaFunction DispatchSocketMsgAction = null;

    public LuaState GetLuaState()
    {
        return luaState;
    }

    /*
    static public void DispatchSocketMsg(NetManager netManager, byte[] data)
    {
        if(DispatchSocketMsgAction != null)
        {
            DispatchSocketMsgAction.Call(netManager, data);
        }
    }
    */

    public void SetDispatchMsgFunction(string functionName)
    {
        DispatchSocketMsgAction = luaState.GetFunction(functionName);
    }

    // static public void DispatchSocketMsg(NetManager netManager, Pluto pluto)
    // {
    //     if (DispatchSocketMsgAction != null)
    //     {
    //         DispatchSocketMsgAction.Call(netManager, pluto);
    //     }
    // }


    public void LoadLuaScriptFile(string fileName)
    {
        luaState.DoFile(fileName);
    }

    public void LoadLuaScriptText(string scriptText, string scriptableName = "LuaState.cs")
    {
        luaState.DoString(scriptText, scriptableName);
    }

    public void CallLuaFunction(string functionName)
    {
        LuaFunction func = luaState.GetFunction(functionName);
        if (func != null)
        {
            func.Call();
            func.Dispose();
            func = null;
        }
        else
        {
            string str = string.Format("LuaManager.CallLuaFunction:调用Lua函数{0}失败", functionName);
            Debug.LogError(str);
        }
    }

    // public object[] CallLuaFunction(string functionName, params object[] args)
    // {
    //     LuaFunction func = luaState.GetFunction(functionName);
    //     if (func != null)
    //     {
    //         object[] results = func.Call(args);
    //         func.Dispose();
    //         func = null;
    //         return results;
    //     }
    //     else
    //     {
    //         string str = string.Format("LuaManager.CallLuaFunction:调用Lua函数{0}失败", functionName);
    //         Debug.LogError(str);
    //     }

    //     return null;
    // }

}
public class MyLuaResLoader : LuaResLoader
{
    static MyLuaResLoader()
    {
        IsArch64 = MyUnityTool.IsProcessorArch64();
    }

    static bool IsArch64 = false;

    public static string X32LuaByteCodeFileSuffix = ".bytes";
    public static string X64LuaByteCodeFileSuffix = ".64.bytes";
    public static string LuaByteCodeFileSuffix
    {
        get
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer && IsArch64)
            {
                return X64LuaByteCodeFileSuffix;
            }

            return X32LuaByteCodeFileSuffix;
        }
    }

    private string[] m_ListLuaSearchDir = new string[]
    {
        "Lua/",
        "Lua/ToLua/"
    };

    public override byte[] ReadFile(string fileName)
    {
        Debug.Log("LuaManager ReadFile fileName:"+fileName);
        if (SystemConfig.Instance.IsUseLuaBytecode)
        {
            if (fileName.EndsWith(".lua"))
            {
                fileName = fileName.Replace(".lua", LuaByteCodeFileSuffix);
            }

            if (fileName.EndsWith(LuaByteCodeFileSuffix) == false)
            {
                fileName = fileName + LuaByteCodeFileSuffix;
            }
        }
        else
        {
            if (fileName.EndsWith(LuaByteCodeFileSuffix))
            {
                fileName = fileName.Replace(LuaByteCodeFileSuffix, ".lua");
            }

            if (fileName.EndsWith(".lua") == false)
            {
                fileName = fileName + ".lua";
            }
        }

        if (ResourcesManager.IsLuaUseZip)
        {
            return GetLuaFileDataFromZip(fileName);
        }
        else
        {
            return GetLuaFileDataFromStreamingAssetsPath(fileName);
        }
    }

    public void ReadFileAsync(string fileName, Action<byte[]> callback)
    {
        if (SystemConfig.Instance.IsUseLuaBytecode)
        {
            if (fileName.EndsWith(".lua"))
            {
                fileName = fileName.Replace(".lua", LuaByteCodeFileSuffix);
            }

            if (fileName.EndsWith(LuaByteCodeFileSuffix) == false)
            {
                fileName = fileName + LuaByteCodeFileSuffix;
            }
        }
        else
        {
            if (fileName.EndsWith(LuaByteCodeFileSuffix))
            {
                fileName = fileName.Replace(LuaByteCodeFileSuffix, ".lua");
            }

            if (fileName.EndsWith(".lua") == false)
            {
                fileName = fileName + ".lua";
            }
        }

        if (ResourcesManager.IsLuaUseZip)
        {
            var bytes = GetLuaFileDataFromZip(fileName);
            callback(bytes);
        }
        else
        {
            GetLuaFileDataFromStreamingAssetsPathAsync(fileName, (bytes) =>
            {
                callback(bytes);
            });
        }
    }

    private byte[] GetLuaFileDataFromStreamingAssetsPath(string fileName)
    {
        for (int i = 0; i < m_ListLuaSearchDir.Length; ++i)
        {
            string filePath = Application.streamingAssetsPath + "/" + m_ListLuaSearchDir[i] + fileName;
            if (filePath.Contains("://"))
            {
                WWW www = new WWW(filePath);
                while (!www.isDone)
                {
                    //等待加载完成
                }
                if (string.IsNullOrEmpty(www.error))
                {
                    return www.bytes;
                }
            }
            else
            {
                if (File.Exists(filePath))
                {
                    
                    // if (SystemConfig.Instance.IsEncryptLuaCode)
                    // {
                    //     return ResourcesManager.DecryptLuaCode(File.ReadAllBytes(filePath));
                    // }
                    return File.ReadAllBytes(filePath);
                }
            }
        }

        string str = string.Format("GetLuaFileDataFromStreamingAssetsPath:读取文件{0}文件失败", fileName);
        Debug.LogError(str);
        return null;
    }

    private void GetLuaFileDataFromStreamingAssetsPathAsync(string fileName, Action<byte[]> callback)
    {
        ScriptThread.Instance.StartCoroutine(GetLuaFileDataFromStreamingAssetsPathAsyncImp(fileName, callback));
    }
    private IEnumerator GetLuaFileDataFromStreamingAssetsPathAsyncImp(string fileName, Action<byte[]> callback)
    {
        for (int i = 0; i < m_ListLuaSearchDir.Length; ++i)
        {
            string filePath = Application.streamingAssetsPath + "/" + m_ListLuaSearchDir[i] + fileName;
            if (filePath.Contains("://"))
            {
                WWW www = new WWW(filePath);
                while (!www.isDone)
                {
                    //等待加载完成
                    yield return null;
                }
                if (string.IsNullOrEmpty(www.error))
                {
                    callback(www.bytes);
                    yield break;
                }
            }
            else
            {
                if (File.Exists(filePath))
                {
                    callback(File.ReadAllBytes(filePath));
                    yield break;
                }
            }
        }

        string str = string.Format("GetLuaFileDataFromStreamingAssetsPath:读取文件{0}文件失败", fileName);
        Debug.LogError(str);
        callback(null);
        yield return null;
    }

    private byte[] GetLuaFileDataFromZip(string fileName)
    {
        for (int i = 0; i < m_ListLuaSearchDir.Length; ++i)
        {
            string path = m_ListLuaSearchDir[i] + fileName;
            byte[] data = ResourcesManager.Instance.GetLuaScriptDataFromZip(path);
            if (data != null)
            {
                return data;
            }
        }

        return null;
    }
}