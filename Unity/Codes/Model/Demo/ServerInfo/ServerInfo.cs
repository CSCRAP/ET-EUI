namespace ET
{


    public enum ServerStatus
    {
        Normal = 1,
        stop = 2,
    }
    public class ServerInfo : Entity,IAwake
    {
        public int Status;
        public string ServerName;
    }
}