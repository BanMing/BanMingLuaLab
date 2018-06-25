// ZipResConfig.xml的resRequireID规则：0为不默认下载，1为默认更新下载，其他为不默认下载

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security;
using Mono.Xml;
using System.Xml;

public class AssetBundlesEditorTools : MonoBehaviour
{
    //-------------------------------------------------------------------------------------------------------------//

    [MenuItem("Build/Build AssetBundles")]
    public static void BuildAllAssetBundles()
    {
        string assetbundlePath = ResourcesManager.AssetBundlesResDirInStreamingAssetsPath;

        //先删除所有旧文件
        string rootDir = MyFileUtil.GetParentDir(assetbundlePath);
        MyFileUtil.DeleteDir(rootDir);
        MyFileUtil.CreateDir(assetbundlePath);

        //获取文件列表
        List<string> fileList = ResourcesManager.GenerateFileList(ResourcesManager.DirForAssetBundlesBuildFrom);

        //-------------------------------------------------------------//

        List<AssetBundleBuild> bundleList = new List<AssetBundleBuild>();

        for (int i = 0; i < fileList.Count; ++i)
        {
            string filePath = fileList[i];
            if (filePath.EndsWith(".meta"))
            {
                continue;
            }

            List<string> assetList = new List<string>();
            string assetPath = filePath.Substring(filePath.IndexOf("Assets"));
            assetList.Add(assetPath);

            string dirNameForAssetBundlesBuildFrom = "/" + ResourcesManager.DirNameForAssetBundlesBuildFrom + "/";
            int startPos = assetPath.IndexOf(dirNameForAssetBundlesBuildFrom) + dirNameForAssetBundlesBuildFrom.Length;
            //int endPos = assetPath.LastIndexOf(".");
            //string assetBundleName = assetPath.Substring(startPos, endPos - startPos);
            string assetBundleName = assetPath.Substring(startPos);

            AssetBundleBuild assetBundleBuild = new AssetBundleBuild();
            assetBundleBuild.assetBundleName = assetBundleName + ResourcesManager.mAssetBundleSuffix;
            //assetBundleBuild.assetBundleVariant = ResourcesManager.mAssetBundleSuffix.Replace(".", "");
            assetBundleBuild.assetNames = assetList.ToArray();

            bundleList.Add(assetBundleBuild);
        }

        BuildPipeline.BuildAssetBundles(assetbundlePath, bundleList.ToArray(), BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);

        Debug.Log("BuildAssetBundles Over");
    }

    [MenuItem("Build/Build AssetBundles By Editor Config")]
    static void BuildAllAssetBundlesByEditorConfig()
    {
        string assetbundlePath = ResourcesManager.AssetBundlesResDirInStreamingAssetsPath;
        if (Directory.Exists(assetbundlePath))
        {
            Directory.Delete(assetbundlePath, true);
        }
        Directory.CreateDirectory(assetbundlePath);

        BuildPipeline.BuildAssetBundles(assetbundlePath, 0, EditorUserBuildSettings.activeBuildTarget);
    }

    [MenuItem("Build/Print AssetBundle names")]
    static void PrintAssetBundleNames()
    {
        var names = AssetDatabase.GetAllAssetBundleNames();
        foreach (var name in names)
        {
            Debug.Log("Asset Bundle: " + name);
        }
    }

    //-------------------------------------------------------------------------------------------------------------//

    [MenuItem("Build/打包准备")]
    static void PrepareForBuild()
    {
        //配置设置提醒
        if (SystemConfig.Instance.IsEncryptConfigFile == false || SystemConfig.Instance.IsEncryptLuaCode == false || SystemConfig.Instance.IsUseLuaBytecode == false ||
            SystemConfig.Instance.IsAutoUpdate == false || SystemConfig.Instance.IsUseAssetBundle == false)
        {
            EditorUtility.DisplayDialog("提醒", "请检查SystemConfig.xml，当前配置不是最终发布版本的配置", "确定");
            return;
        }

        PrepareForBuildImp();
    }

    [MenuItem("Build/打包准备(不检查SystemConfig.xml)")]
    public static void PrepareForBuildImp()
    {
        BuildImp(true);
    }

