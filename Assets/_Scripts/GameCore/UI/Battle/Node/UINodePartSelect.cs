using SCFrame;
using SCFrame.UI;
using UnityEngine;

namespace GameCore.UI
{
    public class UINodePartSelect : _ASCUINodeBase
    {
        public UINodePartSelect(SCUIShowType showType) : base(showType)
        {
        }

        public override bool needHideWhenEnterNewSameTypeNode => false;

        public override bool needShowWhenQuitNewSameTypeNode => false;

        public override bool canQuitByEsc => false;

        public override bool canQuitByMouseRight => false;

        public override bool ignoreOnUIList => false;

        public override SCUINodeFuncType nodeFuncType => SCUINodeFuncType.BATTLE;

        public override bool needMoveToBottomWhenHide => false;

        private GameObject _m_panelGO;
        private UIPanelPartSelect _m_partSelectPanel;
        private UIMonoPartSelect _m_partSelectMono;

        public override string GetNodeName()
        {
            return nameof(UINodePartSelect);
        }

        public override string GetResName()
        {
            return "panel_part_select";
        }

        public override void OnEnterNode()
        {
            _m_panelGO = ResourcesHelper.LoadGameObject(GetResName(), GetRootTransform(), true);
            if (_m_panelGO == null)
            {
                Debug.LogError("未找到资源名为" + GetResName() + "的资源!!!");
                return;
            }

            _m_partSelectMono = _m_panelGO.GetComponent<UIMonoPartSelect>();
            if (_m_partSelectMono == null)
            {
                Debug.LogError("资源名为" + GetResName() + "的资源上不存在对应的Mono!!!");
                return;
            }

            _m_partSelectPanel = new UIPanelPartSelect(_m_partSelectMono, _m_showType);
            _m_partSelectPanel.Initialize();
        }

        public override void OnHideNode()
        {
            _m_partSelectPanel?.HidePanel();
        }

        public override void OnQuitNode()
        {
            _m_partSelectPanel?.Discard();
        }

        public override void OnShowNode()
        {
            _m_partSelectPanel?.ShowPanel();
        }
    }
}
