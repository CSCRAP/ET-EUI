using System;

namespace ET
{
    [ActorMessageHandler]
    public class A2L_LoginAccountRequestHandler : AMActorRpcHandler<Scene,A2L_LoginAccountRequest,L2A_LoginAccountResponse>
    {
        protected override async ETTask Run(Scene scene, A2L_LoginAccountRequest request, L2A_LoginAccountResponse response, Action reply)
        {
            long accountId = request.AccountId;
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.LoginCenterLock,accountId.GetHashCode()))
            {
                if (!scene.GetComponent<LoginInfoRecordComponent>().IsExit(accountId))
                {
                    reply();
                    return;
                }

                int zone = scene.GetComponent<LoginInfoRecordComponent>().Get(accountId);
                StartSceneConfig gateConfig = RealmGateAddressHelper.GetGate(zone,accountId);
                
                //向gate网关，踢用户下线
                var g2LDisconnetGateUnitResponse = (G2L_DisconnetGateUnitResponse) await MessageHelper.CallActor(gateConfig.InstanceId, new L2G_DisconnetGateUnitRequest() { AccountId = accountId });

              
                response.Error = g2LDisconnetGateUnitResponse.Error;

                reply();
              

            }
        }
    }
}