    [MenuItem("Build/打包准备(不检查SystemConfig.xml，不编译ab资源)")]
    static void PrepareForBuildImp2()
    {
        BuildImp(false);
    }

    static void BuildImp(bool isBuildAssetBundle)
    {
        //清理缓存目录
        MyFileUtil.DeleteDir(MyFileUtil.CacheDir);
        MyFileUtil.CreateDir(MyFileUtil.CacheDir);
        //if(MyFileUtil.IsFileExsit(Application.dataPath + "/../Publish/"))
        //{

        MyFileUtil.CreateDir(Application.dataPath + "/../Publish/" +
                MyUnityEditorTool.GetPlatformFolderForAssetBundles(EditorUserBuildSettings.activeBuildTarget) + "/");
        //}
        //清理publish目录
        string publishDir = Application.dataPath + "/../Publish/" +
                MyUnityEditorTool.GetPlatformFolderForAssetBundles(EditorUserBuildSettings.activeBuildTarget) + "/";
        // MyUnityEditorTool.GetPlatformFolderForAssetBundles(EditorUserBuildSettings.activeBuildTarget) + "/" + PackageWizard.m_wizard.version + "/";
        MyFileUtil.DeleteDir(publishDir);
        MyFileUtil.CreateDir(publishDir);

        //警告————首先加密配置文件
        //加密配置文件
        EncodeConfigFile();

        //生成图集信息文件
        GenerateAtlasInfoFile();

        //加密Lua代码
        EncryptLuaCode();

        //编译资源
        if (isBuildAssetBundle)
        {
            BuildAllAssetBundles();
        }

        //拷贝工程
        CopyProject();

        //删除不需要资源
        ClearRes();

        //生成拷贝工程资源列表
        GenerateCopyProjectFileList();

        //生成资源包
        GenerateTargetProjectResZip();


#if UNITY_ANDROID
        //删除目标工程的子游戏bundle
        MyFileUtil.DeleteDir(string.Format("{0}Assets/StreamingAssets/AssetBundles/{1}/qile/games", CopyProjectTargetDir, ResourcesManager.GetPlatformDir()));
#elif UNITY_IPHONE
        var subGameParentDir = string.Format("{0}Assets/StreamingAssets/AssetBundles/{1}/qile/games", CopyProjectTargetDir, ResourcesManager.GetPlatformDir());
        var subGameDirs = new List<string>
        {
            subGameParentDir+"/commonddz",
            subGameParentDir+"/commonpdk",
            subGameParentDir+"/commonpoker",
            subGameParentDir+"/csmj",
            subGameParentDir+"/ddz",
            subGameParentDir+"/ddz3r",
            subGameParentDir+"/emmj",
            subGameParentDir+"/fpf",
            subGameParentDir+"/gymj",
            subGameParentDir+"/jsmj",
            subGameParentDir+"/lxmj",
            subGameParentDir+"/niuniu",
            subGameParentDir+"/njmj",
            subGameParentDir+"/pdk",
            subGameParentDir+"/ssz",
            subGameParentDir+"/ybmj",
            subGameParentDir+"/zzmj"
        };

        foreach (var dir in subGameDirs)
        {
            MyFileUtil.DeleteDir(dir);
        }
#endif

        AssetDatabase.Refresh();
        Debug.Log("打包准备结束");
    }

    //-------------------------------------------------------------------------------------------------------------//

    [MenuItem("Build/加密Lua代码")]
    static void EncryptLuaCode()
    {
        EncryptLuaCodeImp(LuaEncryptType.Auto);
    }

    [MenuItem("Build/加密Lua代码并复制到拷贝工程")]
    static void EncryptLuaCodeCopyToTargetDir()
    {
        EncryptLuaCodeImp(LuaEncryptType.Auto);

        string luaCodeFilePath = CopyProjectTargetDir + "Assets/StreamingAssets/" + ResourcesManager.LuaZipFileName;
        MyFileUtil.CopyFile(ResourcesManager.LuaZipFileFullPath, luaCodeFilePath);

        Debug.Log("加密Lua代码并复制到拷贝工程");
    }

