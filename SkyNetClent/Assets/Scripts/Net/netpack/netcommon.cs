using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
//using System.Threading.Tasks;

namespace netpack
{
    class netcommon
    {

        public static void netcommonHandle(int bigid, int smallid, byte[] buff)
        {
            if (bigid == 0) //
            {
                if (smallid == NetFramework.Protocal.Connect) //客户端返回连接成功
                {
                    Debug.Log("NetFramework.Protocal.Connect connnet ok");
                }
                else if (smallid == NetFramework.Protocal.Disconnect) //客户端返回关断
                {
                    Debug.Log("NetFramework.Protocal.Connect Disconnect");
                }
                else if (smallid == NetFramework.Protocal.Exception) //异常掉线
                {
                    Debug.Log("NetFramework.Protocal.Connect Exception");
                }
                else if (smallid == NetFramework.Protocal.ConnectFail) //连接失败
                {
                    Debug.Log("NetFramework.Protocal.Connect ConnectFail");
                }
            }
            else 
            {
                UnityEngine.Debug.Log(string.Format("netcommonHandle: {0} {1}", bigid, smallid));
                //分发到lua层
                LuaInterface.LuaByteBuffer luaByteBuffer =new LuaInterface.LuaByteBuffer(buff);
                NetFramework.NetworkManager.DispatchSocketMsg(bigid, smallid, luaByteBuffer);
            }

        }
    }
}
