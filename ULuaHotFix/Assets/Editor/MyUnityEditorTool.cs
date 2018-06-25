/******************************************************************
** 文件名:	
** 版  权:	(C)  
** 创建人:  Liange
** 日  期:	2015.5.5
** 描  述: 	

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
*******************************************************************/

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
public class ParticleSystemUnit
{
    public string name;
    public int num;
}

public class MyUnityEditorTool : MonoBehaviour
{
    [MenuItem("Tools/Prefab/CreatePrefab")]
    static private void CreatePrefab()
    {
        foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets))
        {
            GameObject go = obj as GameObject;
            if (go == null)
            {
                continue;
            }

            string path = AssetDatabase.GetAssetPath(go);
            int index = path.LastIndexOf(".");
            if (index > 0)
            {
                path = path.Substring(0, index);
            }
            path += ".prefab";
            PrefabUtility.CreatePrefab(path, go, ReplacePrefabOptions.Default);
        }
    }

    //选择角色目录，然后选择此函数
    [MenuItem("Tools/Prefab/CreateCharacterPrefab")]
    static private void CreateCharacterPrefab()
    {
        foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets))
        {
            GameObject go = obj as GameObject;
            if (go == null)
            {
                continue;
            }

            string path = AssetDatabase.GetAssetPath(go);
            if (path.Contains("Art/Character/") == false)
            {
                continue;
            }

            if (path.Contains("@"))
            {
                continue;
            }

            if (path.EndsWith(".fbx", true, null) == false)
            {
                continue;
            }

            path = path.Replace("Art/Character/", "ForAssetBundles/Player/");
            string dir = MyFileUtil.GetParentDir(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            int index = path.LastIndexOf(".");
            if (index > 0)
            {
                path = path.Substring(0, index);
            }
            path += ".prefab";
            PrefabUtility.CreatePrefab(path, go, ReplacePrefabOptions.Default);
        }

        AssetDatabase.Refresh();
        Debug.Log("CreateCharacterPrefab Over");
    }

    //----------------------------------------------------------------------------//

    [MenuItem("Tools/UI/图集切分")]
    static private void SliceSprite()
    {
        foreach (Object obj in Selection.objects)
        {
            Texture tex = obj as Texture;
            if (tex == null)
            {
                continue;
            }

            string path = AssetDatabase.GetAssetPath(obj);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        Debug.Log("图集切分结束");
    }

    //----------------------------------------------------------------------------//

    [MenuItem("Tools/Shader/将Stand Shader设置为渲染模式")]
    static private void SetStandShaderModel()
    {
        for (int i = 0; i < Selection.gameObjects.Length; ++i)
        {
            GameObject go = Selection.gameObjects[i];
            Renderer ren = go.GetComponent<Renderer>();
            Material[] mats = ren.materials;
            for (int index = 0; index < mats.Length; ++index)
            {
                Material mat = mats[index];
                //
            }
        }
    }

    /************************************************************************************************/

    public static string GetFullName(Transform tran)
    {
        string name = tran.name;
        while (tran.parent != null)
        {
            tran = tran.parent;
            name = tran.name + "/" + name;
        }

        return name;
    }

    //获取指定项在Scene中的完整路径
    [MenuItem("Tools/GetFullPath")]
    static void GetSeletItemFullPath()
    {
        if (Selection.transforms.Length == 0)
        {
            return;
        }

        for (int i = 0; i < Selection.transforms.Length; ++i)
        {
            string strPath = "";
            Transform tran = Selection.transforms[i];
            for (; tran.parent != null; tran = tran.parent)
            {
                strPath = "/" + tran.name + strPath;
            }

            strPath = "/" + tran.name + strPath;
            Debug.Log(strPath);
        }
    }

    //生成资源名和路径
    [MenuItem("Tools/GenerateEffectNamePath")]
    static public void GenerateEffectNamePath()
    {
        try
        {
            string strEffectDir = Application.dataPath + "/Resources/Effect";
            List<FileUnit> fileList = new List<FileUnit>();
            MyFileUtil.GetRelativeFileListWithSpecialFileType(strEffectDir, "Effect", ref fileList, "prefab");

            string filePath = Application.dataPath + "/StreamingAssets/ResoucesConfig/ResoucesConfig.csv";
            StreamWriter writer = new StreamWriter(filePath);
            writer.WriteLine("资源名,资源路径");
            writer.WriteLine("string,string");

            for (int i = 0; i < fileList.Count; ++i)
            {
                fileList[i].name = fileList[i].name.Replace(".prefab", "");
                fileList[i].relativePath = fileList[i].relativePath.Replace(".prefab", "");

                string str = string.Format("{0:s},{1:s}", fileList[i].name, fileList[i].relativePath);
                writer.WriteLine(str);
            }
            writer.Close();
            Debug.Log("GenerateEffectNamePath结束");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("CheckFile.GenerateEffectNamePath:" + ex.Message);
        }
    }

    //检查粒子系统-粒子数量
    [MenuItem("Tools/Check/CheckParticleNum")]
    static public void CheckParticleNum()
    {
        try
        {
            int maxParticles = 60;
            string strEffectDir = Application.dataPath + "/Resources/Effect";
            List<FileUnit> fileList = new List<FileUnit>();
            MyFileUtil.GetRelativeFileListWithSpecialFileType(strEffectDir, "Effect", ref fileList, "prefab");

            Dictionary<string, int> fileNeedModifyList = new Dictionary<string, int>();
            for (int i = 0; i < fileList.Count; ++i)
            {
                fileList[i].relativePath = fileList[i].relativePath.Replace(".prefab", "");

                GameObject go = Resources.Load<GameObject>(fileList[i].relativePath);
                if (go == null)
                {
                    string str = string.Format("CheckFile.CheckParticleNum:effec {0:s} is null", fileList[i].relativePath);
                    Debug.LogError(str);
                    continue;
                }
                go = Instantiate(go) as GameObject;
                ParticleSystem[] psList = go.GetComponentsInChildren<ParticleSystem>();

                if (psList == null || psList.Length == 0)
                {
                    DestroyImmediate(go);
                    continue;
                }

                int totalNum = 0;
                foreach (ParticleSystem ps in psList)
                {
                    totalNum += ps.maxParticles;
                }

                if (totalNum > maxParticles)
                {
                    fileNeedModifyList.Add(fileList[i].relativePath, totalNum);
                }

                DestroyImmediate(go);
            }

            string time = System.DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss");
            string filePath = string.Format("{0:s}/../{1:s}_CheckParticleNum_{2:s}.txt", Application.dataPath, Application.loadedLevelName, time);
            StreamWriter writer = new StreamWriter(filePath);
            foreach (string name in fileNeedModifyList.Keys)
            {
                string str = string.Format("特效名：{0,-90}粒子数量：{1:d}", name, fileNeedModifyList[name]);
                writer.WriteLine(str);
            }
            writer.Close();

        }
        catch (System.Exception ex)
        {
            Debug.LogError("CheckFile.CheckParticleNum:" + ex.Message);
        }
        Debug.Log("CheckParticleNum结束");
    }

    //运行时检查粒子系统-粒子数量
    [MenuItem("Tools/Check/CheckParticleNumRunningTime")]
    static public void CheckParticleNumRunningTime()
    {
        try
        {
            List<ParticleSystemUnit> psUnitList = new List<ParticleSystemUnit>();
            ParticleSystem[] psList = GameObject.FindObjectsOfType<ParticleSystem>();

            int maxParticles = 60;
            int totalNum = 0;
            foreach (ParticleSystem ps in psList)
            {
                totalNum += ps.maxParticles;
                if (ps.maxParticles > maxParticles)
                {
                    ParticleSystemUnit unit = new ParticleSystemUnit();
                    unit.name = GetFullName(ps.transform);
                    unit.num = ps.maxParticles;
                    psUnitList.Add(unit);
                }
            }

            string str = string.Format("MyTools.CheckParticleNumRunningTime:当前场景使用的粒子数量总共是{0:d}", totalNum);
            Debug.Log(str);

            if (psUnitList.Count > 0)
            {
                string time = System.DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss");
                string filePath = string.Format("{0:s}/../{1:s}_CheckParticleNumRunningTime{2:s}.txt", Application.dataPath, Application.loadedLevelName, time);
                StreamWriter writer = new StreamWriter(filePath);
                foreach (ParticleSystemUnit unit in psUnitList)
                {
                    string content = string.Format("路径：{0,-90}粒子数量：{1:d}", unit.name, unit.num);
                    writer.WriteLine(content);
                }
                writer.Close();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("CheckFile.CheckParticleNumRunningTime:" + ex.Message);
        }
        Debug.Log("CheckParticleNumRunningTime结束");
    }

    static void WriteEffectNameToFile(string filePath, List<string> nameList)
    {
        try
        {
            StreamWriter writer = new StreamWriter(filePath);
            foreach (string str in nameList)
            {
                writer.WriteLine(str);
            }
            writer.Close();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("CheckFile.WriteNameToFile:" + ex.Message);
        }
    }

    /************************************************************************************************/

    [MenuItem("Tools/Check/CheckMaterial")]
    static public void ChecksharedMaterial()
    {
        Dictionary<string, ShaderCheckUnit> mShaderCheckUnitDict = new Dictionary<string, ShaderCheckUnit>();

        Renderer[] rendererList = GameObject.FindObjectsOfType<Renderer>();
        foreach (Renderer ren in rendererList)
        {
            if (ren == null)
            {
                continue;
            }
            Material mat = ren.sharedMaterial;
            if (mat == null)
            {
                mat = ren.material;
            }
            string strName = mat.name;

            if (mShaderCheckUnitDict.ContainsKey(strName) == false)
            {
                ShaderCheckUnit unit = new ShaderCheckUnit();
                unit.materialName = mat.name;
                unit.shaderName = mat.shader.name;
                unit.scenePath = GetFullName(ren.gameObject.transform);

                mShaderCheckUnitDict.Add(strName, unit);
            }
        }

        if (mShaderCheckUnitDict.Count > 0)
        {
            string time = System.DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss");
            string filePath = string.Format("{0:s}/../{1:s}_CheckMaterial_{2:s}.txt", Application.dataPath, Application.loadedLevelName, time);

            StreamWriter writer = new StreamWriter(filePath);
            string content = string.Format("当前场景{0:s} 总共使用了{1:d}种纹理", Application.loadedLevelName, mShaderCheckUnitDict.Count);
            writer.WriteLine(content);

            foreach (ShaderCheckUnit unit in mShaderCheckUnitDict.Values)
            {
                content = string.Format("纹理名:{0:s}, 场景中路径:{1:s}", unit.materialName, unit.scenePath);
                writer.WriteLine(content);
            }

            writer.Close();
        }

        Debug.Log("CheckMaterial结束");
    }

    class ShaderCheckUnit
    {
        public string shaderName;
        public string materialName;
        public string scenePath;
    }

    //检查当前场景使用的Shader信息
    [MenuItem("Tools/Check/CheckShader")]
    static void CheckShader()
    {
        Dictionary<string, ShaderCheckUnit> mShaderCheckUnitDict = new Dictionary<string, ShaderCheckUnit>();

        Renderer[] rendererList = GameObject.FindObjectsOfType<Renderer>();
        foreach (Renderer ren in rendererList)
        {
            Material mat = ren.sharedMaterial;
            string strName = mat.shader.name;

            if (mShaderCheckUnitDict.ContainsKey(strName) == false)
            {
                ShaderCheckUnit unit = new ShaderCheckUnit();
                unit.materialName = mat.name;
                unit.shaderName = mat.shader.name;
                unit.scenePath = GetFullName(ren.gameObject.transform);

                mShaderCheckUnitDict.Add(strName, unit);
            }
        }

        if (mShaderCheckUnitDict.Count > 0)
        {
            string time = System.DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss");
            string filePath = string.Format("{0:s}/../{1:s}_ShaderCheck_{2:s}.txt", Application.dataPath, Application.loadedLevelName, time);

            StreamWriter writer = new StreamWriter(filePath);
            string content = string.Format("当前场景{0:s} 总共使用了{1:d}种Shader", Application.loadedLevelName, mShaderCheckUnitDict.Count);
            writer.WriteLine(content);

            foreach (ShaderCheckUnit unit in mShaderCheckUnitDict.Values)
            {
                content = string.Format("Shader名:{0:s}, 纹理名:{1:s}, 场景中路径:{2:s}", unit.shaderName, unit.materialName, unit.scenePath);
                writer.WriteLine(content);
            }

            writer.Close();
        }

        Debug.Log("CheckShader结束");
        //EditorUtility.DisplayDialog("Shader检测", content, "OK");
    }

    /************************************************************************************************/

    //计算节点数量
    [MenuItem("Tools/GetNodeAndChildNodeTotalNum")]
    static public void GetNodeAndChildNodeTotalNum()
    {
        if (Selection.transforms.Length == 0)
        {
            return;
        }

        int maxDepth = 1;
        int monoBehaviourCount = 0;
        int count = Selection.transforms.Length;
        for (int i = 0; i < Selection.transforms.Length; ++i)
        {
            Transform tran = Selection.transforms[i];
            count += GetNodeNum(tran);

            MonoBehaviour[] monoList = tran.GetComponentsInChildren<MonoBehaviour>();
            monoBehaviourCount += monoList.Length;

            GetNodeDepth(tran, 1, ref maxDepth);
        }

        string str = string.Format("CheckFile.GetNodeAndChildNodeTotalNum: Total Node Num is {0:d}, Total MonoBehaviour Num {1:d}, Node Max Depth {2:d}",
            count, monoBehaviourCount, maxDepth);
        Debug.Log(str);
    }

    //计算节点和子节点总数
    static int GetNodeNum(Transform tran)
    {
        int count = 0;
        count += tran.childCount;

        for (int i = 0; i < tran.childCount; ++i)
        {
            count += GetNodeNum(tran.GetChild(i));
        }

        return count;
    }

    //计算节点最大深度值
    static void GetNodeDepth(Transform tran, int depth, ref int maxDepth)
    {
        for (int i = 0; i < tran.childCount; ++i)
        {
            ++depth;
            GetNodeDepth(tran.GetChild(i), depth, ref maxDepth);
            if (depth > maxDepth)
            {
                maxDepth = depth;
            }
            --depth;
        }
    }

    //检查目录下面的文件是否有重复名
    [MenuItem("Tools/Check/CheckRepeatFileName")]
    static void CheckRepeatFileName()
    {
        string dir = MyFileUtil.CacheDir;
        List<FileUnit> fileList = new List<FileUnit>();
        MyFileUtil.GetRelativeFileList(dir, ref fileList, ".meta");

        for (int i = 0; i < fileList.Count; ++i)
        {
            FileUnit unit = fileList[i];
            FileInfo fi = new FileInfo(unit.name);
            string ext = fi.Extension;
            unit.name = unit.name.Replace(ext, "");
        }

        List<string> mNameList = new List<string>();
        HashSet<string> mDictName = new HashSet<string>();
        foreach (FileUnit unit in fileList)
        {
            if (mDictName.Contains(unit.name))
            {
                mNameList.Add(unit.name);
            }
            else
            {
                mDictName.Add(unit.name);
            }
        }

        if (mDictName.Count > 0)
        {
            string time = System.DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss");
            string filePath = string.Format("{0:s}/../CheckRepeatFileName_{1:s}.txt", Application.dataPath, time);

            StreamWriter writer = new StreamWriter(filePath);
            writer.WriteLine("重复文件名：");
            foreach (string name in mNameList)
            {
                writer.WriteLine(name);
            }

            writer.Close();
        }

        Debug.Log("CheckRepeatFileName结束");
    }


    [MenuItem("Tools/GC")]
    static void GC()
    {
        System.GC.Collect();
        Debug.Log("GC结束");
    }

    [MenuItem("Tools/UnloadUnusedAssets")]
    static void ClearMemory()
    {
        Resources.UnloadUnusedAssets();
        Debug.Log("UnloadUnusedAssets结束");
    }

    /************************************************************************************************/
    [MenuItem("Tools/Encoding/将所有lua脚本编码格式转为UTF-8(无BOM)")]
    static void FormatChoose()
    {
        string luaDir = Application.dataPath + "/StreamingAssets/Lua";
        List<FileUnit> fileList = new List<FileUnit>();
        MyFileUtil.GetRelativeFileListWithSpecialFileType(luaDir, "", ref fileList, "lua");
        for (int i = 0; i < fileList.Count; i++)
        {
            TextImporter.ConvertToUTF8(fileList[i].fullPath);
        }

        Debug.Log("转换Lua脚本格式结束");
    }

    /// <summary>
    /// 将选中脚本编码转换为：UTF-8 +BOM
    /// </summary>
    [MenuItem("Tools/Encoding/将选中文件转为UTF-8(无BOM)格式")]
    public static void SetSelectedScriptsToUTF8_BOM()
    {
        foreach (Object obj in Selection.objects)
        {
            string filePath = MyFileUtil.GetParentDir(Application.dataPath) + AssetDatabase.GetAssetPath(obj);
            TextImporter.ConvertToUTF8(filePath);
        }

        Debug.Log("转换文件格式结束");
    }

    /************************************************************************************************/

    static public string GetPlatformFolderForAssetBundles(UnityEditor.BuildTarget target)
    {
        switch (target)
        {
            case UnityEditor.BuildTarget.Android:
                return "Android";
            case UnityEditor.BuildTarget.iOS:
                return "IOS";
            case UnityEditor.BuildTarget.WebGL:
                return "WebPlayer";
            case UnityEditor.BuildTarget.StandaloneWindows:
            case UnityEditor.BuildTarget.StandaloneWindows64:
                return "Windows";
            case UnityEditor.BuildTarget.StandaloneOSXIntel:
            case UnityEditor.BuildTarget.StandaloneOSXIntel64:
            // case UnityEditor.BuildTarget.StandaloneOSX:
                return "OSX";
            // Add more build targets for your own.
            // If you add more targets, don't forget to add the same platforms to GetPlatformFolderForAssetBundles(RuntimePlatform) function.
            default:
                return null;
        }
    }
}
