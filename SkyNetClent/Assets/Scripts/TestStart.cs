using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestStart : MonoBehaviour
{

    public static NetFramework.NetworkManager networkManager;

    public static string ip = "127.0.0.1";

    public static string port = "8888";
    private LuaClient luaClient;
    void Start()
    {
        networkManager = new NetFramework.NetworkManager();
        luaClient = new GameObject("LuaClient").AddComponent<LuaClient>();
    }


    void Update()
    {
        networkManager.Update();
    }

    void OnGUI()
    {
        ip = GUILayout.TextField(ip, GUILayout.Height(30));
        port = GUILayout.TextField(port, GUILayout.Height(30));
        if (GUILayout.Button("Connect Server"))
        {
            if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(port))
            {
                return;
            }
            networkManager.SendConnect(ip, int.Parse(port));
        }
    }
    void OnDestroy()
    {
        networkManager.OnDestroy();
    }
}
