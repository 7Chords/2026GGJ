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

            imgGO.sprite = ResourcesHelper.LoadAsset<Sprite>(_info.partRefObj.partGameObjectName);
            imgGO.SetNativeSize();
            imgPart.sprite = ResourcesHelper.LoadAsset<Sprite>(_info.partRefObj.partGameObjectName);
            imgPart.SetNativeSize();

            UpdatePreview();
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

            //当前鼠标指向的脸部的格子
            UIMonoMaskCombineFaceGrid hitGrid = GetHitGrid(_arg);
            bool placementSuccess = false;//是否放置成功

            if (hitGrid != null)
            {
                if (TryCalculatePlacement(hitGrid, _arg, out Vector2Int logicalOrigin, out List<Vector2Int> rotatedShape))
                {
                    // Placement Success
                    _m_partInfo.startGridPos = logicalOrigin;

                    //SnapToGrid(hitGrid);
                    gameObject.transform.localRotation = Quaternion.Euler(0, 0, _m_currentRotateStep * 90);

                    //UpdateGridColors(true, rotatedShape);

                    placementSuccess = true;
                }
                else
                {
                    SCDebugHelper.LogWarning("该区域已被占用或无效！");
                }
            }

            if (!placementSuccess)
            {
                SCMsgCenter.SendMsg(SCMsgConst.PLACE_PART_FAIL);
                //SCCommon.DestoryGameObject(gameObject);
            }

        }


        private IEnumerator DragLoop()
        {
            while (_m_isDraging)
            {
                //todo
                if (Input.GetMouseButtonDown(1))
                {
                    _m_currentRotateStep = (_m_currentRotateStep + 1) % 4;
                    UpdatePreview();
                }
                yield return null;
            }
        }

        private UIMonoMaskCombineFaceGrid GetHitGrid(PointerEventData _eventData)
        {
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(_eventData, results);
            foreach (var result in results)
            {
                var grid = result.gameObject.GetComponent<UIMonoMaskCombineFaceGrid>();
                if (grid != null) return grid;
            }
            return null;
        }

        private bool TryCalculatePlacement(UIMonoMaskCombineFaceGrid hitGrid, PointerEventData eventData, out Vector2Int logicalOrigin, out List<Vector2Int> rotatedShape)
        {

            logicalOrigin = default;
            rotatedShape = default;
            return true;
        }

        private void UpdatePreview()
        {
            imgGO.transform.rotation = Quaternion.Euler(0, 0, _m_currentRotateStep * 90);
            goHealthInfo.transform.eulerAngles = Vector3.zero;
            goOrder.transform.eulerAngles = Vector3.zero;

        }

    }
}
