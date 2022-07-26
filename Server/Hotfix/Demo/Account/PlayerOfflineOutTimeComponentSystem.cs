using System;

namespace ET
{
    [Timer(TimerType.PlayerOfflineOutTime)]
    public class PlayerOfflineOutTime : ATimer<PlayerOfflineOutTimeComponent>
    {
        public override void Run(PlayerOfflineOutTimeComponent self)
        {
            try
            {
                self.KickPlayer();
            }
            catch (Exception e)
            {
                Log.Error($"playerOffline timer error: {self.Id}\n{e} ");
            }
        }
    }
    
    public class PlayerOfflineOutTimeComponentAwakeSystem : AwakeSystem<PlayerOfflineOutTimeComponent>
    {
        public override void Awake(PlayerOfflineOutTimeComponent self)
        {
            self.Awake();
        }
    }
    

    public class PlayerOfflineOutTimeComponentDestroySystem : DestroySystem<PlayerOfflineOutTimeComponent>
    {
        public override void Destroy(PlayerOfflineOutTimeComponent self)
        {
            TimerComponent.Instance.Remove(ref self.Timer);
        }
    }
    
    
    

    [FriendClass(typeof(PlayerOfflineOutTimeComponent))]
    public static class PlayerOfflineOutTimeComponentSystem
    {

        public static void Awake(this PlayerOfflineOutTimeComponent self)
        {
         self.Timer =  TimerComponent.Instance.NewOnceTimer(TimeHelper.ServerNow() + 10000, TimerType.PlayerOfflineOutTime, self);
         
        }


        public static void KickPlayer(this PlayerOfflineOutTimeComponent self)
        {
            DisconnetHelper.KickPlayer(self.GetParent<Player>()).Coroutine();
        }
        
    }
}