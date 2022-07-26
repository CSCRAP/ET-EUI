using System;
using System.Text;

namespace ET
{
    public class C2R_LoginRealmHandler : AMRpcHandler<C2R_LoginRealm,R2C_LoginRealm>
    {
        protected override async ETTask Run(Session session, C2R_LoginRealm request, R2C_LoginRealm response, Action reply)
        {
            if (session.DomainScene().SceneType != SceneType.Realm)
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

            if (request.RealmTokenKey == null || request.RealmTokenKey != Token)
            {
                response.Error = ErrorCode.ERR_TokenError;
                reply();
                session?.Disconnet().Coroutine();
                return;
            }

            session.DomainScene().GetComponent<TokenComponent>().Remove(request.AccountId);
            
            using (session.AddComponent<SessionLockingComponent>())
            {
                using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.LoginRealm, request.AccountId))
                {
                    //取模固定分配一个Gate
                   StartSceneConfig config = RealmGateAddressHelper.GetGate(session.DomainScene().Zone, request.AccountId);
                   
                   //向gate请求一个Key，客户端可以拿着这个key连接gate
                   G2R_GetLoginGateKey g2RGetLoginGateKey = (G2R_GetLoginGateKey)await MessageHelper.CallActor(config.InstanceId, new R2G_GetLoginGateKey() { AccountId = request.AccountId });

                   if (g2RGetLoginGateKey.Error != ErrorCode.ERR_Success)
                   {
                       response.Error = g2RGetLoginGateKey.Error;
                       reply();
                       return;
                   }

                   response.GateSessionKey = g2RGetLoginGateKey.GateSessionKey;
                   response.GateAddress = config.OuterIPPort.ToString();
                   reply();
                   
                   session.Disconnet().Coroutine();


                }
            }
        }
    }
}