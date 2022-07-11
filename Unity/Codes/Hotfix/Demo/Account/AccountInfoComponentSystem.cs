using System;

namespace ET
{
    
    public class AccountInfoComponentDestorySystem : DestroySystem<AccountInfoComponent>
    {
        public override void Destroy(AccountInfoComponent self)
        {
            self.Token = String.Empty;
            self.AccountId = 0;
        }
    }

    public static class AccountInfoComponentSystem
    {
        
    }
}