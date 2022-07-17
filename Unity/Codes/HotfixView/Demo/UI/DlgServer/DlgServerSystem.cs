using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace ET
{
	[FriendClass(typeof(DlgServer))]
	[FriendClass(typeof(ServerInfosComponent))]
	[FriendClass(typeof(ServerInfo))]
	public static  class DlgServerSystem
	{

		public static void RegisterUIEvent(this DlgServer self)
		{
			//监听进入服务器buttom
			self.View.E_ConfirmButton.AddListenerAsync(() => { return self.OnConfirmClickHandler(); });
				
			self.View.E_ServerListLoopVerticalScrollRect.AddItemRefreshListener((Transform transform, int index) => { self.OnScrollItemRefreshHandler(transform, index); });
		}

		public static void ShowWindow(this DlgServer self, Entity contextData = null)
		{
			int count = self.ZoneScene().GetComponent<ServerInfosComponent>().ServerInfosList.Count;
			self.AddUIScrollItems(ref self.ScrollItemServerTests,count);
			self.View.E_ServerListLoopVerticalScrollRect.SetVisible(true,count);
		}

		
		public static void HideWindow(this DlgServer self)
		{
			self.RemoveUIScrollItems(ref self.ScrollItemServerTests);
		}

		public static void OnScrollItemRefreshHandler(this DlgServer self,Transform transform,int index)
		{
			Scroll_Item_serverTest serverTest = self.ScrollItemServerTests[index].BindTrans(transform);
			ServerInfo serverInfo = self.ZoneScene().GetComponent<ServerInfosComponent>().ServerInfosList[index];
			serverTest.E_selectImage.color = serverInfo.Id == self.ZoneScene().GetComponent<ServerInfosComponent>().CurrentServerId? Color.red : Color.yellow;
			serverTest.E_serverTestTipText.SetText(serverInfo.ServerName);
			
			serverTest.E_selectButton.AddListener(() => { self.OnSelectServerItemHandler(serverInfo.Id);});
			
		}

		public static void OnSelectServerItemHandler(this DlgServer self, long serverId)
		{
			self.ZoneScene().GetComponent<ServerInfosComponent>().CurrentServerId = int.Parse(serverId.ToString());
			Log.Debug($"当前选择的服务器是：{serverId}");
			self.View.E_ServerListLoopVerticalScrollRect.RefillCells();
		}

		public static async ETTask OnConfirmClickHandler(this DlgServer self)
		{
		
			bool isSelect =	self.ZoneScene().GetComponent<ServerInfosComponent>().CurrentServerId != 0;

		
			if (!isSelect)
			{
				Log.Error("请选择区服");
				return;
			}

			try
			{
				
				self.DomainScene().GetComponent<UIComponent>().HideWindow(WindowID.WindowID_Server);
				self.DomainScene().GetComponent<UIComponent>().ShowWindow(WindowID.WindowID_Roles);

			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
				return;
			}


			await ETTask.CompletedTask;

		}

	}
}
