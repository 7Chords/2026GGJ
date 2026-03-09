using SCFrame;
using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.UI
{
    public class UINodeEvent : _ASCUINodeBase
    {
        public UINodeEvent(SCUIShowType _showType) : base(_showType)
        {
        }

        public override bool needHideWhenEnterNewSameTypeNode => false;

        public override bool needShowWhenQuitNewSameTypeNode => false;

        public override bool canQuitByEsc => false;

        public override bool canQuitByMouseRight => false;

        public override bool ignoreOnUIList => false;

        public override SCUINodeFuncType nodeFuncType => SCUINodeFuncType.EVENT;

        public override bool needMoveToBottomWhenHide => false;


        private GameObject _m_panelGO;
        private UIPanelEvent _m_eventPanel;
        private UIMonoEvent _m_eventMono;
        public override string GetNodeName()
        {
            return nameof(UINodeEvent);
        }

        public override string GetResName()
        {
            return "panel_event";
        }

        public override void OnEnterNode()
        {
            _m_panelGO = ResourcesHelper.LoadGameObject(GetResName(), GetRootTransform(), true);
            if (_m_panelGO == null)
            {
                Debug.LogError("未找到资源名为" + GetResName() + "的资源!!!");
                return;
            }
            _m_eventMono = _m_panelGO.GetComponent<UIMonoEvent>();
            if (_m_eventMono == null)
            {
                Debug.LogError("资源名为" + GetResName() + "的资源上不存在对应的Mono!!!");
                return;
            }

            _m_eventPanel = new UIPanelEvent(_m_eventMono, _m_showType);
            _m_eventPanel.Initialize();
        }

        public override void OnHideNode()
        {
            _m_eventPanel?.HidePanel();
        }

        public override void OnQuitNode()
        {
            _m_eventPanel?.Discard();
        }

        public override void OnShowNode()
        {
            _m_eventPanel?.ShowPanel();
        }
    }
}
