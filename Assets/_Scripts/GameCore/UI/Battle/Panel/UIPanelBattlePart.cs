using DG.Tweening;
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
        private void refreshShow()
        {
            if (_m_partInfo == null)
                return;
            mono.imgGO.sprite = ResourcesHelper.LoadAsset<Sprite>(_m_partInfo.partRefObj.partGameObjectName);
            mono.imgGO.SetNativeSize();
            mono.imgPart.sprite = ResourcesHelper.LoadAsset<Sprite>(_m_partInfo.partRefObj.partGameObjectName);
            mono.imgPart.SetNativeSize();
            mono.txtHealth.text = _m_partInfo.currentHealth + "/" + _m_partInfo.maxHealth;

            if(_m_partInfo.isEnemyPart)
                mono.txtOrder.text = GameModel.instance.GetEnemyBattleOrderByPartInfo(_m_partInfo).ToString();
            else
                mono.txtOrder.text = GameModel.instance.GetPlayerBattleOrderByPartInfo(_m_partInfo).ToString();

            refreshBuffShow();

            mono.imgGO.transform.rotation = Quaternion.Euler(0, 0, _m_partInfo.rotateStep * 90);
            //信息子物体自动适配旋转和rect大小
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

            // 是否旋转了 90/270 度
            int rotateMod = _m_partInfo.rotateStep % 2;
            bool isRotated90 = rotateMod != 0;

            // 父物体「视觉上」的宽高（旋转后自动互换）
            float parentVisualW = isRotated90 ? parentRT.rect.height : parentRT.rect.width;
            float parentVisualH = isRotated90 ? parentRT.rect.width : parentRT.rect.height;

            // 世界空间下的真实半宽高
            float parentHalfW = parentVisualW * scale * 0.5f;
            float parentHalfH = parentVisualH * scale * 0.5f;

            // 子物体自身半宽高（让子物体自身也居中对齐）
            float childHalfW = childRT.rect.width * scale * 0.5f;
            float childHalfH = childRT.rect.height * scale * 0.5f;

            // ==========================
            // 核心：子物体放在父物体「内部」
            // ==========================
            float x = parentRT.position.x + _pivotPos.x * parentHalfW;
            float y = parentRT.position.y + _pivotPos.y * parentHalfH;

            Vector3 targetPos = new Vector3(x, y, parentRT.position.z);

            // 应用位置
            _child.transform.position = targetPos;
            // 永远不旋转
            _child.transform.rotation = Quaternion.identity;
        }

        private void onMouseExit(PointerEventData arg1, object[] arg2)
        {
            GameCommon.DiscardToolTip();

        }

        private void onMouseEnter(PointerEventData arg1, object[] arg2)
        {
            //放到最下面 显示在最前面
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
                mono.txtHealth.text = _m_partInfo.currentHealth + "/" + _m_partInfo.maxHealth;
                GameCommon.ShowDamageFloatText(amount, GetGameObject().transform.position);

            }
        }
        private void onPartHeal(object[] _objs)
        {
            if (_objs == null || _objs.Length == 0)
                return;
            PartInfo info = _objs[0] as PartInfo;
            int amount = (int)_objs[1];
            if (_m_partInfo == info)
            {
                mono.txtHealth.text = _m_partInfo.currentHealth + "/" + _m_partInfo.maxHealth;
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
            }
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
