
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 文件基本数据
/// </summary>
public class FileUnit
{
    public string name;
    public string fullPath;
    public string relativePath;
}

public class MyFileUtil 
{
    private static string mCacheDir = null;
    public static string CacheDir
    {
        get
        {
            if(mCacheDir == null)
            {
#if UNITY_EDITOR || UNITY_STANDALONE
                mCacheDir = GetParentDir(Application.dataPath);
#else
                mCacheDir = Application.persistentDataPath + "/";
#endif
                mCacheDir = mCacheDir + "Cache/";
                CreateDir(mCacheDir);
            }

            return mCacheDir;
        }
    }

    //SD卡目录
    private static string mSDCardDir = null;
    public static string SDCardDir
    {
        get
        {
            if (mSDCardDir == null)
            {
                 
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                mSDCardDir = GetParentDir(Application.dataPath) + "SDCard/";

#elif UNITY_ANDROID
                AndroidJavaClass environment = new AndroidJavaClass("android.os.Environment");
                AndroidJavaObject file = environment.CallStatic<AndroidJavaObject>("getExternalStorageDirectory");
                mSDCardDir = file.Call<string>("getAbsolutePath") + "/Mate/";
#else
                mSDCardDir =  CacheDir;
#endif

                CreateDir(mSDCardDir);
            }

            return mSDCardDir;
        }
    }

    public static byte[] EncryptKey = new byte[] { 242, 254, 133, 145, 50, 212, 53, 46 };   //Lua代码加密解密Key

    public static string EncryptXMLFileSuffix = ".bytes";   //加密后的xml文件后缀

    /***************************************************************************************/

    public static byte[] ReadFileBytes(string filePath, bool isAbsolutePath = true)
    {
        if (isAbsolutePath == false)
        {
            filePath = CacheDir + filePath;
        }

        if (File.Exists(filePath))
        {
            return File.ReadAllBytes(filePath);
        }

        return null;
    }

    public static string ReadFileText(string filePath, bool isAbsolutePath = true)
    {
        if (isAbsolutePath == false)
        {
            filePath = CacheDir + filePath;
        }

        if (File.Exists(filePath))
        {
            return File.ReadAllText(filePath);
        }

        return null;
    }

    /***************************************************************************************/

    //写文件，isAbsolutePath: false文件名是相对路径, true 传入的是绝对路径
    public static void WriteFile(string filePath, byte[] data, bool isAbsolutePath = true)
    {
        try
        {
            if (isAbsolutePath == false)
            {
                filePath = CacheDir + filePath;
            }

            if (!File.Exists(filePath))
            {
                string dir = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }

            File.WriteAllBytes(filePath, data);
        }
        catch (System.Exception ex)
        {
            string str = string.Format("MyFileUtil.WriteFileBytes: Write File {0:s} Error, {1:s}", filePath, ex.Message);
            Debug.LogError(str);
        }
    }

    //写文件，isAbsolutePath: false文件名是相对路径, true 传入的是绝对路径
    public static void WriteFile(string filePath, string data, bool isAbsolutePath = true)
    {
        try
        {
            if (isAbsolutePath == false)
            {
                filePath = CacheDir + filePath;
            }

            if (!File.Exists(filePath))
            {
                string dir = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }

            File.WriteAllText(filePath, data);
        }
        catch (System.Exception ex)
        {
            string str = string.Format("MyFileUtil.WriteFileText: Write File {0:s} Error, {1:s}", filePath, ex.Message);
            Debug.LogError(str);
        }
    }

    #region config
    /***************************************************************************************/
    //配置表目录
    static public string InnerConfigDir
    {
        get
        {
            return Application.streamingAssetsPath + "/Config/";
        }
    }

    static public string OuterConfigDir
    {
        get
        {
            return CacheDir + "Config/";
        }
    }

