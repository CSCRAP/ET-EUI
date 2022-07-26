

namespace ET
{
    public static class DisconnetHelper
    {
        public static async ETTask Disconnet(this Session self)
        {
            if (self == null || self.IsDisposed)
            {
                return;
            }

            long instanceId = self.InstanceId;
            await TimerComponent.Instance.WaitAsync(1000);
            if (self.InstanceId != instanceId)
            {
                return;
            }
            
            self.Dispose();

        }
        
        
        public static async ETTask KickPlayer(Player player, bool isException = false)
        {
            if (player == null || player.IsDisposed)
            {
                return;
            }

            long instanceId = player.InstanceId;

            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.LoginGate,player.AccountId.GetHashCode()))
            {
                if (player.IsDisposed || player.InstanceId != instanceId)
                {
                    return;
                }


                if (!isException)
                {
                    switch (player.PlayerState)
                    {
                        case  PlayerState.Disconnet:
                            break;
                        case  PlayerState.Gate:
                            break;
                        case PlayerState.Game:
                            //TODD  通知游戏逻辑服下线Unit角色逻辑，并将数据存入数据库
                            var m2GRequestExitGame = (M2G_RequestExitGame)await MessageHelper.CallLocationActor(player.UnitId, new G2M_RequestExitGame());
                      
                            //通知移除账号角色登录信息
                            var LoginCenterConfigSceneId =  StartSceneConfigCategory.Instance.LoginCenterConfig.InstanceId;
                            var l2GRemoveLoginRecord =(L2G_RemoveLoginRecord)await MessageHelper.CallActor(LoginCenterConfigSceneId,
                                new G2L_RemoveLoginRecord(){ServerId = player.DomainZone(),AccountId = player.AccountId});
                            
                            
                            
                            break;
                    }
             
                }

                player.PlayerState = PlayerState.Disconnet;
                player.DomainScene().GetComponent<PlayerComponent>()?.Remove(player.AccountId);
                player?.Dispose();
                
                await TimerComponent.Instance.WaitAsync(300);

            }
            
            
            
            await ETTask.CompletedTask;
        }

        
    }
}