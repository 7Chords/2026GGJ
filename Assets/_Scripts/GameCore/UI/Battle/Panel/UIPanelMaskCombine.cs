using DG.Tweening;
using GameCore;
using GameCore.Battle;
using SCFrame;
using SCFrame.UI;
using System;
using System.Collections.Generic;
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

        private bool _m_entityHealthPreviewActive;
        private int _m_previewDmgPlayerBody;
        private int _m_previewDmgEnemyBody;

        static readonly int LiquidFillId = Shader.PropertyToID("_Fill");

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
            SCMsgCenter.UnregisterMsgAct(SCMsgConst.CHEAT_DEBUG_UI_REFRESH, onCheatDebugUiRefresh);
            SCMsgCenter.UnregisterMsgAct(SCMsgConst.NEW_GANE_START, onNewBattleStart);
            SCMsgCenter.UnregisterMsg(SCMsgConst.FACE_PART_TARGET_PREVIEW_VALUES, onFacePartTargetPreviewValues);
            SCMsgCenter.UnregisterMsgAct(SCMsgConst.FACE_PART_TARTGET_PREVIEW_CANCEL, onFacePartTargetPreviewCancel);
            SCMsgCenter.UnregisterMsg(SCMsgConst.PLACE_PART_SUCCESS, onPlayerHandBusyChanged);
            SCMsgCenter.UnregisterMsg(SCMsgConst.REPLACE_PART_POS_SUCCESS, onPlayerHandBusyChanged);
            SCMsgCenter.UnregisterMsg(SCMsgConst.REPLACE_PART_POS_FAIL, onPlayerHandBusyChanged);
            SCMsgCenter.UnregisterMsg(SCMsgConst.ENEMY_PASSIVE_TRIGGER, onEnemyPassiveTrigger);

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
            SCMsgCenter.RegisterMsgAct(SCMsgConst.CHEAT_DEBUG_UI_REFRESH, onCheatDebugUiRefresh);
            SCMsgCenter.RegisterMsgAct(SCMsgConst.NEW_GANE_START, onNewBattleStart);
            SCMsgCenter.RegisterMsg(SCMsgConst.FACE_PART_TARGET_PREVIEW_VALUES, onFacePartTargetPreviewValues);
            SCMsgCenter.RegisterMsgAct(SCMsgConst.FACE_PART_TARTGET_PREVIEW_CANCEL, onFacePartTargetPreviewCancel);
            SCMsgCenter.RegisterMsg(SCMsgConst.PLACE_PART_SUCCESS, onPlayerHandBusyChanged);
            SCMsgCenter.RegisterMsg(SCMsgConst.REPLACE_PART_POS_SUCCESS, onPlayerHandBusyChanged);
            SCMsgCenter.RegisterMsg(SCMsgConst.REPLACE_PART_POS_FAIL, onPlayerHandBusyChanged);
            SCMsgCenter.RegisterMsg(SCMsgConst.ENEMY_PASSIVE_TRIGGER, onEnemyPassiveTrigger);

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

            _m_entityHealthPreviewActive = false;
            _m_previewDmgPlayerBody = 0;
            _m_previewDmgEnemyBody = 0;
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

        private void onCheatDebugUiRefresh()
        {
            refreshShow();
            _m_partContainer?.ReloadBusyParts();
        }

        private void onEnemyPassiveTrigger(object[] _objs)
        {
            if (_objs == null || _objs.Length == 0)
                return;
            string passiveName = _objs[0] as string;
            if (string.IsNullOrEmpty(passiveName))
                return;
            if (mono?.imgEnemyHealthBar == null)
                return;
            GameCommon.ShowEffectText(passiveName, mono.imgEnemyHealthBar.transform.position);
        }

        private void refreshShow()
        {
            if (_m_entityHealthPreviewActive)
                applyEntityHealthPreview();
            else
                refreshShowPlain();
            refreshBattleOrderObjects();
        }

        private void refreshShowPlain()
        {
            mono.imgHealthBar.fillAmount = (float)GameModel.instance.playerInfo.currentHealth / GameModel.instance.playerInfo.maxHealth;
            mono.txtHealth.text = GameModel.instance.playerInfo.currentHealth + "/" + GameModel.instance.playerInfo.maxHealth;
            mono.txtCoin.text = GameModel.instance.playerInfo.playerMoney.ToString();
            refreshBusyCountText();
            if (GameModel.instance.curEnemyInfo != null)
            {
                var e = GameModel.instance.curEnemyInfo;
                float ratio = e.maxHealth > 0 ? (float)e.currentHealth / e.maxHealth : 0f;
                if (mono.imgEnemyHealthBar != null)
                    mono.imgEnemyHealthBar.fillAmount = ratio;
                mono.txtEnemyHealth.text = e.currentHealth + "/" + e.maxHealth;
                applyEnemyLiquidHealthFill(ratio);
            }
        }

        /// <summary> Hand (busy) count / max hand size from <see cref="GameConst.BUSY_CARD_MAX_COUNT"/>. </summary>
        private void refreshBusyCountText()
        {
            if (mono.txtBusyCount == null)
                return;
            var p = GameModel.instance?.playerInfo;
            int n = p?.busyPartInfoList != null ? p.busyPartInfoList.Count : 0;
            mono.txtBusyCount.text = n + "/" + GameModel.instance.GetPlayerMaxHandCards();
        }

        private void applyEntityHealthPreview()
        {
            var p = GameModel.instance.playerInfo;
            var e = GameModel.instance.curEnemyInfo;
            if (e == null)
            {
                refreshShowPlain();
                return;
            }
            if (mono.txtHealth != null)
                SCUICommon.ApplyHealthLinePreview(mono.txtHealth, mono.previewDamageColor, mono.previewHealColor,
                    p.currentHealth, p.maxHealth, _m_previewDmgPlayerBody, 0);
            if (mono.imgHealthBar != null && p.maxHealth > 0)
            {
                int hpP = Mathf.Clamp(p.currentHealth - _m_previewDmgPlayerBody, 0, p.maxHealth);
                mono.imgHealthBar.fillAmount = (float)hpP / p.maxHealth;
            }
            if (mono.txtEnemyHealth != null)
                SCUICommon.ApplyHealthLinePreview(mono.txtEnemyHealth, mono.previewDamageColor, mono.previewHealColor,
                    e.currentHealth, e.maxHealth, _m_previewDmgEnemyBody, 0);
            if (e.maxHealth > 0)
            {
                int hpE = Mathf.Clamp(e.currentHealth - _m_previewDmgEnemyBody, 0, e.maxHealth);
                float ratio = (float)hpE / e.maxHealth;
                if (mono.imgEnemyHealthBar != null)
                    mono.imgEnemyHealthBar.fillAmount = ratio;
                applyEnemyLiquidHealthFill(ratio);
            }
            mono.txtCoin.text = GameModel.instance.playerInfo.playerMoney.ToString();
            refreshBusyCountText();
        }

        /// <summary>Show player-first or enemy-first object groups by <see cref="GameModel.curTurnOwner"/>. </summary>
        private void refreshBattleOrderObjects()
        {
            if (GameModel.instance == null)
                return;
            bool playerFirst = GameModel.instance.curTurnOwner == ETurnOwnerType.PLAYER;
            setActiveAll(mono.goIsPlayerFirstShowList, playerFirst);
            setActiveAll(mono.goIsEnemyFirstShowList, !playerFirst);
        }

        /// <summary> Sync <see cref="UIMonoMaskCombine.imgEnemyHealth"/> liquid shader <c>_Fill</c> with HP ratio (same as enemy bar fill). </summary>
        private void applyEnemyLiquidHealthFill(float fill01)
        {
            if (mono.imgEnemyHealth == null)
                return;
            var mat = mono.imgEnemyHealth.material;
            if (mat != null && mat.HasProperty(LiquidFillId))
                mat.SetFloat(LiquidFillId, Mathf.Clamp01(fill01));
        }

        private static void setActiveAll(List<GameObject> list, bool active)
        {
            if (list == null)
                return;
            for (int i = 0; i < list.Count; i++)
            {
                var go = list[i];
                if (go != null)
                    go.SetActive(active);
            }
        }

        private void onPlayerHandBusyChanged(object[] _objs)
        {
            refreshShow();
        }

        private void onFacePartTargetPreviewValues(object[] _objs)
        {
            if (_objs == null || _objs.Length == 0)
            {
                clearEntityHealthPreview();
                return;
            }
            var payload = _objs[0] as PartPlacementPreviewPayload;
            if (payload == null)
            {
                clearEntityHealthPreview();
                return;
            }
            _m_previewDmgPlayerBody = payload.damageToPlayerBody;
            _m_previewDmgEnemyBody = payload.damageToEnemyBody;
            _m_entityHealthPreviewActive = _m_previewDmgPlayerBody > 0 || _m_previewDmgEnemyBody > 0;
            if (!_m_entityHealthPreviewActive)
            {
                clearEntityHealthPreview();
                return;
            }
            applyEntityHealthPreview();
        }

        private void onFacePartTargetPreviewCancel()
        {
            clearEntityHealthPreview();
        }

        private void clearEntityHealthPreview()
        {
            _m_entityHealthPreviewActive = false;
            _m_previewDmgPlayerBody = 0;
            _m_previewDmgEnemyBody = 0;
            refreshShow();
        }

        private void onNewBattleStart()
        {
            clearEntityHealthPreview();
        }
        private void onBtnSettingClickDown(PointerEventData _data, object[] _objs)
        {
            AudioMgr.instance.PlaySfx("sfx_click");
            UICoreMgr.instance.AddNode(new UINodeSetting(SCUIShowType.ADDITION,true));
        }

        private void onBtnGuideClickDown(PointerEventData _data, object[] _objs)
        {
            AudioMgr.instance.PlaySfx("sfx_click");
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
