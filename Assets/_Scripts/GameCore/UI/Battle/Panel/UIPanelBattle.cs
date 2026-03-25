using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameCore.RefData;
using GameCore;
using SCFrame;
using DG.Tweening;

namespace GameCore.UI
{
    public class UIPanelBattle : _ASCUIAnimPanelBase<UIMonoBattle>
    {
        private UIPanelBattleFace _m_playerBattleFace;
        private UIPanelBattleFace _m_enemyBattleFace;

        private TweenContainer _m_tweenContainer;
        public UIPanelBattle(UIMonoBattle _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_playerBattleFace = new UIPanelBattleFace(mono.monoPlayerFace,SCUIShowType.INTERNAL);
            _m_enemyBattleFace = new UIPanelBattleFace(mono.monoEnemyFace, SCUIShowType.INTERNAL);
            _m_tweenContainer = new TweenContainer();
        }

        public override void BeforeDiscard()
        {
            _m_playerBattleFace?.Discard();
            _m_enemyBattleFace?.Discard();
            _m_tweenContainer?.KillAllDoTween();
            _m_tweenContainer = null;
        }

        public override void OnHidePanel()
        {
            SCMsgCenter.UnregisterMsgAct(SCMsgConst.PLAYER_HURT, onPlayerHurt);
            SCMsgCenter.UnregisterMsgAct(SCMsgConst.PLAYER_HEAL, refreshShow);
            SCMsgCenter.UnregisterMsgAct(SCMsgConst.ENEMY_HURT, onEnemyHurt);
            SCMsgCenter.UnregisterMsgAct(SCMsgConst.ENEMY_HEAL, refreshShow);
            _m_playerBattleFace?.HidePanel();
            _m_enemyBattleFace?.HidePanel();
        }

        public override void OnShowPanel()
        {
            SCMsgCenter.RegisterMsgAct(SCMsgConst.PLAYER_HURT, onPlayerHurt);
            SCMsgCenter.RegisterMsgAct(SCMsgConst.PLAYER_HEAL, refreshShow);
            SCMsgCenter.RegisterMsgAct(SCMsgConst.ENEMY_HURT, onEnemyHurt);
            SCMsgCenter.RegisterMsgAct(SCMsgConst.ENEMY_HEAL, refreshShow);

            _m_playerBattleFace?.ShowPanel();
            _m_enemyBattleFace?.ShowPanel();
            refreshShow();
        }

        private void refreshShow()
        {
            mono.txtPlayerHealth.text = GameModel.instance.playerInfo.currentHealth + "/" + GameModel.instance.playerInfo.maxHealth;
            mono.txtEnemyHealth.text = GameModel.instance.curEnemyInfo.currentHealth + "/" + GameModel.instance.curEnemyInfo.maxHealth;
            refreshIsBossShow();
        }

        private void onPlayerHurt()
        {
            _m_tweenContainer?.RegDoTween(mono.goPlayerHealth.transform.DOShakePosition(mono.healthShakeDuration, mono.healthShakeStrength));
            refreshShow();
        }
        private void onEnemyHurt()
        {
            _m_tweenContainer?.RegDoTween(mono.goEnemyHealth.transform.DOShakePosition(mono.healthShakeDuration, mono.healthShakeStrength));
            refreshShow();
        }

        private void refreshIsBossShow()
        {
            foreach (var cell in mono.bossShowCellList)
            {
                SCCommon.SetGameObjectEnable(cell.goBossShow, false);
            }
            if (GameModel.instance.curEnemyInfo.enemyRefObj.isBoss)
            {
                SCCommon.SetGameObjectEnable(mono.bossShowCellList.Find(x => x.bossType == GameModel.instance.curEnemyInfo.enemyRefObj.bossType).goBossShow, true);
            }
        }
    }
}
