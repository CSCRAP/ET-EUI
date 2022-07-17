using UnityEngine;

namespace ET
{
    public class ServerInfosComponentDestroySystem : DestroySystem<ServerInfosComponent>
    {
        public override void Destroy(ServerInfosComponent self)
        {
            foreach (var serverInfo in self.ServerInfosList)
            {
                serverInfo?.Dispose();
            }
            self.ServerInfosList.Clear();
        }
    }

    [FriendClass(typeof(ServerInfosComponent))]
    public static class ServerInfosComponentSystem
    {

        public static void Add(this ServerInfosComponent self, ServerInfo serverInfo)
        {
            self.ServerInfosList.Add(serverInfo);
        }
        
        
        
    }
}