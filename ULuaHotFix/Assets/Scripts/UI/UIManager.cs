
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIManager
{
    static private UIManager mInstance = null;
    static public UIManager Instance
    {
        get
        {
            if (mInstance == null)
            {
                mInstance = new UIManager();
            }

            return mInstance;
        }
    }

    private RectTransform m_UIRootRectTransform = null;
    public RectTransform UIRoot
    {
        get
        {
            if (m_UIRootRectTransform == null)
            {
                InitRootNode();
            }

            return m_UIRootRectTransform;
        }
    }

    private RectTransform m_UITopRoot = null;
    public RectTransform UITopRoot
    {
        get
        {
            if(m_UITopRoot == null)
            {
                InitRootNode();
            }
            return m_UITopRoot;
        }
    }
    private Canvas rootCanvas;
    public Canvas RootCanvas
    {
        get
        {
            if (rootCanvas == null)
            {
                InitRootNode();
            }

            return rootCanvas;
        }
    }   

    private void InitRootNode()
    {
        RectTransform tran = null;
        Canvas[] canvas = GameObject.FindObjectsOfType<Canvas>();
        if (canvas.Length == 0)
        {
            string str = string.Format("UIManager.InitRootNode:获取Canvas失败");
            Debug.LogError(str);
            return;
        }

        if (canvas.Length == 1)
        {
            tran = canvas[0].GetComponent<RectTransform>();
            rootCanvas = canvas[0];
        }
        else
        {
            foreach(Canvas can in canvas)
            {
                if(can.name == "UIRoot")
                {
                    tran = (RectTransform)can.transform;
                    rootCanvas = can;
                    break;
                }
            }
        }

        m_UIRootRectTransform = (RectTransform)MyUnityTool.FindChild(tran, "UILayer5");
        m_UITopRoot = (RectTransform)MyUnityTool.FindChild(tran, "UILayer10");
    }


    private Transform FindChild(Transform parent, string childName)
    {
        if(string.IsNullOrEmpty(childName))
        {
            string str = string.Format("UIManager.FindChild:查找节点{0}的子节点错误,子节点名为空", parent.name);
            Debug.LogError(str);
            return null;
        }

        return parent.Find(childName);
    }

    //-----------------------------------------------------------------------------//

    public Camera GetCamera()
    {
        var uiroot = MyUnityTool.Find("UIRoot");
        return MyUnityTool.FindChild(uiroot.transform, "UICamera").GetComponent<Camera>();
    }
    //-----------------------------------------------------------------------------//
}
