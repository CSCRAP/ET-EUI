namespace ET
{
	 [ComponentOf(typeof(UIBaseWindow))]
	public  class DlgRoles :Entity,IAwake,IUILogic
	{

		public DlgRolesViewComponent View { get => this.Parent.GetComponent<DlgRolesViewComponent>();} 

		 

	}
}
