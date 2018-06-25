
using UnityEngine;
using System.Collections;

public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T m_Instance = null;

    public static T Instance
    {
        get
        {
            if (m_Instance == null)
            {

                m_Instance = FindObjectOfType<T>();
                if (m_Instance == null)
                {
                    GameObject go = new GameObject(typeof(T).Name);
                    m_Instance = go.AddComponent<T>();
                    m_Instance.Invoke("PreInit", 0);
                }
            }

            return m_Instance;
        }
    }

    /*
    //不能通过构造函数来实现，因为构造函数会执行2次，估计是Unity缓存了一次，然后再创建了一次实例，导致构造函数执行了2次
    public SingletonMonoBehaviour()
    {
        if (m_Instance != null)
        {
            string str = string.Format("SingletonMonoBehaviour.Instance: {0} 存在多个实例 ", typeof(T).Name);
            Debug.LogError(str);
        }

        m_Instance = (T)(MonoBehaviour)this;
    }
    */

    virtual protected void Awake()
    {
        try
        {
            if (m_Instance == null)
            {
                m_Instance = (T)(MonoBehaviour)this;
                PreInit();
            }
        }
        catch (System.Exception ex)
        {
            // LogTool.LogException(ex);
            Debug.LogException(ex);
        }
    }

    void OnDestroy()
    {
        if (m_Instance == this)
        {
            m_Instance = null;
        }
    }

    static public bool HasInstance()
    {
        return m_Instance != null;
    }

    private bool m_IsInit = false;

    private void PreInit()
    {
        if (m_IsInit)
        {
            return;
        }
        m_IsInit = true;

        Init();
    }

    virtual public void Init()
    {

    }
}
