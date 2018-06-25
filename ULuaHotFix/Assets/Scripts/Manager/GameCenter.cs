using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 流程：检测网络->检测更新->资源管理初始化->lua初始化并加载
/// </summary>
public class GameCenter : MonoBehaviour
{
    public GameObject debugGo;
    LuaManager luaManager = null;
    static public GameCenter Instance;
    void Awake()
    {
        // debugGo.SetActive(true);
        Instance = this;
    }
    void Start()
    {
        //UnityEngine.Profiler.maxNumberOfSamplesPerFrame = 8096000;
        Application.runInBackground = true;
        // 关闭锁屏
        UnityEngine.Screen.sleepTimeout = -1;
        try
        {
            Debug.Log("GameCenter.Start");

            //LoadingBackground.Instance.SetVisible(true);
            //显示第一个界面
            UIWindowFirstLoading.Instance.SetTargetProgress(UIWindowFirstLoading.StartProgressValue);
            #if !UNITY_EDITOR
            debugGo.SetActive(true);
            #endif
            //检查网络是否可以访问
            CheckNetworkState(Init);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }


    void Init()
    {
        System.Action<bool> updateFinish = delegate (bool result)
        {
            UIWindowUpdate.Close();
            UIWindowFirstLoading.Show();
            Debug.Log("GameCenter.Init:检查更新结束");
            InitResManager();
        };

        if (SystemConfig.Instance.IsAutoUpdate)
        {
            UIWindowUpdate.Show();
            // UIManager.Instance.OpenWindow("PanelUpdate");
            //VersionManager.Instance.UpdateGame(updateFinish);
            UIWindowFirstLoading.Hide();

            Debug.Log("GameCenter.Init:开始检查更新");
            VersionManager.Instance.UpdateGame(updateFinish);
        }
        else
        {
            UIWindowUpdate.Close();
            UIWindowFirstLoading.Show();
            InitResManager();
        }

        Debug.Log("GameCenter.Init");
    }

    void InitResManager()
    {
        Action<bool> initCB = delegate (bool result)
        {
            if (result)
            {
                Debug.Log("GameCenter.InitResManager:资源系统初始化成功");
            }
            else
            {
                Debug.LogError("GameCenter.InitResManager:资源系统初始化失败");
            }

            UIWindowFirstLoading.Instance.SetTargetProgress(UIWindowFirstLoading.FinishResProgressValue);

            InitLuaManager();


        };

        UIWindowFirstLoading.Instance.SetTargetProgress(UIWindowFirstLoading.InitResProgressValue);
        ResourcesManager.Instance.Init(initCB);
        Debug.Log("GameCenter.InitResManager:资源系统开始初始化");
    }

    void InitLuaManager()
    {
        // LuaManager.m_InitFinishCB = delegate ()
        // {
        //     UIWindowFirstLoading.Instance.SetTargetProgress(UIWindowFirstLoading.FullProgressValue);
        //     //UIWindowFirstLoading.Close();
        // };
        UIWindowFirstLoading.Instance.SetTargetProgress(UIWindowFirstLoading.FullProgressValue);
        luaManager = gameObject.AddComponent<LuaManager>();

        Debug.Log("GameCenter.InitLuaManager");
    }

    public LuaManager GetLuaManager()
    {
        return luaManager;
    }

    //--------------------------------------------------------------------------//

    //没有网络提示
    static public void CheckNetworkState(System.Action checkFinish)
    {
        Debug.Log("GameCenter.CheckNetworkState");

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            //没有网络
            System.Action<bool> clickAction = delegate (bool result)
            {
                if (result)
                {
                    if (Application.internetReachability == NetworkReachability.NotReachable)
                    {
                        UITextTips.Instance.ShowText(LanguageConfig.GetText(12));
                    }
                    else
                    {
                        UIMsgBox.Instance.HideMsgBox();

                        if (checkFinish != null)
                        {
                            checkFinish();
                        }
                    }
                }
                else
                {
                    Application.Quit();
                }
            };
            UIMsgBox.Instance.ShowMsgBoxOKCancel(LanguageConfig.GetText(11), LanguageConfig.GetText(12), LanguageConfig.GetText(13), LanguageConfig.GetText(14), clickAction, false);
        }
        else
        {
            if (checkFinish != null)
            {
                checkFinish();
            }
        }
    }
}
