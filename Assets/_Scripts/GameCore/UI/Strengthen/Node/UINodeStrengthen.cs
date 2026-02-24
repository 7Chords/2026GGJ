using SCFrame;
using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GameCore.UI
{
    public class UINodeStrengthen : _ASCUINodeBase
    {
        public UINodeStrengthen(SCUIShowType _showType) : base(_showType)
        {
        }

        public override bool needHideWhenEnterNewSameTypeNode => true;

        public override bool needShowWhenQuitNewSameTypeNode => false;

        public override bool canQuitByEsc => false;

        public override bool canQuitByMouseRight => false;

        public override bool ignoreOnUIList => false;

        public override SCUINodeFuncType nodeFuncType => SCUINodeFuncType.STRENGTHEN;

        public override bool needMoveToBottomWhenHide => false;


        private GameObject _m_panelGO;
        private UIPanelStrengthen _m_strengthenPanel;
        private UIMonoStrengthen _m_strengthenMono;

        public override string GetNodeName()
        {
            return nameof(UINodeStrengthen);
        }

        public override string GetResName()
        {
            return "panel_strengthen";
        }

        public override void OnEnterNode()
        {
            _m_panelGO = ResourcesHelper.LoadGameObject(GetResName(), GetRootTransform(), true);
            if (_m_panelGO == null)
            {
                Debug.LogError("未找到资源名为" + GetResName() + "的资源!!!");
                return;
            }
            _m_strengthenMono = _m_panelGO.GetComponent<UIMonoStrengthen>();
            if (_m_strengthenMono == null)
            {
                Debug.LogError("资源名为" + GetResName() + "的资源上不存在对应的Mono!!!");
                return;
            }

            _m_strengthenPanel = new UIPanelStrengthen(_m_strengthenMono, _m_showType);
            _m_strengthenPanel.Initialize();
        }

        public override void OnHideNode()
        {
            if (_m_strengthenPanel == null)
                return;
            _m_strengthenPanel.HidePanel();
        }

        public override void OnQuitNode()
        {
            if (_m_strengthenPanel == null)
                return;
            _m_strengthenPanel.Discard();
        }

        public override void OnShowNode()
        {
            if (_m_strengthenPanel == null)
                return;
            _m_strengthenPanel.ShowPanel();
        }
    }
}
