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
        [Header("物体图片")]
        public Image imgGO;
        [Header("部位图片")]
        public Image imgPart;
        [Header("生命文本")]
        public Text txtHealth;
        [Header("顺序文本")]
        public Text txtOrder;
        [Header("生命信息物体")]
        public GameObject goHealthInfo;
        [Header("序号信息物体")]
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
                SCMsgCenter.SendMsg(SCMsgConst.CLEAR_PREVIEW);
            }
            else
            {
                List<Vector2Int> faceOccupyPosList = GameModel.instance.GetPlaceFaceOccupyPosList(_m_curHitGridGO, _data.position, _m_partInfo.localOccupyPosList);
                List<Vector2Int> faceEffectPosList = GameModel.instance.GetPlaceFaceEffectPosList(_m_partInfo.localEffectPosList, faceOccupyPosList, _m_partInfo.localOccupyPosList);
                SCMsgCenter.SendMsg(SCMsgConst.PLACE_PART_PREVIEW, faceOccupyPosList, faceEffectPosList);
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

            //当前鼠标指向的脸部的格子物体
            _m_curHitGridGO = GameCommon.GetHitGridGameObj(_data);
            bool placementSuccess = false;//是否放置成功

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
                SCMsgCenter.SendMsg(SCMsgConst.CLEAR_PREVIEW);
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
                    }
                }
                yield return null;
            }
        }

        private void refreshShow()
        {
            if (_m_partInfo == null)
                return;
            imgGO.sprite = ResourcesHelper.LoadAsset<Sprite>(_m_partInfo.partRefObj.partGameObjectName);
            imgGO.SetNativeSize();
            imgPart.sprite = ResourcesHelper.LoadAsset<Sprite>(_m_partInfo.partRefObj.partGameObjectName);
            imgPart.SetNativeSize();

            //信息不要跟着旋转
            imgGO.transform.rotation = Quaternion.Euler(0, 0, _m_partInfo.rotateStep * 90);
            goHealthInfo.transform.eulerAngles = Vector3.zero;
            goOrder.transform.eulerAngles = Vector3.zero;

        }

    }
}
