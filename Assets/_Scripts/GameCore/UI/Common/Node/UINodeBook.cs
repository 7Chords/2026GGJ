using SCFrame;
using SCFrame.UI;
using UnityEngine;

namespace GameCore.UI
{
    public class UINodeBook : _ASCUINodeBase
    {
        public UINodeBook(SCUIShowType _showType) : base(_showType)
        {
        }

        public override bool needHideWhenEnterNewSameTypeNode => false;

        public override bool needShowWhenQuitNewSameTypeNode => false;

        public override bool canQuitByEsc => true;

        public override bool canQuitByMouseRight => false;

        public override bool ignoreOnUIList => false;

        public override SCUINodeFuncType nodeFuncType => SCUINodeFuncType.COMMON;

        public override bool needMoveToBottomWhenHide => false;

        private GameObject _m_panelGO;
        private UIPanelBook _m_bookPanel;
        private UIMonoBook _m_bookMono;

        public override string GetNodeName()
        {
            return nameof(UINodeBook);
        }

        public override string GetResName()
        {
            return "panel_book";
        }

        public override void OnEnterNode()
        {
            _m_panelGO = ResourcesHelper.LoadGameObject(GetResName(), GetRootTransform(), true);
            if (_m_panelGO == null)
            {
                Debug.LogError("未找到资源名为" + GetResName() + "的资源!!!");
                return;
            }
            _m_bookMono = _m_panelGO.GetComponent<UIMonoBook>();
            if (_m_bookMono == null)
            {
                Debug.LogError("资源名为" + GetResName() + "的资源上不存在对应的Mono!!!");
                return;
            }

            _m_bookPanel = new UIPanelBook(_m_bookMono, _m_showType);
            _m_bookPanel.Initialize();
        }

        public override void OnHideNode()
        {
            _m_bookPanel?.HidePanel();
        }

        public override void OnQuitNode()
        {
            _m_bookPanel?.Discard();
        }

        public override void OnShowNode()
        {
            _m_bookPanel?.ShowPanel();
        }
    }
}