    //读取配置文件数据
    public static string ReadConfigData(string filePath)
    {
        string str = ReadConfigDataInCacheDir(filePath);
        if(string.IsNullOrEmpty(str) == false)
        {
            return str;
        }
        return ReadConfigDataInStreamingAssets(filePath);
    }
    //异步读取配置文件数据
    public static void ReadConfigDataAsync(string filePath, Action<string> callback)
    {
        ReadConfigDataInCacheDirAsync(filePath, (str) =>
        {
            if (string.IsNullOrEmpty(str) == false)
            {
                callback(str);
                return;
            }
            ReadConfigDataInStreamingAssetsAsync(filePath, callback);
        });
    }

    public static string ReadConfigDataInCacheDir(string filePath)
    {
        string newPath = OuterConfigDir + filePath;

        if (SystemConfig.Instance.IsEncryptConfigFile && newPath.EndsWith(EncryptXMLFileSuffix) == false)
        {
            newPath = newPath + EncryptXMLFileSuffix;
        }

        if (File.Exists(newPath))
        {
            byte[] data = File.ReadAllBytes(newPath);

            //解密
            if(SystemConfig.Instance.IsEncryptConfigFile)
            {
                data = DESCrypto.Decrypt(data, MyFileUtil.EncryptKey);
            }
            
            return System.Text.UTF8Encoding.UTF8.GetString(data);
        }

        return null;
    }


    public static void ReadConfigDataInCacheDirAsync(string filePath, Action<string> callback)
    {
        string newPath = OuterConfigDir + filePath;

        if (SystemConfig.Instance.IsEncryptConfigFile && newPath.EndsWith(EncryptXMLFileSuffix) == false)
        {
            newPath = newPath + EncryptXMLFileSuffix;
        }

        if (File.Exists(newPath))
        {
            byte[] data = File.ReadAllBytes(newPath);

            //解密
            if (SystemConfig.Instance.IsEncryptConfigFile)
            {
                DESCrypto.DecryptAsync(data, MyFileUtil.EncryptKey, (decryptedData)=>
                {
                    string result = System.Text.UTF8Encoding.UTF8.GetString(decryptedData);
                    callback(result);
                });
            }
            else
            {
                string result = System.Text.UTF8Encoding.UTF8.GetString(data);
                callback(result);
            }
        }
        else
        {
            callback(null);
        }
    }

    public static string ReadNotEncryptionConfigDataInStreamingAssets(string filePath)
    {
        byte[] data = ReadConfigDataInStreamingAssetsImp(filePath);
        return System.Text.UTF8Encoding.UTF8.GetString(data);
    }

    public static string ReadConfigDataInStreamingAssets(string filePath)
    {
        if (SystemConfig.Instance.IsEncryptConfigFile && filePath.EndsWith(EncryptXMLFileSuffix) == false)
        {
            filePath = filePath + EncryptXMLFileSuffix;
        }

        byte[] data = ReadConfigDataInStreamingAssetsImp(filePath);
        //解密
        if (SystemConfig.Instance.IsEncryptConfigFile)
        {
            data = DESCrypto.Decrypt(data, MyFileUtil.EncryptKey);
        }
        return System.Text.UTF8Encoding.UTF8.GetString(data);
    }

    public static void ReadConfigDataInStreamingAssetsAsync(string filePath, Action<string> callback)
    {
        if (SystemConfig.Instance.IsEncryptConfigFile && filePath.EndsWith(EncryptXMLFileSuffix) == false)
        {
            filePath = filePath + EncryptXMLFileSuffix;
        }

        byte[] data = ReadConfigDataInStreamingAssetsImp(filePath);
        //解密
        if (SystemConfig.Instance.IsEncryptConfigFile)
        {
            DESCrypto.DecryptAsync(data, MyFileUtil.EncryptKey, (decryptedData) => {
                string result = System.Text.UTF8Encoding.UTF8.GetString(decryptedData);
                callback(result);
            });
        }
        else
        {
            string result = System.Text.UTF8Encoding.UTF8.GetString(data);
            if(callback != null)
            {
                callback(result);
            }
        }
    }


    public static byte[] ReadConfigDataInStreamingAssetsImp(string filePath)
    {
        filePath = InnerConfigDir + filePath;

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
            else
            {
                string strError = string.Format("MyFileUtil.ReadDataInStreamingAssets:{0}", www.error);
                Debug.LogError(strError);
                return null;
            }
        }
        else
        {
            if (File.Exists(filePath))
            {
                return File.ReadAllBytes(filePath);
            }
        }

