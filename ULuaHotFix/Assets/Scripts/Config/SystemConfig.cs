
using UnityEngine;
using System.Collections;

public class SystemConfig : GameData<SystemConfig>
{
    static public readonly string fileName = "SystemConfig.xml";

    public bool IsUseAssetBundle { set; get; }
    public bool IsLuaUseZip { set; get; }
    public bool IsEncryptLuaCode { set; get; }
    public bool IsUseLuaBytecode { set; get; }
    public bool IsAutoUpdate { set; get; }
    public bool IsEncryptConfigFile { set; get; }


    private static bool IsInit = false;

    public static SystemConfig Instance
    {
        get
        {
            if(IsInit == false)
            {
                byte[] data = MyFileUtil.ReadConfigDataInStreamingAssetsImp(fileName);
                if(data == null)
                {
                    //解密
                    data = MyFileUtil.ReadConfigDataInStreamingAssetsImp(fileName + MyFileUtil.EncryptXMLFileSuffix);
                    data = DESCrypto.Decrypt(data, MyFileUtil.EncryptKey);
                }

                string str = System.Text.UTF8Encoding.UTF8.GetString(data);
                SystemConfig.LoadFromText(str, fileName);

                IsInit = true;
            }

            return dataMap[0];
        }
    }
}
