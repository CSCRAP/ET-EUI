namespace ET
{
    [FriendClass(typeof(RoleInfo))]
    public static class RoleInfoSystem
    {

        public static void FromMessage(this RoleInfo self, RoleInfoProto roleInfoProto)
        {
            self.Id = roleInfoProto.Id;
            self.state = roleInfoProto.State;
            self.Name = roleInfoProto.Name;
            self.AccountId = roleInfoProto.AccountId;
            self.CreateTime = roleInfoProto.CreateTime;
            self.LastLoginTime = roleInfoProto.LastLoginTime;
        }


        public static RoleInfoProto ToMessage(this RoleInfo self)
        {
          return  new RoleInfoProto()
          {
              Id = self.Id, 
              Name = self.Name,
              State = self.state,
              AccountId = self.AccountId,
              CreateTime = self.CreateTime,
              LastLoginTime = self.LastLoginTime
          };



            
        }
    }
}