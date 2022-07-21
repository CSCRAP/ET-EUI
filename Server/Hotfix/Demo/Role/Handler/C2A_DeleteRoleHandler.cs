using System;

namespace ET
{
    [MessageHandler]
    [FriendClass(typeof(RoleInfo))]
    public class C2A_DeleteRoleHandler : AMRpcHandler<C2A_DeleteRole,A2C_DeleteRole>
    {
        protected override async ETTask Run(Session session, C2A_DeleteRole request, A2C_DeleteRole response, Action reply)
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
                using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.CreateRole,request.AccountId))
                {
                  var roleInfos =await DBManagerComponent.Instance.GetZoneDB(session.DomainZone())
                            .Query<RoleInfo>(d => d.Id == request.RoleInfoId && d.ServerId == request.ServerId);

                  if (roleInfos == null || roleInfos.Count <= 0)
                  {
                      response.Error = ErrorCode.ERR_RoleNotExist;
                      reply();
                      return;
                  }

                  var roleInfo = roleInfos[0];
                  session.AddChild(roleInfo);

                  roleInfo.state = (int) RoleInfoState.Freeze;

                  await DBManagerComponent.Instance.GetZoneDB(session.DomainZone()).Save(roleInfo);

                  response.DeletedRoleInfoId = roleInfo.Id;
                  roleInfo?.Dispose();

                  reply();

                }
            }
            await ETTask.CompletedTask;
        }
    }
}