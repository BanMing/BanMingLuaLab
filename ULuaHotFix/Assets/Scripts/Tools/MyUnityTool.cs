
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 工具集
/// </summary>
public class MyUnityTool 
{
    static private int SortByName(Transform a, Transform b) { return string.Compare(a.name, b.name); }

    static public List<Transform> GetSortTransformChildList(Transform myTran)
    {
        List<Transform> list = new List<Transform>();

        for (int i = 0; i < myTran.childCount; ++i)
        {
            Transform tran = myTran.GetChild(i);
            if (tran)
            {
                list.Add(tran);
            }
        }

        list.Sort(SortByName);
        return list;
    }

    //--------------------------------------------------------------------------//

    //查找节点，如果是"/xx/xx/xx"路径，查找失败后会寻找父节点，再通过父节点查找子节点
    static public GameObject Find(string name)
    {
        GameObject go = GameObject.Find(name);
        if (go != null)
        {
            return go;
        }
        string childName = "";
        while (true)
        {
            int index = name.LastIndexOf('/');
            if (index <= 0)
            {
                break;
            }

            childName = name.Substring(index) + childName;   //
            name = name.Substring(0, index);

            go = GameObject.Find(name);
            if (go != null)
            {
                if (childName[0] == '/')
                {
                    childName = childName.Substring(1);
                }

                Transform tran = go.transform.Find(childName);
                if (tran != null)
                {
                    return tran.gameObject;
                }

                return null;
            }
        }

        return null;
    }

    //--------------------------------------------------------------------------//

    static public Transform FindChild(UnityEngine.Object parent, string childName)
    {
        GameObject go = (GameObject)parent;
        if(go == null)
        {
            string str = string.Format("MyUnityTool.FindChild:在节点{0}下查找子节点{1}", parent.name, childName);
            Debug.LogError(str);
            return null;
        }

        return FindChild(go.transform, childName);
    }

    static public Transform FindChild(UnityEngine.GameObject parent, string childName)
    {
        return FindChild(parent.transform, childName);
    }

    static public Transform FindChild(Transform parent, string childName)
    {
        if (childName.Contains("/"))
        {
            return parent.Find(childName);
        }

        Transform child = parent.Find(childName);
        if (child != null)
        {
            return child;
        }

        Transform[] tranArray = parent.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < tranArray.Length; ++i)
        {
            Transform tran = tranArray[i];
            if (tran.name == childName)
            {
                return tran;
            }
        }
        
