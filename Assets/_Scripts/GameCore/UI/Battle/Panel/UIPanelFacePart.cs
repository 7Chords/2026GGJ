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
    public class UIPanelFacePart : _ASCUIPanelBase<UIMonoFacePart>
    {

        private PartInfo _m_partInfo;
        public PartInfo partInfo => _m_partInfo;

        private string _m_dragLoopCoroutineId;

        private GameObject _m_curHitGridGO;

        private bool _m_isDraging;

        private TweenContainer _m_tweenContainer;

        public UIPanelFacePart(UIMonoFacePart _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_tweenContainer = new TweenContainer();
        }

        public override void BeforeDiscard()
        {
            GameCommon.DiscardToolTip();
            _m_tweenContainer?.KillAllDoTween();
            _m_tweenContainer = null;
        }

        public override void OnHidePanel()
        {
            SCMsgCenter.UnregisterMsg(SCMsgConst.BEGIN_DRAG_PART, onBeginDragPart);
            SCMsgCenter.UnregisterMsgAct(SCMsgConst.FINISH_DRAG_PART, onFinishDragPart);
            SCMsgCenter.UnregisterMsgAct(SCMsgConst.FACE_PART_ORDER_CHG, refreshShow);

            mono.imgGO.RemoveMouseEnter(onMouseEnter);
            mono.imgGO.RemoveMouseExit(onMouseExit);

            mono.imgGO.RemoveBeginDrag(onBeginDrag);
            mono.imgGO.RemoveDrag(onDrag);
            mono.imgGO.RemoveEndDrag(onEndDrag);
        }


        public override void OnShowPanel()
        {
            SCMsgCenter.RegisterMsg(SCMsgConst.BEGIN_DRAG_PART, onBeginDragPart);
            SCMsgCenter.RegisterMsgAct(SCMsgConst.FINISH_DRAG_PART, onFinishDragPart);
            SCMsgCenter.RegisterMsgAct(SCMsgConst.FACE_PART_ORDER_CHG, refreshShow);

            mono.imgGO.AddMouseEnter(onMouseEnter);
            mono.imgGO.AddMouseExit(onMouseExit);


            mono.imgGO.AddBeginDrag(onBeginDrag);
            mono.imgGO.AddDrag(onDrag);
            mono.imgGO.AddEndDrag(onEndDrag);

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
            mono.txtHealth.text = _m_partInfo.currentHealth +"/" + _m_partInfo.maxHealth;
            mono.txtOrder.text = GameModel.instance.GetBattleOrderByPartInfo(_m_partInfo).ToString();
            //信息不要跟着旋转
            mono.imgGO.transform.rotation = Quaternion.Euler(0, 0, _m_partInfo.rotateStep * 90);
            mono.goHealthInfo.transform.eulerAngles = Vector3.zero;
            mono.goOrder.transform.eulerAngles = Vector3.zero;
        }

        private IEnumerator dragLoop()
        {
            while (_m_isDraging)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    _m_partInfo.RotateOnce();
                    refreshShow();
                    if (_m_curHitGridGO != null)
                    {
                        List<Vector2Int> faceOccupyPosList = GameModel.instance.GetPlaceFaceOccupyPosList(_m_curHitGridGO, Input.mousePosition, _m_partInfo.localOccupyPosList);
                        List<Vector2Int> faceEffectPosList = GameModel.instance.GetPlaceFaceEffectPosList(_m_partInfo.localEffectPosList, faceOccupyPosList, _m_partInfo.localOccupyPosList);
                        SCMsgCenter.SendMsg(SCMsgConst.PLACE_PART_PREVIEW, faceOccupyPosList, faceEffectPosList);
                    }
                }
                yield return null;
            }
        }

        #region UI响应回调
        public void onBeginDrag(PointerEventData _data, object[] _objs)
        {
            if (_m_isDraging)
                return;
            _m_isDraging = true;
            //放到最下面 显示在最前面
            GetGameObject().transform.SetAsLastSibling();

            SCMsgCenter.SendMsg(SCMsgConst.BEGIN_DRAG_PART, GetGameObject());

            if (!string.IsNullOrEmpty(_m_dragLoopCoroutineId)) SCTaskHelper.instance.KillAllCoroutines(this);
            _m_dragLoopCoroutineId = SCTaskHelper.instance.CreateCoroutine(this,dragLoop());

            //设置占据的格子的状态
            GameModel.instance.SetGridsEmpty(_m_partInfo.curOccupyFacePosList);
            //取消信息的脸部状态
            partInfo.ClearOnFaceState();

        }
        private void onEndDrag(PointerEventData _data, object[] _objs)
        {
            if (!_m_isDraging)
                return;
            _m_isDraging = false;
            SCMsgCenter.SendMsg(SCMsgConst.FINISH_DRAG_PART);

            if (!string.IsNullOrEmpty(_m_dragLoopCoroutineId))
                SCTaskHelper.instance.KillAllCoroutines(this);

            //当前鼠标指向的脸部的格子物体
            _m_curHitGridGO = GameCommon.GetHitGridGameObj(_data);
            bool placementSuccess = false;//是否放置成功

            if (_m_curHitGridGO != null)
            {
                if (GameModel.instance.CanPlacePart(_m_curHitGridGO, _data.position, _m_partInfo.localOccupyPosList))
                {
                    placementSuccess = true;

                    List<Vector2Int> faceOccupyPosList = GameModel.instance.GetPlaceFaceOccupyPosList(_m_curHitGridGO, _data.position, _m_partInfo.localOccupyPosList);
                    List<Vector2Int> faceEffectPosList = GameModel.instance.GetPlaceFaceEffectPosList(_m_partInfo.localEffectPosList, faceOccupyPosList, _m_partInfo.localOccupyPosList);
                    SCMsgCenter.SendMsg(SCMsgConst.REPLACE_PART_POS_SUCCESS, 
                        this,
                        faceOccupyPosList,
                        faceEffectPosList);

                }
            }

            if (!placementSuccess)
            {
                SCMsgCenter.SendMsg(SCMsgConst.REPLACE_PART_POS_FAIL,_m_partInfo);
                SCMsgCenter.SendMsg(SCMsgConst.CLEAR_PREVIEW);
                HidePanel();
                Discard();
            }
        }
        private void onDrag(PointerEventData _data, object[] _objs)
        {
            if (!_m_isDraging)
                return;

            RectTransform parentRect = GetGameObject().transform.parent as RectTransform;
            GetGameObject().transform.localPosition = GameCommon.ScreenPoint2UILocalPoint(parentRect, _data.position);

            _m_curHitGridGO = GameCommon.GetHitGridGameObj(_data);
            if (_m_curHitGridGO == null)
            {
                SCMsgCenter.SendMsg(SCMsgConst.CLEAR_PREVIEW);
            }
            else
            {
                List<Vector2Int> faceOccupyPosList = GameModel.instance.GetPlaceFaceOccupyPosList(_m_curHitGridGO, _data.position, _m_partInfo.localOccupyPosList);
                List<Vector2Int> faceEffectPosList = GameModel.instance.GetPlaceFaceEffectPosList(_m_partInfo.localEffectPosList, faceOccupyPosList, _m_partInfo.localOccupyPosList);
                SCMsgCenter.SendMsg(SCMsgConst.PLACE_PART_PREVIEW, faceOccupyPosList, faceEffectPosList);
            }

        }

        private void onMouseExit(PointerEventData _data, object[] _objs)
        {
            if (_m_isDraging)
                return;
            _m_tweenContainer.RegDoTween(mono.imgGO.transform.DOScale(Vector3.one, mono.scaleChgDuration));
            GameCommon.DiscardToolTip();
            SCMsgCenter.SendMsg(SCMsgConst.CLEAR_PREVIEW);
        }

        private void onMouseEnter(PointerEventData _data, object[] _objs)
        {
            if (_m_isDraging)
                return;
            _m_tweenContainer.RegDoTween(mono.imgGO.transform.DOScale(mono.scaleMouseEnter, mono.scaleChgDuration));
            GameCommon.ShowTooltip(_m_partInfo.partRefObj.partName, _m_partInfo.partRefObj.partDesc, new Vector2(GameConst.SHOW_FACE_PART_TIP_SCREEN_RATIO_X, GameConst.SHOW_FACE_PART_TIP_SCREEN_RATIO_Y));
            SCMsgCenter.SendMsg(SCMsgConst.FACE_PART_RANGE_HIGHLIGHT,_m_partInfo);

        }
        #endregion

        #region 事件回调
        private void onFinishDragPart()
        {
            mono.canvasGroup.blocksRaycasts = true;
        }

        private void onBeginDragPart(object[] _objs)
        {
            if (_objs == null || _objs.Length == 0)
                return;
            GameObject go = _objs[0] as GameObject;
            if (go != GetGameObject())
                mono.canvasGroup.blocksRaycasts = false;
        }

        #endregion

    }
}
