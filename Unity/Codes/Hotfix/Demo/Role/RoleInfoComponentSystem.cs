using System.Collections.Generic;

namespace ET
{
    public class RoleInfoComponentDestroySystem : DestroySystem<RoleInfoComponent>
    {
        public override void Destroy(RoleInfoComponent self)
        {
            foreach (var roleInfo in self.RoleInfos)
            {
                roleInfo?.Dispose();
            }
            self.RoleInfos.Clear();
            self.CurrentRoleId = 0;
        }
    }

    [FriendClass(typeof(RoleInfoComponent))]
    public static class RoleInfoComponentSystem
    {
        
    }
}