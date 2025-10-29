using M2MqttUnity;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using uPLibrary.Networking.M2Mqtt.Messages;

public class MQTTClient : M2MqttUnityClient
{
    [Space]
    [Header("Device ID")]
    [SerializeField]
    public string strDeviceID = "";

    [Space]
    [Header("Topic")]
    [SerializeField]
    public string strTopic = "";

    [Space]
    [Header("Message")]
    [SerializeField]
    public string strMsg = "";

    [Space]
    [Header("Song Audio Source")]
    [SerializeField]
    List<string> eventMessages = new List<string>();

    [Space]
    [Header("Publish Msg Unity Event")]
    [SerializeField]
    UnityEvent PublishMsgUnityEvent;

    [Space]
    [Header("Receive Msg Unity Events")]
    [SerializeField]
    List<UnityEvent> ReceiveMsgUnityEvents;

    [Space]
    [Header("Ping Timer")]
    [SerializeField]
    float fPingTimer = 1000f;

    [Space]
    [Header("Ping Timer Count")]
    [SerializeField]
    float fPingTimerCount = 1000f;

    public void AddMsg(int imsgid, string strmsg)
    {
        string[] msgs = strMsg.Split('/');

        if (msgs.Length == imsgid + 1) 
        {
            strMsg = "";

            msgs[imsgid] = strmsg;

            foreach (var msg in msgs)
            {
                strMsg += msg;
            }
        }
        else
        {
            strMsg += strmsg;
        }
    }

    public void SendMsg()
    {
        PublishMsg(strMsg);
    }

    public void PublishMsg(string strmsg)
    {
        if (client == null || !client.IsConnected)
        {
            Connect();
        }
        else
        {
            client.Publish(strTopic, System.Text.Encoding.UTF8.GetBytes(strmsg), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
            //Debug.Log(strmsg);
            PublishMsgUnityEvent.Invoke();
        }
    }

    public void PublishImage(byte[] imagebytes)
    {
        if (client == null || !client.IsConnected)
        {
            Connect();
        }
        else
        {
            client.Publish(strTopic, imagebytes, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
            //Debug.Log(strmsg);
            PublishMsgUnityEvent.Invoke();
        }
    }

    public void SetEncrypted(bool isEncrypted)
    {
        this.isEncrypted = isEncrypted;
    }

    protected override void OnConnecting()
    {
        base.OnConnecting();
    }

    protected override void OnConnected()
    {
        SubscribeTopics();
        base.OnConnected();
    }

    protected override void SubscribeTopics()
    {
        client.Subscribe(new string[] { strTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
    }

    protected override void UnsubscribeTopics()
    {
        client.Unsubscribe(new string[] { strTopic });
    }

    protected override void OnConnectionFailed(string errorMessage)
    {
        Debug.Log("CONNECTION FAILED! " + errorMessage);
    }

    protected override void OnDisconnected()
    {
        Debug.Log("Disconnected.");
    }

    protected override void OnConnectionLost()
    {
        Debug.Log("CONNECTION LOST!");
    }

    protected override void Start()
    {
        Debug.Log("Ready.");
        Application.runInBackground = true;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        base.Start();
    }

    protected override void DecodeMessage(string topic, byte[] message)
    {
        string msg = System.Text.Encoding.UTF8.GetString(message);
        //Debug.Log("Received: " + msg);
        StoreMessage(msg);
    }

    private void StoreMessage(string eventMsg)
    {
        eventMessages.Add(eventMsg);
    }

    private void ProcessMessage(string msg)
    {
        //Debug.Log("Received: " + msg);
        if (msg.Contains("Stop"))
        {
            ReceiveMsgUnityEvents[0].Invoke();
        }
        else if (msg.Contains("Celebrate"))
        {
            ReceiveMsgUnityEvents[1].Invoke();
        }
        else if (msg.Contains("End"))
        {
            ReceiveMsgUnityEvents[2].Invoke();
        }
    }

    protected override void Update()
    {
        base.Update(); // call ProcessMqttEvents()

        fPingTimerCount += Time.deltaTime;
        if (fPingTimerCount >= fPingTimer)
        {
            fPingTimerCount = 0f;
            if (client != null && client.IsConnected)
            {
                client.Publish(strTopic, Encoding.UTF8.GetBytes("Ping"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
                Debug.Log("MQTT Ping sent.");
            }
            else
            {
                Connect();
            }
        }

        if (eventMessages.Count > 0)
        {
            foreach (string msg in eventMessages)
            {
                ProcessMessage(msg);
            }
            eventMessages.Clear();
        }
    }

    private void OnDestroy()
    {
        Disconnect();
    }
}