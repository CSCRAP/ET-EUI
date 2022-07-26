using System;

namespace ET
{
    [FriendClass(typeof(SessionPlayerComponent))]
    [FriendClass(typeof(SessionStateComponent))]
    public class C2G_LoginGameGateHandler : AMRpcHandler<C2G_LoginGameGate,G2C_LoginGameGate>
    {
        protected override async ETTask Run(Session session, C2G_LoginGameGate request, G2C_LoginGameGate response, Action reply)
        {
            
            if (session.DomainScene().SceneType != SceneType.Gate)
            {
                
                Log.Error($"请求Scene错误，当前请求的Scene为{session.DomainScene().SceneType}");
                session.Dispose();
                
                return ;
            }

            session.RemoveComponent<SessionAcceptTimeoutComponent>();

            if (session.GetComponent<SessionLockingComponent>() != null)
            {
                response.Error = ErrorCode.ERR_RequestRepeatedly;
                reply();
                session.Disconnet().Coroutine();
                return;
            }
            
            Scene scene = session.DomainScene();

            string tokenKey = scene.GetComponent<GateSessionKeyComponent>().Get(request.AccountId);
            if (tokenKey == null || tokenKey != request.Key)
            {
                response.Error = ErrorCode.ERR_ConnectGateKeyError;
                response.Message = "GateKey 验证失败";
                reply();
                session.Disconnet().Coroutine();
                return;
            }
            
            scene.GetComponent<GateSessionKeyComponent>().Remove(request.AccountId);

            long instanceId = session.InstanceId;
            using (session.AddComponent<SessionLockingComponent>())
            {
                using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.LoginGate,request.AccountId.GetHashCode()))
                {
                    if (instanceId != session.InstanceId)
                    {
                        return;
                    }
                    
                    //通知登录中心服，记录本次登录的服务器zone

                    StartSceneConfig loginCenterConfig = StartSceneConfigCategory.Instance.LoginCenterConfig;
                    L2G_AddLoginRecord l2GAddLoginRecord = (L2G_AddLoginRecord)await MessageHelper.CallActor(loginCenterConfig.InstanceId,
                        new G2L_AddLoginRecord() { AccountId = request.AccountId, ServerId = scene.Zone });

                    if (l2GAddLoginRecord.Error != ErrorCode.ERR_Success)
                    {

                        response.Error = l2GAddLoginRecord.Error;
                        reply();
                        session?.Disconnet().Coroutine();
                        return;
                    }

                    SessionStateComponent sessionStateComponent =  session.GetComponent<SessionStateComponent>();
                    if (sessionStateComponent == null)
                    {
                        sessionStateComponent = session.AddComponent<SessionStateComponent>();
                    }

                    sessionStateComponent.State = SessionState.Normal;
                    
                    Player player = scene.GetComponent<PlayerComponent>().Get(request.AccountId);

                    if (player == null)
                    {
                        //添加一个新的GateUnit
                       player = scene.GetComponent<PlayerComponent>().AddChildWithId<Player, long, long>(request.RoleId, request.AccountId, request.RoleId);
                       player.PlayerState = PlayerState.Gate;
                       scene.GetComponent<PlayerComponent>().Add(player);
                       
                       session.AddComponent<MailBoxComponent, MailboxType>(MailboxType.GateSession);
                       
                    }
                    else
                    {
                        player.RemoveComponent<PlayerOfflineOutTimeComponent>();
                    }

                    session.AddComponent<SessionPlayerComponent>().PlayerId = player.Id;
                    session.GetComponent<SessionPlayerComponent>().PlayerInstanceId = player.InstanceId;
                    session.GetComponent<SessionPlayerComponent>().AccountId = player.AccountId;
                    player.SessionInstanceId = session.InstanceId;
                    
                }


                reply();
            }

        }
    }
}