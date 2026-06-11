using SCFrame;
using SCFrame.UI;
using UnityEngine;

namespace GameCore.UI
{
    public class UINodeHistory : _ASCUINodeBase
    {
        public UINodeHistory(SCUIShowType _showType) : base(_showType)
        {
        }

        public override bool needHideWhenEnterNewSameTypeNode => false;

        public override bool needShowWhenQuitNewSameTypeNode => false;

        public override bool canQuitByEsc => true;

        public override bool canQuitByMouseRight => true;

        public override bool ignoreOnUIList => false;

        public override SCUINodeFuncType nodeFuncType => SCUINodeFuncType.COMMON;

        public override bool needMoveToBottomWhenHide => false;

        private GameObject _m_panelGO;
        private UIPanelHistory _m_historyPanel;
        private UIMonoHistory _m_historyMono;

        public override string GetNodeName()
        {
            return nameof(UINodeHistory);
        }

        public override string GetResName()
        {
            return "panel_history";
        }

        public override void OnEnterNode()
        {
            _m_panelGO = ResourcesHelper.LoadGameObject(GetResName(), GetRootTransform(), true);
            if (_m_panelGO == null)
            {
                Debug.LogError("未找到资源名为" + GetResName() + "的资源!!!");
                return;
            }
            _m_historyMono = _m_panelGO.GetComponent<UIMonoHistory>();
            if (_m_historyMono == null)
            {
                Debug.LogError("资源名为" + GetResName() + "的资源上不存在对应的Mono!!!");
                return;
            }

            _m_historyPanel = new UIPanelHistory(_m_historyMono, _m_showType);
            _m_historyPanel.Initialize();
        }

        public override void OnHideNode()
        {
            _m_historyPanel?.HidePanel();
        }

        public override void OnQuitNode()
        {
            _m_historyPanel?.Discard();
        }

        public override void OnShowNode()
        {
            _m_historyPanel?.ShowPanel();
        }
    }
}
