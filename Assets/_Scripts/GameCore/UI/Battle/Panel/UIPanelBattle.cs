using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameCore.RefData;
using GameCore;
using SCFrame;

namespace GameCore.UI
{
    public class UIPanelBattle : _ASCUIAnimPanelBase<UIMonoBattle>
    {
        private UIPanelBattleFace _m_playerBattleFace;
        private UIPanelBattleFace _m_enemyBattleFace;

        public UIPanelBattle(UIMonoBattle _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_playerBattleFace = new UIPanelBattleFace(mono.monoPlayerFace,SCUIShowType.INTERNAL);
            _m_enemyBattleFace = new UIPanelBattleFace(mono.monoEnemyFace, SCUIShowType.INTERNAL);

        }

        public override void BeforeDiscard()
        {
            _m_playerBattleFace?.Discard();
            _m_enemyBattleFace?.Discard();

        }

        public override void OnHidePanel()
        {
            SCMsgCenter.UnregisterMsgAct(SCMsgConst.PLAYER_HURT, refreshShow);
            SCMsgCenter.UnregisterMsgAct(SCMsgConst.PLAYER_HEAL, refreshShow);
            SCMsgCenter.UnregisterMsgAct(SCMsgConst.ENEMY_HURT, refreshShow);
            SCMsgCenter.UnregisterMsgAct(SCMsgConst.ENEMY_HEAL, refreshShow);
            _m_playerBattleFace?.HidePanel();
            _m_enemyBattleFace?.HidePanel();
        }

        public override void OnShowPanel()
        {
            SCMsgCenter.RegisterMsgAct(SCMsgConst.PLAYER_HURT, refreshShow);
            SCMsgCenter.RegisterMsgAct(SCMsgConst.PLAYER_HEAL, refreshShow);
            SCMsgCenter.RegisterMsgAct(SCMsgConst.ENEMY_HURT, refreshShow);
            SCMsgCenter.RegisterMsgAct(SCMsgConst.ENEMY_HEAL, refreshShow);

            _m_playerBattleFace?.ShowPanel();
            _m_enemyBattleFace?.ShowPanel();
            refreshShow();
        }

        private void refreshShow()
        {
            mono.txtHealth_player.text = GameModel.instance.playerInfo.currentHealth + "/" + GameModel.instance.playerInfo.maxHealth;
            mono.txtHealth_enemy.text = GameModel.instance.curEnemyInfo.currentHealth + "/" + GameModel.instance.curEnemyInfo.maxHealth;

        }
    }
}
