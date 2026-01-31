using SCFrame;
using SCFrame.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameCore.UI
{
    public class UIPanelMaskCombinePartContainerItem : _ASCUIPanelBase<UIMonoMaskCombinePartContainerItem>
    {
        private PartInfo _m_partInfo;

        private GameObject _m_dragCloneGO;

        private bool _m_isDraging;

        private UIPanelMaskCombinePartContainer _m_container;

        public UIPanelMaskCombinePartContainerItem(UIMonoMaskCombinePartContainerItem _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public void SetContainer(UIPanelMaskCombinePartContainer container)
        {
            _m_container = container;
        }

        public PartInfo GetPartInfo()
        {
            return _m_partInfo;
        }

        public override void AfterInitialize()
        {

        }

        public override void BeforeDiscard()
        {

        }

        public override void OnHidePanel()
        {
            mono.imgGoods.RemoveBeginDrag(onBeginDrag);
            mono.imgGoods.RemoveDrag(onDrag);
            mono.imgGoods.RemoveEndDrag(onEndDrag);
        }

        public override void OnShowPanel()
        {
            mono.imgGoods.AddBeginDrag(onBeginDrag);
            mono.imgGoods.AddDrag(onDrag);
            mono.imgGoods.AddEndDrag(onEndDrag);

        }

        public void SetInfo(PartInfo _info)
        {
            _m_partInfo = _info;
            refreshShow();
            
            // 初次设置位置需要根据 gridPos 更新 (这里先不处理，假设初始Layout负责)
            // 如果需要初始化位置：
            // UpdatePositionByGrid(_m_partInfo.gridPos); 
            // 但需要找到对应的Grid Transform，比较复杂，暂时只处理拖拽后的位置更新
        }
        private void refreshShow()
        {
            if (_m_partInfo == null)
                return;
            mono.imgGoods.sprite = ResourcesHelper.LoadAsset<Sprite>(_m_partInfo.partRefObj.partSpriteObjName);
            mono.txtHealth.text = _m_partInfo.currentHealth + "/" + _m_partInfo.partRefObj.partHealth;
        }

        private void onBeginDrag(PointerEventData _arg, object[] _objs)
        {
            _m_isDraging = true;

            // 隐藏原物体的交互
            if (mono.canvasGroup != null)
            {
                mono.canvasGroup.alpha = 0f;
                mono.canvasGroup.blocksRaycasts = false;
            }
            createDragClone();
            
            if (_dragLoopCoroutine != null) mono.StopCoroutine(_dragLoopCoroutine);
            _dragLoopCoroutine = mono.StartCoroutine(DragLoop());
            UpdateGridColors(false);
        }
        private void onEndDrag(PointerEventData _arg, object[] _objs)
        {
            _m_isDraging = false;
            if (_dragLoopCoroutine != null)
            {
                mono.StopCoroutine(_dragLoopCoroutine);
                _dragLoopCoroutine = null;
            }

            UIMonoMaskCombineFaceGrid hitGrid = GetHitGrid(_arg);
            bool placementSuccess = false;

            if (hitGrid != null)
            {
                 // Calculate Logical Grid Origin based on midPos
                 Vector2Int hitPos = Vector2Int.RoundToInt(hitGrid.gridPos);
                 Vector2Int midPos = Vector2Int.zero;
                 if (_m_partInfo != null && _m_partInfo.partRefObj != null)
                 {
                     midPos = _m_partInfo.partRefObj.midPos;
                 }
                 
                 // CRITICAL FIX: Rotate midPos before subtracting!
                 // The relative position of the handle changes when the item rotates.
                 Vector2Int rotatedMidPos = RotateVector(midPos, _currentRotation);
                 
                 Vector2Int logicalOrigin = hitPos - rotatedMidPos;
                 
                 // Check Validity of Region (All Shape cells must exist in Grid)
                 bool regionValid = true;
                 var shape = (_m_partInfo != null && _m_partInfo.partRefObj != null) ? _m_partInfo.partRefObj.posList : null;
                 
                 // Get all Grids in parent to verify existence
                 var allGrids = hitGrid.transform.parent.GetComponentsInChildren<UIMonoMaskCombineFaceGrid>();
                 
                 List<Vector2Int> requiredPositions = new List<Vector2Int>();
                 // Apply Rotation to Shape
                 List<GameCore.RefData.PosEffectObj> rotatedShape = new List<GameCore.RefData.PosEffectObj>();
                 if (shape != null && shape.Count > 0)
                 {
                     foreach(var p in shape) 
                     {
                         Vector2Int rotatedP = RotateVector(new Vector2Int(p.x, p.y), _currentRotation);
                         requiredPositions.Add(logicalOrigin + rotatedP);
                         rotatedShape.Add(new GameCore.RefData.PosEffectObj() { x = rotatedP.x, y = rotatedP.y });
                     }
                 }
                 else
                 {
                      requiredPositions.Add(logicalOrigin);
                      rotatedShape.Add(new GameCore.RefData.PosEffectObj() { x = 0, y = 0 });
                 }
                 
                 foreach(var req in requiredPositions)
                 {
                     bool found = false;
                     foreach(var g in allGrids)
                     {
                         if (Vector2Int.RoundToInt(g.gridPos) == req)
                         {
                             found = true;
                             break;
                         }
                     }
                     if (!found)
                     {
                         regionValid = false;
                         break;
                     }
                 }
                 
                 if (regionValid)
                 {
                     // Check Occupancy (Collision with other items)
                     // Pass the ROTATED shape
                     bool isOccupied = _m_container != null && _m_container.CheckRegionOccupancy(logicalOrigin, rotatedShape, _m_partInfo);
                     
                     if (!isOccupied)
                     {
                         // Placement Success
                         _m_partInfo.gridPos = logicalOrigin;
                         _m_partInfo.rotation = _currentRotation; // Save rotation
                         
                         // Determine which grid corresponds to the logicalOrigin (to be parent?)
                         // Actually, user logic was: "place item parent to hitGrid (midPos location)"
                         // Current logic: SnapToGrid(hitGrid). hitGrid IS the grid under mouse.
                         // This is correct visual pivot.
                         SnapToGrid(hitGrid); 
                         
                         // Apply Rotation to actual item
                         GetGameObject().transform.localRotation = Quaternion.Euler(0, 0, _currentRotation * 90);
                         
                         // Update Grid Colors using rotated shape
                         UpdateGridColors(true, rotatedShape); 
                         
                         placementSuccess = true;
                     }
                     else
                     {
                         Debug.Log("该区域已被占用！");
                     }
                 }
                 else
                 {
                     Debug.Log("部分区域超出格子范围！");
                 }
            }
            else
            {
                // 如果没有放在格子上，检查是否放在了背包里
                UIMonoCommonContainer hitContainer = GetHitContainer(_arg); 
                if (hitContainer != null)
                {
                    ReturnToBag();
                    placementSuccess = true;
                }
            }
            
            if (!placementSuccess)
            {
                ReturnToBag();
            }

            if (_m_dragCloneGO != null)
            {
                SCCommon.DestoryGameObject(_m_dragCloneGO);
                _m_dragCloneGO = null;
            }

            // 恢复原物体的显示和交互
            if (mono.canvasGroup != null)
            {
                mono.canvasGroup.alpha = 1f;
                mono.canvasGroup.blocksRaycasts = true;
            }
            
        }

        private void onDrag(PointerEventData _arg, object[] _objs)
        {
            if (!_m_isDraging)
                return;
            updateDragClonePosition(_arg);
        }
        
        private void SnapToGrid(UIMonoMaskCombineFaceGrid grid)
        {
            // 将 Item 的中心对齐到 Grid 的中心
            // 修改为：将 item 的父物体设置为 grid
            GetGameObject().transform.SetParent(grid.transform);
            GetGameObject().transform.localPosition = Vector3.zero;
            
            // 确保 scale 正确 (防止父物体 scale 影响)
            GetGameObject().transform.localScale = new Vector3(0.7f, 0.7f, 1);
        }

        /// <summary>
        /// 更新格子颜色
        /// </summary>
        /// <param name="isOccupied">true:显示占用(红色), false:恢复默认</param>
        private void UpdateGridColors(bool isOccupied, List<GameCore.RefData.PosEffectObj> customShape = null)
        {
            if (_m_partInfo == null || _m_partInfo.gridPos.x == -1) return;
            
            // ... (Parent checks) ...
            Transform currentParent = GetGameObject().transform.parent;
            if (currentParent == null) return;

            UIMonoMaskCombineFaceGrid parentGrid = currentParent.GetComponent<UIMonoMaskCombineFaceGrid>();
            if (parentGrid == null) return; 
            
            Transform gridContainer = parentGrid.transform.parent;
            if (gridContainer == null) return;
            
            var allGrids = gridContainer.GetComponentsInChildren<UIMonoMaskCombineFaceGrid>();
            
            // 计算需要变色的各位置 (优先使用传入的 rotated shape)
            List<Vector2Int> occupiedPositions = new List<Vector2Int>();
            
            if (customShape != null)
            {
                foreach(var p in customShape) occupiedPositions.Add(_m_partInfo.gridPos + new Vector2Int(p.x, p.y));
            }
            else
            {
                var shape = (_m_partInfo.partRefObj != null) ? _m_partInfo.partRefObj.posList : null;
                int rot = _m_partInfo.rotation; 
                
                if (shape != null && shape.Count > 0)
                {
                    foreach(var p in shape) 
                    {
                        var rotatedP = RotateVector(new Vector2Int(p.x, p.y), rot);
                        occupiedPositions.Add(_m_partInfo.gridPos + rotatedP);
                    }
                }
                else
                {
                    occupiedPositions.Add(_m_partInfo.gridPos);
                }
            }
            
            foreach (var grid in allGrids)
            {
                if (occupiedPositions.Contains(Vector2Int.RoundToInt(grid.gridPos)))
                {
                    var img = grid.GetComponent<UnityEngine.UI.Image>();
                    if (img != null)
                    {
                        if (isOccupied)
                        {
                            if (grid != parentGrid) img.color = Color.red;
                        }
                        else
                        {
                            img.color = grid.colorDefault;
                        }
                    }
                }
            }
        }
        
        private void ReturnToBag()
        {
            if (_m_container == null) return;
            
            // 在重置父物体之前，先恢复格子颜色
            // 此时 transform.parent 还是 Grid (如果它之前在格子上)
            UpdateGridColors(false);
            
            // 将 item 的父物体设置回容器的布局组件
            // 注意：_m_container.GetMono() 可以获取 Mono 引用
            var containerMono = _m_container.mono;
            if (containerMono != null && containerMono.layoutGroup != null)
            {
                GetGameObject().transform.SetParent(containerMono.layoutGroup.transform);
                GetGameObject().transform.localScale = Vector3.one;
                GetGameObject().transform.localRotation = Quaternion.identity; // Reset rotation
                
                // 重置 GridPos，表示不再占用格子
                if (_m_partInfo != null)
                {
                    _m_partInfo.gridPos = new Vector2Int(-1, -1);
                    _m_partInfo.rotation = 0;
                }
            }
        }
        
        private UIMonoCommonContainer GetHitContainer(PointerEventData _eventData)
        {
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(_eventData, results);
            foreach (var result in results)
            {
                var container = result.gameObject.GetComponentInParent<UIMonoCommonContainer>();
                if (container != null) return container;
            }
            return null;
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

        /// <summary>
        /// 创建拖拽克隆体
        /// </summary>
        private void createDragClone()
        {
            if (_m_dragCloneGO != null) return;
            
            // 继承当前的旋转状态
            _currentRotation = (_m_partInfo != null) ? _m_partInfo.rotation : 0;

            // 创建克隆体
            _m_dragCloneGO = SCCommon.InstantiateGameObject(mono.imgGoods.gameObject, SCGame.instance.fullLayerRoot.transform);
            
            // 应用初始旋转
            _m_dragCloneGO.transform.localRotation = Quaternion.Euler(0, 0, _currentRotation * 90);
            
            // 确保克隆体不阻挡射线，否则 RaycastAll 可能会先打到克隆体
            var canvasGroup = _m_dragCloneGO.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = _m_dragCloneGO.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.7f;
            
            // 复制 Image 用于拖拽显示
            /*var srcImg = GetGameObject().GetComponent<UnityEngine.UI.Image>();
            var dstImg = _m_dragCloneGO.GetComponent<UnityEngine.UI.Image>();
            if (srcImg != null && dstImg != null) dstImg.sprite = srcImg.sprite;*/ // 确保图标一致（如有必要）

            // 移除克隆体上不需要的组件
            var cloneInstrumentItem = _m_dragCloneGO.GetComponent<UIMonoMaskCombinePartContainerItem>();
            if (cloneInstrumentItem != null)
                cloneInstrumentItem.enabled = false;

        }


        /// <summary>
        /// 更新拖拽克隆体位置
        /// </summary>
        private void updateDragClonePosition(PointerEventData eventData)
        {
            if (_m_dragCloneGO == null) return;

            // 修复坐标转换问题：使用 RectTransformUtility.ScreenPointToLocalPointInRectangle
            // 尝试获取 UI Camera (如果有的话，通常 Overlay Canvas 传 null)
            // 假设 SCUICommon 内部实现可能有误，改为手动实现标准转换
            RectTransform parentRect = _m_dragCloneGO.transform.parent as RectTransform;
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
                _m_dragCloneGO.transform.localPosition = localPoint;
            }
        }
        
        private Coroutine _dragLoopCoroutine;

        private IEnumerator DragLoop()
        {
            while (_m_isDraging)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    RotateDragItem();
                }
                yield return null;
            }
        }
        
        private int _currentRotation = 0; // 0, 1, 2, 3
        
        private void RotateDragItem()
        {
            _currentRotation = (_currentRotation + 1) % 4;
            // 逆时针旋转 90 度 -> Z 轴 +90
            _m_dragCloneGO.transform.Rotate(0, 0, 90);
        }

        // Helper to rotate vector around (0,0) logic
        private Vector2Int RotateVector(Vector2Int v, int rotationSteps)
        {
            Vector2Int ret = v;
            for(int i=0; i<rotationSteps; i++)
            {
                // (x, y) -> (-y, x) for 90 degrees counter-clockwise
                ret = new Vector2Int(-ret.y, ret.x);
            }
            return ret;
        }


        /// <summary>
        /// 检查是否放置在有效区域 (已弃用，改为 GetHitGrid)
        /// </summary>
        private bool checkIsValidInstrument(PointerEventData _eventData)
        {
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(_eventData, results);
            GameObject go = results.Find(x => x.gameObject.GetComponent<UIMonoMaskCombineFaceGrid>() != null).gameObject;
            return go != null;
        }
    }
}
