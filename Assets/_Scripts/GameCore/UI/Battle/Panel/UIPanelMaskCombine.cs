using SCFrame;
using SCFrame.UI;
using System;
using UnityEngine.EventSystems;

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
            SCMsgCenter.UnregisterMsgAct(SCMsgConst.NEW_GANE_START, onNewBattleStart);

            mono.btnConfirm.RemoveClickDown(OnBtnConfirmClick);
            mono.btnDeck.RemoveClickDown(onBtnDeckClickDown);
            mono.btnGuide.RemoveClickDown(onBtnGuideClickDown);
            mono.btnSetting.RemoveClickDown(onBtnSettingClickDown);

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
            mono.btnSetting.AddMouseLeftClickDown(onBtnSettingClickDown);

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
            mono.txtBattleOrder.text = GameModel.instance.curTurnOwner == ETurnOwnerType.PLAYER ? "我方先手" : "敌方先手";
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
    }
}
