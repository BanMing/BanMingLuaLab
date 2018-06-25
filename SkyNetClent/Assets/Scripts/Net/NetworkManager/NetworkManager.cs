using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace NetFramework
{
    public class NetworkManager
    {
        public static bool isOpenEncry = false; //是否开启加密
        public static bool isEncryOffsetEnable = false; //加密是否启用动态偏移
        public static int s_protoNumberLen = 2;
        public static int s_PackLen = 2;
        private SocketClient socket;
        static readonly object m_lockObject = new object();
        static Queue<KeyValuePair<int, byte[]>> mEvents = new Queue<KeyValuePair<int, byte[]>>();

        static private LuaInterface.LuaFunction DispatchSocketMsgAction = null;

        static public void SetLuaDispatchMsgAction(string functionName)
        {
            DispatchSocketMsgAction = LuaClient.GetMainState().GetFunction(functionName);
        }
        static public void DispatchSocketMsg(int bigid, int smallid, byte[] buff)
        {
            if (DispatchSocketMsgAction != null)
            {
                DispatchSocketMsgAction.Call( bigid,smallid,buff);
            }
        }

        static public void DispatchSocketMsg(int bigid, int smallid, LuaInterface.LuaByteBuffer buff)
        {
            if (DispatchSocketMsgAction != null)
            {
                DispatchSocketMsgAction.Call( bigid,smallid,buff);
            }
        }

        public NetworkManager()
        {
            SocketClient.OnRegister();
        }

        public SocketClient SocketClient
        {
            get
            {
                if (socket == null)
                    socket = new SocketClient();
                return socket;
            }
        }
        public void SetEncry(bool bencry)
        {
            isOpenEncry = bencry;
        }


        ///------------------------------------------------------------------------------------
        public static void AddEvent(int _event, byte[] data)
        {
            lock (m_lockObject)
            {
                mEvents.Enqueue(new KeyValuePair<int, byte[]>(_event, data));
            }
        }

        /// <summary>
        /// 交给Command，这里不想关心发给谁。
        /// </summary>
        public void Update()
        {
            if (mEvents.Count > 0)
            {
                while (mEvents.Count > 0)
                {
                    KeyValuePair<int, byte[]> _event = mEvents.Dequeue();
                    HandlePack(_event.Key, _event.Value);
                }
            }
        }

        public void HandlePack(int id, byte[] buff)
        {
            int bigid = ((id >> 8) & 0xff);
            int smallid = (id & 0xff);
            netpack.netcommon.netcommonHandle(bigid, smallid, buff);
        }
        /// <summary>
        /// 发送链接请求
        /// </summary>
        public void SendConnect(string host, int port)
        {
            Debug.Log("Start Connect Server ip:" + host + "port:" + port);
            CEncryptClient.instance.Reset();
            SocketClient.SendConnect(host, port);
        }

        /// <summary>
        /// 发送SOCKET消息
        /// </summary>
        public void SendMessage(int bigid, int smallid, byte[] buffer)
        {
            byte[] newbuffer = null;
            if (NetworkManager.isOpenEncry)
            {
                //加密
                byte[] encrybuffer = new byte[buffer.Length + NetworkManager.s_protoNumberLen];
                encrybuffer[0] = (byte)(bigid & 0xff); //压入大协议号
                encrybuffer[1] = (byte)(smallid & 0xff);//压入子协议号
                System.Buffer.BlockCopy(buffer, 0, encrybuffer, NetworkManager.s_protoNumberLen, buffer.Length);
                buffer = CEncryptClient.instance.SendEncrypt(encrybuffer, encrybuffer.Length, NetworkManager.isEncryOffsetEnable);
                //
                newbuffer = new byte[buffer.Length + NetworkManager.s_PackLen];
                Converter.write_size(newbuffer, buffer.Length);
                //
                System.Buffer.BlockCopy(buffer, 0, newbuffer, NetworkManager.s_PackLen, buffer.Length);
            }
            else
            {
                newbuffer = new byte[buffer.Length + NetworkManager.s_PackLen + NetworkManager.s_protoNumberLen];
                Converter.write_size(newbuffer, buffer.Length + NetworkManager.s_protoNumberLen);
                newbuffer[2] = (byte)(bigid & 0xff); //压入大协议号
                newbuffer[3] = (byte)(smallid & 0xff);
                System.Buffer.BlockCopy(buffer, 0, newbuffer, NetworkManager.s_PackLen + NetworkManager.s_protoNumberLen, buffer.Length);
            }
            SocketClient.SendMessage(newbuffer);
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        public void OnDestroy()
        {
            SocketClient.OnRemove();
            Debug.Log("~NetworkManager was destroy");
        }
    }
}