using SCFrame.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameCore.RefData;
using GameCore;
using GameCore.Battle;
using SCFrame;
using DG.Tweening;

namespace GameCore.UI
{
    public class UIPanelBattle : _ASCUIAnimPanelBase<UIMonoBattle>
    {
        public static UIPanelBattle Current { get; private set; }

        private UIPanelBattleFace _m_playerBattleFace;
        private UIPanelBattleFace _m_enemyBattleFace;

        private TweenContainer _m_tweenContainer;
        Coroutine _defeatFaceEffectRoutine;
        List<UIPanelBattlePart> _defeatHiddenPartPanels;
        public UIPanelBattle(UIMonoBattle _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            Current = this;
            _m_playerBattleFace = new UIPanelBattleFace(mono.monoPlayerFace,SCUIShowType.INTERNAL);
            _m_enemyBattleFace = new UIPanelBattleFace(mono.monoEnemyFace, SCUIShowType.INTERNAL);
            _m_tweenContainer = new TweenContainer();
        }

        public override void BeforeDiscard()
        {
            if (Current == this)
                Current = null;
            if (_defeatFaceEffectRoutine != null && mono != null)
            {
                mono.StopCoroutine(_defeatFaceEffectRoutine);
                _defeatFaceEffectRoutine = null;
            }

            RestoreDefeatBattlePartItemsVisibility();
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
            panel.PlayMouthAttackShake(
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
        }

        private void onPlayerHurt(object[] _objs)
        {
            if (_objs == null || _objs.Length == 0)
                return;
            int amount = (int)_objs[0];
            if (amount > 0 && mono.goPlayerHealth != null)
                GameCommon.ShowDamageFloatText(amount, mono.imgPlayerHealthBar.transform.position);
            _m_tweenContainer?.RegDoTween(mono.goPlayerHealth.transform.DOShakePosition(mono.healthShakeDuration, mono.healthShakeStrength));
            _m_playerBattleFace?.PlayBodyDamageFeedback(_m_tweenContainer);
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
            _m_enemyBattleFace?.PlayBodyDamageFeedback(_m_tweenContainer);
            refreshShow();
        }

        /// <summary>
        /// Plays defeat dissolve on the losing side face <see cref="UIMonoBattleFace.imgFace"/>, then invokes <paramref name="onComplete"/>.
        /// Returns false if there is nothing to play (caller should run end flow immediately).
        /// </summary>
        public bool TryRunDefeatFaceEffectThen(bool playerWon, Action onComplete)
        {
            UIMonoBattleFace face = playerWon ? mono.monoEnemyFace : mono.monoPlayerFace;
            if (face == null || face.imgFace == null || mono == null)
                return false;

            if (_defeatFaceEffectRoutine != null)
            {
                mono.StopCoroutine(_defeatFaceEffectRoutine);
                _defeatFaceEffectRoutine = null;
            }

            RestoreDefeatBattlePartItemsVisibility();
            UIPanelBattleFace facePanel = playerWon ? _m_enemyBattleFace : _m_playerBattleFace;
            if (_defeatHiddenPartPanels == null)
                _defeatHiddenPartPanels = new List<UIPanelBattlePart>();
            else
                _defeatHiddenPartPanels.Clear();

            facePanel?.HideAllBattlePartItemsForDefeatFx(_defeatHiddenPartPanels);

            float duration = Mathf.Max(0.05f, face.defeatFaceEffectDuration);
            _defeatFaceEffectRoutine = mono.StartCoroutine(CoDefeatFaceEffect(face.imgFace, face.defeatFaceHideImageWhenDone, duration, () =>
            {
                _defeatFaceEffectRoutine = null;
                RestoreDefeatBattlePartItemsVisibility();
                onComplete?.Invoke();
            }));
            return true;
        }

        void RestoreDefeatBattlePartItemsVisibility()
        {
            if (_defeatHiddenPartPanels == null)
                return;
            for (int i = 0; i < _defeatHiddenPartPanels.Count; i++)
                _defeatHiddenPartPanels[i]?.ShowPanel();
            _defeatHiddenPartPanels.Clear();
        }

        IEnumerator CoDefeatFaceEffect(Image imgFace, bool hideWhenDone, float duration, Action onDone)
        {
            GameObject go = imgFace.gameObject;
            bool wasActive = go.activeSelf;
            go.SetActive(true);

            UIDefeatFaceMaterialDriver driver = imgFace.GetComponent<UIDefeatFaceMaterialDriver>();
            if (driver == null)
                driver = go.AddComponent<UIDefeatFaceMaterialDriver>();

            driver.Progress = 0f;
            yield return driver.CoAnimateProgress(0f, 1f, duration, AnimationCurve.EaseInOut(0f, 0f, 1f, 1f));

            driver.ResetToDefaultMaterial();
            driver.Progress = 0f;

            if (hideWhenDone)
                go.SetActive(false);
            else
                go.SetActive(wasActive);

            onDone?.Invoke();
        }
    }
}
