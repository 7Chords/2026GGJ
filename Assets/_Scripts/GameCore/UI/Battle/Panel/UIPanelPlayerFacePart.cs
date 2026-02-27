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
    public class UIPanelPlayerFacePart : _ASCUIPanelBase<UIMonoPlayerFacePart>
    {

        private PartInfo _m_partInfo;
        public PartInfo partInfo => _m_partInfo;

        private string _m_dragLoopCoroutineId;

        private GameObject _m_curHitGridGO;

        private bool _m_isDraging;

        private TweenContainer _m_tweenContainer;

        private List<UIPanelPartBuff> _m_partBuffItemList;

        public UIPanelPlayerFacePart(UIMonoPlayerFacePart _mono, SCUIShowType _showType) : base(_mono, _showType)
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
            if (_m_partBuffItemList != null)
            {
                foreach (var item in _m_partBuffItemList)
                    item?.Discard();
                _m_partBuffItemList.Clear();

            }
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

            if (_m_partBuffItemList != null)
            {
                foreach (var item in _m_partBuffItemList)
                    item?.HidePanel();
            }
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

            if (_m_partBuffItemList != null)
            {
                foreach (var item in _m_partBuffItemList)
                    item?.ShowPanel();
            }
        }

        public void SetInfo(PartInfo _info)
        {
            _m_partInfo = _info;
            //Sequence seq = DOTween.Sequence();
            //seq.Append(mono.imgGO.transform.DOScale(mono.scaleMouseEnter * 1.2f, mono.scaleChgDuration + 0.2f));
            //seq.Append(mono.imgGO.transform.DOScale(mono.scaleMouseEnter, mono.scaleChgDuration + 0.2f));
            //_m_tweenContainer.RegDoTween(seq);

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
            mono.txtOrder.text = GameModel.instance.GetPlayerBattleOrderByPartInfo(_m_partInfo).ToString();

            refreshBuffShow();

            mono.imgGO.transform.rotation = Quaternion.Euler(0, 0, _m_partInfo.rotateStep * 90);

            //信息子物体自动适配旋转和rect大小
            autoAdjustPosAndRotate(mono.imgGO.gameObject, mono.goHealthInfo,mono.goHealthPosPivot);
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
            //只有鼠标左键才处理
            if (_data.button != PointerEventData.InputButton.Left)
                return;
            _m_isDraging = true;
            //放到最下面 显示在最前面
            GetGameObject().transform.SetAsLastSibling();
            SCCommon.SetGameObjectEnable(mono.goOrder, false);

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
            //只有鼠标左键才处理
            if (_data.button != PointerEventData.InputButton.Left)
                return;
            _m_isDraging = false;

            SCCommon.SetGameObjectEnable(mono.goOrder, true);

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
                SCMsgCenter.SendMsg(SCMsgConst.CLEAR_PLAYER_PREVIEW);
                HidePanel();
                Discard();
            }
        }
        private void onDrag(PointerEventData _data, object[] _objs)
        {
            if (!_m_isDraging)
                return;
            //只有鼠标左键才处理
            if (_data.button != PointerEventData.InputButton.Left)
                return;
            RectTransform parentRect = GetGameObject().transform.parent as RectTransform;
            GetGameObject().transform.localPosition = GameCommon.ScreenPoint2UILocalPoint(parentRect, _data.position);

            _m_curHitGridGO = GameCommon.GetHitGridGameObj(_data);
            if (_m_curHitGridGO == null)
            {
                SCMsgCenter.SendMsg(SCMsgConst.CLEAR_PLAYER_PREVIEW);
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
            SCMsgCenter.SendMsg(SCMsgConst.CLEAR_PLAYER_PREVIEW);
        }

        private void onMouseEnter(PointerEventData _data, object[] _objs)
        {
            if (_m_isDraging)
                return;
            AudioMgr.instance.PlaySfx("sfx_mouse_enter");

            //放到最下面 显示在最前面
            GetGameObject().transform.SetAsLastSibling();
            _m_tweenContainer.RegDoTween(mono.imgGO.transform.DOScale(mono.scaleMouseEnter, mono.scaleChgDuration));
            GameCommon.ShowTooltip(_m_partInfo,
                new Vector2(GameConst.SHOW_FACE_PART_TIP_SCREEN_RATIO_X_IN_COMBINE, GameConst.SHOW_FACE_PART_TIP_SCREEN_RATIO_Y_IN_COMBINE),
                false);
            SCMsgCenter.SendMsg(SCMsgConst.PLAYER_FACE_PART_RANGE_HIGHLIGHT,_m_partInfo);

        }
        #endregion

        #region 事件回调
        private void onFinishDragPart()
        {
            mono.canvasGroup.blocksRaycasts = true;
            SCCommon.SetGameObjectEnable(mono.goOrder, true);

        }

        private void onBeginDragPart(object[] _objs)
        {
            if (_objs == null || _objs.Length == 0)
                return;
            GameObject go = _objs[0] as GameObject;
            if (go != GetGameObject())
            {
                mono.canvasGroup.blocksRaycasts = false;
                SCCommon.SetGameObjectEnable(mono.goOrder, false);
            }
        }

        #endregion

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
    }
}
