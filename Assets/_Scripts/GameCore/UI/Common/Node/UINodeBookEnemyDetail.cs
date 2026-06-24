using GameCore.RefData;
using SCFrame;
using SCFrame.UI;
using UnityEngine;

namespace GameCore.UI
{
    public class UINodeBookEnemyDetail : _ASCUINodeBase
    {
        private EnemyRefObj _m_enemyRef;

        public UINodeBookEnemyDetail(SCUIShowType showType, EnemyRefObj enemyRef) : base(showType)
        {
            _m_enemyRef = enemyRef;
        }

        public override bool needHideWhenEnterNewSameTypeNode => false;

        public override bool needShowWhenQuitNewSameTypeNode => false;

        public override bool canQuitByEsc => true;

        public override bool canQuitByMouseRight => true;

        public override bool ignoreOnUIList => false;

        public override SCUINodeFuncType nodeFuncType => SCUINodeFuncType.COMMON;

        public override bool needMoveToBottomWhenHide => true;

        private GameObject _m_panelGO;
        private UIPanelBookEnemyDetail _m_detailPanel;
        private UIMonoBookEnemyDetail _m_detailMono;

        public override string GetNodeName()
        {
            return nameof(UINodeBookEnemyDetail);
        }

        public override string GetResName()
        {
            return "panel_book_enemy_detail";
        }

        public override void OnEnterNode()
        {
            _m_panelGO = ResourcesHelper.LoadGameObject(GetResName(), GetRootTransform(), true);
            if (_m_panelGO == null)
            {
                Debug.LogError("未找到资源名为" + GetResName() + "的资源!!!");
                return;
            }

            _m_detailMono = _m_panelGO.GetComponent<UIMonoBookEnemyDetail>();
            if (_m_detailMono == null)
            {
                Debug.LogError("资源名为" + GetResName() + "的资源上不存在对应的Mono!!!");
                return;
            }

            _m_detailPanel = new UIPanelBookEnemyDetail(_m_detailMono, _m_showType);
            _m_detailPanel.Initialize();
        }

        public override void OnHideNode()
        {
            _m_detailPanel?.HidePanel();
        }

        public override void OnQuitNode()
        {
            _m_detailPanel?.Discard();
        }

        public override void OnShowNode()
        {
            _m_detailPanel?.ShowPanel();
            _m_detailPanel?.SetInfo(_m_enemyRef);
        }

        public override void CopyData(_ASCUINodeBase anotherNode)
        {
            if (anotherNode is not UINodeBookEnemyDetail other)
                return;

            _m_enemyRef = other._m_enemyRef;
            if (!hasHideNode)
                _m_detailPanel?.SetInfo(_m_enemyRef);
        }
    }
}
