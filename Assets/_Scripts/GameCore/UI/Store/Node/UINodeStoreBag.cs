using SCFrame;
using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.UI
{
    public class UINodeStoreBag : _ASCUINodeBase
    {
        public UINodeStoreBag(SCUIShowType _showType) : base(_showType)
        {
        }

        public override bool needHideWhenEnterNewSameTypeNode => false;

        public override bool needShowWhenQuitNewSameTypeNode => false;

        public override bool canQuitByEsc => true;

        public override bool canQuitByMouseRight => true;

        public override bool ignoreOnUIList => false;

        public override SCUINodeFuncType nodeFuncType => SCUINodeFuncType.STORE;

        public override bool needMoveToBottomWhenHide => false;

        private GameObject _m_panelGO;
        private UIPanelStoreBag _m_storeBagPanel;
        private UIMonoStoreBag _m_storeBagMono;
        public override string GetNodeName()
        {
            return nameof(UINodeStoreBag);
        }

        public override string GetResName()
        {
            return "panel_store_bag";
        }

        public override void OnEnterNode()
        {
            _m_panelGO = ResourcesHelper.LoadGameObject(GetResName(), GetRootTransform(), true);
            if (_m_panelGO == null)
            {
                Debug.LogError("未找到资源名为" + GetResName() + "的资源!!!");
                return;
            }
            _m_storeBagMono = _m_panelGO.GetComponent<UIMonoStoreBag>();
            if (_m_storeBagMono == null)
            {
                Debug.LogError("资源名为" + GetResName() + "的资源上不存在对应的Mono!!!");
                return;
            }

            _m_storeBagPanel = new UIPanelStoreBag(_m_storeBagMono, _m_showType);
            _m_storeBagPanel.Initialize();
        }

        public override void OnHideNode()
        {
            _m_storeBagPanel?.HidePanel();
        }

        public override void OnQuitNode()
        {
            _m_storeBagPanel?.Discard();
        }

        public override void OnShowNode()
        {
            _m_storeBagPanel?.ShowPanel();
        }
    }
}
