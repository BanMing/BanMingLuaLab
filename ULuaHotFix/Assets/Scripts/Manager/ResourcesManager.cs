
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using Mono.Xml;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ResourcesManager : SingletonMonoBehaviour<ResourcesManager>
{
    private Transform mResourcesRoot = null;
    public Transform ResourcesRoot
    {
        get
        {
            if (mResourcesRoot == null)
            {
                GameObject go = new GameObject("ResourcesManager");
                mResourcesRoot = mResourcesRoot.transform;
            }

            return mResourcesRoot;
        }
    }

    //true使用AssetBundle加载资源，false使用Resources加载资源
    static public bool IsUseAssetBundle
    {
        get
        {
            return SystemConfig.Instance.IsUseAssetBundle;
        }
    }

    static public bool IsLuaUseZip
    {
        get
        {
            return SystemConfig.Instance.IsLuaUseZip;
        }
    }

    //-----------------------------------------------------------------------------//

    public class ResourceUnit
    {
        public UnityEngine.Object prefab;
        public Queue<UnityEngine.Object> instanceList = new Queue<UnityEngine.Object>();
    }

    private Dictionary<string, ResourceUnit> mDictResources = new Dictionary<string, ResourceUnit>();

    //---------------------------------以下方法在加载资源调用时不优先使用--------------------------------------------//

    public void PreLoadResources(string name)
    {
        ResourceUnit unit = new ResourceUnit();
        unit.prefab = Resources.Load(name);

        //UnityEngine.Object instance = UnityEngine.Object.Instantiate(unit.prefab);
        //unit.instanceList.Enqueue(instance);

        mDictResources.Add(name, unit);
    }

    //异步读取资源
    public void AsyncPreLoadResources(string name)
    {
        ScriptThread.Instance.StartCoroutine(AsyncPreLoadResourcesImp(name));
    }

    private IEnumerator AsyncPreLoadResourcesImp(string name)
    {
        ResourceRequest req = Resources.LoadAsync(name);
        yield return req;

        ResourceUnit unit = new ResourceUnit();
        unit.prefab = req.asset;

        //UnityEngine.Object instance = UnityEngine.Object.Instantiate(unit.prefab);
        //unit.instanceList.Enqueue(instance);

        mDictResources.Add(name, unit);
    }

    //获取原始资源
    public UnityEngine.Object GetResourcePrefab(string name)
    {
        if (mDictResources.ContainsKey(name) == false)
        {
            PreLoadResources(name);
        }

        ResourceUnit unit = mDictResources[name];
        return unit.prefab;
    }

    public UnityEngine.Object GetResourceInstance(string name)
    {
        if (mDictResources.ContainsKey(name) == false)
        {
            PreLoadResources(name);
        }

        ResourceUnit unit = mDictResources[name];
        if (unit.instanceList.Count == 0)
        {
            UnityEngine.Object instance = UnityEngine.Object.Instantiate(unit.prefab);
            return instance;
        }
        else
        {
            UnityEngine.Object instance = unit.instanceList.Dequeue();
            return instance;
        }
    }

    //获取GameObject实例
    public GameObject GetGameObjectInstance(string name)
    {
        UnityEngine.Object instance = GetResourceInstance(name);
        if (instance == null)
        {
            return null;
        }

        GameObject go = (GameObject)instance;
        if (go == null)
        {
            return null;
        }

        ResetResourcesGameObject(go);

        //重置为默认值
        Transform prefab = (Transform)mDictResources[name].prefab;
        Transform tran = go.GetComponent<Transform>();

        tran.position = prefab.position;
        tran.rotation = prefab.rotation;
        tran.localScale = prefab.localScale;
        tran.name = prefab.name;

        go.SetActive(true);

        return go;
    }

    //重置动画、粒子特效、音乐
    private void ResetResourcesGameObject(GameObject instance)
    {
        //重置动画组件
        Animator[] animatorComponents = instance.GetComponentsInChildren<Animator>();
        for (int i = 0; i < animatorComponents.Length; i++)
        {
            Animator animator = animatorComponents[i];
            //animator.Play(animator.GetCurrentAnimatorStateInfo(0).nameHash);
            animator.Play(animator.GetCurrentAnimatorStateInfo(0).fullPathHash);
        }

        //重置粒子动画
        ParticleSystem[] particleSystemComponents = instance.GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < particleSystemComponents.Length; i++)
        {
            ParticleSystem particleSystem = particleSystemComponents[i];
            particleSystem.Clear();
            particleSystem.time = 0;
        }

        //重置声音
        AudioSource[] audioSourceComponents = instance.GetComponentsInChildren<AudioSource>();
        for (int j = 0; j < audioSourceComponents.Length; j++)
        {
            AudioSource audioSource = audioSourceComponents[j];
            if (audioSource.playOnAwake)
            {
                audioSource.Play();
            }
        }
    }

    //-----------------------------------------------------------------------------//

    public void DestroyResourceInstance(UnityEngine.Object instance)
    {
        if(instance != null)
        {
            DestroyResourceInstance(instance.name, instance);
        }
    }

    //删除资源，放入缓存池中
    public void DestroyResourceInstance(string name, UnityEngine.Object instance)
    {
        if (mDictResources.ContainsKey(name) == false)
        {
            UnityEngine.Object.Destroy(instance);
        }
        else
        {
            GameObject go = (GameObject)instance;
            if (go != null)
            {
                go.GetComponent<Transform>().parent = ResourcesRoot;
                go.SetActive(false);
            }

            ResourceUnit unit = mDictResources[name];
            unit.instanceList.Enqueue(instance);
        }
    }

    //延时删除
    public void DestroyResourceInstance(string name, UnityEngine.Object instance, float delaySeconds)
    {
        if (mDictResources.ContainsKey(name) == false)
        {
            UnityEngine.Object.Destroy(instance, delaySeconds);
        }
        else
        {
            ScriptThread.Instance.StartCoroutine(DestroyAfterSeconds(name, instance, delaySeconds));
        }
    }

    private IEnumerator DestroyAfterSeconds(string name, UnityEngine.Object instance, float seconds)
    {
        while (seconds > 0)
        {
            yield return null;
            seconds -= Time.deltaTime;
        }

        DestroyResourceInstance(name, instance);
    }

    //-----------------------------------------------------------------------------//

    //     public class AssetBundleInfo
    //     {
    //         public string assetBundleName;  //名字，路径，相对于assets的路径
    //         public string hashCode;         //服务器版本 hash code
    //     }

    public enum AssetBundleState
    {
        Loading,
        Loaded,
    }

    public class LoadedAssetBundle
    {
        public string abName = null;
        public AssetBundle assetBundle = null;
        public int referencedCount = 0;
        public AssetBundleState State = AssetBundleState.Loading;

        public LoadedAssetBundle()
        {
            referencedCount = 1;
        }
    }

    //-----------------------------------------------------------------------------//

    //工作目录
    static public string WorkDirInProject
    {
        get
        {
            return Application.streamingAssetsPath + "/";
        }
    }

    public const string DirNameForAssetBundlesBuildFrom = "ForAssetBundles";  //用于打包输入的文件夹名字

    //用于打包输入的文件夹路径
    public static string DirForAssetBundlesBuildFrom
    {
        get
        {
            return Application.dataPath + "/Resources/" + DirNameForAssetBundlesBuildFrom + "/";
        }
    }

    static private string m_AssetBundlesResDirInStreamingAssetsPath = null; //资源目录，所有ab资源都在此目录下

    static public string AssetBundlesResDirInStreamingAssetsPath
    {
        get
        {
            if (m_AssetBundlesResDirInStreamingAssetsPath == null)
            {
                m_AssetBundlesResDirInStreamingAssetsPath = WorkDirInProject + "AssetBundles/" + GetPlatformDir() + "/";
            }

            return m_AssetBundlesResDirInStreamingAssetsPath;
        }
    }

    //带前缀的资源目录
    static private string AssetBundlesResDirInStreamingAssetsPathForWWW
    {
        get
        {
            string dir = AssetBundlesResDirInStreamingAssetsPath;
            if (Application.platform != RuntimePlatform.Android)
            {
                dir = "file://" + AssetBundlesResDirInStreamingAssetsPath;
            }

            return dir;
        }
    }

    static public string AssetBundlesResDirInCacheDir
    {
        get
        {
            return MyFileUtil.CacheDir + "AssetBundles/" + GetPlatformDir() + "/";
        }
    }

    //内部工作目录(Application.streamingAssetsPath)对应——外部缓存目录(CacheDir)

    //用于区分外部文件还是内部文件
    private string GetFileFinalPath(string fileName)
    {
        string outFilePath = MyFileUtil.CacheDir + fileName;
        if (File.Exists(outFilePath))
        {
            return outFilePath;
        }
        else
        {
            string path = WorkDirInProject + fileName;
            return path;
        }
    }

    private string GetABFileFinalPath(string fileName)
    {
        string outFilePath = AssetBundlesResDirInCacheDir + fileName;
        if (File.Exists(outFilePath))
        {
            return outFilePath;
        }
        else
        {
            string path = AssetBundlesResDirInStreamingAssetsPath + fileName;
            return path;
        }
    }

    //用于区分外部文件还是内部文件 用于www
    private string GetFileFinalPathForWWW(string fileName)
    {
        string outFilePath = AssetBundlesResDirInCacheDir + fileName;
        if (File.Exists(outFilePath))
        {
            outFilePath = "file://" + outFilePath;
            return outFilePath;
        }
        else
        {
            string path = AssetBundlesResDirInStreamingAssetsPathForWWW + fileName;
            return path;
        }
    }

    //-----------------------------------------------------------------------------//

    static public string GetPlatformDir()
    {
#if UNITY_EDITOR
        return GetPlatformFolderForAssetBundles(EditorUserBuildSettings.activeBuildTarget);
#else
        return GetPlatformFolderForAssetBundles(Application.platform);
#endif
    }

