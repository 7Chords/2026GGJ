using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SCFrame;
using System;
using GameCore.RefData;
using SCFrame.UI;

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
        public void EndDrag(PointerEventData _arg)
        {
            _m_isDraging = false;
            if (_dragLoopCoroutine != null)
            {
                StopCoroutine(_dragLoopCoroutine);
                _dragLoopCoroutine = null;
            }

            //当前鼠标指向的脸部的格子
            UIMonoMaskCombineFaceGrid hitGrid = GetHitGrid(_arg);
            bool placementSuccess = false;//是否放置成功

            if (hitGrid != null)
            {
                if (TryCalculatePlacement(hitGrid, _arg, out Vector2Int logicalOrigin,out List <Vector2Int> rotatedShape))
                {
                    // Placement Success
                    _m_partInfo.startGridPos = logicalOrigin;

                    SnapToGrid(hitGrid);
                    gameObject.transform.localRotation = Quaternion.Euler(0, 0, _currentRotateStep * 90);

                    UpdateGridColors(true, rotatedShape);

                    placementSuccess = true;
                }
                else
                {
                    Debug.Log("该区域已被占用或无效！");
                }
            }

            if (!placementSuccess)
            {
                SCMsgCenter.SendMsg(SCMsgConst.PLACE_PART_FAIL);
                ReturnToBag();
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
                UpdatePreview();
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
        /// <summary>
        /// 从异形物品的occupyPosList计算包围盒信息
        /// </summary>
        /// <param name="occupyPosList">异形物品占用格子列表</param>
        /// <param name="minPos">包围盒最小坐标（虚拟左上角）</param>
        /// <param name="boundsWidth">包围盒宽度</param>
        /// <param name="boundsHeight">包围盒高度</param>
        /// <param name="boundsCenter">包围盒中心相对于minPos的偏移</param>
        private bool CalculateBounds(List<Vector2Int> occupyPosList, out Vector2Int minPos, out int boundsWidth, out int boundsHeight, out Vector2Int boundsCenter)
        {
            minPos = Vector2Int.zero;
            boundsWidth = 0;
            boundsHeight = 0;
            boundsCenter = Vector2Int.zero;

            if (occupyPosList == null || occupyPosList.Count == 0) return false;

            // 1. 找包围盒的最小/最大x/y
            int minX = int.MaxValue;
            int maxX = int.MinValue;
            int minY = int.MaxValue;
            int maxY = int.MinValue;

            foreach (var pos in occupyPosList)
            {
                minX = Mathf.Min(minX, pos.x);
                maxX = Mathf.Max(maxX, pos.x);
                minY = Mathf.Min(minY, pos.y);
                maxY = Mathf.Max(maxY, pos.y);
            }

            // 2. 计算包围盒信息
            minPos = new Vector2Int(minX, minY);
            boundsWidth = maxX - minX + 1; // 宽度（格子数）
            boundsHeight = maxY - minY + 1; // 高度（格子数）
                                            // 3. 计算包围盒中心（相对于minPos的偏移，向下取整）
            boundsCenter = new Vector2Int(
                Mathf.FloorToInt(boundsWidth / 2f),
                Mathf.FloorToInt(boundsHeight / 2f)
            );

            return true;
        }

        /// <summary>
        /// 获取鼠标在命中格子UI内的像素坐标（相对于格子左上角）
        /// </summary>
        /// <param name="hitGrid">命中的格子UI</param>
        /// <param name="eventData">拖拽事件数据</param>
        /// <param name="pixelPosInGrid">鼠标在格子内的像素坐标（x:0~格子宽，y:0~格子高）</param>
        /// <param name="gridSizePixel">格子的像素尺寸（宽/高）</param>
        private bool GetMouseInGridPixelPos(UIMonoMaskCombineFaceGrid hitGrid, PointerEventData eventData, out Vector2 pixelPosInGrid, out Vector2 gridSizePixel)
        {
            pixelPosInGrid = Vector2.zero;
            gridSizePixel = Vector2.zero;

            RectTransform gridRect = hitGrid.GetComponent<RectTransform>();
            if (gridRect == null) return false;

            // 1. 获取格子的像素尺寸（世界空间转像素）
            gridSizePixel = new Vector2(gridRect.rect.width, gridRect.rect.height);

            // 2. 转换鼠标屏幕坐标到格子Rect内的本地坐标
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                gridRect,
                eventData.position,
                eventData.pressEventCamera,
                out pixelPosInGrid))
            {
                return false;
            }

            // 3. 修正坐标（转为相对于格子左上角的正数）
            pixelPosInGrid += new Vector2(gridSizePixel.x / 2f, gridSizePixel.y / 2f);

            return true;
        }

        /// <summary>
        /// 根据鼠标在格子内的像素比重计算动态偏移（核心交互优化）
        /// </summary>
        /// <param name="pixelPosInGrid">鼠标在格子内的像素坐标</param>
        /// <param name="gridSizePixel">格子像素尺寸</param>
        /// <param name="boundsWidth">包围盒宽度</param>
        /// <param name="boundsHeight">包围盒高度</param>
        private Vector2Int CalculateDynamicOffset(Vector2 pixelPosInGrid, Vector2 gridSizePixel, int boundsWidth, int boundsHeight)
        {
            Vector2Int dynamicOffset = Vector2Int.zero;

            // 1. 计算鼠标在格子内的比重（0~1）
            float xRatio = pixelPosInGrid.x / gridSizePixel.x; // x轴比重（0=最左，1=最右）
            float yRatio = pixelPosInGrid.y / gridSizePixel.y; // y轴比重（0=最上，1=最下）

            // 2. 动态调整X轴偏移（左右）
            if (boundsWidth > 1)
            {
                // 阈值：50%，可根据需求调整（比如40%/60%）
                if (xRatio < 0.5f)
                {
                    // 鼠标偏左 → 起始格左移（抵消部分中心偏移）
                    dynamicOffset.x = -(boundsWidth - 1);
                }
                // 鼠标偏右 → 不偏移（默认）
            }

            // 3. 动态调整Y轴偏移（上下）
            if (boundsHeight > 1)
            {
                if (yRatio < 0.5f)
                {
                    // 鼠标偏上 → 起始格上移
                    dynamicOffset.y = -(boundsHeight - 1);
                }
                // 鼠标偏下 → 不偏移（默认）
            }

            // 4. 偏移修正（保证偏移量为整数，适配格子数）
            dynamicOffset.x = Mathf.RoundToInt(dynamicOffset.x / 2f);
            dynamicOffset.y = Mathf.RoundToInt(dynamicOffset.y / 2f);

            return dynamicOffset;
        }




        public void RefreshShow()
        {

        }


        public void TriggerEffect()
        {

        }






        /// <summary>
        /// 旋转物品尺寸（90度为一步，顺时针）
        /// </summary>
        private Vector2Int RotateItemSize(Vector2Int originalSize, int rotateStep)
        {
            int step = rotateStep % 4; // 保证step在0-3之间
            return step % 2 == 0 ? originalSize : new Vector2Int(originalSize.y, originalSize.x);
        }


        private void UpdatePreview()
        {
            
        }
        private void SnapToGrid(UIMonoMaskCombineFaceGrid grid)
        {
        }


        private void UpdateVisuals(RectTransform gridRect)
        {

        }

        private void ReturnToBag()
        {
            
        }
        /// <summary>
        /// 更新格子颜色
        /// </summary>
        /// <param name="isOccupied">true:显示占用(红色), false:恢复默认</param>
        private void UpdateGridColors(bool isOccupied, List<Vector2Int> customShape = null)
        {

        }
    }
}
