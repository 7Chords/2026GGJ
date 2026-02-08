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

        private int _m_currentRotateStep = 0;
        private bool _m_isDraging;
         public void Initialize(PartInfo _info)
        {
            if (_info == null)
                return;

            var canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;

            _m_isDraging = true;
            if (_m_dragLoopCoroutine != null) StopCoroutine(_m_dragLoopCoroutine);
            _m_dragLoopCoroutine = StartCoroutine(DragLoop());

            _m_partInfo = _info;

            refreshShow();
        }


        public void Drag(PointerEventData eventData)
        {
            RectTransform parentRect = gameObject.transform.parent as RectTransform;
            transform.localPosition = GameCommon.ScreenPoint2UILocalPoint(parentRect,eventData.position);
        }
        public void EndDrag(PointerEventData _arg)
        {
            _m_isDraging = false;
            if (_m_dragLoopCoroutine != null)
            {
                StopCoroutine(_m_dragLoopCoroutine);
                _m_dragLoopCoroutine = null;
            }

            //当前鼠标指向的脸部的格子物体
            GameObject girdGO = getHitGridGameObj(_arg);
            bool placementSuccess = false;//是否放置成功

            if (girdGO != null)
            {
                //if (TryCalculatePlacement(girdGO, _arg, out Vector2Int logicalOrigin, out List<Vector2Int> rotatedShape))
                //{
                //    // Placement Success
                //    _m_partInfo.startGridPos = logicalOrigin;

                //    //SnapToGrid(hitGrid);
                //    gameObject.transform.localRotation = Quaternion.Euler(0, 0, _m_currentRotateStep * 90);

                //    //UpdateGridColors(true, rotatedShape);

                //    placementSuccess = true;
                //}
                //else
                //{
                //    SCDebugHelper.LogWarning("该区域已被占用或无效！");
                //}
            }

            if (!placementSuccess)
            {
                _m_partInfo.Reset();
                SCMsgCenter.SendMsg(SCMsgConst.PLACE_PART_FAIL);
                SCCommon.DestoryGameObject(gameObject);
            }

        }


        private IEnumerator DragLoop()
        {
            while (_m_isDraging)
            {
                if (Input.GetMouseButtonDown(1))
                    rotatePart();
                yield return null;
            }
        }

        private GameObject getHitGridGameObj(PointerEventData _eventData)
        {
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(_eventData, results);
            foreach (var result in results)
            {
                if(result.gameObject.tag == GameConst.FACE_GRID_TAG)
                {
                    return result.gameObject;
                }
            }
            return null;
        }

        private bool TryCalculatePlacement(UIMonoMaskCombineFaceGrid hitGrid, PointerEventData eventData, out Vector2Int logicalOrigin, out List<Vector2Int> rotatedShape)
        {

            logicalOrigin = default;
            rotatedShape = default;
            return true;
        }

        private void rotatePart()
        {
            _m_currentRotateStep = (_m_currentRotateStep + 1) % 4;
            _m_partInfo.rotateStep = _m_currentRotateStep;
            _m_partInfo.gridPosList = GameCommon.Rotate(_m_partInfo.gridPosList, 1);
            refreshShow();
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
            imgGO.transform.rotation = Quaternion.Euler(0, 0, _m_currentRotateStep * 90);
            goHealthInfo.transform.eulerAngles = Vector3.zero;
            goOrder.transform.eulerAngles = Vector3.zero;

        }

    }
}
