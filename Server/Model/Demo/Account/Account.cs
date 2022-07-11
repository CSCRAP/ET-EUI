namespace ET
{

    public enum AccountType
    {
        Genneral = 0,
        
        BlackList = 1,
        
    }
    
   
   
    public class Account : Entity, IAwake
    {

        public string AccountName;  //账户名

        public string password;     //账户密码

        public long CreateTime;     //账号创建时间

        public int AccountType;     //账号类型

    }
}