using SCFrame;
using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.UI
{
    public class UINodeStore : _ASCUINodeBase
    {
        public UINodeStore(SCUIShowType _showType) : base(_showType)
        {
        }

        public override bool needHideWhenEnterNewSameTypeNode => false;

        public override bool needShowWhenQuitNewSameTypeNode => false;

        public override bool canQuitByEsc => false;

        public override bool canQuitByMouseRight => false;

        public override bool ignoreOnUIList => false;

        public override SCUINodeFuncType nodeFuncType => SCUINodeFuncType.STORE;

        public override bool needMoveToBottomWhenHide => false;


        private GameObject _m_panelGO;
        private UIPanelStore _m_storePanel;
        private UIMonoStore _m_storeMono;
        public override string GetNodeName()
        {
            return nameof(UINodeStore);
        }

        public override string GetResName()
        {
            return "panel_store";
        }

        public override void OnEnterNode()
        {
            _m_panelGO = ResourcesHelper.LoadGameObject(GetResName(), GetRootTransform(), true);
            if (_m_panelGO == null)
            {
                Debug.LogError("未找到资源名为" + GetResName() + "的资源!!!");
                return;
            }
            _m_storeMono = _m_panelGO.GetComponent<UIMonoStore>();
            if (_m_storeMono == null)
            {
                Debug.LogError("资源名为" + GetResName() + "的资源上不存在对应的Mono!!!");
                return;
            }

            _m_storePanel = new UIPanelStore(_m_storeMono, _m_showType);
            _m_storePanel.Initialize();
        }

        public override void OnHideNode()
        {
            _m_storePanel?.HidePanel();
        }

        public override void OnQuitNode()
        {
            _m_storePanel?.Discard();
        }

        public override void OnShowNode()
        {
            _m_storePanel?.ShowPanel();
        }
    }
}
