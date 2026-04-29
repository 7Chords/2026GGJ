using DG.Tweening;
using GameCore;
using GameCore.Helpers;
using SCFrame;
using SCFrame.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameCore.UI
{
    public class UIPanelBattlePart : _ASCUIPanelBase<UIMonoBattlePart>
    {
        private PartInfo _m_partInfo;
        private TweenContainer _m_tweenContainer;

        public PartInfo partInfo => _m_partInfo;
        private List<UIPanelPartBuff> _m_partBuffItemList;

        public UIPanelBattlePart(UIMonoBattlePart _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_tweenContainer = new TweenContainer();
            _m_partBuffItemList = new List<UIPanelPartBuff>();
        }

        public override void BeforeDiscard()
        {
            resetPartImageColors();
            _m_tweenContainer?.KillAllDoTween();
            _m_tweenContainer = null;
            if (_m_partBuffItemList != null)
            {
                foreach (var item in _m_partBuffItemList)
                    item?.Discard();
                _m_partBuffItemList.Clear();
            }
        }

        public override void OnHidePanel()
        {
            SCMsgCenter.UnregisterMsg(SCMsgConst.PART_HURT, onPartHurt);
            SCMsgCenter.UnregisterMsg(SCMsgConst.PART_HEAL, onPartHeal);
            SCMsgCenter.UnregisterMsg(SCMsgConst.PART_ACTIVE_START, onPartActiveStart);
            SCMsgCenter.UnregisterMsg(SCMsgConst.PART_TRIGGER_SUCCESS, onPartTriggerSuccess);
            SCMsgCenter.UnregisterMsg(SCMsgConst.PART_TRIGGER_FAIL, onPartTriggerFail);
            SCMsgCenter.UnregisterMsg(SCMsgConst.PART_TRIGGER_EFFECT, onPartTriggerEffect);

            SCMsgCenter.UnregisterMsg(SCMsgConst.PART_ACTIVE_END, onPartActiveEnd);
            SCMsgCenter.UnregisterMsgAct(SCMsgConst.BATTLE_ENEMY_PART_ORDER_CHG, onBattleEnemyPartOrderChg);
            SCMsgCenter.UnregisterMsg(SCMsgConst.PART_BUFF_ADD, onPartBuffAdd);
            SCMsgCenter.UnregisterMsg(SCMsgConst.PART_BUFF_UPDATE, onPartUpdate);
            SCMsgCenter.UnregisterMsg(SCMsgConst.PART_BUFF_REMOVE, onPartBuffRemove);
            SCMsgCenter.UnregisterMsg(SCMsgConst.PART_POSITIVE_BUFF_GAIN, onCombatGainVfx);
            SCMsgCenter.UnregisterMsg(SCMsgConst.PART_DEBUFF_GAIN, onDebuffGainVfx);

            mono.imgGO.RemoveMouseEnter(onMouseEnter);
            mono.imgGO.RemoveMouseExit(onMouseExit);
            if (_m_partBuffItemList != null)
            {
                foreach (var item in _m_partBuffItemList)
                    item?.HidePanel();
            }
        }

        public override void OnShowPanel()
        {
            SCMsgCenter.RegisterMsg(SCMsgConst.PART_HURT, onPartHurt);
            SCMsgCenter.RegisterMsg(SCMsgConst.PART_HEAL, onPartHeal);
            SCMsgCenter.RegisterMsg(SCMsgConst.PART_ACTIVE_START, onPartActiveStart);
            SCMsgCenter.RegisterMsg(SCMsgConst.PART_TRIGGER_SUCCESS, onPartTriggerSuccess);
            SCMsgCenter.RegisterMsg(SCMsgConst.PART_TRIGGER_FAIL, onPartTriggerFail);
            SCMsgCenter.RegisterMsg(SCMsgConst.PART_TRIGGER_EFFECT, onPartTriggerEffect);

            SCMsgCenter.RegisterMsg(SCMsgConst.PART_ACTIVE_END, onPartActiveEnd);
            SCMsgCenter.RegisterMsgAct(SCMsgConst.BATTLE_ENEMY_PART_ORDER_CHG, onBattleEnemyPartOrderChg);
            SCMsgCenter.RegisterMsg(SCMsgConst.PART_BUFF_ADD, onPartBuffAdd);
            SCMsgCenter.RegisterMsg(SCMsgConst.PART_BUFF_UPDATE, onPartUpdate);
            SCMsgCenter.RegisterMsg(SCMsgConst.PART_BUFF_REMOVE, onPartBuffRemove);
            SCMsgCenter.RegisterMsg(SCMsgConst.PART_POSITIVE_BUFF_GAIN, onCombatGainVfx);
            SCMsgCenter.RegisterMsg(SCMsgConst.PART_DEBUFF_GAIN, onDebuffGainVfx);

            mono.imgGO.AddMouseEnter(onMouseEnter);
            mono.imgGO.AddMouseExit(onMouseExit);
            if (_m_partBuffItemList != null)
            {
                foreach (var item in _m_partBuffItemList)
                    item?.ShowPanel();
            }
        }


        public void SetInfo(PartInfo _info)
        {
            _m_partInfo = _info;
            refreshShow();
        }
        public void SetLocalPos(Vector2 _pos)
        {
            GetGameObject().transform.localPosition = _pos;
        }

        /// <summary> Extra shake when face body HP is hurt (layered on top of root shake). </summary>
        public void PlayBodyFollowShake(TweenContainer _tc)
        {
            if (_tc == null)
                return;
            var t = GetGameObject().transform;
            t.DOKill(false);
            float dur = mono.hurtShakeDuration * 0.82f;
            float str = mono.hurtShakeStrength * mono.bodyHurtPartFollowShakeStrengthMul;
            _tc.RegDoTween(t.DOShakePosition(dur, str));
        }

        /// <summary> Local Z rotation shake on the mouth graphic; damage at start of feedback. </summary>
        public void PlayMouthAttackShake(System.Action _onHit, System.Action _onComplete)
        {
            var t = mono.imgGO != null ? mono.imgGO.transform : GetGameObject().transform;
            t.DOKill(false);
            float z = _m_partInfo != null ? _m_partInfo.rotateStep * 90f : 0f;
            void applyBaseRot() { t.rotation = Quaternion.Euler(0f, 0f, z); }
            applyBaseRot();

            var seq = DOTween.Sequence();
            seq.AppendCallback(() => _onHit?.Invoke());
            float dur = Mathf.Max(0.04f, mono.mouthAttackShakeDuration);
            float ang = mono.mouthAttackShakeAngle;
            int vib = Mathf.Clamp(mono.mouthAttackShakeVibrato, 1, 30);
            seq.Append(t.DOShakeRotation(dur, new Vector3(0f, 0f, ang), vib, 90f, true));
            void finish()
            {
                applyBaseRot();
                _onComplete?.Invoke();
            }
            seq.OnComplete(finish);
            seq.OnKill(applyBaseRot);
            _m_tweenContainer?.RegDoTween(seq);
        }
        private void refreshShow()
        {
            if (_m_partInfo == null)
                return;
            mono.imgGO.sprite = ResourcesHelper.LoadAsset<Sprite>(_m_partInfo.partRefObj.partPlayerGameObjectName);
            mono.imgGO.SetNativeSize();
            mono.imgPart.sprite = ResourcesHelper.LoadAsset<Sprite>(_m_partInfo.partRefObj.partPlayerGameObjectName);
            mono.imgPart.SetNativeSize();
            mono.txtHealth.text = PartHealthDisplay.FormatSlashLine(_m_partInfo.currentHealth, _m_partInfo.maxHealth);

            if(_m_partInfo.isEnemyPart)
                mono.txtOrder.text = GameModel.instance.GetEnemyBattleOrderByPartInfo(_m_partInfo).ToString();
            else
                mono.txtOrder.text = GameModel.instance.GetPlayerBattleOrderByPartInfo(_m_partInfo).ToString();

            refreshBuffShow();

            mono.imgGO.transform.rotation = Quaternion.Euler(0, 0, _m_partInfo.rotateStep * 90);
            autoAdjustPosAndRotate(mono.imgGO.gameObject, mono.goHealthInfo, mono.goHealthPosPivot);
            autoAdjustPosAndRotate(mono.imgGO.gameObject, mono.goOrder, mono.goOrderPosPivot);
            autoAdjustPosAndRotate(mono.imgGO.gameObject, mono.goBuff, mono.goBuffPosPivot);

        }

        private void refreshBuffShow()
        {
            if (_m_partBuffItemList != null)
            {
                foreach (var item in _m_partBuffItemList)
                    item?.Discard();

                _m_partBuffItemList.Clear();
            }

            GameObject buffInfoGO = null;
            UIMonoPartBuff monoPartBuff = null;
            UIPanelPartBuff panelPartBuff = null;
            for (int i = 0; i < _m_partInfo.buffLogic.buffList.Count; i++)
            {
                buffInfoGO = ResourcesHelper.LoadGameObject(GameConst.PREFAB_PART_BUFF_ITEM, mono.goBuff.transform);
                monoPartBuff = buffInfoGO.GetComponent<UIMonoPartBuff>();
                if (monoPartBuff != null)
                    panelPartBuff = new UIPanelPartBuff(monoPartBuff, SCUIShowType.INTERNAL);
                panelPartBuff?.SetInfo(_m_partInfo.buffLogic.buffList[i]);
                panelPartBuff?.ShowPanel();
                _m_partBuffItemList.Add(panelPartBuff);
            }
        }
        private void autoAdjustPosAndRotate(GameObject _parent, GameObject _child, Vector2 _pivotPos)
        {
            RectTransform parentRT = _parent.GetComponent<RectTransform>();
            RectTransform childRT = _child.GetComponent<RectTransform>();

            float scale = parentRT.lossyScale.y;

            int rotateMod = _m_partInfo.rotateStep % 2;
            bool isRotated90 = rotateMod != 0;

            float parentVisualW = isRotated90 ? parentRT.rect.height : parentRT.rect.width;
            float parentVisualH = isRotated90 ? parentRT.rect.width : parentRT.rect.height;

            float parentHalfW = parentVisualW * scale * 0.5f;
            float parentHalfH = parentVisualH * scale * 0.5f;

            float childHalfW = childRT.rect.width * scale * 0.5f;
            float childHalfH = childRT.rect.height * scale * 0.5f;

            float x = parentRT.position.x + _pivotPos.x * parentHalfW;
            float y = parentRT.position.y + _pivotPos.y * parentHalfH;

            Vector3 targetPos = new Vector3(x, y, parentRT.position.z);

            _child.transform.position = targetPos;
            _child.transform.rotation = Quaternion.identity;
        }

        private void onMouseExit(PointerEventData arg1, object[] arg2)
        {
            GameCommon.DiscardToolTip();

        }

        private void onMouseEnter(PointerEventData arg1, object[] arg2)
        {
            GetGameObject().transform.SetAsLastSibling();
            GameCommon.ShowTooltip(_m_partInfo,
                new Vector2(GameConst.SHOW_FACE_PART_TIP_SCREEN_RATIO_X_IN_BATTLE, GameConst.SHOW_FACE_PART_TIP_SCREEN_RATIO_Y_IN_BATTLE),
                false);
        }

        private void onPartHurt(object[] _objs)
        {
            if (_objs == null || _objs.Length == 0)
                return;
            PartInfo info = _objs[0] as PartInfo;
            int amount = (int)_objs[1];

            if (_m_partInfo == info)
            {
                _m_tweenContainer.RegDoTween(GetGameObject().transform.DOShakePosition(mono.hurtShakeDuration, mono.hurtShakeStrength));
                playHurtRedFlash();
                ParticleMgr.instance.PlayOneShot("vfx_blood", GetGameObject().GetRectTransform(), Vector2.zero, 1f, true);
                if (!PartHealthDisplay.UseInfiniteHpDisplay(_m_partInfo.maxHealth))
                    mono.txtHealth.text = PartHealthDisplay.FormatSlashLine(_m_partInfo.currentHealth, _m_partInfo.maxHealth);
                GameCommon.ShowDamageFloatText(amount, GetGameObject().transform.position);

            }
        }

        private void resetPartImageColors()
        {
            if (mono.imgGO != null)
            {
                mono.imgGO.DOKill(false);
                mono.imgGO.color = Color.white;
            }
            if (mono.imgPart != null)
            {
                mono.imgPart.DOKill(false);
                mono.imgPart.color = Color.white;
            }
        }

        private void playHurtRedFlash()
        {
            if (mono.imgGO == null) return;
            Color baseTint = Color.white;
            Color flashTint = mono.hurtFlashTint;
            mono.imgGO.DOKill(false);
            if (mono.imgPart != null)
                mono.imgPart.DOKill(false);
            var seq = DOTween.Sequence();
            seq.Append(mono.imgGO.DOColor(flashTint, mono.hurtFlashInDuration).SetEase(Ease.OutQuad));
            if (mono.imgPart != null)
                seq.Join(mono.imgPart.DOColor(flashTint, mono.hurtFlashInDuration).SetEase(Ease.OutQuad));
            seq.Append(mono.imgGO.DOColor(baseTint, mono.hurtFlashOutDuration).SetEase(Ease.InQuad));
            if (mono.imgPart != null)
                seq.Join(mono.imgPart.DOColor(baseTint, mono.hurtFlashOutDuration).SetEase(Ease.InQuad));
            _m_tweenContainer?.RegDoTween(seq);
        }

        private void onPartHeal(object[] _objs)
        {
            if (_objs == null || _objs.Length == 0)
                return;
            PartInfo info = _objs[0] as PartInfo;
            int amount = (int)_objs[1];
            if (_m_partInfo == info)
            {
                if (!PartHealthDisplay.UseInfiniteHpDisplay(_m_partInfo.maxHealth))
                    mono.txtHealth.text = PartHealthDisplay.FormatSlashLine(_m_partInfo.currentHealth, _m_partInfo.maxHealth);
                GameCommon.ShowHealFloatText(amount, GetGameObject().transform.position);
            }
        }
        private void onPartActiveStart(object[] _objs)
        {
            if (_objs == null || _objs.Length == 0)
                return;
            PartInfo info = _objs[0] as PartInfo;
            if (_m_partInfo == info)
            {
                GameCommon.ShowEffectText("行动", GetGameObject().transform.position);
                Tween tween = GetGameObject().transform.DOScale(mono.activeScale, mono.scaleChgDuration);
                _m_tweenContainer?.RegDoTween(tween);
            }

        }

        private void onPartTriggerSuccess(object[] _objs)
        {
            if (_objs == null || _objs.Length == 0)
                return;
            PartInfo info = _objs[0] as PartInfo;
            if (_m_partInfo == info)
            {
                GameCommon.ShowEffectText(_m_partInfo.partRefObj.triggerSuccessTip, GetGameObject().transform.position);
                var refObj = _m_partInfo.partRefObj;
                if (refObj != null)
                {
                    EPartType t = refObj.partType;
                    if (t == EPartType.EYE || t == EPartType.NOSE)
                        playEyeNoseTriggerSuccessBounce();
                }
            }
        }

        /// <summary> Vertical hop + light scale punch; avoids random DOShake used by hurt. </summary>
        private void playEyeNoseTriggerSuccessBounce()
        {
            var rt = GetGameObject().transform as RectTransform;
            if (rt == null)
                return;
            Vector2 baseAp = rt.anchoredPosition;
            float h = mono.triggerSuccessBounceHeight;
            float total = Mathf.Max(0.05f, mono.triggerSuccessBounceDuration);
            float upPortion = Mathf.Clamp(mono.triggerSuccessBounceUpPortion, 0.15f, 0.55f);
            float upT = total * upPortion;
            float downT = total - upT;
            var seq = DOTween.Sequence();
            seq.Append(rt.DOAnchorPos(new Vector2(baseAp.x, baseAp.y + h), upT).SetEase(Ease.OutQuad));
            seq.Append(rt.DOAnchorPos(baseAp, downT).SetEase(Ease.OutBounce));
            if (mono.imgGO != null && mono.triggerSuccessPunchScale > 0.001f)
            {
                seq.Insert(0,
                    mono.imgGO.transform.DOPunchScale(Vector3.one * mono.triggerSuccessPunchScale, total * 0.85f, 5, 0.35f));
            }
            _m_tweenContainer?.RegDoTween(seq);
        }


        private void onPartTriggerFail(object[] _objs)
        {
            if (_objs == null || _objs.Length == 0)
                return;
            PartInfo info = _objs[0] as PartInfo;
            if (_m_partInfo == info)
            {
                GameCommon.ShowEffectText(_m_partInfo.partRefObj.triggerFailTip, GetGameObject().transform.position);
            }

        }
        private void onPartTriggerEffect(object[] _objs)
        {
            if (_objs == null || _objs.Length < 2)
                return;
            PartInfo info = _objs[0] as PartInfo;
            PartInfo casterInfo = _objs[1] as PartInfo;

            if (_m_partInfo == info)
            {

                GameCommon.ShowEffectText(casterInfo.partRefObj.triggerEffectTip, GetGameObject().transform.position);
            }
        }
        private void onPartActiveEnd(object[] _objs)
        {
            if (_objs == null || _objs.Length == 0)
                return;
            PartInfo info = _objs[0] as PartInfo;
            if (_m_partInfo == info)
            {
                Tween tween = GetGameObject().transform.DOScale(Vector3.one, mono.scaleChgDuration);
                _m_tweenContainer?.RegDoTween(tween);

            }
        }

        private void onBattleEnemyPartOrderChg()
        {
            if (_m_partInfo.isEnemyPart)
                mono.txtOrder.text = GameModel.instance.GetEnemyBattleOrderByPartInfo(_m_partInfo).ToString();
            else
                mono.txtOrder.text = GameModel.instance.GetPlayerBattleOrderByPartInfo(_m_partInfo).ToString();

        }

        private void onCombatGainVfx(object[] _objs)
        {
            if (_objs == null || _objs.Length == 0)
                return;
            PartInfo info = _objs[0] as PartInfo;
            if (_m_partInfo == info)
                ParticleMgr.instance.PlayOneShot("vfx_buff", GetGameObject().GetRectTransform(), Vector2.zero, 1f, true);
        }

        private void onDebuffGainVfx(object[] _objs)
        {
            if (_objs == null || _objs.Length == 0)
                return;
            PartInfo info = _objs[0] as PartInfo;
            if (_m_partInfo == info)
                ParticleMgr.instance.PlayOneShot("vfx_debuff", GetGameObject().GetRectTransform(), Vector2.zero, 1f, true);
        }

        private void onPartBuffAdd(object[] _objs)
        {
            if (_objs == null || _objs.Length == 0)
                return;
            BuffInfo info = _objs[0] as BuffInfo;
            if(info.owner == _m_partInfo)
            {
                refreshBuffShow();
            }
        }
        private void onPartBuffRemove(object[] _objs)
        {
            if (_objs == null || _objs.Length == 0)
                return;
            BuffInfo info = _objs[0] as BuffInfo;
            if (info.owner == _m_partInfo)
            {
                refreshBuffShow();
            }
        }
        private void onPartUpdate(object[] _objs)
        {
            if (_objs == null || _objs.Length == 0)
                return;
            BuffInfo info = _objs[0] as BuffInfo;
            if (info.owner == _m_partInfo)
            {
                refreshBuffShow();
            }
        }
    }
}
