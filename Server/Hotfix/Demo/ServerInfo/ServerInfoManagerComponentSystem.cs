namespace ET
{
    public class ServerInfoManagerComponentAwakeSystem: AwakeSystem<ServerInfoManagerComponent>
    {
        public override void Awake(ServerInfoManagerComponent self)
        {
            self.awake().Coroutine();
        }
    }
    
    
    
    public class ServerInfoManagerComponentDestroySystem : DestroySystem<ServerInfoManagerComponent>
    {
        public override void Destroy(ServerInfoManagerComponent self)
        {
            foreach (var serverInfo in self.ServerInfos)
            {
             serverInfo?.Dispose();   
            }
            
            self.ServerInfos.Clear();
        }
    }
    
    public class ServerInfoManagerComponentLoadSystem : LoadSystem<ServerInfoManagerComponent>
    {
        public override void Load(ServerInfoManagerComponent self)
        {
            self.awake().Coroutine();
        }
    }

    [FriendClass(typeof(ServerInfoManagerComponent))]
    [FriendClass(typeof(ServerInfo))]
    public static class ServerInfoManagerComponentSystem
    {
        public static async ETTask awake(this ServerInfoManagerComponent self)
        {

           var serverInfos = await DBManagerComponent.Instance.GetZoneDB(self.DomainZone()).Query<ServerInfo>(d => true);

           if (serverInfos == null || serverInfos.Count <= 0)
           {
               Log.Error("ServerInfo count is zero");
               
               self.ServerInfos.Clear();
               var serverInfoConfigs = ServerInfoConfigCategory.Instance.GetAll();
               foreach (var info in serverInfoConfigs.Values)
               {
                   ServerInfo newServerInfo = self.AddChildWithId<ServerInfo>(info.Id);
                   newServerInfo.ServerName = info.ServerName;
                   newServerInfo.Status = (int)ServerStatus.Normal;
                   self.ServerInfos.Add(newServerInfo);
                   await DBManagerComponent.Instance.GetZoneDB(self.DomainZone()).Save(newServerInfo);

               }
               return;
           }
           
           self.ServerInfos.Clear();

           foreach (var serverInfo in serverInfos)
           {
               self.AddChild(serverInfo);
               self.ServerInfos.Add(serverInfo);
           }

           await ETTask.CompletedTask;

        }
    }
}