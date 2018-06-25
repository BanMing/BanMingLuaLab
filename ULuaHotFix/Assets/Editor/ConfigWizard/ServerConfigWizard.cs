using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Security.Cryptography;
using System.Xml;

public class ServerConfigWizard : ScriptableWizard{
    public static ServerConfigWizard m_wizard;

    public string CfgMapURL;
    public string Version;

    [MenuItem("Config/ServerConfig")]
    static void CreateWindow()
    {
        m_wizard = ScriptableWizard.DisplayWizard<ServerConfigWizard>("ServerConfig", "Apply");

        m_wizard.CfgMapURL = ServerConfig.Instance.CfgMapURL;
        m_wizard.Version = ServerConfig.Instance.Version;
        

    }

    void OnWizardUpdate()
    {
    }

    void OnWizardCreate()
    {
        XmlDocument xmlDoc = new XmlDocument();

        //root
        XmlElement root = xmlDoc.CreateElement("root");
        xmlDoc.AppendChild(root);

        //record
        XmlElement record = xmlDoc.CreateElement("record");
        root.AppendChild(record);


        //content
        XmlElement id = xmlDoc.CreateElement("id");
        id.InnerText = "0";
        record.AppendChild(id);

        
        XmlElement CfgMapURL = xmlDoc.CreateElement("CfgMapURL");
        CfgMapURL.InnerText = m_wizard.CfgMapURL.ToString();
        record.AppendChild(CfgMapURL);
        ServerConfig.Instance.CfgMapURL = m_wizard.CfgMapURL;

        XmlElement Version = xmlDoc.CreateElement("Version");
        Version.InnerText = m_wizard.Version.ToString();
        record.AppendChild(Version);
        ServerConfig.Instance.Version = m_wizard.Version;

        string outputPath = MyFileUtil.InnerConfigDir + ServerConfig.fileName;
        xmlDoc.Save(outputPath);
        Debug.Log("ServerConfig修改完成");
    }
}
