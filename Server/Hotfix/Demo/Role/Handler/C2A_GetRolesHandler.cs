using System;

namespace ET
{
    [FriendClass(typeof(RoleInfo))]
    public class C2A_GetRolesHandler : AMRpcHandler<C2A_GetRoles,A2C_GetRoles>
    {
        protected override async ETTask Run(Session session, C2A_GetRoles request, A2C_GetRoles response, Action reply)
        {

            if (session.DomainScene().SceneType != SceneType.Account)
            {
                
                Log.Error($"请求Scene错误，当前请求的Scene为{session.DomainScene().SceneType}");
                session.Dispose();
                
                return ;
            }
            
            if (session.GetComponent<SessionLockingComponent>() != null)
            {
                response.Error = ErrorCode.ERR_RequestRepeatedly;
                reply();
                session.Disconnet().Coroutine();
                return;
            }
            
            string Token = session.DomainScene().GetComponent<TokenComponent>().Get(request.AccountId);

            if (request.Token == null || request.Token != Token)
            {
                response.Error = ErrorCode.ERR_TokenError;
                reply();
                session?.Disconnet().Coroutine();
                return;
            }

            //防止游戏客户端频繁创建角色
            using (session.AddComponent<SessionLockingComponent>())
            {
                using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.CreateRole, request.AccountId))
                {
                   var roleInfos = await DBManagerComponent.Instance.GetZoneDB(session.DomainZone())
                            .Query<RoleInfo>(d => d.AccountId == request.AccountId && d.ServerId == request.ServerId && d.state ==(int) RoleInfoState.Normal);
                   
                   if (roleInfos == null || roleInfos.Count <= 0 )
                   {
                       reply();
                       return;
                   }
                    

                   foreach (var roleInfo in roleInfos)
                   {
                        response.RoleInfos.Add(roleInfo.ToMessage());
                        roleInfo?.Dispose();
                   }
                   
                   roleInfos.Clear();
                   
                    
                   reply();
                   
                }
                
            }
            
        }
    }
}