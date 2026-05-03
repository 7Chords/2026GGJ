using DG.Tweening;
using GameCore.Battle;
using GameCore.Helpers;
using SCFrame;
using SCFrame.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameCore.UI
{
    public class UIPanelEnemyFacePart : _ASCUIPanelBase<UIMonoEnemyFacePart>
    {
        private PartInfo _m_partInfo;
        public PartInfo partInfo => _m_partInfo;

        private TweenContainer _m_tweenContainer;

        private List<UIPanelPartBuff> _m_partBuffItemList;

        private int _m_previewDamageAmt;
        private int _m_previewHealAmt;
        private bool _m_healthPreviewActive;

        public UIPanelEnemyFacePart(UIMonoEnemyFacePart _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_tweenContainer = new TweenContainer();
            _m_partBuffItemList = new List<UIPanelPartBuff>();
        }

        public override void BeforeDiscard()
        {
            GameCommon.DiscardToolTip();
            _m_tweenContainer?.KillAllDoTween();
            _m_tweenContainer = null;

            if(_m_partBuffItemList != null)
            {
                foreach (var item in _m_partBuffItemList)
                    item?.Discard();
                _m_partBuffItemList.Clear();

            }
        }

        public override void OnHidePanel()
        {
            mono.imgGO.RemoveMouseEnter(onMouseEnter);
            mono.imgGO.RemoveMouseExit(onMouseExit);
            SCMsgCenter.UnregisterMsg(SCMsgConst.ENEMY_FACE_PART_TARGET_PREVIEW_HIGHLIGHT, onFacePartTargetPreviewHightlight);
            SCMsgCenter.UnregisterMsg(SCMsgConst.PLAYER_FACE_PART_TARGET_PREVIEW_HIGHLIGHT, onFacePartTargetPreviewHightlight);
            SCMsgCenter.UnregisterMsg(SCMsgConst.FACE_PART_TARGET_PREVIEW_VALUES, onFacePartTargetPreviewValues);
            SCMsgCenter.UnregisterMsgAct(SCMsgConst.FACE_PART_TARTGET_PREVIEW_CANCEL, onFacePartTargetPreviewCancel);

            if (_m_partBuffItemList != null)
            {
                foreach (var item in _m_partBuffItemList)
                    item?.HidePanel();
            }
        }


        public override void OnShowPanel()
        {
            mono.imgGO.AddMouseEnter(onMouseEnter);
            mono.imgGO.AddMouseExit(onMouseExit);
            SCMsgCenter.RegisterMsg(SCMsgConst.ENEMY_FACE_PART_TARGET_PREVIEW_HIGHLIGHT, onFacePartTargetPreviewHightlight);
            SCMsgCenter.RegisterMsg(SCMsgConst.PLAYER_FACE_PART_TARGET_PREVIEW_HIGHLIGHT, onFacePartTargetPreviewHightlight);
            SCMsgCenter.RegisterMsg(SCMsgConst.FACE_PART_TARGET_PREVIEW_VALUES, onFacePartTargetPreviewValues);
            SCMsgCenter.RegisterMsgAct(SCMsgConst.FACE_PART_TARTGET_PREVIEW_CANCEL, onFacePartTargetPreviewCancel);

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
            mono.imgGO.sprite = ResourcesHelper.LoadAsset<Sprite>(_m_partInfo.partRefObj.partEnemyGameObjectName);
            mono.imgGO.SetNativeSize();
            mono.imgPart.sprite = ResourcesHelper.LoadAsset<Sprite>(_m_partInfo.partRefObj.partEnemyGameObjectName);
            mono.imgPart.SetNativeSize();
            applyHealthLineDisplay();
            mono.txtOrder.text = GameModel.instance.GetEnemyBattleOrderByPartInfo(_m_partInfo).ToString();

            GameObject buffInfoGO = null;
            UIMonoPartBuff monoPartBuff = null;
            UIPanelPartBuff panelPartBuff = null;
            for (int i =0;i<_m_partInfo.buffLogic.buffList.Count;i++)
            {
                buffInfoGO = ResourcesHelper.LoadGameObject(GameConst.PREFAB_PART_BUFF_ITEM,mono.goBuff.transform);
                monoPartBuff = buffInfoGO.GetComponent<UIMonoPartBuff>();
                if (monoPartBuff != null)
                    panelPartBuff = new UIPanelPartBuff(monoPartBuff,SCUIShowType.INTERNAL);
                panelPartBuff?.SetInfo(_m_partInfo.buffLogic.buffList[i]);
                panelPartBuff?.ShowPanel();
                _m_partBuffItemList.Add(panelPartBuff);
            }

            mono.imgGO.transform.rotation = Quaternion.Euler(0, 0, _m_partInfo.rotateStep * 90);

            //?????????????????????rect????
            autoAdjustPosAndRotate(mono.imgGO.gameObject, mono.goHealthInfo, mono.goHealthPosPivot);
            autoAdjustPosAndRotate(mono.imgGO.gameObject, mono.goOrder, mono.goOrderPosPivot);
            autoAdjustPosAndRotate(mono.imgGO.gameObject, mono.goBuff, mono.goBuffPosPivot);

            mono.imgGO.transform.localScale = mono.scaleGO * Vector3.one;
        }

        private void onMouseExit(PointerEventData _data, object[] _objs)
        {
            _m_tweenContainer.RegDoTween(mono.imgGO.transform.DOScale(mono.scaleGO, mono.scaleChgDuration));
            GameCommon.DiscardToolTip();
            SCMsgCenter.SendMsg(SCMsgConst.CLEAR_ENEMY_PREVIEW);
            SCMsgCenter.SendMsg(SCMsgConst.FACE_PART_TARTGET_PREVIEW_CANCEL);
        }

        private void onMouseEnter(PointerEventData _data, object[] _objs)
        {
            AudioMgr.instance.PlaySfx("sfx_mouse_enter");

            //????????? ??????????
            GetGameObject().transform.SetAsLastSibling();
            _m_tweenContainer.RegDoTween(mono.imgGO.transform.DOScale(mono.scaleMouseEnter, mono.scaleChgDuration));
            GameCommon.ShowTooltip(_m_partInfo,
                new Vector2(GameConst.SHOW_FACE_PART_TIP_SCREEN_RATIO_X_IN_COMBINE, GameConst.SHOW_FACE_PART_TIP_SCREEN_RATIO_Y_IN_COMBINE),
                false);
            SCMsgCenter.SendMsg(SCMsgConst.ENEMY_FACE_PART_RANGE_HIGHLIGHT, _m_partInfo);
            SCMsgCenter.SendMsg(SCMsgConst.ENEMY_FACE_PART_TARGET_PREVIEW_HIGHLIGHT, GameModel.instance.GetPartPreviewTargetPartList(_m_partInfo));
            PlacementPreviewHelper.BroadcastValues(_m_partInfo);
        }
        private void autoAdjustPosAndRotate(GameObject _parent, GameObject _child, Vector2 _pivotPos)
        {
            RectTransform parentRT = _parent.GetComponent<RectTransform>();
            RectTransform childRT = _child.GetComponent<RectTransform>();

            float scale = parentRT.lossyScale.y;

            // ???????? 90/270 ??
            int rotateMod = _m_partInfo.rotateStep % 2;
            bool isRotated90 = rotateMod != 0;

            // ??????p??????????????????????????
            float parentVisualW = isRotated90 ? parentRT.rect.height : parentRT.rect.width;
            float parentVisualH = isRotated90 ? parentRT.rect.width : parentRT.rect.height;

            // ?????????????????
            float parentHalfW = parentVisualW * scale * 0.5f;
            float parentHalfH = parentVisualH * scale * 0.5f;

            // ?????????????????????????????????????
            float childHalfW = childRT.rect.width * scale * 0.5f;
            float childHalfH = childRT.rect.height * scale * 0.5f;

            // ==========================
            // ???????????????????p?????
            // ==========================
            float x = parentRT.position.x + _pivotPos.x * parentHalfW;
            float y = parentRT.position.y + _pivotPos.y * parentHalfH;

            Vector3 targetPos = new Vector3(x, y, parentRT.position.z);

            // ???????
            _child.transform.position = targetPos;
            // ????????
            _child.transform.rotation = Quaternion.identity;
        }

        private void onFacePartTargetPreviewHightlight(object[] _objs)
        {
            if (_objs == null || _objs.Length == 0)
                return;
            List<PartInfo> infoList  = _objs[0] as List<PartInfo>;
            if (infoList == null)
                return;
            setPreviewHighlight(infoList.Contains(partInfo));
        }
        private void onFacePartTargetPreviewCancel()
        {
            setPreviewHighlight(false);
            clearHealthPreviewState();
        }

        private void onFacePartTargetPreviewValues(object[] _objs)
        {
            if (mono.txtHealth == null)
                return;
            if (_objs == null || _objs.Length == 0)
            {
                clearHealthPreviewState();
                return;
            }
            var payload = _objs[0] as PartPlacementPreviewPayload;
            if (payload == null)
            {
                clearHealthPreviewState();
                return;
            }
            int dmg = 0;
            int heal = 0;
            if (payload.damageToPart != null && payload.damageToPart.TryGetValue(partInfo, out var d))
                dmg = d;
            if (payload.healToPart != null && payload.healToPart.TryGetValue(partInfo, out var h))
                heal = h;
            _m_previewDamageAmt = dmg;
            _m_previewHealAmt = heal;
            if (dmg <= 0 && heal <= 0)
                clearHealthPreviewState();
            else
            {
                _m_healthPreviewActive = true;
                applyHealthLineDisplay();
            }
        }

        private void clearHealthPreviewState()
        {
            _m_healthPreviewActive = false;
            _m_previewDamageAmt = 0;
            _m_previewHealAmt = 0;
            applyHealthLineDisplay();
        }

        private void applyHealthLineDisplay()
        {
            if (_m_partInfo == null || mono.txtHealth == null)
                return;
            if (PartHealthDisplay.UseInfiniteHpDisplay(_m_partInfo.maxHealth))
            {
                mono.txtHealth.supportRichText = false;
                mono.txtHealth.text = PartHealthDisplay.MaxHpDisplayText;
                return;
            }
            if (_m_healthPreviewActive && (_m_previewDamageAmt > 0 || _m_previewHealAmt > 0))
                SCUICommon.ApplyHealthLinePreview(mono.txtHealth, mono.previewDamageColor, mono.previewHealColor,
                    _m_partInfo.currentHealth, _m_partInfo.maxHealth, _m_previewDamageAmt, _m_previewHealAmt);
            else
            {
                mono.txtHealth.supportRichText = false;
                mono.txtHealth.text = PartHealthDisplay.FormatSlashLine(_m_partInfo.currentHealth, _m_partInfo.maxHealth);
            }
        }
        private void setPreviewHighlight(bool _isHighlight)
        {
            mono.imgPart.material = _isHighlight? ResourcesHelper.LoadAsset<Material>(GameConst.MAT_UI_OUTLINE) : null;
        }

    }
}