#if UNITY_EDITOR

    static private string GetPlatformFolderForAssetBundles(UnityEditor.BuildTarget target)
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
            // case UnityEditor.BuildTarget.StandaloneOSXUniversal:
                return "OSX";
            // Add more build targets for your own.
            // If you add more targets, don't forget to add the same platforms to GetPlatformFolderForAssetBundles(RuntimePlatform) function.
            default:
                return null;
        }
    }

#endif

    static private string GetPlatformFolderForAssetBundles(RuntimePlatform platform)
    {
        switch (platform)
        {
            case RuntimePlatform.Android:
                return "Android";
            case RuntimePlatform.IPhonePlayer:
                return "IOS";
            case RuntimePlatform.WebGLPlayer:
            // case RuntimePlatform.OSXWebPlayer:
                return "WebPlayer";
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
                return "Windows";
            case RuntimePlatform.OSXPlayer:
                return "OSX";
            // Add more build platform for your own.
            // If you add more platforms, don't forget to add the same targets to GetPlatformFolderForAssetBundles(BuildTarget) function.
            default:
                return null;
        }
    }

    //-----------------------------------------------------------------------------//

    private AssetBundleManifest mAssetBundleManifest = null;

    static private string ManifestAssetBundleName
    {
        get
        {
            return GetPlatformDir();
        }
    }

    static public string ManifestAssetBundlePath
    { 
        get
        {
            return WorkDirInProject + "AssetBundles/" + ManifestAssetBundleName + "/" + ManifestAssetBundleName;
        }
    }

    static private string mManifestAssetName = "AssetBundleManifest";

    public const string mAssetBundleSuffix = ".unity3d";

    private Dictionary<string, LoadedAssetBundle> mDictLoadedAssetBundle = new Dictionary<string, LoadedAssetBundle>(); //记录已经加载的AssetBundle

    //-----------------------------------------------------------------------------//

    private void LoadAssetBundleManifest(Action<bool> initFinishCB)
    {
        //读取Manifest
        System.Action<UnityEngine.Object> loadManifestCallBack = delegate (UnityEngine.Object obj)
        {
            mAssetBundleManifest = (AssetBundleManifest)obj;
            if (mAssetBundleManifest == null)
            {
                initFinishCB(false);
                Debug.LogError("ResourcesManager.LoadAssetBundleManifest: AssetBundle系统初始化失败");
            }
            else
            {
                initFinishCB(true);
                Debug.Log("ResourcesManager.LoadAssetBundleManifest: AssetBundle系统初始化成功");
            }
        };

        //防止多次初始化
        if(mAssetBundleManifest != null)
        {
            initFinishCB(true);
            return;
        }

        ScriptThread.Instance.StartCoroutine(LoadAssetBundleList(ManifestAssetBundleName, mManifestAssetName, loadManifestCallBack, true));
    }

    public void LoadAssetBundle(string assetBundleName, string assetName, System.Action<UnityEngine.Object> loadFinishCallBack)
    {
        if (assetBundleName.EndsWith(mAssetBundleSuffix) == false)
        {
            assetBundleName = assetBundleName + mAssetBundleSuffix;
        }

        assetBundleName = assetBundleName.ToLower();

        ScriptThread.Instance.StartCoroutine(LoadAssetBundleList(assetBundleName, assetName, loadFinishCallBack, false));
    }

    public UnityEngine.Object LoadAssetBundleSync(string assetBundleName, string assetName)
    {
        if (assetBundleName.EndsWith(mAssetBundleSuffix) == false)
        {
            assetBundleName = assetBundleName + mAssetBundleSuffix;
        }

        assetBundleName = assetBundleName.ToLower();

        return LoadAssetBundleListSync(assetBundleName, assetName, false);
    }

    //assetBundleNameList: 需要加载的所有的assetBundle列表,包括依赖列表
    //targetAssetBundleName： 从此assetBundle中加载资源
    //assetName: 资源名
    private IEnumerator LoadAssetBundleList(string targetAssetBundleName, string assetName, System.Action<UnityEngine.Object> loadFinishCallBack, bool isLoadManifest = false)
    {
        //获取需要加载的assetBundle列表，包括依赖
        List<string> assetBundleNameList = new List<string>();
        if (isLoadManifest == false)
        {
            //依赖
            GetLoadDependencies(targetAssetBundleName, ref assetBundleNameList);
        }
        //自身
        assetBundleNameList.Add(targetAssetBundleName);

        //---------------------------------------------------------//

        for (int i = 0; i < assetBundleNameList.Count; ++i)
        {
            string assetBundleName = assetBundleNameList[i];

            //判断是否已经加载
            //判断是否正在加载，如果真正加载则等待加载结束，如果2个资源在同一个bundle中,同时调用加载这两个资源则需要避免加载此bundle2次
            LoadedAssetBundle loadAB = null;
            if (mDictLoadedAssetBundle.TryGetValue(assetBundleName, out loadAB))
            {
                while (loadAB.State == AssetBundleState.Loading)
                {
                    yield return null;
                }

                loadAB.referencedCount++;
                continue;
            }

            //---------------------------------------------------------//

            loadAB = new LoadedAssetBundle();
            loadAB.abName = assetBundleName;
            mDictLoadedAssetBundle.Add(assetBundleName, loadAB);

            string path = GetFileFinalPathForWWW(assetBundleName);
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                path = System.Uri.EscapeUriString(path);
            }

            WWW www = new WWW(path);
            yield return www;

            if (string.IsNullOrEmpty(www.error) == false)
            {
                mDictLoadedAssetBundle.Remove(assetBundleName);
                string str = string.Format("ResourcesManager.LoadAssetBundleList:assetBundle name {0}, asset name{1}, filePath {2}加载失败，错误信息 {3}", 
                    targetAssetBundleName, assetName, path, www.error);
                Debug.LogError(str);

                if (loadFinishCallBack != null)
                {
                    loadFinishCallBack(null);
                    yield break;
                }
            }
            else
            {
                loadAB.State = AssetBundleState.Loaded;
                loadAB.assetBundle = www.assetBundle;

                //修复Shader问题
                if(IsFixShader)
                {
                    FixShader(loadAB);
                }
            }
        }

        LoadedAssetBundle bundle = mDictLoadedAssetBundle[targetAssetBundleName];

        //处理加载场景的情况
        if (bundle.assetBundle.isStreamedSceneAssetBundle)
        {
            if (loadFinishCallBack != null)
            {
                loadFinishCallBack(null);
            }

            yield break;
        }

        AssetBundleRequest req = null;
        if (string.IsNullOrEmpty(assetName))
        {
            req = bundle.assetBundle.LoadAllAssetsAsync();
        }
        else
        {
            req = bundle.assetBundle.LoadAssetAsync(assetName);
        }

        yield return req;

        if (loadFinishCallBack != null)
        {
            loadFinishCallBack(req.asset);
        }
    }

    //同步加载资源
    //assetBundleNameList: 需要加载的所有的assetBundle列表,包括依赖列表
    //targetAssetBundleName： 从此assetBundle中加载资源
    //assetName: 资源名
    private UnityEngine.Object LoadAssetBundleListSync(string targetAssetBundleName, string assetName, bool isLoadManifest = false)
    {
        //获取需要加载的assetBundle列表，包括依赖
        List<string> assetBundleNameList = new List<string>();
        if (isLoadManifest == false)
        {
            //依赖
            GetLoadDependencies(targetAssetBundleName, ref assetBundleNameList);
        }
        //自身
        assetBundleNameList.Add(targetAssetBundleName);

        //---------------------------------------------------------//

        for (int i = 0; i < assetBundleNameList.Count; ++i)
        {
            string assetBundleName = assetBundleNameList[i];

            //判断是否已经加载
            //判断是否正在加载，如果真正加载则等待加载结束，如果2个资源在同一个bundle中,同时调用加载这两个资源则需要避免加载此bundle2次
            LoadedAssetBundle loadAB = null;

            if (mDictLoadedAssetBundle.TryGetValue(assetBundleName, out loadAB))
            {
                loadAB.referencedCount++;
                continue;
            }

            //---------------------------------------------------------//

            loadAB = new LoadedAssetBundle();
            loadAB.abName = assetBundleName;
            mDictLoadedAssetBundle.Add(assetBundleName, loadAB);

            string path = GetABFileFinalPath(assetBundleName);

            AssetBundle asset_bundle = AssetBundle.LoadFromFile(path);

            if(asset_bundle == null)
            {
                mDictLoadedAssetBundle.Remove(assetBundleName);
                string str = string.Format("ResourcesManager.LoadAssetBundleList:assetBundle name {0}, asset name{1}, filePath {2}加载失败，错误信息",
                    targetAssetBundleName, assetName, path);
                Debug.LogError(str);
                return null;
            }
            else
            {
                loadAB.State = AssetBundleState.Loaded;
                loadAB.assetBundle = asset_bundle;

                //修复Shader问题
                if (IsFixShader)
                {
                    FixShader(loadAB);
                }
            }
        }

        LoadedAssetBundle bundle = mDictLoadedAssetBundle[targetAssetBundleName];

        //处理加载场景的情况
        if (bundle.assetBundle.isStreamedSceneAssetBundle)
        {
            return null;
        }
        UnityEngine.Object result = null;

        if (string.IsNullOrEmpty(assetName))
        {
            UnityEngine.Object[] results = bundle.assetBundle.LoadAllAssets();
            if(results!=null)
            {
                result = results[0];
            }
        }
        else
        {
            result = bundle.assetBundle.LoadAsset(assetName);
        }
        
        return result;
    }

    //加载依赖
    private void GetLoadDependencies(string assetBundleName, ref List<string> assetBundleNameList)
    {
        if (mAssetBundleManifest == null)
        {
            string str = "ResourcesManager.GetLoadDependencies: mAssetBundleManifest is null";
            Debug.LogError(str);
            return;
        }

        string[] dependencies = mAssetBundleManifest.GetAllDependencies(assetBundleName);
        if (dependencies.Length == 0)
        {
            return;
        }

        for (int i = 0; i < dependencies.Length; ++i)
        {
            GetLoadDependencies(dependencies[i], ref assetBundleNameList);
        }

        assetBundleNameList.AddRange(dependencies);
    }

    //-----------------------------------------------------------------------------//

    public static bool IsFixShader = false;

    public static Dictionary<string, Shader> m_DictShader = new Dictionary<string, Shader>();

    public static void FixShader(LoadedAssetBundle bundle)
    {
        if(bundle.abName.Contains(".mat"))
        {
            var objs = bundle.assetBundle.LoadAllAssets();
            if(objs == null || objs.Length == 0)
            {
                return;
            }

            for(int i = 0; i < objs.Length; ++i)
            {
                FixShader(objs[i]);
            }
        }
        else if(bundle.abName.Contains(".shader"))
        {
            var objs = bundle.assetBundle.LoadAllAssets();
            if (objs == null || objs.Length == 0)
            {
                return;
            }

            for (int i = 0; i < objs.Length; ++i)
            {
                Shader shader = objs[i] as Shader;
                if(shader != null && m_DictShader.ContainsKey(shader.name) == false)
                {
                    m_DictShader.Add(shader.name, shader);
                }
            }
        }
    }

    public static void FixShader(UnityEngine.Object obj)
    {
        Material material = obj as Material;
        if (material)
        {
            Shader shader = Shader.Find(material.shader.name);
            if (shader != null)
            {
                material.shader = shader;
            }
            else
            {
                if (m_DictShader.ContainsKey(material.shader.name))
                {
                    material.shader = m_DictShader[material.shader.name];
                }
            }
        }
    }

    //-----------------------------------------------------------------------------//

    public static bool IsReleaseAssetBundleImmediately = false; //是否立即释放AssetBundle

	public void UnloadAsset(string assetBundleName)
	{
		if (IsUseAssetBundle) {
			assetBundleName = GetFilePathByFileName(assetBundleName);
			UnloadAssetBundle(assetBundleName,true);
		} else {
			assetBundleName = GetFilePathByFileName(assetBundleName);
			if (assetBundleName.EndsWith(mAssetBundleSuffix))
			{
				assetBundleName = assetBundleName.Remove(assetBundleName.Length - mAssetBundleSuffix.Length);
			}

			//去掉后缀名
			assetBundleName = MyFileUtil.GetFileNameWithoutExtension(assetBundleName);

			string newAbName = DirNameForAssetBundlesBuildFrom + "/" + assetBundleName;
			if (mDictResources.ContainsKey (newAbName) == false) {
				if (mDictResources.ContainsKey (assetBundleName) == false) {
					ResourceUnit ru = mDictResources[assetBundleName];
					mDictResources.Remove (assetBundleName);
					foreach (UnityEngine.Object obj in ru.instanceList) {
						UnityEngine.Object.Destroy (obj);
					}
					ru.prefab = null;
					ru = null;
					Resources.UnloadUnusedAssets();
				} else {
					Debug.LogError ("Not Found Resource When UnloadAsset : " + assetBundleName);
				}
			} else {
				ResourceUnit ru = mDictResources[newAbName];
				mDictResources.Remove (newAbName);
				foreach (UnityEngine.Object obj in ru.instanceList) {
					UnityEngine.Object.Destroy (obj);
				}
				ru.prefab = null;
				ru = null;
			}
		}
	}

	public void UnloadUnusedAssets()
	{
		Resources.UnloadUnusedAssets ();
	}

	public void UnloadAssetBundle(string assetBundleName,bool isForceUnload = false)
    {
        if (mDictLoadedAssetBundle.ContainsKey(assetBundleName) == false)
        {
            return;
        }

        //卸载自身
		UnloadAssetBundleImp(assetBundleName,isForceUnload);

        //卸载依赖
        List<string> assetBundleNameList = new List<string>();
        GetUnloadDependencies(assetBundleName, ref assetBundleNameList);

        for (int i = 0; i < assetBundleNameList.Count; ++i)
        {
			UnloadAssetBundleImp(assetBundleNameList[i],isForceUnload);
        }
    }

	private void UnloadAssetBundleImp(string assetBundleName,bool isForceUnload = false)
    {
        LoadedAssetBundle bundle = mDictLoadedAssetBundle[assetBundleName];
        --bundle.referencedCount;
		if (bundle.referencedCount == 0 && (IsReleaseAssetBundleImmediately || isForceUnload))
        {
            bundle.assetBundle.Unload(false);
			mDictLoadedAssetBundle.Remove(assetBundleName);
			Resources.UnloadAsset (bundle.assetBundle);
        }
    }

    private void GetUnloadDependencies(string assetBundleName, ref List<string> assetBundleNameList)
    {
        if (mAssetBundleManifest == null)
        {
            string str = "ResourcesManager.GetUnloadDependencies: mAssetBundleManifest is null";
            Debug.LogError(str);
            return;
        }

        string[] dependencies = mAssetBundleManifest.GetAllDependencies(assetBundleName);
        if (dependencies.Length == 0)
        {
            return;
        }

        assetBundleNameList.AddRange(dependencies);

        for (int i = 0; i < dependencies.Length; ++i)
        {
            GetUnloadDependencies(dependencies[i], ref assetBundleNameList);
        }
    }

    //强制清理引用计数为0的资源
    public void ForceUnloadAssetBundle()
    {
        List<string> list = new List<string>(mDictLoadedAssetBundle.Keys);

        foreach(var assetBundleName in list)
        {
            LoadedAssetBundle bundle = mDictLoadedAssetBundle[assetBundleName];
            if (bundle.referencedCount == 0)
            {
                bundle.assetBundle.Unload(false);
                mDictLoadedAssetBundle.Remove(assetBundleName);
            }
        }
    }

    //-----------------------------------------------------------------------------//

    public void PrintReferencedCountInfo()
    {
        Debug.Log("输出引用信息:");
        foreach(var item in mDictLoadedAssetBundle)
        {
            string str = string.Format("资源名:{0} 引用数:{1}", item.Value.abName, item.Value.referencedCount);
            Debug.Log(str);
        }
    }

    //-----------------------------------------------------------------------------//
   
    Dictionary<string, byte[]> m_DictLuaScriptData = new Dictionary<string, byte[]>(); //key:Lua文件路径, value:Lua脚本内容

    public const string LuaZipFileName = "code.bytes";

    static public string LuaZipFileFullPath
    {
        get
        {
            return WorkDirInProject + LuaZipFileName;
        }
    }   //加密后的Lua文件位置

    static public string LuaDirForBuild
    {
        get
        {
            return WorkDirInProject + "Lua";
        }
    }

    private bool m_IsInitLuaZipData = false;

    //加载lua代码文件
    public void LoadLuaZipFile(string luaCodeZipFileName, Action<bool> loadFinish)
    {
        Action<byte[]> loadZip = delegate (byte[] data)
        {
            if(data == null)
            {
                string str = string.Format("ResourcesManager.InitLuaZipFile:加载Lua代码文件失败");
                Debug.LogError(str);
                loadFinish(false);
                return;
            }
            //解密
            //data = DecryptLuaCode(data);
            DecryptLuaCodeAsync(data, (decryptedData) =>
            {
                ParseZipAsync(decryptedData, (unzipedData)=>
                {
                    m_IsInitLuaZipData = true;
                    loadFinish(true);
                });
            });
        };
        StartCoroutine(LoadLuaZipFileImp(luaCodeZipFileName, loadZip));
    }

    private IEnumerator LoadLuaZipFileImp(string luaCodeZipFileName, Action<byte[]> loadFinish)
    {
        string filePath = GetFileFinalPath(luaCodeZipFileName);
        if (filePath.Contains("://"))
        {
            WWW www = new WWW(filePath);
            yield return www;
            loadFinish(www.bytes);
        }
        else
        {
            byte[] data = File.ReadAllBytes(filePath);
            loadFinish(data);
        }
    }

    private void ParseZip(byte[] zipFileData)
    {
        MemoryStream stream = new MemoryStream(zipFileData);
        stream.Seek(0, SeekOrigin.Begin);
        ICSharpCode.SharpZipLib.Zip.ZipInputStream zipStream = new ICSharpCode.SharpZipLib.Zip.ZipInputStream(stream);
       
        for(ICSharpCode.SharpZipLib.Zip.ZipEntry theEntry = zipStream.GetNextEntry(); theEntry != null; theEntry = zipStream.GetNextEntry())
        {
            if(theEntry.IsFile == false)
            {
                continue;
            }

            if(theEntry.Name.EndsWith(".meta"))
            {
                continue;
            }

            string fileName = Path.GetFileName(theEntry.Name);
            if (fileName != String.Empty)
            {
                List<byte> result = new List<byte>();
                byte[] data = new byte[2048];
                while (true)
                {
                    int size = zipStream.Read(data, 0, data.Length);
                    if (size > 0)
                    {
                        var bytes = new Byte[size];
                        Array.Copy(data, bytes, size);
                        result.AddRange(bytes);
                    }
                    else
                    {
                        break;
                    }
                }

                //文件名都转为小写
                if(m_DictLuaScriptData.ContainsKey(fileName.ToLower()))
                {
                    string str = string.Format("ResourcesManager.InitZip:Zip中文件名{0}重复", fileName);
                    Debug.LogError(str);
                    continue;
                }
                m_DictLuaScriptData.Add(theEntry.Name.ToLower(), result.ToArray());
            }
        }

        zipStream.Close();
        stream.Close();
    }

    private void ParseZipAsync(byte[] zipFileData, Action<byte[]> callback)
    {
        Loom.RunAsync(() =>
        {
            MemoryStream stream = new MemoryStream(zipFileData);
            stream.Seek(0, SeekOrigin.Begin);
            ICSharpCode.SharpZipLib.Zip.ZipInputStream zipStream = new ICSharpCode.SharpZipLib.Zip.ZipInputStream(stream);

            for (ICSharpCode.SharpZipLib.Zip.ZipEntry theEntry = zipStream.GetNextEntry(); theEntry != null; theEntry = zipStream.GetNextEntry())
            {
                if (theEntry.IsFile == false)
                {
                    continue;
                }

                if (theEntry.Name.EndsWith(".meta"))
                {
                    continue;
                }

                string fileName = Path.GetFileName(theEntry.Name);
                if (fileName != String.Empty)
                {
                    List<byte> result = new List<byte>();
                    byte[] data = new byte[2048];
                    while (true)
                    {
                        int size = zipStream.Read(data, 0, data.Length);
                        if (size > 0)
                        {
                            var bytes = new Byte[size];
                            Array.Copy(data, bytes, size);
                            result.AddRange(bytes);
                        }
                        else
                        {
                            break;
                        }
                    }

                    //文件名都转为小写
                    if (m_DictLuaScriptData.ContainsKey(fileName.ToLower()))
                    {
                        string str = string.Format("ResourcesManager.InitZip:Zip中文件名{0}重复", fileName);
                        Debug.LogError(str);
                        continue;
                    }
                    m_DictLuaScriptData.Add(theEntry.Name.ToLower(), result.ToArray());
                }
            }

            zipStream.Close();
            stream.Close();

            Loom.QueueOnMainThread(() =>
            {
                callback(zipFileData);
            });
        });
    }

    public byte[] GetLuaScriptDataFromZip(string fileName)
    {
        fileName = fileName.ToLower(); //文件名转为小写
        if (m_DictLuaScriptData.ContainsKey(fileName))
        {
            return m_DictLuaScriptData[fileName];
        }

        if (m_IsInitLuaZipData == false)
        {
            string str = string.Format("ResourcesManager.GetLuaScriptDataFromZip:读取Lua文件{0}失败，Lua代码库未初始化", fileName);
            Debug.LogError(str);
        }

        return null;
    }

    //加密Lua代码
    static public byte[] EncryptLuaCode(byte[] data)
    {
        if(SystemConfig.Instance.IsEncryptLuaCode)
        {
            return DESCrypto.Encrypt(data, MyFileUtil.EncryptKey);
        }

        return data;
    }

    static public void EncryptLuaCode(string filePath)
    {
        if (SystemConfig.Instance.IsEncryptLuaCode)
        {
            byte[] data = File.ReadAllBytes(filePath);
            data = DESCrypto.Encrypt(data, MyFileUtil.EncryptKey);
            File.WriteAllBytes(filePath, data);
        }
    }

    //解密Lua代码
    static public byte[] DecryptLuaCode(byte[] data)
    {
        if(SystemConfig.Instance.IsEncryptLuaCode)
        {
            return DESCrypto.Decrypt(data, MyFileUtil.EncryptKey);
        }

        return data;
    }

    //解密Lua代码
    static private void DecryptLuaCodeAsync(byte[] data, Action<byte[]> callback)
    {
        if (SystemConfig.Instance.IsEncryptLuaCode)
        {
            DESCrypto.DecryptAsync(data, MyFileUtil.EncryptKey, callback);
        }
        else
        {
            callback(data);
        }
    }

    //获得lua zip文件md5值
    public void GetLuaZipFileMD5(string luaCodeZipFileName, System.Action<string> getFinish)
    {
        Action<byte[]> loadZip = delegate (byte[] data)
        {
            if (data == null)
            {
                string str = string.Format("ResourcesManager.GetLuaZipFileMD5:加载Lua代码文件失败");
                Debug.LogError(str);
                getFinish("");
                return;
            }

            string md5 = MD5Tool.Get(data);
            getFinish(md5);
        };
        StartCoroutine(LoadLuaZipFileImp(luaCodeZipFileName, loadZip));
    }

    //获取默认lua zip文件md5
    public void GetMainLuaZipFileMD5(System.Action<string> getFinish)
    {
        GetLuaZipFileMD5(LuaZipFileName, getFinish);
    }

    //-----------------------------------------------------------------------------//

    #region 文件列表

    static public string FileListConfigFileName = "FileListInfo.xml";

    static private Dictionary<string, string> m_DictFileInfo = new Dictionary<string, string>();

    static private void LoadFileList(Action callback = null)
    {
#if UNITY_EDITOR
        SaveFileList();
#endif
        //加载文件列表
        MyFileUtil.ReadConfigDataAsync(FileListConfigFileName, (xmlContent) =>
        {
            //string xmlContent = MyFileUtil.ReadConfigData(FileListConfigFileName);
            Loom.RunAsync(() =>
            {
                SecurityParser securityParser = new SecurityParser();
                securityParser.LoadXml(xmlContent);
                SecurityElement xml = securityParser.ToXml();

                for (int i = 0; i < xml.Children.Count; i += 2)
                {
                    var key = xml.Children[i] as System.Security.SecurityElement;
                    var value = xml.Children[i + 1] as System.Security.SecurityElement;

                    if(m_DictFileInfo.ContainsKey(key.Text))
                    {
                        string str = string.Format("ResourcesManager.LoadFileList:资源名{0}重复", key.Text);
                        //Debug.LogError(str);
                        continue;
                    }
                    m_DictFileInfo.Add(key.Text, value.Text);
                }
                Loom.QueueOnMainThread(() =>
                {
                    if(callback != null)
                    {
                        callback();
                    }
                });
            });
        });
    }

    static public List<string> GenerateFileList(string rootDir)
    {
        List<string> dirList = new List<string>();

        //资源目录--生成此目录下的资源名列表
        //Common路径不能交换，即Common目录首先加入
        dirList.Add(rootDir + "Common");
        //TODO:
        // dirList.Add(rootDir + SDKConfig.GetCurrentVersionResPath());

        Dictionary<string, string> dictFilePath = new Dictionary<string, string>();
        foreach (string dir in dirList)
        {
            List<string> fileList = new List<string>();
            MyFileUtil.GetFileList(dir, ref fileList);

            foreach(string filePath in fileList)
            {
                string fileName = Path.GetFileName(filePath);
                if (dictFilePath.ContainsKey(fileName))
                {
                    if(fileName.EndsWith(".meta"))
                    {
                        continue;
                    }

                    //后加入的资源路径覆盖掉前面的资源路径，即版本目录下的文件覆盖掉Common目录下的同名文件
                    //string str = string.Format("ResourcesManager.GenerateFileList:资源名{0}重复，使用的资源路径{1}", fileName, filePath);
                    //Debug.Log(str);
                    dictFilePath[fileName] = filePath;
                }
                else
                {
                    dictFilePath.Add(fileName, filePath);
                }
            }
        }

        return new List<string>(dictFilePath.Values);
    }

    //removePath将要删除的路径
    static public string GetFileListXMLString(List<string> fileList, string removePath)
    {
        var root = new System.Security.SecurityElement("root");
        foreach (var item in fileList)
        {
            string fileName = Path.GetFileName(item);
            string ext = Path.GetExtension(item);
            if (string.Compare(ext, ".meta", true) == 0)
            {
                continue;
            }

            string filePath = null;
            if (IsUseAssetBundle)
            {
                filePath = item.Replace(removePath, "");
                root.AddChild(new System.Security.SecurityElement("k", fileName.ToLower()));
                root.AddChild(new System.Security.SecurityElement("v", filePath.ToLower()));
            }
            else
            {
                filePath = item.Replace(removePath, "");
                root.AddChild(new System.Security.SecurityElement("k", fileName));
                root.AddChild(new System.Security.SecurityElement("v", filePath));
            }
        }

        return root.ToString();
    }

    static public void SaveFileList()
    {
        string dir = "";
        if (IsUseAssetBundle)
        {
            dir = AssetBundlesResDirInStreamingAssetsPath;
        }
        else
        {
            dir = DirForAssetBundlesBuildFrom;
        }

        List<string> fileList = GenerateFileList(dir);
        string xml = GetFileListXMLString(fileList, dir);
        MyFileUtil.WriteConfigDataInStreamingAssets(FileListConfigFileName, xml);
    }

    static public string GetFilePathByFileName(string fileName)
    {
        string newFileName = fileName;
        if(IsUseAssetBundle)
        {
            newFileName = newFileName.ToLower();

            if (newFileName.EndsWith(mAssetBundleSuffix) == false)
            {
                newFileName = newFileName + mAssetBundleSuffix;
            }
        }

        if (m_DictFileInfo.ContainsKey(newFileName))
        {
            return m_DictFileInfo[newFileName];
        }

        return fileName;
    }

    #endregion

    //---------------------------------------加载资源优先调用--------------------------------------//

    #region 提供外部调用的接口

    Dictionary<int, string> m_DictPrefabIDABName = new Dictionary<int, string>();
    Dictionary<int, string> m_DictInstanceIDABName = new Dictionary<int, string>();

    public void Init(Action<bool> initFinishCB)
    {
#if UNITY_EDITOR
        IsFixShader = true;
#endif

        //加载文件列表
        LoadFileList(()=>
        {
            Action<bool> initAction = delegate (bool result)
            {
                LoadLuaZipFile(LuaZipFileName, initFinishCB);
            };

            if(IsLuaUseZip)
            {
                if(IsUseAssetBundle)
                {
                    LoadAssetBundleManifest(initAction);
                }
                else
                {
                    LoadLuaZipFile(LuaZipFileName, initFinishCB);
                }
            }
            else
            {
                if(IsUseAssetBundle)
                {
                    LoadAssetBundleManifest(initFinishCB);
                }
                else
                {
                    initFinishCB(true);
                }
            }
        });

    }


    public UnityEngine.Object GetPrefabSync(string abName, string assetName)
    {
        abName = GetFilePathByFileName(abName);


        if (IsUseAssetBundle)
        {
            //LoadAssetBundle(abName, assetName, loadPrefab);
            return LoadAssetBundleSync(abName, assetName);
        }
        else
        {
            if (abName.EndsWith(mAssetBundleSuffix))
            {
                abName = abName.Remove(abName.Length - mAssetBundleSuffix.Length);
            }

            //去掉后缀名
            abName = MyFileUtil.GetFileNameWithoutExtension(abName);

            string newAbName = DirNameForAssetBundlesBuildFrom + "/" + abName;
            UnityEngine.Object obj = GetResourcePrefab(newAbName);
            if (obj == null)
            {
                obj = GetResourcePrefab(abName);
            }

            return obj;
        }
    }

    public void GetPrefab(string abName, string assetName, System.Action<UnityEngine.Object> onLoad)
    {
        abName = GetFilePathByFileName(abName);


        if (IsUseAssetBundle)
        {
            //LoadAssetBundle(abName, assetName, loadPrefab);
            LoadAssetBundle(abName, assetName, onLoad);
        }
        else
        {
            if(abName.EndsWith(mAssetBundleSuffix))
            {
                abName = abName.Remove(abName.Length - mAssetBundleSuffix.Length);
            }

            //去掉后缀名
            abName = MyFileUtil.GetFileNameWithoutExtension(abName);

            string newAbName = DirNameForAssetBundlesBuildFrom + "/" + abName;
            UnityEngine.Object obj = GetResourcePrefab(newAbName);
            if(obj == null)
            {
                obj = GetResourcePrefab(abName);
            }

            onLoad(obj);
			obj = null;
        }
    }
    /// <summary>
    /// 同步获取实例
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="assetName"></param>
    /// <param name="onLoad"></param>
    public UnityEngine.Object GetInstanceSync(string abName, string assetName, System.Action<UnityEngine.Object> onLoad)
    {

        UnityEngine.Object obj = GetPrefabSync(abName, assetName);
        if (obj == null)
        {
            string str = string.Format("ResourcesManager.GetInstance:abName {0}, assetName {1} 加载失败", abName, assetName);
            Debug.LogError(str);
            return null;
        }

        obj = UnityEngine.Object.Instantiate(obj);
        m_DictInstanceIDABName.Add(obj.GetInstanceID(), abName);
        if(onLoad!= null)
        {
            onLoad(obj);
        }
        return obj;
    }


    public void GetInstance(string abName, string assetName, System.Action<UnityEngine.Object> onLoad)
    {
        System.Action<UnityEngine.Object> loadPrefab = delegate (UnityEngine.Object obj)
        {
            if (obj == null)
            {
                string str = string.Format("ResourcesManager.GetInstance:abName {0}, assetName {1} 加载失败", abName, assetName);
                Debug.LogError(str);
            }

            obj = UnityEngine.Object.Instantiate(obj);
            m_DictInstanceIDABName.Add(obj.GetInstanceID(), abName);
            onLoad(obj);
			obj = null;
        };

        GetPrefab(abName, assetName, loadPrefab);
    }

    public void ReleasePrefab(string abName)
    {
        if (IsUseAssetBundle)
        {
            abName = GetFilePathByFileName(abName);
            UnloadAssetBundle(abName);
        }
    }


    public void ReleaseInstance(UnityEngine.Object go)
    {
        if (IsUseAssetBundle)
        {
            int id = go.GetInstanceID();
            if (m_DictInstanceIDABName.ContainsKey(id))
            {
                string abName = m_DictInstanceIDABName[id];
                m_DictInstanceIDABName.Remove(id);
                ReleasePrefab(abName);
            }

            UnityEngine.Object.Destroy(go);
        }
        else
        {
            DestroyResourceInstance(go);
        }
    }

    //-----------------------------------------------------------------------------//

    public UnityEngine.Object GetUIPrefabSync(string abName, string assetName)
    {
        string newPath = abName;
        if (newPath.StartsWith("UI") == false)
        {
            newPath = "UI/" + abName;
        }

        newPath = MyFileUtil.GetFileNameWithoutExtension(newPath);

        GameObject prefab = Resources.Load<GameObject>(newPath);
        if (prefab != null)
        {
            //常用且不轻易改动的UI
            return prefab;
        }
        else
        {
            return GetPrefabSync(abName, assetName);
        }
    }

    public void GetUIPrefab(string abName, string assetName, System.Action<UnityEngine.Object> onLoad)
    {
        string newPath = abName;
        if (newPath.StartsWith("UI") == false)
        {
            newPath = "UI/" + abName;
        }

        newPath = MyFileUtil.GetFileNameWithoutExtension(newPath);

        GameObject prefab = Resources.Load<GameObject>(newPath);
        if (prefab != null)
        {
            //常用且不轻易改动的UI
            onLoad(prefab);
        }
        else
        {            
            GetPrefab(abName, assetName, onLoad);
        }
    }

    //加载UI
    public void GetUIInstance(string abName, string assetName, System.Action<UnityEngine.Object> onLoad)
    {
        System.Action<UnityEngine.Object> loadPrefab = delegate (UnityEngine.Object obj)
        {
            if (obj == null)
            {
                string str = string.Format("ResourcesManager.GetInstance:abName {0}, assetName {1} 加载失败", abName, assetName);
                Debug.LogError(str);
            }

            obj = UnityEngine.Object.Instantiate(obj);
            m_DictInstanceIDABName.Add(obj.GetInstanceID(), abName);
            onLoad(obj);
			obj = null;
        };

        GetUIPrefab(abName, assetName, loadPrefab);
    }

    //同步加载UI
    public UnityEngine.Object GetUIInstanceSync(string abName, string assetName)
    {
        UnityEngine.Object prefab = GetUIPrefabSync(abName, assetName);
        if(prefab == null)
        {
            string str = string.Format("ResourcesManager.GetInstance:abName {0}, assetName {1} 加载失败", abName, assetName);
            Debug.LogError(str);
            return null;
        }
        UnityEngine.Object obj = UnityEngine.Object.Instantiate(prefab);
        m_DictInstanceIDABName.Add(obj.GetInstanceID(), abName);
        return obj;
    }

    public void ReleaseUIPrefab(string abName)
    {
        ReleasePrefab(abName);
    }

    public void ReleaseUIInstance(UnityEngine.Object go)
    {
        ReleaseInstance(go);
    }

    #endregion
}
