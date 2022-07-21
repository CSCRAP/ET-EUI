using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace ET
{
	[FriendClass(typeof(DlgRoles))]
	[FriendClass(typeof(RoleInfoComponent))]
	[FriendClass(typeof(RoleInfo))]
	public static  class DlgRolesSystem
	{

		public static void RegisterUIEvent(this DlgRoles self)
		{
			self.View.E_CreateRoleButton.AddListenerAsync(() => {return self.OnCreateRoleClickHandler();});
			
			self.View.E_RolesLoopHorizontalScrollRect.
					AddItemRefreshListener(((Transform transform, int index) => { self.OnRoleListRefreshHandler(transform, index); }));
			self.View.E_DeleteRoleButton.AddListenerAsync((() => { return self.OnDeleteRoleClickHandler();}));
			//self.View.E_ConfirmButton.AddListenerAsync(() => {return self.OnRoleItemClickHandler()});
		}
		 
		

		public static void ShowWindow(this DlgRoles self, Entity contextData = null)
		{
			self.RefreshRoleItems();
		}

		public static void HideWindow(this DlgRoles self)
		{
			
		}
		public static void RefreshRoleItems(this DlgRoles self)
		{
			int count = self.ZoneScene().GetComponent<RoleInfoComponent>().RoleInfos.Count;
			self.AddUIScrollItems(ref self.ScrollItemRoles,count);
			self.View.E_RolesLoopHorizontalScrollRect.SetVisible(true,count);
		}

		public static void OnRoleListRefreshHandler(this DlgRoles self, Transform transform, int index)
		{
			Scroll_Item_role scrollItemRoles  = self.ScrollItemRoles[index].BindTrans(transform);
			RoleInfo roleInfo = self.ZoneScene().GetComponent<RoleInfoComponent>().RoleInfos[index];
			
			scrollItemRoles.E_RoleSelectImage.color = roleInfo.Id == self.ZoneScene().GetComponent<RoleInfoComponent>().CurrentRoleId? Color.green : Color.grey;
			
			scrollItemRoles.E_RoleNameText.SetText(roleInfo.Name);
			
			scrollItemRoles.E_RoleSelectButton.AddListener(() => { self.OnRoleItemClickHandler(roleInfo.Id);});
		}

		public static void OnRoleItemClickHandler(this DlgRoles self, long roleId)
		{
			self.ZoneScene().GetComponent<RoleInfoComponent>().CurrentRoleId = roleId;
			Log.Error($"roleId{roleId}");
			self.View.E_RolesLoopHorizontalScrollRect.RefillCells();
		}

		public static async ETTask OnCreateRoleClickHandler(this DlgRoles self)
		{
			string name = self.View.E_RoleNameInputField.text;
			
			if (string.IsNullOrEmpty(name))
			{
				Log.Error("Name is null");
				return;
			}

			try
			{

				int errorCode =	await LoginHelper.CreateRole(self.ZoneScene(), name);
				if (errorCode != ErrorCode.ERR_Success)
				{
					Log.Error(errorCode.ToString());
					return;
				}
				
				self.RefreshRoleItems();

			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
			}
			
		}
		
		public static async ETTask OnDeleteRoleClickHandler(this DlgRoles self)
		{
			if (self.ZoneScene().GetComponent<RoleInfoComponent>().CurrentRoleId == 0)
			{
				Log.Error($"请选择要删除的角色");
				return;
			}

			try
			{
				var errorCode = await LoginHelper.DeleteRole(self.ZoneScene());
				if (errorCode != ErrorCode.ERR_Success)
				{
					Log.Error(errorCode.ToString());
					return;
				}
				
				self.RefreshRoleItems();

			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
				
			}
			
			
		}
		
	}
}
