using System;

namespace ET
{
    [ActorMessageHandler]
    public class L2G_DisconnetGateUnitRequestHandler : AMActorRpcHandler<Scene,L2G_DisconnetGateUnitRequest,G2L_DisconnetGateUnitResponse>
    {
        protected override async ETTask Run(Scene scene, L2G_DisconnetGateUnitRequest request, G2L_DisconnetGateUnitResponse response, Action reply)
        {

            long accountId = request.AccountId;

            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.LoginGate,accountId.GetHashCode()))
            {
               PlayerComponent playerComponent = scene.GetComponent<PlayerComponent>();
               Player player = playerComponent.Get(accountId);
               
               if (player == null)
               {
                   reply();
                   return;
               }
               
                
               scene.GetComponent<GateSessionKeyComponent>().Remove(accountId);
               Session gateSession = Game.EventSystem.Get(player.SessionInstanceId) as Session;
               if (gateSession != null && !gateSession.IsDisposed)
               {
                   gateSession.Send(new A2C_Disconnet(){Error = ErrorCode.ERR_OtherAccountLogin});
                   gateSession?.Disconnet().Coroutine();
               }

               player.SessionInstanceId = 0;
               player.AddComponent<PlayerOfflineOutTimeComponent>();
               
            }

            reply();
            
        }
    }
}