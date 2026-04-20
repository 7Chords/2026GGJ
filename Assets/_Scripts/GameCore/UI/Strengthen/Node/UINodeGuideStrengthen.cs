using SCFrame;
using SCFrame.UI;
using UnityEngine;

namespace GameCore.UI
{
    public class UINodeGuideStrengthen : _ASCUINodeBase
    {
        public UINodeGuideStrengthen(SCUIShowType _showType) : base(_showType)
        {
        }

        public override bool needHideWhenEnterNewSameTypeNode => false;

        public override bool needShowWhenQuitNewSameTypeNode => false;

        public override bool canQuitByEsc => false;

        public override bool canQuitByMouseRight => false;

        public override bool ignoreOnUIList => false;

        public override SCUINodeFuncType nodeFuncType => SCUINodeFuncType.EVENT;

        public override bool needMoveToBottomWhenHide => true;

        private GameObject _m_panelGO;
        private UIPanelCommonGuide _m_guidePanel;
        private UIMonoCommonGuide _m_guideMono;

        public override string GetNodeName()
        {
            return nameof(UINodeGuideStrengthen);
        }

        public override string GetResName()
        {
            return "panel_guide_strengthen";
        }

        public override void OnEnterNode()
        {
            _m_panelGO = ResourcesHelper.LoadGameObject(GetResName(), GetRootTransform(), true);
            if (_m_panelGO == null)
            {
                Debug.LogError("未找到资源名为" + GetResName() + "的资源!!!");
                return;
            }
            _m_guideMono = _m_panelGO.GetComponent<UIMonoCommonGuide>();
            if (_m_guideMono == null)
            {
                Debug.LogError("资源名为" + GetResName() + "的资源上不存在对应的Mono!!!");
                return;
            }

            _m_guidePanel = new UIPanelCommonGuide(_m_guideMono, _m_showType);
            _m_guidePanel.Initialize();
        }

        public override void OnHideNode()
        {
            _m_guidePanel?.HidePanel();
        }

        public override void OnQuitNode()
        {
            _m_guidePanel?.Discard();
        }

        public override void OnShowNode()
        {
            _m_guidePanel?.ShowPanel();
        }
    }
}