    [MenuItem("Build/为Windows编辑器加密Lua代码")]
    static void EncryptLuaCodeForWindowsEditor()
    {
        EncryptLuaCodeImp(LuaEncryptType.WindowsEditor);
    }

    [MenuItem("Build/为Mac编辑器加密Lua代码")]
    static void EncryptLuaCodeForMacEditor()
    {
        EncryptLuaCodeImp(LuaEncryptType.MacEditor);
    }

    enum LuaEncryptType
    {
        Auto,
        WindowsEditor,
        MacEditor,
    }

    static void EncryptLuaCodeImp(LuaEncryptType type)
    {
        ClearCurrentProjectLuaRes();

        //编译Lua代码为bytecode
        if (SystemConfig.Instance.IsUseLuaBytecode)
        {
            List<string> luaFileList = new List<string>();
            MyFileUtil.GetFileList(ResourcesManager.LuaDirForBuild, ref luaFileList, ".lua");

            foreach (string filePath in luaFileList)
            {
                string newFilePath = filePath.Replace(".lua", MyLuaResLoader.LuaByteCodeFileSuffix);

                switch (type)
                {
                    case LuaEncryptType.Auto:
                        ///ios 为64位 
                        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
                        { newFilePath = filePath.Replace(".lua", MyLuaResLoader.X64LuaByteCodeFileSuffix); }
                        EncodeLuaFile(filePath, newFilePath, EditorUserBuildSettings.activeBuildTarget, EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS);
                        break;
                    case LuaEncryptType.WindowsEditor: EncodeLuaFileForEditor(filePath, newFilePath, RuntimePlatform.WindowsEditor); break;
                    case LuaEncryptType.MacEditor: EncodeLuaFileForEditor(filePath, newFilePath, RuntimePlatform.OSXEditor); break;
                }
            }
            AssetDatabase.Refresh();

            // if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            // {
            //     foreach (string filePath in luaFileList)
            //     {
            //         string newFilePath = filePath.Replace(".lua", MyLuaResLoader.X64LuaByteCodeFileSuffix);
            //         EncodeLuaFile(filePath, newFilePath, EditorUserBuildSettings.activeBuildTarget, true);
            //     }
            // }
        }

        //将lua代码打包为zip
        List<string> luaCodeFileList = new List<string>();
        if (SystemConfig.Instance.IsUseLuaBytecode)
        {
            MyFileUtil.GetFileList(ResourcesManager.LuaDirForBuild, ref luaCodeFileList, ".bytes");
        }
        else
        {
            MyFileUtil.GetFileList(ResourcesManager.LuaDirForBuild, ref luaCodeFileList, ".lua");
        }

        ZIPTool.CompressFiles(luaCodeFileList, MyFileUtil.GetParentDir(ResourcesManager.LuaDirForBuild), ResourcesManager.LuaZipFileFullPath, 0, true, true);

        //加密lua代码
        if (SystemConfig.Instance.IsEncryptLuaCode)
        {
            ResourcesManager.EncryptLuaCode(ResourcesManager.LuaZipFileFullPath);
        }

        AssetDatabase.Refresh();
        Debug.Log("加密Lua结束");
    }

