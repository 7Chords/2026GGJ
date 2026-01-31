using SCFrame;
using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.UI
{
    public class UINodeStart : _ASCUINodeBase
    {
        public UINodeStart(SCUIShowType _showType) : base(_showType)
        {
        }

        public override bool needHideWhenEnterNewSameTypeNode => true;

        public override bool needShowWhenQuitNewSameTypeNode => false;

        public override bool canQuitByEsc => false;

        public override bool canQuitByMouseRight => false;

        public override bool ignoreOnUIList => false;

        public override SCUINodeFuncType nodeFuncType => SCUINodeFuncType.COMMON;

        public override bool needMoveToBottomWhenHide => false;

        private GameObject _m_panelGO;
        private UIPanelStart _m_startPanel;
        private UIMonoStart _m_startMono;
        public override string GetNodeName()
        {
            return nameof(UINodeStart);
        }

        public override string GetResName()
        {
            return "panel_start";
        }

        public override void OnEnterNode()
        {
            _m_panelGO = ResourcesHelper.LoadGameObject(GetResName(), GetRootTransform(), true);
            if (_m_panelGO == null)
            {
                Debug.LogError("未找到资源名为" + GetResName() + "的资源!!!");
                return;
            }
            _m_startMono = _m_panelGO.GetComponent<UIMonoStart>();
            if (_m_startMono == null)
            {
                Debug.LogError("资源名为" + GetResName() + "的资源上不存在对应的Mono!!!");
                return;
            }

            _m_startPanel = new UIPanelStart(_m_startMono, _m_showType);
            _m_startPanel.Initialize();
        }

        public override void OnHideNode()
        {
            if (_m_startPanel == null)
                return;
            _m_startPanel.HidePanel();
        }

        public override void OnQuitNode()
        {
            if (_m_startPanel == null)
                return;
            _m_startPanel.Discard();
        }

        public override void OnShowNode()
        {
            if (_m_startPanel == null)
                return;
            _m_startPanel.ShowPanel();
        }
    }
}
