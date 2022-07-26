using System;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEngine;

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


        public static async ETTask<int> GetRealmKey(Scene zoneScene)
        {
            A2C_GetRealmKey a2CGetRealmKey = null;
            try
            {
            a2CGetRealmKey = (A2C_GetRealmKey) await zoneScene.GetComponent<SessionComponent>().Session.Call(new C2A_GetRealmKey()
                {
                    ServerId = zoneScene.GetComponent<ServerInfosComponent>().CurrentServerId,
                    AccountId = zoneScene.GetComponent<AccountInfoComponent>().AccountId,
                    Token = zoneScene.GetComponent<AccountInfoComponent>().Token
                });

            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                return ErrorCode.ERR_NetWorkError;
            }

            if (a2CGetRealmKey.Error != ErrorCode.ERR_Success)
            {
                Log.Error(a2CGetRealmKey.Error.ToString());
                return a2CGetRealmKey.Error;
            }

            zoneScene.GetComponent<AccountInfoComponent>().RealmKey = a2CGetRealmKey.RealmKey;
            zoneScene.GetComponent<AccountInfoComponent>().RealmAddress = a2CGetRealmKey.RealmAdderss;
            zoneScene.GetComponent<SessionComponent>().Session.Dispose();


            await ETTask.CompletedTask;
            return ErrorCode.ERR_Success;
        }
        
        
        
        public static async ETTask<int> EnterGame(Scene zoneScene)
        {
            string realmAddress = zoneScene.GetComponent<AccountInfoComponent>().RealmAddress;
            //1、连接Realm，获取分配的Gate
            R2C_LoginRealm r2CLoginRealm ;
            Session session = zoneScene.GetComponent<NetKcpComponent>().Create(NetworkHelper.ToIPEndPoint(realmAddress));
            
            try
            {
                r2CLoginRealm = (R2C_LoginRealm)await session.Call(new C2R_LoginRealm()
                {
                    AccountId = zoneScene.GetComponent<AccountInfoComponent>().AccountId,
                    RealmTokenKey = zoneScene.GetComponent<AccountInfoComponent>().RealmKey,
                });
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                session?.Dispose();
                return ErrorCode.ERR_NetWorkError;
            }
            
            session?.Dispose();
            
            if (r2CLoginRealm.Error != ErrorCode.ERR_Success)
            {
                return r2CLoginRealm.Error;
            }

            
            Log.Warning($"GateAddresss: {r2CLoginRealm.GateAddress}");
            Session gateSession = zoneScene.GetComponent<NetKcpComponent>().Create(NetworkHelper.ToIPEndPoint(r2CLoginRealm.GateAddress));
            
            gateSession.AddComponent<PingComponent>();
            zoneScene.GetComponent<SessionComponent>().Session = gateSession;


            //2、开始连接Gate
            long roleId = zoneScene.GetComponent<RoleInfoComponent>().CurrentRoleId;
            G2C_LoginGameGate g2CLoginGameGate;
            
            try
            {
               long accountId = zoneScene.GetComponent<AccountInfoComponent>().AccountId;
               
               g2CLoginGameGate = (G2C_LoginGameGate) await zoneScene.GetComponent<SessionComponent>().Session.Call(new C2G_LoginGameGate() { AccountId = accountId, Key = r2CLoginRealm.GateSessionKey, RoleId = roleId });
               
              
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                zoneScene.GetComponent<SessionComponent>().Session?.Dispose();
                return ErrorCode.ERR_NetWorkError;
            }
            
            if (g2CLoginGameGate.Error != ErrorCode.ERR_Success)
            {
                Log.Error(g2CLoginGameGate.Error.ToString());
                return g2CLoginGameGate.Error;
            }
            
            Log.Debug("登录 Gate 成功！！！！！！！！！！！！！！");
            
            //3、角色正式请求进入游戏逻辑服
            G2C_EnterGame g2CEnterGame = null;

            try
            {
             
                g2CEnterGame = (G2C_EnterGame) await gateSession?.Call(new C2G_EnterGame() { });
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                zoneScene.GetComponent<SessionComponent>().Session?.Dispose();
                return ErrorCode.ERR_NetWorkError;
            }


            if (g2CEnterGame.Error != ErrorCode.ERR_Success )
            {
                Log.Error(g2CEnterGame.Error.ToString());
                return g2CEnterGame.Error;
            }
            
            Log.Debug("角色进入游戏成功！！！！！");
            
            
            return ErrorCode.ERR_Success;
        }
    }
}