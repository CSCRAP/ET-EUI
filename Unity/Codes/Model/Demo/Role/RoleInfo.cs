namespace ET
{

    public enum RoleInfoState
    {
        Normal = 1,
        Freeze = 2,
    }
    
    
    
    public class RoleInfo : Entity,IAwake
    {

        public string Name;

        public int ServerId;

        public int state;

        public long AccountId;

        public long LastLoginTime;

        public long CreateTime;
        

    }
}