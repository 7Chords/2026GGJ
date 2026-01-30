using SCFrame;
using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.UI
{
    public class UINodeEnemyMask : _ASCUINodeBase
    {
        public UINodeEnemyMask(SCUIShowType _showType) : base(_showType)
        {
        }

        public override bool needHideWhenEnterNewSameTypeNode => false;

        public override bool needShowWhenQuitNewSameTypeNode => false;

        public override bool canQuitByEsc => true;

        public override bool canQuitByMouseRight => true;

        public override bool ignoreOnUIList => false;

        public override SCUINodeFuncType nodeFuncType => SCUINodeFuncType.BATTLE;

        public override bool needMoveToBottomWhenHide => false;


        private GameObject _m_panelGO;
        private UIPanelEnemyMask _m_enemyMaskPanel;
        private UIMonoEnemyMask _m_enemyMaskMono;
        public override string GetNodeName()
        {
            return nameof(UINodeEnemyMask);
        }

        public override string GetResName()
        {
            return "panel_enemy_mask";
        }

        public override void OnEnterNode()
        {
            _m_panelGO = ResourcesHelper.LoadGameObject(GetResName(), GetRootTransform(), true);
            if (_m_panelGO == null)
            {
                Debug.LogError("未找到资源名为" + GetResName() + "的资源!!!");
                return;
            }
            _m_enemyMaskMono = _m_panelGO.GetComponent<UIMonoEnemyMask>();
            if (_m_enemyMaskMono == null)
            {
                Debug.LogError("资源名为" + GetResName() + "的资源上不存在对应的Mono!!!");
                return;
            }

            _m_enemyMaskPanel = new UIPanelEnemyMask(_m_enemyMaskMono, _m_showType);
            _m_enemyMaskPanel.Initialize();
        }

        public override void OnHideNode()
        {
            _m_enemyMaskPanel?.HidePanel();

        }

        public override void OnQuitNode()
        {
            _m_enemyMaskPanel?.Discard();

        }

        public override void OnShowNode()
        {
            _m_enemyMaskPanel?.ShowPanel();

        }
    }
}
