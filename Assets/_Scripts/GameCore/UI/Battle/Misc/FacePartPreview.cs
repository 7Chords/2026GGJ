using GameCore.Battle;
using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class FacePartPreview : MonoBehaviour
    {
        [Header("??????")]
        public Image imgGO;
        [Header("??????")]
        public Image imgPart;
        [Header("???????")]
        public Text txtHealth;
        [Header("??????")]
        public Text txtOrder;
        [Header("???????????")]
        public GameObject goHealthInfo;
        [Header("??????????")]
        public GameObject goOrder;


        private PartInfo _m_partInfo;

        private Coroutine _m_dragLoopCoroutine;

        private GameObject _m_curHitGridGO;

        private bool _m_isDraging;
        public void Initialize(PartInfo _info)
        {
            if (_info == null)
                return;

            var canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;

            _m_isDraging = true;
            SCMsgCenter.SendMsg(SCMsgConst.BEGIN_DRAG_PART,gameObject);


            if (_m_dragLoopCoroutine != null) StopCoroutine(_m_dragLoopCoroutine);
            _m_dragLoopCoroutine = StartCoroutine(dragLoop());

            _m_partInfo = _info;

            refreshShow();
        }

        public void Drag(PointerEventData _data)
        {
            if (!_m_isDraging)
                return;
            if (_m_partInfo == null)
                return;
            RectTransform parentRect = gameObject.transform.parent as RectTransform;
            transform.localPosition = GameCommon.ScreenPoint2UILocalPoint(parentRect,_data.position);

            _m_curHitGridGO = GameCommon.GetHitGridGameObj(_data);
            if (_m_curHitGridGO == null)
            {
                SCMsgCenter.SendMsg(SCMsgConst.CLEAR_PLAYER_PREVIEW);
                PlacementPreviewHelper.BroadcastClear();
            }
            else
            {
                List<Vector2Int> faceOccupyPosList = GameModel.instance.GetPlaceFaceOccupyPosList(_m_curHitGridGO, _data.position, _m_partInfo.localOccupyPosList);
                List<Vector2Int> faceEffectPosList = GameModel.instance.GetPlaceFaceEffectPosList(_m_partInfo.localEffectPosList, faceOccupyPosList, _m_partInfo.localOccupyPosList);
                SCMsgCenter.SendMsg(SCMsgConst.PLACE_PART_PREVIEW, faceOccupyPosList, faceEffectPosList);
                _m_partInfo.curOccupyFacePosList = faceOccupyPosList;
                _m_partInfo.curEffectFacePosList = faceEffectPosList;
                SCMsgCenter.SendMsg(SCMsgConst.PLAYER_FACE_PART_TARGET_PREVIEW_HIGHLIGHT, GameModel.instance.GetPartPreviewTargetPartList(_m_partInfo));
                PlacementPreviewHelper.BroadcastValues(_m_partInfo);
            }

        }
        public void EndDrag(PointerEventData _data)
        {
            if (!_m_isDraging)
                return;
            _m_isDraging = false;
            if (_m_partInfo == null)
                return;
            SCMsgCenter.SendMsg(SCMsgConst.FINISH_DRAG_PART);

            if (_m_dragLoopCoroutine != null)
            {
                StopCoroutine(_m_dragLoopCoroutine);
                _m_dragLoopCoroutine = null;
            }

            //???????????????????????
            _m_curHitGridGO = GameCommon.GetHitGridGameObj(_data);
            bool placementSuccess = false;//?????????

            if (_m_curHitGridGO != null)
            {
                if (GameModel.instance.CanPlacePart(_m_curHitGridGO, _data.position ,_m_partInfo.localOccupyPosList))
                {
                    placementSuccess = true;
                    List<Vector2Int> faceOccupyPosList = GameModel.instance.GetPlaceFaceOccupyPosList(_m_curHitGridGO, _data.position, _m_partInfo.localOccupyPosList);
                    List<Vector2Int> faceEffectPosList = GameModel.instance.GetPlaceFaceEffectPosList(_m_partInfo.localEffectPosList, faceOccupyPosList, _m_partInfo.localOccupyPosList);
                    SCMsgCenter.SendMsg(SCMsgConst.PLACE_PART_SUCCESS,
                        _m_partInfo,
                        faceOccupyPosList,
                        faceEffectPosList);

                    SCCommon.DestoryGameObject(gameObject);

                }
            }

            if (!placementSuccess)
            {
                _m_partInfo.ResetToBusy();
                SCMsgCenter.SendMsg(SCMsgConst.PLACE_PART_FAIL,_m_partInfo);
                SCMsgCenter.SendMsg(SCMsgConst.CLEAR_PLAYER_PREVIEW);
                SCMsgCenter.SendMsg(SCMsgConst.FACE_PART_TARTGET_PREVIEW_CANCEL);

                SCCommon.DestoryGameObject(gameObject);
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
                        _m_partInfo.curOccupyFacePosList = faceOccupyPosList;
                        _m_partInfo.curEffectFacePosList = faceEffectPosList;
                        SCMsgCenter.SendMsg(SCMsgConst.PLAYER_FACE_PART_TARGET_PREVIEW_HIGHLIGHT, GameModel.instance.GetPartPreviewTargetPartList(_m_partInfo));
                        PlacementPreviewHelper.BroadcastValues(_m_partInfo);
                    }
                }
                yield return null;
            }
        }

        private void refreshShow()
        {
            if (_m_partInfo == null)
                return;
            imgGO.sprite = ResourcesHelper.LoadAsset<Sprite>(_m_partInfo.partRefObj.partPlayerGameObjectName);
            imgGO.SetNativeSize();
            imgPart.sprite = ResourcesHelper.LoadAsset<Sprite>(_m_partInfo.partRefObj.partPlayerGameObjectName);
            imgPart.SetNativeSize();

            //?????????????
            imgGO.transform.rotation = Quaternion.Euler(0, 0, _m_partInfo.rotateStep * 90);
            goHealthInfo.transform.eulerAngles = Vector3.zero;
            goOrder.transform.eulerAngles = Vector3.zero;

        }

    }
}
