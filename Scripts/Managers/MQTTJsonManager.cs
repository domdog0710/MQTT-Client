using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MQTTJsonManager : MonoBehaviour
{
    [Space, Header("MQTT Client"), SerializeField]
    MQTTClient MQTTClient;

    [Space, Header("Setting Folder Paths"), SerializeField]
    List<string> SettingFolderPaths;
    [Space, Header("Setting Json Path"), SerializeField]
    string SettingJsonPath;

    [Space, Header("MQTT Json Default Data"), SerializeField]
    MQTTJsonDefaultData MQTTJsonDefaultData = new MQTTJsonDefaultData();

    [Space, Header("MQTT Data"), SerializeField]
    public MQTTData MQTTData = new MQTTData();

    void Awake()
    {
        string basePath;

        // 根據平台決定使用哪個資料夾路徑
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        basePath = Application.streamingAssetsPath;
#elif UNITY_ANDROID || UNITY_IOS
        basePath = Application.persistentDataPath;
#else
        basePath = Application.streamingAssetsPath; // 預設 fallback
#endif

        SettingFolderPaths.Add(basePath);
        SettingFolderPaths.Add(basePath + "/Setting Json");
        SettingFolderPaths.Add(basePath + "/Setting Json/MQTT");
        SettingJsonPath = basePath + "/Setting Json/MQTT/MQTT Data.json";

        CheckFolder();
        CheckJson();
    }

    void CheckFolder()
    {
        foreach (string folder in SettingFolderPaths)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }
    }

    void CheckJson()
    {
        if (!File.Exists(SettingJsonPath))
        {
            WriteJson(SettingJsonPath);
        }
        ReadJson(SettingJsonPath);
    }

    public void WriteJson(string settingdatapath)
    {
        MQTTData MQTTData = new MQTTData()
        {
            ServerIP = MQTTJsonDefaultData.ServerIP,
            Port = MQTTJsonDefaultData.Port,
            DeviceName = SystemInfo.deviceName,
            Topics = MQTTJsonDefaultData.Topics,
            UserName = MQTTJsonDefaultData.UserName,
            PassWord = MQTTJsonDefaultData.PassWord
        };

        string settingdata = JsonUtility.ToJson(MQTTData);

        StreamWriter file = new StreamWriter(settingdatapath);
        file.Write(settingdata);
        file.Close();
    }

    public void ReadJson(string path)
    {
        using (StreamReader streamreader = File.OpenText(path))
        {
            string settingdata = streamreader.ReadToEnd();
            streamreader.Close();

            MQTTData = JsonUtility.FromJson<MQTTData>(settingdata);
        }

        LoadData();
    }

    void LoadData()
    {
        MQTTClient.brokerAddress = MQTTData.ServerIP;
        MQTTClient.brokerPort = MQTTData.Port;

        MQTTClient.strClientID = MQTTData.DeviceName;
        MQTTClient.strTopic = MQTTData.Topics;

        MQTTClient.mqttUserName = MQTTData.UserName;
        MQTTClient.mqttPassword = MQTTData.PassWord;

        if (!MQTTData.DeviceName.Contains('_'))
        {
            MQTTClient.strDeviceID = "1";
        }
        else
        {
            string[] DeviceID = MQTTData.DeviceName.Split('_');
            MQTTClient.strDeviceID = DeviceID[1];
        }

        MQTTClient.AddMsg(0, MQTTClient.strDeviceID);
    }
}

[Serializable]
public class MQTTJsonDefaultData
{
    public string ServerIP;
    public int Port;

    public string DeviceName;
    public string Topics;

    public string UserName;
    public string PassWord;
}

[Serializable]
public class MQTTData
{
    public string ServerIP;
    public int Port;

    public string DeviceName;
    public string Topics;

    public string UserName;
    public string PassWord;
}