    //编译Lua代码为bytecode ios 64位 Android 32位
    public static void EncodeLuaFile(string srcFile, string outFile, UnityEditor.BuildTarget buildTarget, bool isArch64 = false)
    {
        bool isWin = true;
        string luaexe = string.Empty;
        string args = string.Empty;
        string exedir = string.Empty;
        string currDir = Directory.GetCurrentDirectory();

        switch (buildTarget)
        {
            case BuildTarget.iOS:
                {
                    if (Application.platform == RuntimePlatform.WindowsEditor)
                    {
                        isWin = true;

                        args = "-b " + srcFile + " " + outFile;
                        // if (isArch64)
                        // {
                        luaexe = "luajit64.exe";
                        exedir = Application.dataPath.ToLower().Replace("assets", "") + "LuaEncoder/luajit_win/Luajit64/";
                        // }
                        // else
                        // {
                        //     luaexe = "luajit32.exe";
                        //     exedir = Application.dataPath.ToLower().Replace("assets", "") + "LuaEncoder/luajit_win/Luajit/";
                        // }
                    }
                    else
                    {
                        isWin = false;
                        luaexe = "./luajit";
                        exedir = Application.dataPath.ToLower().Replace("assets", "") + "LuaEncoder/luajit_ios/64/";
                        args = "-b " + srcFile + " " + outFile;
                    }
                }
                break;
            case BuildTarget.StandaloneOSXIntel:
            case BuildTarget.StandaloneOSXIntel64:
            case BuildTarget.StandaloneOSX:
                {
                    isWin = false;
                    luaexe = "./luac";
                    args = "-b " + srcFile + " " + outFile;
                    exedir = Application.dataPath.ToLower().Replace("assets", "") + "LuaEncoder/luavm/";
                }
                break;
            case BuildTarget.Android:
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                {
                    if (Application.platform == RuntimePlatform.WindowsEditor)
                    {
                        isWin = true;
                        luaexe = "luajit32.exe";
                        exedir = Application.dataPath.ToLower().Replace("assets", "") + "LuaEncoder/luajit_win/Luajit/";
                        args = "-b " + srcFile + " " + outFile;
                    }
                    else
                    {
                        isWin = false;
                        luaexe = "./luajit";
                        exedir = Application.dataPath.ToLower().Replace("assets", "") + "LuaEncoder/luajit_ios/32/";
                        args = "-b " + srcFile + " " + outFile;
                    }
                }
                break;
        }

        Directory.SetCurrentDirectory(exedir);
        System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();
        info.FileName = luaexe;
        info.Arguments = args;
        info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        info.ErrorDialog = true;
        info.UseShellExecute = isWin;

        System.Diagnostics.Process pro = System.Diagnostics.Process.Start(info);
        pro.WaitForExit();
        Directory.SetCurrentDirectory(currDir);
    }

    public static void EncodeLuaFileForEditor(string srcFile, string outFile, UnityEngine.RuntimePlatform platform)
    {
        bool isWin = true;
        string luaexe = string.Empty;
        string args = string.Empty;
        string exedir = string.Empty;
        string currDir = Directory.GetCurrentDirectory();

        if (platform == RuntimePlatform.WindowsEditor)
        {
            isWin = true;
            luaexe = "luajit.exe";
            args = "-b " + srcFile + " " + outFile;
            exedir = Application.dataPath.ToLower().Replace("assets", "") + "LuaEncoder/luajit/";
        }
        else if (platform == RuntimePlatform.OSXEditor)
        {
            isWin = false;
            luaexe = "./luac";
            //args = "-o " + outFile + " " + srcFile;
            args = "-b " + srcFile + " " + outFile;
            exedir = Application.dataPath.ToLower().Replace("assets", "") + "LuaEncoder/luavm/";
        }

        Directory.SetCurrentDirectory(exedir);
        System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();
        info.FileName = luaexe;
        info.Arguments = args;
        info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        info.ErrorDialog = true;
        info.UseShellExecute = isWin;

        System.Diagnostics.Process pro = System.Diagnostics.Process.Start(info);
        pro.WaitForExit();
        Directory.SetCurrentDirectory(currDir);
    }

    //-------------------------------------------------------------------------------------------------------------//

    //加密配置文件
    [MenuItem("Build/加密配置文件")]
    public static void EncodeConfigFile()
    {
        if (SystemConfig.Instance.IsEncryptConfigFile == false)
        {
            return;
        }

        List<string> configList = new List<string>();
        MyFileUtil.GetFileList(MyFileUtil.InnerConfigDir, ref configList, ".xml");

        foreach (string fileName in configList)
        {
            if (fileName.EndsWith(".meta") == true)
            {
                continue;
            }

            byte[] data = File.ReadAllBytes(fileName);
            data = DESCrypto.Encrypt(data, MyFileUtil.EncryptKey);
            File.WriteAllBytes(fileName + MyFileUtil.EncryptXMLFileSuffix, data);
            //MyFileUtil.DeleteFile(fileName);
        }

        AssetDatabase.Refresh();
        Debug.Log("加密配置文件结束");
    }

