
namespace ET
{
    [FriendClass(typeof(ServerInfo))]
    public static class ServerInfoSystem
    {

        public static void FromMessage(this ServerInfo self, ServerInfosProto serverInfosProto)
        {
            self.Id = serverInfosProto.Id;
            self.Status = serverInfosProto.Status;
            self.ServerName = serverInfosProto.ServerName;
        }


        public static ServerInfosProto ToMessage(this ServerInfo self)
        {
            return new ServerInfosProto() { Id = (int)self.Id, ServerName = self.ServerName, Status = self.Status };

        }
        
    }
}