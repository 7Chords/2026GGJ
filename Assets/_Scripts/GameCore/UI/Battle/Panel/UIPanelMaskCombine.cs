using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using SCFrame;
using UnityEngine;
using System;

namespace GameCore.UI
{
    public class UIPanelMaskCombine : _ASCUIAnimPanelBase<UIMonoMaskCombine>
    {
        private UIPanelMaskCombinePartContainer _m_partContainer;
        private UIPanelMaskCombineFace _m_playerFace;
        private UIPanelEnemyMask _m_enemyMask;
        public UIPanelMaskCombine(UIMonoMaskCombine _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_partContainer = new UIPanelMaskCombinePartContainer(mono.monoPartContainer);
            _m_playerFace = new UIPanelMaskCombineFace(mono.monoFace, SCUIShowType.INTERNAL);
            _m_enemyMask = new UIPanelEnemyMask(mono.monoEnemyMask, SCUIShowType.INTERNAL);

        }

        public override void BeforeDiscard()
        {
            _m_partContainer?.Discard();
            _m_partContainer = null;
            _m_playerFace?.Discard();
            _m_playerFace = null;
            _m_enemyMask?.Discard();
            _m_enemyMask = null;
        }

        public override void OnHidePanel()
        {
            mono.btnConfirm.onClick.RemoveAllListeners();
            mono.btnDeck.onClick.RemoveAllListeners();
            _m_partContainer?.HidePanel();
            _m_playerFace?.HidePanel();
            _m_enemyMask?.HidePanel();

        }


        public override void OnShowPanel()
        {

            _m_playerFace?.ShowPanel();
            _m_partContainer?.ShowPanel();
            _m_enemyMask?.ShowPanel();

            mono.btnConfirm.onClick.AddListener(OnConfirmClick);
            mono.btnDeck.onClick.AddListener(() =>
            {
                AudioMgr.instance.PlaySfx("sfx_click");
                UICoreMgr.instance.AddNode(new UINodeDeck(SCFrame.UI.SCUIShowType.ADDITION));
            });
            
            refreshShow();
        }

        private void OnConfirmClick()
        {
            AudioMgr.instance.PlaySfx("sfx_click");            
            UICoreMgr.instance.AddNode(new UINodeBattle(SCUIShowType.FULL));
            BattleManager.instance.StartBattle();
        }
        
        private void refreshShow()
        {
            mono.imgHealthBar.fillAmount = (float)GameModel.instance.playerInfo.playerHealth / GameModel.instance.playerInfo.playerMaxHealth;
            mono.txtHealth.text = GameModel.instance.playerInfo.playerHealth +"/" + GameModel.instance.playerInfo.playerMaxHealth;
        }

    }
}