    private static string CopyProjectTargetDir
    {
        get
        {
            string srcDir = MyFileUtil.GetParentDir(Application.dataPath);
            string dstDir = Path.GetDirectoryName(srcDir) + "_tmp/";
            return dstDir;
        }
    }

    [MenuItem("Build/拷贝工程")]
    public static void CopyProject()
    {
        //先删除旧工程
        MyFileUtil.DeleteDir(CopyProjectTargetDir);

        string srcDir = MyFileUtil.GetParentDir(Application.dataPath);
        MyFileUtil.CopyDir(srcDir, CopyProjectTargetDir);

        AssetDatabase.Refresh();
        Debug.Log("拷贝工程结束");
    }

    [MenuItem("Build/清理多余资源")]
    public static void ClearRes()
    {
        //ClearCurrentProjectRes();
        ClearCoyProjectRes();
        Debug.Log("清理多余资源结束");
    }

    [MenuItem("Build/清理当前工程多余资源")]
    public static void ClearCurrentProjectRes()
    {
        //清理当前工程下加密的配置文件
        List<string> srcConfigFileList = new List<string>();
        MyFileUtil.GetFileList(MyFileUtil.InnerConfigDir, ref srcConfigFileList, MyFileUtil.EncryptXMLFileSuffix);

        foreach (string filePath in srcConfigFileList)
        {
            MyFileUtil.DeleteFile(filePath);
        }

        //清理当前工程下加密的lua文件
        ClearCurrentProjectLuaRes();

        AssetDatabase.Refresh();
        Debug.Log("清理当前工程多余资源");
    }

    //[MenuItem("Build/清理当前工程下加密的lua文件")]
    public static void ClearCurrentProjectLuaRes()
    {
        List<string> srcLuaFileList = new List<string>();
        MyFileUtil.GetFileList(ResourcesManager.LuaDirForBuild, ref srcLuaFileList, MyFileUtil.EncryptXMLFileSuffix);
        foreach (string filePath in srcLuaFileList)
        {
            MyFileUtil.DeleteFile(filePath);
        }

        MyFileUtil.DeleteFile(ResourcesManager.LuaZipFileFullPath);

        AssetDatabase.Refresh();
    }

    [MenuItem("Build/清理拷贝工程多余资源")]
    public static void ClearCoyProjectRes()
    {
        string targetDir = CopyProjectTargetDir;

        //清理目标工程下未加密的配置文件
        List<string> configFileList = new List<string>();

        MyFileUtil.GetFileList(targetDir + "Assets/StreamingAssets/Config", ref configFileList, ".xml");
        foreach (string filePath in configFileList)
        {
            MyFileUtil.DeleteFile(filePath);
        }

        //清理sdk配置文件
        // string localVersionInfoConfigFilePath = targetDir + "Assets/StreamingAssets/Config/" + LocalVersionInfoConfig.fileName + MyFileUtil.EncryptXMLFileSuffix;
        // MyFileUtil.DeleteFile(localVersionInfoConfigFilePath);

        //清理目标工程下未加密的lua文件
        string luaDir = targetDir + "Assets/StreamingAssets/Lua";
        MyFileUtil.DeleteDir(luaDir);

        //清理lua项目配置文件
        List<string> luaProjectFiles = new List<string>();
        string streamingAssetsDir = targetDir + "Assets/StreamingAssets";
        DirectoryInfo di = new DirectoryInfo(streamingAssetsDir);
        var fs = di.GetFiles();
        foreach (var item in fs)
        {
            if (item.FullName.EndsWith(".user"))
            {
                MyFileUtil.DeleteFile(item.FullName);
            }

            if (item.FullName.EndsWith(".luaproj"))
            {
                MyFileUtil.DeleteFile(item.FullName);
            }

            if (item.FullName.EndsWith(".config"))
            {
                MyFileUtil.DeleteFile(item.FullName);
            }
        }

        //清理Resources目录下的文件
        string ResourcesDir = targetDir + "Assets/Resources/" + ResourcesManager.DirNameForAssetBundlesBuildFrom;
        MyFileUtil.DeleteDir(ResourcesDir);


        Debug.Log("清理拷贝工程多余资源");
    }

