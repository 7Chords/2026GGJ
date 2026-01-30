using SCFrame;
using SCFrame.UI;
using UnityEngine;

namespace GameCore.UI
{
    public class UINodeCommonTwoBtn : _ASCUINodeBase
    {
        public UINodeCommonTwoBtn(SCUIShowType _showType) : base(_showType)
        {
        }

        public override bool needHideWhenEnterNewSameTypeNode => false;

        public override bool needShowWhenQuitNewSameTypeNode => false;

        public override bool canQuitByEsc => true;

        public override bool canQuitByMouseRight => true;

        public override bool ignoreOnUIList => true;

        public override SCUINodeFuncType nodeFuncType => SCUINodeFuncType.COMMON;

        public override bool needMoveToBottomWhenHide => false;



        private GameObject _m_panelGO;
        private UIPanelCommonTwoBtn _m_commonTwoBtnPanel;
        private UIMonoCommonTwoBtn _m_commonTwoBtnMono;
        public override string GetNodeName()
        {
            return nameof(UINodeCommonTwoBtn);
        }

        public override string GetResName()
        {
            return "panel_common_two_btn";
        }

        public override void OnEnterNode()
        {
            _m_panelGO = ResourcesHelper.LoadGameObject(GetResName(), GetRootTransform(), true);
            if (_m_panelGO == null)
            {
                Debug.LogError("未找到资源名为" + GetResName() + "的资源!!!");
                return;
            }
            _m_commonTwoBtnMono = _m_panelGO.GetComponent<UIMonoCommonTwoBtn>();
            if (_m_commonTwoBtnMono == null)
            {
                Debug.LogError("资源名为" + GetResName() + "的资源上不存在对应的Mono!!!");
                return;
            }

            _m_commonTwoBtnPanel = new UIPanelCommonTwoBtn(_m_commonTwoBtnMono, _m_showType);
            _m_commonTwoBtnPanel.Initialize();
        }

        public override void OnHideNode()
        {
            _m_commonTwoBtnPanel?.HidePanel();
        }

        public override void OnQuitNode()
        {
            _m_commonTwoBtnPanel?.Discard();
        }

        public override void OnShowNode()
        {
            _m_commonTwoBtnPanel?.ShowPanel();
        }
    }
}
