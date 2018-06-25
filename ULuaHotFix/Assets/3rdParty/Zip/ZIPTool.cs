using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;
using zlib;
using LuaInterface;
using System.Text;

public static class ZIPTool
{
    public static void CompressDirectory(string sourcePath, string outputFilePath)
    {
        FileStream compressed = new FileStream(outputFilePath, FileMode.OpenOrCreate);
        compressed.CompressDirectory(sourcePath, 0);
    }

    public static void CompressDirectory(string sourcePath, string outputFilePath, int zipLevel, bool isIncludeFirstDir = true, bool isResetTime = false)
    {
        FileStream compressed = new FileStream(outputFilePath, FileMode.OpenOrCreate);
        compressed.CompressDirectory(sourcePath, zipLevel, isIncludeFirstDir, isResetTime);
    }

    //isIncludeFirstDir:是否包含顶级目录,如第一级目录d:/test/, true则包含test，false则不包含
    public static void CompressDirectory(this Stream target, string sourcePath, int zipLevel, bool isIncludeFirstDir = true, bool isResetTime = false)
    {
        sourcePath = Path.GetFullPath(sourcePath);

        int trimOffset = 0;
        if (isIncludeFirstDir)
        {
            string parentDirectory = Path.GetDirectoryName(sourcePath);
            trimOffset = (string.IsNullOrEmpty(parentDirectory) ? Path.GetPathRoot(parentDirectory).Length : parentDirectory.Length);
        }
        else
        {
            trimOffset = (string.IsNullOrEmpty(sourcePath) ? Path.GetPathRoot(sourcePath).Length : sourcePath.Length);
        }
         
        List<string> fileSystemEntries = new List<string>();

        fileSystemEntries
            .AddRange(Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories)
                          .Select(d => d + "\\"));

