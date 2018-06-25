
using UnityEngine;
using System.Collections;

public class SingletonGameObject<T> : MonoBehaviour where T : MonoBehaviour
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
                    GameObject go = LoadGameObjectByResources();
                    m_Instance = go.GetComponentInChildren<T>();
                    
                    if(m_Instance == null)
                    {
                        m_Instance = go.AddComponent<T>();
                    }
                }

                // ReflectionTool.CallInstanceFunction(m_Instance, "PreInit", null);
                 m_Instance.Invoke("PreInit", 0);
            }

            return m_Instance;
        }
    }

    static public bool HasInstance()
    {
        return m_Instance != null;
    }

    //------------------------------------------------------------------------//

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
            Debug.LogException(ex);
        }
    }

    virtual protected void OnDestroy()
    {
        if (m_Instance == this)
        {
            m_Instance = null;
        }
    }

    //------------------------------------------------------------------------//

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

    //------------------------------------------------------------------------//

    static public string ResPath = "";

    static public GameObject LoadGameObjectByResources()
    {
        if (string.IsNullOrEmpty(ResPath))
        {
            var type = typeof(T);
            ResPath = (string)ReflectionTool.GetStaticFieldValue(type, "ResPath");
        }

        GameObject prefab = Resources.Load<GameObject>(ResPath);
        GameObject go = GameObject.Instantiate(prefab);

        return go;
    }
}
