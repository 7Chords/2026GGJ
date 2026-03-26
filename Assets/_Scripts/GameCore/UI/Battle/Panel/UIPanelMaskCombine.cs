using DG.Tweening;
using SCFrame;
using SCFrame.UI;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameCore.UI
{
    public class UIPanelMaskCombine : _ASCUIAnimPanelBase<UIMonoMaskCombine>
    {
        private UIPanelMaskCombinePartContainer _m_partContainer;
        private UIPanelMaskCombineFace _m_playerFace;
        private UIPanelEnemyMask _m_enemyMask;
        private TweenContainer _m_tweenContainer;
        public UIPanelMaskCombine(UIMonoMaskCombine _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_tweenContainer = new TweenContainer();
            _m_partContainer = new UIPanelMaskCombinePartContainer(mono.monoPartContainer);
            _m_playerFace = new UIPanelMaskCombineFace(mono.monoFace, SCUIShowType.INTERNAL);
            _m_enemyMask = new UIPanelEnemyMask(mono.monoEnemyMask, SCUIShowType.INTERNAL);

        }

        public override void BeforeDiscard()
        {
            _m_tweenContainer?.KillAllDoTween();
            _m_tweenContainer = null;
            _m_partContainer?.Discard();
            _m_partContainer = null;
            _m_playerFace?.Discard();
            _m_playerFace = null;
            _m_enemyMask?.Discard();
            _m_enemyMask = null;
        }

        public override void OnHidePanel()
        {
            SCMsgCenter.UnregisterMsgAct(SCMsgConst.NEW_GANE_START, onNewBattleStart);

            mono.btnConfirm.RemoveClickDown(OnBtnConfirmClick);
            mono.btnDeck.RemoveClickDown(onBtnDeckClickDown);
            mono.btnGuide.RemoveClickDown(onBtnGuideClickDown);
            mono.btnGuide.RemoveMouseEnter(onBtnGuideMouseEnter);
            mono.btnGuide.RemoveMouseExit(onBtnGuideMouseExit);
            mono.btnSetting.RemoveClickDown(onBtnSettingClickDown);
            mono.btnSetting.RemoveMouseEnter(onBtnSettingMouseEnter);
            mono.btnSetting.RemoveMouseExit(onBtnSettingMouseExit);

            _m_partContainer?.HidePanel();
            _m_playerFace?.HidePanel();
            _m_enemyMask?.HidePanel();

        }

        public override void OnShowPanel()
        {
            SCMsgCenter.RegisterMsgAct(SCMsgConst.NEW_GANE_START, onNewBattleStart);

            mono.btnConfirm.AddMouseLeftClickDown(OnBtnConfirmClick);
            mono.btnDeck.AddMouseLeftClickDown(onBtnDeckClickDown);
            mono.btnGuide.AddMouseLeftClickDown(onBtnGuideClickDown);
            mono.btnGuide.AddMouseEnter(onBtnGuideMouseEnter);
            mono.btnGuide.AddMouseExit(onBtnGuideMouseExit);
            mono.btnSetting.AddMouseLeftClickDown(onBtnSettingClickDown);
            mono.btnSetting.AddMouseEnter(onBtnSettingMouseEnter);
            mono.btnSetting.AddMouseExit(onBtnSettingMouseExit);

            _m_playerFace?.ShowPanel();
            _m_partContainer?.ShowPanel();
            _m_enemyMask?.ShowPanel();

            refreshShow();
        }

        private void onBtnDeckClickDown(PointerEventData arg1, object[] arg2)
        {
            AudioMgr.instance.PlaySfx("sfx_click");
            UICoreMgr.instance.AddNode(new UINodeDeck(SCUIShowType.ADDITION, GameModel.instance.playerInfo.deckPartInfoList));
        }

        private void OnBtnConfirmClick(PointerEventData arg1, object[] arg2)
        {
            AudioMgr.instance.PlaySfx("sfx_click");
            UICoreMgr.instance.AddNode(new UINodeBattle(SCUIShowType.FULL));
            BattleManager.instance.StartBattle();
        }

        
        private void refreshShow()
        {
            mono.imgHealthBar.fillAmount = (float)GameModel.instance.playerInfo.currentHealth / GameModel.instance.playerInfo.maxHealth;
            mono.txtHealth.text = GameModel.instance.playerInfo.currentHealth +"/" + GameModel.instance.playerInfo.maxHealth;
            mono.txtBattleOrder.text = GameModel.instance.curTurnOwner == ETurnOwnerType.PLAYER ? "玩家回合" : "敌人回合";
            mono.txtCoin.text = GameModel.instance.playerInfo.playerMoney.ToString();
            if(GameModel.instance.curEnemyInfo != null)
            {
                mono.imgEnemyHealthBar.fillAmount = (float)GameModel.instance.curEnemyInfo.currentHealth / GameModel.instance.curEnemyInfo.maxHealth;
                mono.txtEnemyHealth.text = GameModel.instance.curEnemyInfo.currentHealth + "/" + GameModel.instance.curEnemyInfo.maxHealth;
            }
        }
        private void onNewBattleStart()
        {
            refreshShow();
        }
        private void onBtnSettingClickDown(PointerEventData _data, object[] _objs)
        {
            UICoreMgr.instance.AddNode(new UINodeSetting(SCUIShowType.ADDITION));
        }

        private void onBtnGuideClickDown(PointerEventData _data, object[] _objs)
        {
            UICoreMgr.instance.AddNode(new UINodeGuideBattle(SCUIShowType.ADDITION));
        }

        private void onBtnGuideMouseEnter(PointerEventData _arg1, object[] _arg2)
        {
            if (mono.btnGuide == null) return;
            _m_tweenContainer.RegDoTween(mono.btnGuide.transform.DOScale(mono.scaleMouseEnter, mono.scaleChgDuration));
        }

        private void onBtnGuideMouseExit(PointerEventData _arg1, object[] _arg2)
        {
            if (mono.btnGuide == null) return;
            _m_tweenContainer.RegDoTween(mono.btnGuide.transform.DOScale(Vector3.one, mono.scaleChgDuration));
        }

        private void onBtnSettingMouseEnter(PointerEventData _arg1, object[] _arg2)
        {
            if (mono.btnSetting == null) return;
            _m_tweenContainer.RegDoTween(mono.btnSetting.transform.DOScale(mono.scaleMouseEnter, mono.scaleChgDuration));
        }

        private void onBtnSettingMouseExit(PointerEventData _arg1, object[] _arg2)
        {
            if (mono.btnSetting == null) return;
            _m_tweenContainer.RegDoTween(mono.btnSetting.transform.DOScale(Vector3.one, mono.scaleChgDuration));
        }
    }
}
