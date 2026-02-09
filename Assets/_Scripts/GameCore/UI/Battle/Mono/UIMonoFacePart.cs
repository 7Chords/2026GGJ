using SCFrame;
using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoFacePart : _ASCUIMonoBase
    {
        [Header("物体图片")]
        public Image imgGO;
        [Header("部位图片")]
        public Image imgPart;
        [Header("生命文本")]
        public Text txtHealth;
        [Header("顺序文本")]
        public Text txtOrder;


        private PartInfo _m_partInfo;
        public PartInfo partInfo => _m_partInfo;


        private Coroutine _dragLoopCoroutine;

        // 继承当前的旋转状态
        private int _currentRotateStep = 0;

        private bool _m_isDraging = false;


        private void Start()
        {
            imgGO.transform.AddBeginDrag(onBeginDrag);
            imgGO.transform.AddDrag(onDrag);
            imgGO.transform.AddEndDrag(onEndDrag);

        }
        public void onBeginDrag(PointerEventData arg1, object[] arg2)
        {
            _m_isDraging = true;
        }
        private void onEndDrag(PointerEventData arg1, object[] arg2)
        {
            SCDebugHelper.Log("EndDrag");
        }

        private void onDrag(PointerEventData arg1, object[] arg2)
        {
            SCDebugHelper.Log("Drag");


            // 修复坐标转换问题：使用 RectTransformUtility.ScreenPointToLocalPointInRectangle
            // 尝试获取 UI Camera (如果有的话，通常 Overlay Canvas 传 null)
            // 假设 SCUICommon 内部实现可能有误，改为手动实现标准转换
            RectTransform parentRect = gameObject.transform.parent as RectTransform;
            Vector2 localPoint;

            // 注意：如果 Canvas 是 Screen Space - Overlay，cam 参数应为 null
            // 如果是 Screen Space - Camera，应传入对应的 Camera
            // 这里尝试自动获取 Canvas 的 Camera
            Camera uiCam = null;
            Canvas canvas = parentRect.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                uiCam = canvas.worldCamera;
            }

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, arg1.position, uiCam, out localPoint))
            {
                transform.localPosition = localPoint;

            }
        }


        private void OnDisable()
        {
            
        }

        public void Initialize(PartInfo _info)
        {
            if (_info == null)
                return;
            _m_isDraging = true;

            var canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = true;

            if (_dragLoopCoroutine != null) StopCoroutine(_dragLoopCoroutine);
            _dragLoopCoroutine = StartCoroutine(DragLoop());

            imgGO.sprite = ResourcesHelper.LoadAsset<Sprite>(_info.partRefObj.partGameObjectName);
            imgGO.SetNativeSize();
            imgPart.sprite = ResourcesHelper.LoadAsset<Sprite>(_info.partRefObj.partGameObjectName);
            imgPart.SetNativeSize();

        }


        public void Drag(PointerEventData eventData)
        {
            // 修复坐标转换问题：使用 RectTransformUtility.ScreenPointToLocalPointInRectangle
            // 尝试获取 UI Camera (如果有的话，通常 Overlay Canvas 传 null)
            // 假设 SCUICommon 内部实现可能有误，改为手动实现标准转换
            RectTransform parentRect = gameObject.transform.parent as RectTransform;
            Vector2 localPoint;

            // 注意：如果 Canvas 是 Screen Space - Overlay，cam 参数应为 null
            // 如果是 Screen Space - Camera，应传入对应的 Camera
            // 这里尝试自动获取 Canvas 的 Camera
            Camera uiCam = null;
            Canvas canvas = parentRect.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                uiCam = canvas.worldCamera;
            }

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, uiCam, out localPoint))
            {
                transform.localPosition = localPoint;

            }
        }
        
        private IEnumerator DragLoop()
        {
            while (_m_isDraging)
            {
                //todo
                if (Input.GetMouseButtonDown(1))
                {
                    _currentRotateStep = (_currentRotateStep + 1) % 4;
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

        public void RefreshShow()
        {

        }


        public void TriggerEffect()
        {

        }

    }
}