        string str = string.Format("MyFileUtil.ReadDataInStreamingAssets:读取文件{0}文件失败", filePath);
        Debug.LogError(str);
        return null;
    }

    //存储配置文件数据
    public static void WriteConfigDataInStreamingAssets(string filePath, string text)
    {
        filePath = InnerConfigDir + filePath;
        string dir = GetParentDir(filePath);
        if (Directory.Exists(dir) == false)
        {
            Directory.CreateDirectory(dir);
        }

        //加密配置文件
        if(SystemConfig.Instance.IsEncryptConfigFile)
        {
            if (filePath.EndsWith(EncryptXMLFileSuffix) == false)
            {
                filePath = filePath + EncryptXMLFileSuffix;
            }

            byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes(text);
            data = DESCrypto.Encrypt(data, EncryptKey);
            File.WriteAllBytes(filePath, data);
        }
        else
        {
            File.WriteAllText(filePath, text);
        }
    }

    public static void WriteUnEncryptConfigDataInStreamingAssets(string filePath, string text)
    {
        filePath = InnerConfigDir + filePath;
        string dir = GetParentDir(filePath);
        if (Directory.Exists(dir) == false)
        {
            Directory.CreateDirectory(dir);
        }

        File.WriteAllText(filePath, text);
    }

    //存储配置文件数据
    public static void WriteConfigDataInCacheDir(string filePath, string text)
    {
        filePath = OuterConfigDir + filePath;
        string dir = GetParentDir(filePath);
        if (Directory.Exists(dir) == false)
        {
            Directory.CreateDirectory(dir);
        }

        //加密
        if (SystemConfig.Instance.IsEncryptConfigFile)
        {
            if (filePath.EndsWith(EncryptXMLFileSuffix) == false)
            {
                filePath = filePath + EncryptXMLFileSuffix;
            }

            byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes(text);
            data = DESCrypto.Encrypt(data, EncryptKey);
            File.WriteAllBytes(filePath, data);
        }
        else
        {
            File.WriteAllText(filePath, text);
        }
    }
    /***************************************************************************************/
    #endregion


    #region streamingAssets/Cache目录文件操作
    /***************************************************************************************/
    //配置表目录
    static public string InnerDir
    {
        get
        {
            return Application.streamingAssetsPath;
        }
    }

    static public string OuterDir
    {
        get
        {
            return CacheDir;
        }
    }

    //读取配置文件数据
    public static string ReadData(string filePath)
    {
        string str = ReadDataInCacheDir(filePath);
        if (string.IsNullOrEmpty(str) == false)
        {
            return str;
        }
        return ReadDataInStreamingAssets(filePath);
    }

    public static string ReadDataInCacheDir(string filePath)
    {
        string newPath = OuterDir + "/" + filePath;

        if (SystemConfig.Instance.IsEncryptConfigFile && newPath.EndsWith(EncryptXMLFileSuffix) == false)
        {
            newPath = newPath + EncryptXMLFileSuffix;
        }

        if (File.Exists(newPath))
        {
            byte[] data = File.ReadAllBytes(newPath);

            //解密
            if (SystemConfig.Instance.IsEncryptConfigFile)
            {
                data = DESCrypto.Decrypt(data, MyFileUtil.EncryptKey);
            }

            return System.Text.UTF8Encoding.UTF8.GetString(data);
        }

        return null;
    }

    public static string ReadNotEncryptionDataInStreamingAssets(string filePath)
    {
        byte[] data = ReadDataInStreamingAssetsImp(filePath);
        return System.Text.UTF8Encoding.UTF8.GetString(data);
    }

    public static string ReadDataInStreamingAssets(string filePath)
    {
        if (SystemConfig.Instance.IsEncryptConfigFile && filePath.EndsWith(EncryptXMLFileSuffix) == false)
        {
            filePath = filePath + EncryptXMLFileSuffix;
        }

        byte[] data = ReadDataInStreamingAssetsImp(filePath);
        //解密
        if (SystemConfig.Instance.IsEncryptConfigFile)
        {
            data = DESCrypto.Decrypt(data, MyFileUtil.EncryptKey);
        }
        return System.Text.UTF8Encoding.UTF8.GetString(data);
    }

    public static byte[] ReadDataInStreamingAssetsImp(string filePath)
    {
        filePath = InnerDir + "/" + filePath;

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
            else
            {
                string strError = string.Format("MyFileUtil.ReadDataInStreamingAssets:{0}", www.error);
                Debug.LogError(strError);
                return null;
            }
        }
        else
        {
            if (File.Exists(filePath))
            {
                return File.ReadAllBytes(filePath);
            }
        }

        string str = string.Format("MyFileUtil.ReadDataInStreamingAssets:读取文件{0}文件失败", filePath);
        Debug.LogError(str);
        return null;
    }

    //存储配置文件数据
    public static void WriteDataInStreamingAssets(string filePath, string text)
    {
        filePath = InnerDir + filePath;
        string dir = GetParentDir(filePath);
        if (Directory.Exists(dir) == false)
        {
            Directory.CreateDirectory(dir);
        }

        //加密配置文件
        if (SystemConfig.Instance.IsEncryptConfigFile)
        {
            if (filePath.EndsWith(EncryptXMLFileSuffix) == false)
            {
                filePath = filePath + EncryptXMLFileSuffix;
            }

            byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes(text);
            data = DESCrypto.Encrypt(data, EncryptKey);
            File.WriteAllBytes(filePath, data);
        }
        else
        {
            File.WriteAllText(filePath, text);
        }
    }

    public static void WriteUnEncryptDataInStreamingAssets(string filePath, string text)
    {
        filePath = InnerDir + filePath;
        string dir = GetParentDir(filePath);
        if (Directory.Exists(dir) == false)
        {
            Directory.CreateDirectory(dir);
        }

        File.WriteAllText(filePath, text);
    }

    //存储配置文件数据
    public static void WriteDataInCacheDir(string filePath, string text)
    {
        filePath = OuterDir + "/" + filePath;
        string dir = GetParentDir(filePath);
        if (Directory.Exists(dir) == false)
        {
            Directory.CreateDirectory(dir);
        }

        //加密
        if (SystemConfig.Instance.IsEncryptConfigFile)
        {
            if (filePath.EndsWith(EncryptXMLFileSuffix) == false)
            {
                filePath = filePath + EncryptXMLFileSuffix;
            }

            byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes(text);
            data = DESCrypto.Encrypt(data, EncryptKey);
            File.WriteAllBytes(filePath, data);
        }
        else
        {
            File.WriteAllText(filePath, text);
        }
    }
    /***************************************************************************************/
    #endregion

    public static byte[] EncryptData(byte[] data)
    {
        data = DESCrypto.Encrypt(data, EncryptKey);
        return data;
    }

    public static byte[] DecryptData(byte[] data)
    {
        data = DESCrypto.Decrypt(data, EncryptKey);
        return data;
    }

    /***************************************************************************************/

    //文件拷贝,将文件拷贝到指定路径
    public static void CopyFile(string srcPath, string targetPath)
    {
        try
        {
            string dir = Path.GetDirectoryName(targetPath);
            if (Directory.Exists(dir) == false)
            {
                Directory.CreateDirectory(dir);
            }

            File.Copy(srcPath, targetPath, true);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("MyFileUtil.CopyFile:" + ex.Message);
        }
    }

    static private void CopyFile(List<string> fileList, string srcDir, string destDir)
    {
        srcDir = MyFileUtil.DealFilePathSlash(srcDir);
        destDir = MyFileUtil.DealFilePathSlash(destDir);

        if (srcDir.EndsWith("/") == false)
        {
            srcDir = srcDir + "/";
        }

        if (destDir.EndsWith("/") == false)
        {
            destDir = destDir + "/";
        }

        foreach (string filePath in fileList)
        {
            try
            {
                string newFilePath = destDir + filePath.Replace(srcDir, "");
                string dir = Path.GetDirectoryName(newFilePath);
                if (Directory.Exists(dir) == false)
                {
                    Directory.CreateDirectory(dir);
                }
                File.Copy(filePath, newFilePath, true);
            }
            catch(System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }

    //复制文件并重命名
    public static void CopyFileAndRename(string srcPath, string targetPath, string newName)
    {
        try
        {
            CopyFile(srcPath, targetPath);
            string dir = Path.GetDirectoryName(targetPath);
            newName = dir + "/" + newName;

            File.Move(targetPath, newName);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("CommandBuild.CopyFileAndRename:" + ex.Message);
        }
    }

    public static void CopyDir(string srcDir, string destDir)
    {
       try
        {
            srcDir = Path.GetFullPath(srcDir);
            destDir = Path.GetFullPath(destDir);
            List<string> fileList = new List<string>();
            GetFileList(new DirectoryInfo(srcDir), ref fileList, null, null);
            CopyFile(fileList, srcDir, destDir);
        }
        catch(System.Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    /***************************************************************************************/

    static public void GetFileList(string dir, ref List<string> fileList, string fileType = null, string ignoreFileType = null)
    {
        DirectoryInfo di = new DirectoryInfo(dir);

        List<string> fileTypes = new List<string>();
        if(string.IsNullOrEmpty(fileType) == false)
        {
            fileTypes.Add(fileType);
        }

        List<string> ignoreFileTypeList = new List<string>();
        if (string.IsNullOrEmpty(ignoreFileType) == false)
        {
            ignoreFileTypeList.Add(ignoreFileType);
        }

        GetFileList(di, ref fileList, fileTypes, ignoreFileTypeList);
    }

    static public void GetFileList(string dir, ref List<string> fileList, string[] fileTypeList, string[] ignoreFileTypeList = null)
    {
        DirectoryInfo di = new DirectoryInfo(dir);

        if(ignoreFileTypeList == null)
        {
            GetFileList(di, ref fileList, new List<string>(fileTypeList), null);
        }
        else
        {
            GetFileList(di, ref fileList, new List<string>(fileTypeList), new List<string>(ignoreFileTypeList));
        }
    }

    static public void GetFileList(string dir, ref List<string> fileList, List<string> fileTypeList, List<string> ignoreFileTypeList = null)
    {
        DirectoryInfo di = new DirectoryInfo(dir);
        GetFileList(di, ref fileList, fileTypeList, ignoreFileTypeList);
    }

    static public void GetFileList(DirectoryInfo info, ref List<string> fileList, List<string> fileTypeList, List<string> ignoreFileTypeList)
    {
        if(info == null)
        {
            return;
        }

        if(info.Exists == false)
        {
            string str = string.Format("MyFileUtil.GetFileList:获取文件列表失败，目录{0}不存在", info.FullName);
            Debug.LogError(str);
            return;
        }

        var ds = info.GetDirectories().Where(t => t.Name.StartsWith(".") == false);
        var fs = info.GetFiles();

        foreach (var item in fs)
        {
            string filePath = item.FullName.Replace("\\", "/");

            //过滤掉需要忽略的文件列表
            if(ignoreFileTypeList != null && ignoreFileTypeList.Count != 0)
            {
                foreach(string fileType in ignoreFileTypeList)
                {
                    if(string.IsNullOrEmpty(fileType) == false)
                    {
                        if (filePath.EndsWith(fileType))
                        {
                            continue;
                        }
                    }
                }
            }

            if(fileTypeList == null || fileTypeList.Count == 0)
            {
                fileList.Add(filePath);
            }
            else
            {
                foreach (string fileType in fileTypeList)
                {
                    if (string.IsNullOrEmpty(fileType) == false)
                    {
                        if (filePath.EndsWith(fileType))
                        {
                            fileList.Add(filePath);
                        }
                    }
                    else
                    {
                        fileList.Add(filePath);
                    }
                }
            }
        }

        foreach (var item in ds)
        {
            GetFileList(item, ref fileList, fileTypeList, ignoreFileTypeList);
        }
    }

    /***************************************************************************************/

    //获取文件列表，目录带有通配符，通配符为*
    static public void GetFileListWithWildcard(string dir, ref List<string> fileList, List<string> fileTypeList, List<string> ignoreFileTypeList)
    {
        int index = dir.IndexOf("*");
        if (index < 0)
        {
            GetFileList(dir, ref fileList, fileTypeList, ignoreFileTypeList);
            return;
        }

        dir = DealFilePathSlash(dir);
        string pathPrefix = dir.Substring(0, index);    //路径前缀
        string dirPrefix = pathPrefix;
        string pathSuffix = dir.Substring(index + 1);   //路径后缀

        if (dirPrefix.EndsWith("/") == false)
        {
            dirPrefix = GetParentDir(dirPrefix);
        }
        
        if (Directory.Exists(dirPrefix))
        {
            List<string> tmpFileList = new List<string>();
            GetFileList(dirPrefix, ref tmpFileList, fileTypeList, ignoreFileTypeList);

            if (string.IsNullOrEmpty(pathSuffix))
            {
                fileList = tmpFileList;
            }
            else
            {
                foreach (string filePath in tmpFileList)
                {
                    if (filePath.Contains(pathPrefix) && filePath.Contains(pathSuffix))
                    {
                        fileList.Add(filePath);
                    }
                }
            }
        }
        else
        {
            string str = string.Format("MyFileUtil.GetFileListWithWildcard:通配符路径{0}异常", dir);
            Debug.LogError(str);
        }
    }

    /***************************************************************************************/

    //获取路径
    public static void GetRelativeFileList(string fullDirPath, ref List<FileUnit> fileList, string ignoreFileType)
    {
        GetRelativeFileList(fullDirPath, "", ref fileList, ignoreFileType);
    }

    //获取指定目录下的文件，忽略某种类别的文件
    public static void GetRelativeFileList(string fullDirPath, string relativePath, ref List<FileUnit> fileList, string ignoreFileType)
    {
        GetRelativeFileList(fullDirPath, relativePath, ref fileList, FileSearchMode.Ignore, ignoreFileType);
    }

    //获取指定目录下的指定类别的文件
    public static void GetRelativeFileListWithSpecialFileType(string fullDirPath, string relativePath, ref List<FileUnit> fileList, string fileType)
    {
        GetRelativeFileList(fullDirPath, relativePath, ref fileList, FileSearchMode.Special, fileType);
    }

    public enum FileSearchMode
    {
        Ignore, //忽略模式，
        Special,//查找指定类型的文件
    }

    /// <summary>
    /// 获取指定路径的文件信息,
    /// 
    /// 假设有如下目录和文件
    /// C;/MyTest/Liange/MyTest.txt
    /// 
    /// 现在获取C:/MyTest目录下的文件，
    /// 如果相对路径(relativePath)为空("")，则MyTest.txt获取出来的相对路径就是Liange/MyTest.txt
    /// 如果相对路径(relativePath)为"MyDir"，则MyTest.txt获取出来的相对路径就是MyDir/Liange/MyTest.txt
    /// 
    /// </summary>
    /// <param name="fullDirPath">需要获取文件列表的目录路径</param>
    /// <param name="relativePath">获取的文件相对路径</param>
    /// <param name="fileList"></param>
    /// <param name="mode"></param>
    /// <param name="fileType">需要处理的文件类别，Ignore:忽略这种文件类型，Special:只查找这种文件类型</param>
    public static void GetRelativeFileList(string fullDirPath, string relativePath, ref List<FileUnit> fileList, FileSearchMode mode, string fileType)
    {
        try
        {
            if (string.IsNullOrEmpty(fileType) == false)
            {
                if (fileType[0] != '.')
                {
                    fileType = '.' + fileType;
                }
            }

            DirectoryInfo currentDir = new DirectoryInfo(fullDirPath);
            FileInfo[] tmpFileList = currentDir.GetFiles();
            foreach (FileInfo fi in tmpFileList)
            {
                switch (mode)
                {
                    case FileSearchMode.Ignore:
                        {
                            if (fi.Extension == fileType)
                            {
                                continue;
                            }
                        }
                        break;
                    case FileSearchMode.Special:
                        {
                            if (fi.Extension != fileType)
                            {
                                continue;
                            }
                        }
                        break;
                }

                FileUnit unit = new FileUnit();
                unit.name = fi.Name;
                unit.fullPath = fi.FullName.Replace('\\', '/');
                if (string.IsNullOrEmpty(relativePath))
                {
                    unit.relativePath = fi.Name;
                }
                else
                {
                    unit.relativePath = relativePath + "/" + fi.Name;
                }

                fileList.Add(unit);
            }

            DirectoryInfo[] dirList = currentDir.GetDirectories();
            foreach (DirectoryInfo di in dirList)
            {
                string newRelativePath = null;
                if (string.IsNullOrEmpty(relativePath))
                {
                    newRelativePath = di.Name;
                }
                else
                {
                    newRelativePath = relativePath + "/" + di.Name;
                }

                GetRelativeFileList(di.FullName, newRelativePath, ref fileList, mode, fileType);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("MyFileUtil.GetRelativeFileList:" + ex.Message);
        }
    }

    /***************************************************************************************/

    public static void CreateDir(string dir)
    {
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }

    public static void DeleteDir(string dir)
    {
        if(Directory.Exists(dir))
        {
            Directory.Delete(dir, true);
        }
    }

    public static string GetParentDir(string path)
    {
        path = path.Replace("\\", "/");
        if (path.EndsWith("/"))
        {
            path = path.Remove(path.Length - 1);
        }

        int index = path.LastIndexOf("/");
        if (index > 0)
        {
            string dir = path.Substring(0, index) + "/";
            return dir;
        }

        return null;
    }

    //获取指定文件的绝对路径
    public static string GetFullPath(string fileName)
    {
        string newFilePath = CacheDir + fileName;
        if (!File.Exists(newFilePath))
        {
            string dir = Path.GetDirectoryName(newFilePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        return newFilePath;
    }

    /***************************************************************************************/

    public static void DeleteFile(string filePath)
    {
        if(File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    /***************************************************************************************/

    //处理分隔符
    public static string DealFilePathSlash(string filePath)
    {
        return filePath.Replace("\\", "/");
    }

    public static string GetFileNameWithoutExtension(string filePath)
    {
        return GetParentDir(filePath) + Path.GetFileNameWithoutExtension(filePath);
    }

    /***************************************************************************************/

    public static long GetFileSize(string filePath)
    {
        FileInfo fileInfo = new FileInfo(filePath);
        return fileInfo.Length;
    }

    /***************************************************************************************/

    public static bool IsFileExsit(string filePath, bool isAbsolutePath = false)
    {
        string newFilePath = null;
        if(isAbsolutePath == false)
        {
            newFilePath = CacheDir + filePath;
        }
        else
        {
            newFilePath = filePath;
        }

        return File.Exists(newFilePath); 
    }

    /*****************************************************************************/
    //从xml字符串中按key获取值(xml字符串,key)
    public static string GetValueFromXmlString(string xmlstr, string key)
    {
        try
        {
            XElement xe = XElement.Parse(xmlstr);
            return ReadFromXml(xe, key);
        }
        catch (Exception e)
        {
            return null;
        }
    }

    public static string ReadFromXml(XElement xe, string key)
    {
        string result = null;
        try
        {
            IEnumerable<string> results = (from ele in xe.Elements()
                                           where ele.Name.ToString() == key
                                           select ele.Value);
            if (results.ToArray().Length > 0)
                result = results.ToArray()[0];
            else
                result = null;
            if (result == null)
            {
                foreach (XElement ele in xe.Elements())
                {
                    if (ele.HasElements)
                    {
                        string tryResult = ReadFromXml(ele, key);
                        if (tryResult != null)
                        {
                            result = tryResult;
                            break;
                        }
                    }
                }
            }
            return result;
        }
        catch (Exception e)
        {
            Debug.LogError("wrong in get value from xml: " + e.ToString());
            result = null;
            return result;
        }
    }
}