    //-------------------------------------------------------------------------------------------------------------//

    //[MenuItem("Build/生成更新包")]
    static public void VersionsDifferenceZipImp()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        string oldVersionDir = args[1];
        string newVersionDir = args[2];

        //         string oldVersionDir = @"D:/Projects/SVN/QiPai/trunk/MahJongLua/Publish/2016.10.17";
        //         string newVersionDir = @"D:/Projects/SVN/QiPai/trunk/MahJongLua/Publish/2016.10.18";
        VersionsDifferenceZipImp(oldVersionDir, newVersionDir);
    }

    static void VersionsDifferenceZipImp(string oldVersionDir, string newVersionDir)
    {
        oldVersionDir = MyFileUtil.DealFilePathSlash(oldVersionDir);
        newVersionDir = MyFileUtil.DealFilePathSlash(newVersionDir);

        List<string> oldFileList = new List<string>();
        List<string> newFileList = new List<string>();

        MyFileUtil.GetFileList(oldVersionDir, ref oldFileList);
        MyFileUtil.GetFileList(newVersionDir, ref newFileList);

        List<string> modificationFileList = new List<string>();
        foreach (string newFilePath in newFileList)
        {
            int index = oldFileList.IndexOf(newFilePath);
            if (index < 0)
            {
                //添加新的文件
                modificationFileList.Add(newFilePath);
            }
            else
            {
                string oldFilePath = oldFileList[index];
                if (MD5Tool.VerifyFile(newFilePath, oldFilePath) == false)
                {
                    //文件改变
                    modificationFileList.Add(newFilePath);
                }
            }
        }

        //string time = System.DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss");

        string modificationDir = MyFileUtil.GetParentDir(newVersionDir) + "update_tmp";
        MyFileUtil.DeleteDir(modificationDir);
        MyFileUtil.CreateDir(modificationDir);

        foreach (string filePath in modificationFileList)
        {
            string newFilePath = filePath.Replace(newVersionDir, modificationDir);
            MyFileUtil.CopyFile(filePath, newFilePath);
        }

        string zipFilePath = modificationDir + ".zip";
        ZIPTool.CompressDirectory(modificationDir, zipFilePath, 0, false);

        Debug.Log("");
    }

    //-------------------------------------------------------------------------------------------------------------//

    [MenuItem("Build/生成图集资源列表文件")]
    static void GenerateAtlasInfoFile()
    {
        // UIAtlasTool.SaveConfig();
        Debug.Log("生成图集资源列表文件");
    }

    [MenuItem("Build/生成当前工程资源列表")]
    static void GenerateCurrentProjectFileList()
    {
        ResourcesManager.SaveFileList();

        Debug.Log("生成当前工程资源列表结束");
    }

    [MenuItem("Build/生成拷贝工程资源列表")]
    static void GenerateCopyProjectFileList()
    {
        string dir = CopyProjectTargetDir + "Assets/StreamingAssets/AssetBundles/" + ResourcesManager.GetPlatformDir() + "/";
        List<string> fileList = ResourcesManager.GenerateFileList(dir);
        string xml = ResourcesManager.GetFileListXMLString(fileList, dir);

        string filePath = CopyProjectTargetDir + "Assets/StreamingAssets/Config/" + ResourcesManager.FileListConfigFileName + MyFileUtil.EncryptXMLFileSuffix;
        File.WriteAllText(filePath, xml);
        DESCrypto.Encrypt(filePath, MyFileUtil.EncryptKey);

        Debug.Log("生成拷贝工程资源列表结束");
    }

    //-------------------------------------------------------------------------------------------------------------//

    [MenuItem("Build/生成当前工程资源包(ZIP)")]
    static void GenerateCurrentProjectResZip()
    {
        string srcDir = MyFileUtil.GetParentDir(Application.dataPath);
        GenerateResZipImp(srcDir);
    }

    [MenuItem("Build/生成拷贝工程资源包(ZIP)")]
    static void GenerateTargetProjectResZip()
    {
        GenerateResZipImp(CopyProjectTargetDir);
    }

    static void GenerateResZipImp(string rootDir)
    {
        string dirRemove = rootDir + "Assets/StreamingAssets";

        string publishDir = rootDir + "Publish/" +
            MyUnityEditorTool.GetPlatformFolderForAssetBundles(EditorUserBuildSettings.activeBuildTarget)
            + "/";
        // if (PackageWizard.m_wizard)
        // {
        //     publishDir += PackageWizard.m_wizard.version;
        // }

        MyFileUtil.DeleteDir(publishDir);
        MyFileUtil.CreateDir(publishDir);

        List<ResInfo> listResInfo = new List<ResInfo>();

        //----------------------------------------------------//

        List<ZipResConfig> listZipConfig = ZipResConfig.GetConfigList();
        foreach (ZipResConfig zipConfig in listZipConfig)
        {
            //生成Zip资源包
            List<string> fileList = new List<string>();

            if (string.IsNullOrEmpty(zipConfig.resDir) == false)
            {
                string fileDir = rootDir + zipConfig.resDir;

                if (fileDir.Contains("@Platform"))
                {
                    fileDir = fileDir.Replace("@Platform", ResourcesManager.GetPlatformDir());
                }

                if (fileDir.Contains("@Version"))
                {
                    // fileDir = fileDir.Replace("@Version", SDKConfig.GetCurrentVersionResPath().ToLower());
                    fileDir = fileDir.Replace("@Version", "test");
                }

                if (Directory.Exists(fileDir))
                {
                    List<string> ignoreFileTypeList = new List<string>();
                    ignoreFileTypeList.Add(".meta");
                    MyFileUtil.GetFileList(fileDir, ref fileList, zipConfig.listResSuffix, ignoreFileTypeList);
                }
            }

            //特殊文件
            foreach (string specialResPath in zipConfig.listSpecialResPath)
            {
                string newFilePath = rootDir + specialResPath;
                if (File.Exists(newFilePath))
                {
                    fileList.Add(newFilePath);
                }
            }

            if (fileList.Count == 0)
            {
                continue;
            }

            //zip中不记录文件的时间
            string zipResFilePath;
            // if (PackageWizard.m_wizard)
            // {
            //     zipResFilePath = rootDir + "Publish/" +
            //     MyUnityEditorTool.GetPlatformFolderForAssetBundles(EditorUserBuildSettings.activeBuildTarget)
            //     + "/" + PackageWizard.m_wizard.version + "/" + zipConfig.resZipName;
            // }
            // else
            // {
            zipResFilePath = rootDir + "Publish/" +
            MyUnityEditorTool.GetPlatformFolderForAssetBundles(EditorUserBuildSettings.activeBuildTarget)
            + "/" + zipConfig.resZipName;
            // }
            ZIPTool.CompressFiles(fileList, dirRemove, zipResFilePath, 0, false, true);

            //资源信息
            ResInfo resInfo = new ResInfo();
            resInfo.resName = zipConfig.resZipName;
            resInfo.resMD5 = MD5Tool.GetByFilePath(zipResFilePath);
            resInfo.resSize = (int)MyFileUtil.GetFileSize(zipResFilePath);
            resInfo.resRequireID = zipConfig.resRequireID;
            // if (PackageWizard.m_wizard)
            // {
            //     resInfo.resURL = PackageWizard.m_wizard.serverAddress + resInfo.resName;
            // }
            // else
            // {
            resInfo.resURL = resInfo.resName;
            // }

            listResInfo.Add(resInfo);
        }

        //----------------------------------------------------//
        //写到发布目录
        string xmlContent = VersionInfo.SerializeInEditor(listResInfo);
        string xmlFilePath;
        // if (PackageWizard.m_wizard)
        // {
        //     xmlFilePath = rootDir + "Publish/" +
        //     MyUnityEditorTool.GetPlatformFolderForAssetBundles(EditorUserBuildSettings.activeBuildTarget)
        //     + "/" + PackageWizard.m_wizard.version + "/VersionInfo.xml";
        // }
        // else
        // {
        xmlFilePath = rootDir + "Publish/" +
        MyUnityEditorTool.GetPlatformFolderForAssetBundles(EditorUserBuildSettings.activeBuildTarget) + "/VersionInfo.xml";
        // }

        File.WriteAllText(xmlFilePath, xmlContent);

        listResInfo.RemoveAll(r => r.resRequireID > 0);
        xmlContent = VersionInfo.SerializeInEditor(listResInfo);
        //写到工程目录
        string xmlFileInProjectPath = rootDir + "Assets/StreamingAssets/Config/" + VersionManager.VersionInfoFilePath + MyFileUtil.EncryptXMLFileSuffix;
        byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes(xmlContent);
        data = MyFileUtil.EncryptData(data);
        File.WriteAllBytes(xmlFileInProjectPath, data);
        Debug.Log("生成资源包结束");
    }

    //-------------------------------------------------------------------------------------------------------------//

    [MenuItem("Build/解密选中文件")]
    static void DecryptFile()
    {
        foreach (UnityEngine.Object obj in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
            if (string.IsNullOrEmpty(path))
            {
                continue;
            }

            string filePath = Application.dataPath.Replace("Assets", "") + path;
            DESCrypto.Decrypt(filePath, MyFileUtil.EncryptKey);
        }

        AssetDatabase.Refresh();
        Debug.Log("文件解密结束");
    }

    [MenuItem("Build/加密选中文件")]
    static void EncryptFile()
    {
        foreach (UnityEngine.Object obj in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
            if (string.IsNullOrEmpty(path))
            {
                continue;
            }

            string filePath = Application.dataPath.Replace("Assets", "") + path;
            DESCrypto.Encrypt(filePath, MyFileUtil.EncryptKey);
        }

        AssetDatabase.Refresh();
        Debug.Log("文件加密结束");
    }

    //-------------------------------------------------------------------------------------------------------------//

    [MenuItem("Build/清理编辑器内存")]
    static void ClearEditorCacheMemory()
    {
        ScriptThread.Start(ClearEditorCacheMemoryImp());
    }

    static IEnumerator ClearEditorCacheMemoryImp()
    {
        string filePath = Application.dataPath + "/TmpMyTest.cs";
        string str = "class TmpMyTest{}";
        System.IO.File.WriteAllText(filePath, str);
        AssetDatabase.Refresh();
        System.IO.File.Delete(filePath);
        AssetDatabase.Refresh();

        while (EditorApplication.isCompiling)
        {
            yield return null;
        }
    }
}

