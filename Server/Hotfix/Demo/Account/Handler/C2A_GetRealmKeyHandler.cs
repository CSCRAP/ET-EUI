using System;

namespace ET
{
    [MessageHandler]
    public class C2A_GetRealmKeyHandler : AMRpcHandler<C2A_GetRealmKey,A2C_GetRealmKey>
    {
        protected override async ETTask Run(Session session, C2A_GetRealmKey request, A2C_GetRealmKey response, Action reply)
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


            using (session.AddComponent<SessionLockingComponent>())
            {
                using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.LoginAccount, request.AccountId))
                {
                    StartSceneConfig realmStartSceneConfig = RealmGateAddressHelper.GetRealm(request.ServerId);


              
                    R2A_GetRealmKey r2AGetRealmKey =  (R2A_GetRealmKey) await  MessageHelper.CallActor(realmStartSceneConfig.InstanceId, 
                        new A2R_GetRealmKey() { AccountId = request.AccountId });
                    if (r2AGetRealmKey.Error != ErrorCode.ERR_Success)
                    {
                        response.Error = r2AGetRealmKey.Error;
                        reply();
                        session.Disconnet().Coroutine();
                        return;
                    }

                    response.RealmKey = r2AGetRealmKey.RealmKey;
                    response.RealmAdderss = realmStartSceneConfig.OuterIPPort.ToString();
                    reply();
                    session.Disconnet().Coroutine();

                }
            }

            await ETTask.CompletedTask;
        }
    }
}