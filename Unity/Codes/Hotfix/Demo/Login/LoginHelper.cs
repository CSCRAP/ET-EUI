using System;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace ET
{
    [FriendClass(typeof(AccountInfoComponent))]
    public static class LoginHelper
    {
        public static async ETTask<int> Login(Scene zoneScene, string address, string account, string password)
        {
            A2C_LoginAccount a2CLoginAccount = null;
            Session accountSession = null;
            
            try
            {
                accountSession = zoneScene.GetComponent<NetKcpComponent>().Create(NetworkHelper.ToIPEndPoint(address));

               password = MD5Helper.StringMD5(password);

               a2CLoginAccount = (A2C_LoginAccount) await accountSession.Call(new C2A_LoginAccount() { AccountName = account, Password = password });
            }
            catch (Exception e)
            {
                accountSession?.Dispose();
                Log.Error(e.ToString());
                return ErrorCode.ERR_NetWorkError;
            }


            if (a2CLoginAccount.Error != ErrorCode.ERR_Success)
            {
                accountSession?.Dispose();
                return a2CLoginAccount.Error;
            }


            //保存session
            zoneScene.AddComponent<SessionComponent>().Session = accountSession;
            //PingComponent组件每个一段时间向客户端发送一条消息，保证链接（与服务器的SessionIdleCheckerComponent相配合）
            zoneScene.GetComponent<SessionComponent>().Session.AddComponent<PingComponent>();

            //保存链接token和accountId
            zoneScene.GetComponent<AccountInfoComponent>().Token = a2CLoginAccount.Token;
            zoneScene.GetComponent<AccountInfoComponent>().AccountId = a2CLoginAccount.AccountId;

            

     
            
            return ErrorCode.ERR_Success;

        } 
    }
}