class ZipResConfig
{
    public string resDir = null;

    public List<string> listResSuffix = new List<string>();

    public string resZipName = null;

    public List<string> listSpecialResPath = new List<string>();

    public int resRequireID = 0;

    //-------------------------------------------------------------------------------------------------------------//

    private static List<ZipResConfig> m_ListConfig = null;

    static private void ParseConfig()
    {
        m_ListConfig = new List<ZipResConfig>();

        string xmlContent = MyFileUtil.ReadConfigDataInStreamingAssets("ZipResConfig.xml");

        XmlDocument doc = new XmlDocument();
        doc.LoadXml(xmlContent);

        var root = doc.DocumentElement;
        var recordList = root.GetElementsByTagName("Record");

        foreach (var recordNode in recordList)
        {
            ZipResConfig config = new ZipResConfig();

            XmlElement recordElem = ((XmlElement)recordNode);
            foreach (var resNode in recordElem.ChildNodes)
            {
                XmlElement resElem = ((XmlElement)resNode);
                string nodeName = resElem.Name.ToLower();
                switch (nodeName)
                {
                    case "resdir": config.resDir = resElem.InnerText; break;
                    case "ressuffix":
                        {
                            string[] suffixArray = resElem.InnerText.Split(',');
                            foreach (string suffix in suffixArray)
                            {
                                config.listResSuffix.Add(suffix.Trim());
                            }
                        }
                        break;
                    case "specialrespath": config.listSpecialResPath.Add(resElem.InnerText); break;
                    case "reszipname": config.resZipName = resElem.InnerText; break;
                    case "resrequireid":
                        config.resRequireID = int.Parse(resElem.InnerText);
                        break;
                }
            }

            m_ListConfig.Add(config);
        }
    }

    static public List<ZipResConfig> GetConfigList()
    {
        ParseConfig();
        return m_ListConfig;
    }
}