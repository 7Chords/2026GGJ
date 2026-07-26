using GameCore;
using SCFrame;
using SCFrame.UI;
using UnityEngine;

namespace GameCore.UI
{
    public class UINodeEventPartExchange : _ASCUINodeBase
    {
        public UINodeEventPartExchange(SCUIShowType _showType,EEventType _eventType) : base(_showType)
        {
            _m_eventType = _eventType;
        }

        public override bool needHideWhenEnterNewSameTypeNode => false;

        public override bool needShowWhenQuitNewSameTypeNode => false;

        public override bool canQuitByEsc => true;

        public override bool canQuitByMouseRight => false;

        public override bool ignoreOnUIList => false;

        public override SCUINodeFuncType nodeFuncType => SCUINodeFuncType.EVENT;

        public override bool needMoveToBottomWhenHide => false;

        private GameObject _m_panelGO;
        private UIPanelEventPartExchange _m_panel;
        private UIMonoEventPartExchange _m_mono;

        private EEventType _m_eventType;
        public override string GetNodeName()
        {
            return nameof(UINodeEventPartExchange);
        }

        public override string GetResName()
        {
            return "panel_event_part_exchange";
        }

        public override void OnEnterNode()
        {
            _m_panelGO = ResourcesHelper.LoadGameObject(GetResName(), GetRootTransform(), true);
            if (_m_panelGO == null)
            {
                Debug.LogError("Missing UI prefab: " + GetResName());
                return;
            }
            _m_mono = _m_panelGO.GetComponent<UIMonoEventPartExchange>();
            if (_m_mono == null)
            {
                Debug.LogError("UIMonoEventPartExchange missing on " + GetResName());
                return;
            }
            _m_panel = new UIPanelEventPartExchange(_m_mono, _m_showType,_m_eventType);
            _m_panel.Initialize();
        }

        public override void OnHideNode()
        {
            _m_panel?.HidePanel();
        }

        public override void OnQuitNode()
        {
            _m_panel?.Discard();
        }

        public override void OnShowNode()
        {
            _m_panel?.ShowPanel();
        }
    }
}
