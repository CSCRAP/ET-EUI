using System;
using System.Text.RegularExpressions;

namespace ET
{
    [FriendClass(typeof(Account))]
    public class C2A_LoginAccountHandler : AMRpcHandler<C2A_LoginAccount,A2C_LoginAccount>
    {
        protected override async ETTask Run(Session session, C2A_LoginAccount request, A2C_LoginAccount response, Action reply)
        {

            if (session.DomainScene().SceneType != SceneType.Account)
            {
                
                Log.Error($"请求Scene错误，当前请求的Scene为{session.DomainScene().SceneType}");
                session.Dispose();
                
                return ;
            }

            session.RemoveComponent<SessionAcceptTimeoutComponent>();

            if (session.GetComponent<SessionLockingComponent>() != null)
            {
                response.Error = ErrorCode.ERR_RequestRepeatedly;
                reply();
                session.Disconnet().Coroutine();
                return;
            }

            if (string.IsNullOrEmpty(request.AccountName) || string.IsNullOrEmpty(request.Password))
            {

                response.Error = ErrorCode.ERR_LoginInfoIsNull;
                reply();
                session.Disconnet().Coroutine();
                return;
            }
            
            if (!Regex.IsMatch(request.AccountName.Trim(),@"^(?=.*[0-9].*)(?=.*[A-Z].*)(?=.*[a-z].*).{6,15}$") )
            {
                response.Error = ErrorCode.ERR_AccountNameFormError;
                reply();
                session.Disconnet().Coroutine();
                return;
            }

            if (!Regex.IsMatch(request.Password.Trim(),@"^[A-Za-z0-9]+$") )
            {
                response.Error = ErrorCode.ERR_PasswordFormError;
                reply();
                session.Disconnet().Coroutine();
                return;
            }

            if (session.GetComponent<AccountsZone>() ==null)
            {
                session.AddComponent<AccountsZone>();
            }

            using (session.AddComponent<SessionLockingComponent>())
            {
                using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.LoginAccount,request.AccountName.Trim().GetHashCode()))
                {
                    var accountInfoLists = await DBManagerComponent.Instance.GetZoneDB(session.DomainZone()).Query<Account>(d => d.AccountName.Equals(request.AccountName.Trim()));

                    Account account = null;
                    if (accountInfoLists != null && accountInfoLists.Count > 0)
                    {
                        account = accountInfoLists[0];
                        session.GetComponent<AccountsZone>().AddChild(account);
                        if (account.AccountType == (int)AccountType.BlackList)
                        {
                            response.Error = ErrorCode.ERR_AccountInBlackListError;
                            reply();
                            session.Disconnet().Coroutine();
                            account.Dispose();
                            return;
                        }


                        if (!account.password.Equals(request.Password))
                        {
                            response.Error = ErrorCode.ERR_LoginPasswordError;
                            reply();
                            session.Disconnet().Coroutine();
                            account.Dispose();
                            return;
                        }

                    }
                    else
                    {
                        account = session.GetComponent<AccountsZone>().AddChild<Account>();
                        account.AccountName = request.AccountName.Trim();
                        account.password = request.Password.Trim();
                        account.AccountType = (int)AccountType.Genneral;
                        account.CreateTime = TimeHelper.ServerNow();

                        await DBManagerComponent.Instance.GetZoneDB(session.DomainZone()).Save(account);
                    }
                    
                    //操作登录中心服,（判断是否有玩家在线，踢在线玩家下线，定好登录）
                    StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.GetBySceneName(session.DomainZone(), "LoginCenter");
                    long loginCenterInstanceIdnstanceId = startSceneConfig.InstanceId;
                   var l2ALoginAccountResponse = (L2A_LoginAccountResponse)await ActorMessageSenderComponent.Instance.Call(loginCenterInstanceIdnstanceId,new A2L_LoginAccountRequest() { AccountId = account.Id });
                  
                   if (l2ALoginAccountResponse.Error != ErrorCode.ERR_Success)
                   {
                       response.Error = l2ALoginAccountResponse.Error;
                       reply();
                       session?.Disconnet().Coroutine();
                       account?.Dispose();
                       return;
                   }
                    
                    
                    

                    long accountSessionInstanceId = session.DomainScene().GetComponent<AccountSessionsComponent>().Get(account.Id);

                    Session otherSession = Game.EventSystem.Get(accountSessionInstanceId) as Session;

                    otherSession?.Send(new A2C_Disconnet(){Error = 0 });
                    otherSession?.Disconnet().Coroutine();

                    session.DomainScene().GetComponent<AccountSessionsComponent>().Add(account.Id,session.InstanceId);
                    session.AddComponent<AccountCheckOutTimeComponent, long>(account.Id);


                    string Token = TimeHelper.ServerNow().ToString() + RandomHelper.RandomNumber(int.MinValue,int.MaxValue).ToString(); 
                    session.DomainScene().GetComponent<TokenComponent>().Remove(account.Id);
                    session.DomainScene().GetComponent<TokenComponent>().Add(account.Id,Token);

                    response.AccountId = account.Id;
                    response.Token = Token;

                    reply();
                    account?.Dispose();

                }
                
            }
            
            
            
          
        }
    }
}