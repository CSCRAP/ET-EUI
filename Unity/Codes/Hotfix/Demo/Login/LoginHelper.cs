using System;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace ET
{
    [FriendClass(typeof(AccountInfoComponent))]
    [FriendClass(typeof(ServerInfosComponent))]
    [FriendClass(typeof(RoleInfoComponent))]
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
        
        
        
        //获取服务器的方法
        public static async ETTask<int> GetServerInfos(Scene zoneScene)
        {
            A2C_GetServerInfos a2CGetServerInfos = null;

            try
            {
                a2CGetServerInfos = (A2C_GetServerInfos)  await zoneScene.GetComponent<SessionComponent>().Session.Call(new C2A_GetServerInfos()
                {
                    Token = zoneScene.GetComponent<AccountInfoComponent>().Token,
                    AccountId = zoneScene.GetComponent<AccountInfoComponent>().AccountId,

                });
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }



            if (a2CGetServerInfos.Error != ErrorCode.ERR_Success)
            {
                Log.Error(a2CGetServerInfos.Error.ToString());
                return a2CGetServerInfos.Error;
                
            }


            foreach (var serverInfosProto in a2CGetServerInfos.ServerInfosList)
            {
               ServerInfo serverInfo = zoneScene.GetComponent<ServerInfosComponent>().AddChild<ServerInfo>();
               serverInfo.FromMessage(serverInfosProto);
               zoneScene.GetComponent<ServerInfosComponent>().Add(serverInfo);
            }
            
            await ETTask.CompletedTask;

            return ErrorCode.ERR_Success;
        }

        
        
        //创建角色的方法
        public static async ETTask<int> CreateRole(Scene zoneScene, string Name)
        {
            A2C_CreateRole a2CCreateRole = null;
            try
            {
             a2CCreateRole =  (A2C_CreateRole) await zoneScene.GetComponent<SessionComponent>().Session.Call(new C2A_CreateRole()
                {
                    AccountId = zoneScene.GetComponent<AccountInfoComponent>().AccountId,
                    Token = zoneScene.GetComponent<AccountInfoComponent>().Token,
                    Name = Name,
                    ServerId = zoneScene.GetComponent<ServerInfosComponent>().CurrentServerId,
                });

            }
            catch (Exception e)
            {
             Log.Error(e.ToString());
             return ErrorCode.ERR_NetWorkError;
            }


            if (a2CCreateRole.Error != ErrorCode.ERR_Success)
            {
                Log.Error(a2CCreateRole.Error.ToString());
                return a2CCreateRole.Error;
            }

            RoleInfo newRoleInfo = zoneScene.GetComponent<RoleInfoComponent>().AddChild<RoleInfo>();
            newRoleInfo.FromMessage(a2CCreateRole.RoleInfo);
            
            zoneScene.GetComponent<RoleInfoComponent>().RoleInfos.Add(newRoleInfo);
            
            return ErrorCode.ERR_Success;
        }
        
        
        
        //删除角色
        public static async ETTask<int> DeleteRole(Scene zoneScene)
        {
            A2C_DeleteRole a2CDeleteRole = null;

            try
            {
             a2CDeleteRole = (A2C_DeleteRole) await zoneScene.GetComponent<SessionComponent>().Session.Call(new C2A_DeleteRole()
                {
                    Token = zoneScene.GetComponent<AccountInfoComponent>().Token,
                    AccountId = zoneScene.GetComponent<AccountInfoComponent>().AccountId,
                    RoleInfoId = zoneScene.GetComponent<RoleInfoComponent>().CurrentRoleId,
                    ServerId = zoneScene.GetComponent<ServerInfosComponent>().CurrentServerId
                });
                     

            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                return ErrorCode.ERR_NetWorkError;
            }

            if (a2CDeleteRole.Error != ErrorCode.ERR_Success)
            {
                Log.Error(a2CDeleteRole.Error.ToString());
                return a2CDeleteRole.Error;
            }

            int index = zoneScene.GetComponent<RoleInfoComponent>().RoleInfos.FindIndex(((info) => { return info.Id == a2CDeleteRole.DeletedRoleInfoId; }));
            zoneScene.GetComponent<RoleInfoComponent>().RoleInfos.RemoveAt(index);
            
            return ErrorCode.ERR_Success;
        }
        
        //获取角色信息
        public static async ETTask<int> GetRoles(Scene zoneScene)
        {

            A2C_GetRoles a2CGetRoles = null;

            try
            {
             a2CGetRoles = (A2C_GetRoles)await zoneScene.GetComponent<SessionComponent>().Session.Call(new C2A_GetRoles()
                {
                    AccountId = zoneScene.GetComponent<AccountInfoComponent>().AccountId,
                    ServerId = zoneScene.GetComponent<ServerInfosComponent>().CurrentServerId,
                    Token = zoneScene.GetComponent<AccountInfoComponent>().Token,
                });

            }
            catch (Exception e)
            {
              Log.Error(e.ToString());
              return ErrorCode.ERR_NetWorkError;
            }
            
            

            if (a2CGetRoles.Error != ErrorCode.ERR_Success)
            {
                Log.Error(a2CGetRoles.Error.ToString());
                return a2CGetRoles.Error;
            }
            
            zoneScene.GetComponent<RoleInfoComponent>().RoleInfos.Clear();
            
            foreach (var roleInfoProto in a2CGetRoles.RoleInfos)
            {
              var  roleInfo = zoneScene.GetComponent<RoleInfoComponent>().AddChild<RoleInfo>();
              roleInfo.FromMessage(roleInfoProto);
              zoneScene.GetComponent<RoleInfoComponent>().RoleInfos.Add(roleInfo);
            }
            
            return ErrorCode.ERR_Success;
        }

      
        
    }
}