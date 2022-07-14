using System;

namespace ET
{
    [ActorMessageHandler]
    public class L2G_DisconnetGateUnitRequestHandler : AMActorRpcHandler<Scene,L2G_DisconnetGateUnitRequest,G2L_DisconnetGateUnitResponse>
    {
        protected override async ETTask Run(Scene scene, L2G_DisconnetGateUnitRequest request, G2L_DisconnetGateUnitResponse response, Action reply)
        {

            long accountId = request.AccountId;

            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.GateLoginLock,accountId.GetHashCode()))
            {
               PlayerComponent playerComponent = scene.GetComponent<PlayerComponent>();
               Player gateUnit = playerComponent.Get(accountId);
               if (gateUnit == null)
               {
                   reply();
                   return;
               }
               
               playerComponent.Remove(accountId);
               
               //下线处理逻辑，这里临时处理
               gateUnit.Dispose();
               
               
               
               
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}