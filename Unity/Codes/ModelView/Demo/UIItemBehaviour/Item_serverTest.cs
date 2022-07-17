
using UnityEngine;
using UnityEngine.UI;
namespace ET
{
	[EnableMethod]
	public  class Scroll_Item_serverTest : Entity,IAwake,IDestroy,IUIScrollItem 
	{
		private bool isCacheNode = false;
		public void SetCacheMode(bool isCache)
		{
			this.isCacheNode = isCache;
		}

		public Scroll_Item_serverTest BindTrans(Transform trans)
		{
			this.uiTransform = trans;
			return this;
		}

		public UnityEngine.UI.Image E_serverTestImage
     	{
     		get
     		{
     			if (this.uiTransform == null)
     			{
     				Log.Error("uiTransform is null.");
     				return null;
     			}
     			if (this.isCacheNode)
     			{
     				if( this.m_E_serverTestImage == null )
     				{
		    			this.m_E_serverTestImage = UIFindHelper.FindDeepChild<UnityEngine.UI.Image>(this.uiTransform.gameObject,"E_serverTest");
     				}
     				return this.m_E_serverTestImage;
     			}
     			else
     			{
		    		return UIFindHelper.FindDeepChild<UnityEngine.UI.Image>(this.uiTransform.gameObject,"E_serverTest");
     			}
     		}
     	}

		public UnityEngine.UI.Button E_selectButton
     	{
     		get
     		{
     			if (this.uiTransform == null)
     			{
     				Log.Error("uiTransform is null.");
     				return null;
     			}
     			if (this.isCacheNode)
     			{
     				if( this.m_E_selectButton == null )
     				{
		    			this.m_E_selectButton = UIFindHelper.FindDeepChild<UnityEngine.UI.Button>(this.uiTransform.gameObject,"E_select");
     				}
     				return this.m_E_selectButton;
     			}
     			else
     			{
		    		return UIFindHelper.FindDeepChild<UnityEngine.UI.Button>(this.uiTransform.gameObject,"E_select");
     			}
     		}
     	}

		public UnityEngine.UI.Image E_selectImage
     	{
     		get
     		{
     			if (this.uiTransform == null)
     			{
     				Log.Error("uiTransform is null.");
     				return null;
     			}
     			if (this.isCacheNode)
     			{
     				if( this.m_E_selectImage == null )
     				{
		    			this.m_E_selectImage = UIFindHelper.FindDeepChild<UnityEngine.UI.Image>(this.uiTransform.gameObject,"E_select");
     				}
     				return this.m_E_selectImage;
     			}
     			else
     			{
		    		return UIFindHelper.FindDeepChild<UnityEngine.UI.Image>(this.uiTransform.gameObject,"E_select");
     			}
     		}
     	}

		public UnityEngine.UI.Text E_serverTestTipText
     	{
     		get
     		{
     			if (this.uiTransform == null)
     			{
     				Log.Error("uiTransform is null.");
     				return null;
     			}
     			if (this.isCacheNode)
     			{
     				if( this.m_E_serverTestTipText == null )
     				{
		    			this.m_E_serverTestTipText = UIFindHelper.FindDeepChild<UnityEngine.UI.Text>(this.uiTransform.gameObject,"E_serverTestTip");
     				}
     				return this.m_E_serverTestTipText;
     			}
     			else
     			{
		    		return UIFindHelper.FindDeepChild<UnityEngine.UI.Text>(this.uiTransform.gameObject,"E_serverTestTip");
     			}
     		}
     	}

		public void DestroyWidget()
		{
			this.m_E_serverTestImage = null;
			this.m_E_selectButton = null;
			this.m_E_selectImage = null;
			this.m_E_serverTestTipText = null;
			this.uiTransform = null;
		}

		private UnityEngine.UI.Image m_E_serverTestImage = null;
		private UnityEngine.UI.Button m_E_selectButton = null;
		private UnityEngine.UI.Image m_E_selectImage = null;
		private UnityEngine.UI.Text m_E_serverTestTipText = null;
		public Transform uiTransform = null;
	}
}