        return null;
    }

    //寻找指定节点的子节点, 会遍历该节点的所有子节点直到找到该节点，深度优先
    static public Transform FindChildByDFS(Transform parent, string childName)
    {
        if (childName.Contains("/"))
        {
            return parent.Find(childName);
        }

        for (int i = 0; i < parent.childCount; ++i)
        {
            Transform tran = parent.GetChild(i);
            if (tran.name == childName)
            {
                return tran;
            }

            tran = FindChildByDFS(tran, childName);
            if (tran != null)
            {
                return tran;
            }
        }

        return null;
    }

    //寻找指定节点的子节点, 会遍历该节点的所有子节点直到找到该节点，广度优先
    static public Transform FindChildByBFS(Transform parent, string childName)
    {
        if (childName.Contains("/"))
        {
            return parent.Find(childName);
        }

        Queue<Transform> tranQueue = new Queue<Transform>();
        Transform tran = FindChildByBFSImp(parent, childName, ref tranQueue);
        while (tranQueue.Count > 0 && tran == null)
        {
            parent = tranQueue.Dequeue();
            tran = FindChildByBFSImp(parent, childName, ref tranQueue);
        }

        return tran;
    }

    static private Transform FindChildByBFSImp(Transform parent, string childName, ref Queue<Transform> tranQueue)
    {
        for (int i = 0; i < parent.childCount; ++i)
        {
            Transform tran = parent.GetChild(i);
            if (tran.name == childName)
            {
                return tran;
            }
        }

        for (int i = 0; i < parent.childCount; ++i)
        {
            Transform tran = parent.GetChild(i);
            if (tran.childCount > 0)
            {
                tranQueue.Enqueue(tran); //
            }            
        }


        return null;
    }

    //--------------------------------------------------------------------------//

    static public void SetActive(string name, bool active)
    {
        GameObject go = Find(name);
        if (go != null)
        {
            go.SetActive(active);
        }
    }

    static public void SetActive(Component com, bool active)
    {
        com.gameObject.SetActive(active);
    }

    //激活或隐藏某个节点，如果指定路径的节点没有找到，则会寻找该节点的父节点，然后通过该该节点的父节点来激活子节点
    static public bool SetActiveRecursion(string name, bool active)
    {
        GameObject go = GameObject.Find(name);
        if (go != null)
        {
            go.SetActive(active);
            return true;
        }

        if (active)
        {
            Stack<string> stackChildName = new Stack<string>();
            while (true)
            {
                int index = name.LastIndexOf('/');
                if (index <= 0)
                {
                    return false;
                }

                string childName = name.Substring(index + 1);   // +1去掉"/"
                stackChildName.Push(childName);

                name = name.Substring(0, index);
                go = GameObject.Find(name);

                if (go != null)
                {
                    break;
                }
            }

            Transform tran = go.transform;
            while (stackChildName.Count > 0)
            {
                string childName = stackChildName.Pop();
                tran = tran.Find(childName);
                if (tran == null)
                {
                    return false;
                }

                tran.gameObject.SetActive(true);
            }

            return true;
        }

        return false;
    }

    //激活或隐藏某个节点，如果指定路径的节点没有找到，则会寻找该节点的父节点，然后通过该该节点的父节点来激活子节点,递归算法
    static public bool SetActiveRecursionByRecursion(string name, bool active)
    {
        GameObject go = GameObject.Find(name);
        if (go != null)
        {
            go.SetActive(active);
            return true;
        }

        if (active)
        {
            int index = name.LastIndexOf('/');
            if (index <= 0)
            {
                return false;
            }

            string parentName = name.Substring(0, index);
            bool result = SetActiveRecursionByRecursion(parentName, active);
            if (result)
            {
                GameObject parent = GameObject.Find(parentName);
                string childName = name.Substring(index + 1);   // +1去掉"/"
                Transform child = parent.transform.Find(childName);
                if (child != null)
                {
                    child.gameObject.SetActive(active);
                    return true;
                }
            }
        }

        return false;
    }

    static public bool SetActiveRecursion(Transform tran, bool active)
    {
        if (active == false)
        {
            tran.gameObject.SetActive(false);
        }
        else
        {
            tran.gameObject.SetActive(true);
            while (tran != null && tran.parent != null && tran.parent.gameObject.activeInHierarchy == false)
            {
                tran = tran.parent;
                tran.gameObject.SetActive(true);
            }
        }

        return false;
    }

    static public void SetActiveChild(Transform parent, string childName, bool active)
    {
        Transform tran = parent.Find(childName);
        if (tran != null)
        {
            tran.gameObject.SetActive(active);
        }
    }

    static public void SetActiveChildren(string name, bool active)
    {
        GameObject go = Find(name);
        if (go != null)
        {
            for (int i = 0; i < go.transform.childCount; ++i )
            {
                go.transform.GetChild(i).gameObject.SetActive(active);
            }
        }
        else
        {
            string str = string.Format("MyUnityTool.SetActiveChildren: find node {0:s} fail", name);
            Debug.LogError(str);
        }
    }

    //--------------------------------------------------------------------------//

    static public void DestroyGameObject(Component com)
    {
        GameObject.Destroy(com.gameObject);
    }

    //--------------------------------------------------------------------------//

    //设置父节点，然后恢复子节点之前的Transform信息
    static public void SetParentWithLocalInfo(Transform tran, Transform parent)
    {
        if (parent != null)
        {
            Vector3 pos = tran.localPosition;
            Vector3 scale = tran.localScale;
            Quaternion quaternion = tran.localRotation;
            tran.parent = parent;
            tran.localPosition = pos;
            tran.localScale = scale;
            tran.localRotation = quaternion;
        }
    }

    static public void SetUIParentWithLocalInfo(Transform tran, RectTransform parent)
    {
        SetUIParentWithLocalInfo((RectTransform)tran, parent);
    }

    static public void SetUIParentWithLocalInfo(RectTransform tran, RectTransform parent)
    {
        if (parent != null)
        {
            Vector3 pos = tran.localPosition;
            Vector3 scale = tran.localScale;
            Quaternion quaternion = tran.localRotation;

            Vector2 anchoredPosition = tran.anchoredPosition;
            Vector2 anchoredPosition3D = tran.anchoredPosition3D;
            Vector2 anchorMax = tran.anchorMax;
            Vector2 anchorMin = tran.anchorMin;
            Vector2 offsetMax = tran.offsetMax;
            Vector2 offsetMin = tran.offsetMin;
            Vector2 pivot = tran.pivot;
            Vector2 sizeDelta = tran.sizeDelta;

            tran.SetParent(parent);

            tran.anchoredPosition = anchoredPosition;
            tran.anchoredPosition3D = anchoredPosition3D;
            tran.anchorMax = anchorMax;
            tran.anchorMin = anchorMin;
            tran.offsetMax = offsetMax;
            tran.offsetMin = offsetMin;
            tran.pivot = pivot;
            tran.sizeDelta = sizeDelta;

            tran.localPosition = pos;
            tran.localScale = scale;
            tran.localRotation = quaternion;
        }
    }

    //--------------------------------------------------------------------------//

    public static string GetFullName(Transform tran)
    {
        string name = tran.name;
        while (tran.parent != null)
        {
            tran = tran.parent;
            name = tran.name + "/" + name;
        }
        name = "/" + name;
        return name;
    }

    //--------------------------------------------------------------------------//

    static public void PlayAnimation(GameObject go, string animationName)
    {
        Animator animator = go.GetComponent<Animator>();
        if (animator != null)
        {
            if (animator.enabled == false)
            {
                animator.enabled = true;
            }
            animator.Play(animationName);
        }
    }

    //--------------------------------------------------------------------------//

    static public void SetLayer(GameObject go, int layer, bool recursion = true)
    {
        go.layer = layer;

        if (recursion)
        {
            Transform[] tranArray = go.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < tranArray.Length; ++i)
            {
                tranArray[i].gameObject.layer = layer;
            }
        }
    }

    //--------------------------------------------------------------------------//   

    static public UnityEngine.Component GetComponentByString(Transform parent, string childName, string type)
    {
        Transform tran = FindChild(parent, childName);
        return GetComponentByString(tran, type);
    }

    static public UnityEngine.Component GetComponentByString(Transform tran, string type)
    {
        return tran.gameObject.GetComponent(type);
    }

    /*
    static public void ConvertType(System.Object obj, string strType)
    {
        System.Type type = System.Type.GetType(strType);
        dynamic teacher = type.IsInstanceOfType(obj) ? obj : null;
    }
    */

    //--------------------------------------------------------------------------//

    public static System.Object CreateInstance(string typeName)
    {
        System.Type type = System.Type.GetType(typeName);
        if (type == null)
        {
            string str = string.Format("LuaDynamicImportTool.CreateInstance:获取Type {0}失败", typeName);
            Debug.LogError(str);
            return null;
        }
        var obj = type.GetConstructor(System.Type.EmptyTypes).Invoke(null);
        return obj;
    }

    //--------------------------------------------------------------------------//

    //判断两个浮点数是否相近(相等)
    public static bool Approximately(float a, float b, float admissibleValue = 0.1f)
    {
        return Mathf.Abs(a - b) <= admissibleValue;
    }

    public static int Compare(long n1,long n2)
    {
        if (n1 > n2)
            return 1;
        else if (n1 < n2)
            return -1;
        else
            return 0;
    }

    //--------------------------------------------------------------------------//

    static private string GetUniqueID()
    {
        long tick = System.DateTime.Now.Ticks;
        System.Random random = new System.Random((int)tick);
        int num = random.Next(0, 1000000);

        string str = string.Format("{0}_{1}", tick, num);
        return str;
    }

    //--------------------------------------------------------------------------//

    public static bool IsProcessorArch64()
    {
        return System.IntPtr.Size != 4;
    }

    // #region 识别二维码
    // static public string DecodeQR(Texture2D texture)
    // {
    //     var reader = new BarcodeReader();
    //     var bitmap = texture.GetPixels32();
    //     var result = reader.Decode(bitmap, texture.width, texture.height);

    //     if (result != null)
    //     {
    //         return result.Text;
    //     }
    //     return null;
    // }
    // #endregion
}
