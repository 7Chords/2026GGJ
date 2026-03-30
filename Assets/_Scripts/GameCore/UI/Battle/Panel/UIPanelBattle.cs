using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameCore.RefData;
using GameCore;
using GameCore.Battle;
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
            SCMsgCenter.UnregisterMsg(SCMsgConst.PLAYER_HURT, onPlayerHurt);
            SCMsgCenter.UnregisterMsgAct(SCMsgConst.PLAYER_HEAL, refreshShow);
            SCMsgCenter.UnregisterMsg(SCMsgConst.ENEMY_HURT, onEnemyHurt);
            SCMsgCenter.UnregisterMsgAct(SCMsgConst.ENEMY_HEAL, refreshShow);
            SCMsgCenter.UnregisterMsg(SCMsgConst.PART_MOUTH_ATTACK, onMouthAttack);
            _m_playerBattleFace?.HidePanel();
            _m_enemyBattleFace?.HidePanel();
        }

        public override void OnShowPanel()
        {
            SCMsgCenter.RegisterMsg(SCMsgConst.PLAYER_HURT, onPlayerHurt);
            SCMsgCenter.RegisterMsgAct(SCMsgConst.PLAYER_HEAL, refreshShow);
            SCMsgCenter.RegisterMsg(SCMsgConst.ENEMY_HURT, onEnemyHurt);
            SCMsgCenter.RegisterMsgAct(SCMsgConst.ENEMY_HEAL, refreshShow);
            SCMsgCenter.RegisterMsg(SCMsgConst.PART_MOUTH_ATTACK, onMouthAttack);

            _m_playerBattleFace?.ShowPanel();
            _m_enemyBattleFace?.ShowPanel();
            refreshShow();
        }

        private void onMouthAttack(object[] _objs)
        {
            if (_objs == null || _objs.Length == 0)
            {
                MouthAttackCoordinator.ApplyPendingDamage();
                MouthAttackCoordinator.NotifyAnimationComplete();
                return;
            }
            PartInfo caster = _objs[0] as PartInfo;
            if (caster == null)
            {
                MouthAttackCoordinator.ApplyPendingDamage();
                MouthAttackCoordinator.NotifyAnimationComplete();
                return;
            }
            UIPanelBattlePart panel = caster.isEnemyPart
                ? _m_enemyBattleFace.FindPartPanel(caster)
                : _m_playerBattleFace.FindPartPanel(caster);
            if (panel == null)
            {
                MouthAttackCoordinator.ApplyPendingDamage();
                MouthAttackCoordinator.NotifyAnimationComplete();
                return;
            }
            UIPanelBattleFace opponentFace = caster.isEnemyPart ? _m_playerBattleFace : _m_enemyBattleFace;
            Vector3 targetWorld = opponentFace.GetWorldCenterForGridPositions(caster.curEffectFacePosList);
            panel.PlayMouthLungeTowardWorld(targetWorld,
                () => MouthAttackCoordinator.ApplyPendingDamage(),
                () => MouthAttackCoordinator.NotifyAnimationComplete());
        }

        private void refreshShow()
        {
            PlayerInfo playerInfo = GameModel.instance.playerInfo;
            EnemyInfo currentEnemyInfo = GameModel.instance.curEnemyInfo;
            mono.txtPlayerHealth.text = playerInfo.currentHealth + "/" + playerInfo.maxHealth;
            if (mono.imgPlayerHealthBar != null && playerInfo.maxHealth > 0)
                mono.imgPlayerHealthBar.fillAmount = (float)playerInfo.currentHealth / playerInfo.maxHealth;
            if (currentEnemyInfo != null)
            {
                mono.txtEnemyHealth.text = currentEnemyInfo.currentHealth + "/" + currentEnemyInfo.maxHealth;
                if (mono.imgEnemyHealthBar != null && currentEnemyInfo.maxHealth > 0)
                    mono.imgEnemyHealthBar.fillAmount = (float)currentEnemyInfo.currentHealth / currentEnemyInfo.maxHealth;
            }
            refreshIsBossShow();
        }

        private void onPlayerHurt(object[] _objs)
        {
            if (_objs == null || _objs.Length == 0)
                return;
            int amount = (int)_objs[0];
            if (amount > 0 && mono.goPlayerHealth != null)
                GameCommon.ShowDamageFloatText(amount, mono.imgPlayerHealthBar.transform.position);
            _m_tweenContainer?.RegDoTween(mono.goPlayerHealth.transform.DOShakePosition(mono.healthShakeDuration, mono.healthShakeStrength));
            refreshShow();
        }

        private void onEnemyHurt(object[] _objs)
        {
            if (_objs == null || _objs.Length == 0)
                return;
            int amount = (int)_objs[0];
            if (amount > 0 && mono.goEnemyHealth != null)
                GameCommon.ShowDamageFloatText(amount, mono.imgEnemyHealthBar.transform.position);
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