        fileSystemEntries
            .AddRange(Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories));


        using (ZipOutputStream compressor = new ZipOutputStream(target))
        {
            compressor.SetLevel(zipLevel);

            foreach (string filePath in fileSystemEntries)
            {
                var trimFile = filePath.Substring(trimOffset);
                var file = trimFile.StartsWith(@"\") ? trimFile.ReplaceFirst(@"\", "") : trimFile;
                file = file.Replace(@"\", "/");

                ZipEntry entry = new ZipEntry(file);
                if (isResetTime)
                {
                    entry.DateTime = new DateTime();
                    entry.DosTime = 0;
                }
                compressor.PutNextEntry(entry);

                if (filePath.EndsWith(@"\"))
                {
                    continue;
                }

                byte[] data = new byte[2048];

                using (FileStream input = File.OpenRead(filePath))
                {
                    int bytesRead;

                    while ((bytesRead = input.Read(data, 0, data.Length)) > 0)
                    {
                        compressor.Write(data, 0, bytesRead);
                    }
                }
            }

            compressor.Finish();
        }
    }

    //fileList：需要打包的文件列表， removePath：将要打包的那些文件列表 中将要删除的文件路径
    public static void CompressFiles(List<string> fileList, string removePath, string zipFilePath, int zipLevel, bool isIncludeFirstDir = true, bool isResetTime = false)
    {
        FileStream compressed = new FileStream(zipFilePath, FileMode.OpenOrCreate);

        int trimOffset = 0;
        if (isIncludeFirstDir)
        {
            string parentDirectory = Path.GetDirectoryName(removePath);
            trimOffset = (string.IsNullOrEmpty(parentDirectory) ? Path.GetPathRoot(parentDirectory).Length : parentDirectory.Length);
        }
        else
        {
            trimOffset = (string.IsNullOrEmpty(removePath) ? Path.GetPathRoot(removePath).Length : removePath.Length);
        }

        using (ZipOutputStream compressor = new ZipOutputStream(compressed))
        {
            compressor.SetLevel(zipLevel);

            foreach (string filePath in fileList)
            {
                var trimFile = filePath.Substring(trimOffset);
                var file = trimFile.StartsWith(@"\") ? trimFile.ReplaceFirst(@"\", "") : trimFile;
                file = file.Replace(@"\", "/");

                ZipEntry entry = new ZipEntry(file);
                if(isResetTime)
                {
                    entry.DateTime = new DateTime();
                    entry.DosTime = 0;
                }
                compressor.PutNextEntry(entry);

                if (filePath.EndsWith(@"\"))
                {
                    continue;
                }

                byte[] data = new byte[2048];

                using (FileStream input = File.OpenRead(filePath))
                {
                    int bytesRead;

                    while ((bytesRead = input.Read(data, 0, data.Length)) > 0)
                    {
                        compressor.Write(data, 0, bytesRead);
                    }
                }
            }

            compressor.Finish();
        }
    }

    public static void DecompressToDirectory(string targetPath, string zipFilePath)
    {
        if (File.Exists(zipFilePath))
        {
            var compressed = File.OpenRead(zipFilePath);
            compressed.DecompressToDirectory(targetPath);
        }
        else
        {
            Debug.LogError("ZIPTool.DecompressToDirectory :Zip文件不存在" + zipFilePath);
        }
    }

    public static void DecompressToDirectory(byte[] zipMemory, string targetPath)
    {
        MemoryStream stream = new MemoryStream(zipMemory);
        stream.Seek(0, SeekOrigin.Begin);
        DecompressToDirectory(stream, targetPath);
    }

    public static void DecompressToDirectory(this Stream source, string targetPath)
    {
        targetPath = Path.GetFullPath(targetPath);
        using (ZipInputStream decompressor = new ZipInputStream(source))
        {
            ZipEntry entry;

            while ((entry = decompressor.GetNextEntry()) != null)
            {
                string name = entry.Name;
                if (entry.IsDirectory && entry.Name.StartsWith("\\"))
                    name = entry.Name.ReplaceFirst("\\", "");

                string filePath = Path.Combine(targetPath, name);
                string directoryPath = Path.GetDirectoryName(filePath);

                if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                if (entry.IsDirectory)
                    continue;

                byte[] data = new byte[2048];
                using (FileStream streamWriter = File.Create(filePath))
                {
                    int bytesRead;
                    while ((bytesRead = decompressor.Read(data, 0, data.Length)) > 0)
                    {
                        streamWriter.Write(data, 0, bytesRead);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Create a zip archive.
    /// </summary>
    /// <param name="filename">The filename.</param>
    /// <param name="directory">The directory to zip.</param> 
    public static void PackFiles(string filename, string directory, string fileFilter)
    {
        try
        {
            FastZip fz = new FastZip();
            fz.CreateEmptyDirectories = true;
            fz.CreateZip(filename, directory, false, fileFilter);
            fz = null;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogException(ex);
        }
    }

    public static byte[] UnpackMemory(byte[] zipMemory)
    {
        MemoryStream stream = new MemoryStream(zipMemory);
        stream.Seek(0, SeekOrigin.Begin);
        ZipInputStream s = new ZipInputStream(stream);

        ZipEntry theEntry;
        List<byte> result = new List<byte>();
        while ((theEntry = s.GetNextEntry()) != null)
        {
            string fileName = Path.GetFileName(theEntry.Name);

            if (fileName != String.Empty)
            {
                int size = 2048;
                byte[] data = new byte[2048];
                while (true)
                {
                    size = s.Read(data, 0, data.Length);
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
            }
        }
        s.Close();
        stream.Close();
        return result.ToArray();
    }
    
    public static void ZLib_CopyStream(Stream input, Stream output)
    {
        byte[] buffer = new byte[2000];
        int len;
        while ((len = input.Read(buffer, 0, 2000)) > 0)
        {
            output.Write(buffer, 0, len);
        }
        output.Flush();
    }

    public static byte[] ZLib_Decompress(byte[] inBuffer)
    {
        var inStream = new MemoryStream(inBuffer);
        var outStream = new MemoryStream();
        ZOutputStream zOutStream = new ZOutputStream(outStream);
       
        try
        {
            ZLib_CopyStream(inStream, zOutStream);

            return outStream.ToArray();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        finally
        {
            inStream.Close();
            zOutStream.Close();
            outStream.Close();
        }
        return null;
    }

    public static byte[] ZLib_Compress(byte[] inBuffer)
    {
        var inStream = new MemoryStream(inBuffer);
        var outStream = new MemoryStream();
        ZOutputStream zOutStream = new ZOutputStream(outStream, zlibConst.Z_DEFAULT_COMPRESSION);

        try
        {
            ZLib_CopyStream(inStream, zOutStream);

            return outStream.ToArray();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        finally
        {
            inStream.Close();
            zOutStream.Close();
            outStream.Close();
        }
        return null;
    }

    /// <summary>
    /// Unpacks the files.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <returns>if succeed return true,otherwise false.</returns>
    public static bool UnpackFiles(string file, string dir)
    {
        try
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            ZipInputStream s = new ZipInputStream(File.OpenRead(file));

            ZipEntry theEntry;
            while ((theEntry = s.GetNextEntry()) != null)
            {

                string directoryName = Path.GetDirectoryName(theEntry.Name);
                string fileName = Path.GetFileName(theEntry.Name);

                if (directoryName != String.Empty)
                    Directory.CreateDirectory(dir + directoryName);

                if (fileName != String.Empty)
                {
                    FileStream streamWriter = File.Create(dir + theEntry.Name);

                    int size = 2048;
                    byte[] data = new byte[2048];
                    while (true)
                    {
                        size = s.Read(data, 0, data.Length);
                        if (size > 0)
                        {
                            streamWriter.Write(data, 0, size);
                        }
                        else
                        {
                            break;
                        }
                    }

                    streamWriter.Close();
                }
            }
            s.Close();
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            return false;
        }
    }

}
