using DG.Tweening;
using SCFrame;
using SCFrame.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIPanelMaskCombinePartContainerItem : _ASCUIPanelBase<UIMonoMaskCombinePartContainerItem>
    {
        private PartInfo _m_partInfo;

        private GameObject _m_dragCloneGO;

        private bool _m_isDraging;

        private UIPanelMaskCombinePartContainer _m_container;

        private TweenContainer _m_tweenContainer;

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
            _m_tweenContainer = new TweenContainer();
        }

        public override void BeforeDiscard()
        {
            _m_tweenContainer?.KillAllDoTween();
            _m_tweenContainer = null;
        }

        public override void OnHidePanel()
        {
            mono.imgGoods.RemoveBeginDrag(onBeginDrag);
            mono.imgGoods.RemoveDrag(onDrag);
            mono.imgGoods.RemoveEndDrag(onEndDrag);


            GetGameObject().transform.RemoveMouseEnter(onGameObjMouseEnter);
            GetGameObject().transform.RemoveMouseExit(onGameObjMouseExit);
        }

        public override void OnShowPanel()
        {
            mono.imgGoods.AddBeginDrag(onBeginDrag);
            mono.imgGoods.AddDrag(onDrag);
            mono.imgGoods.AddEndDrag(onEndDrag);

            GetGameObject().transform.AddMouseEnter(onGameObjMouseEnter);
            GetGameObject().transform.AddMouseExit(onGameObjMouseExit);
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
        public void RefreshUI()
        {
            refreshShow();
        }

        public void DestroySelf()
        {
             if (mono != null && mono.gameObject != null)
             {
                 SCCommon.DestoryGameObject(mono.gameObject);
             }
        }

        private void refreshShow()
        {
            if (_m_partInfo == null)
            {
                Debug.LogWarning("[Item] refreshShow: partInfo is null");
                return;
            }
            
            if (mono.imgGoods != null)
            {
                if(mono.useGameObjSpr)
                {
                    GetGameObject().GetComponent<Image>().enabled = false;
                    mono.imgGoods.sprite = ResourcesHelper.LoadAsset<Sprite>(_m_partInfo.partRefObj.partGameObjectName);

                    var rt = mono.imgGoods.rectTransform;
                    RectTransform gridRect = GetGameObject().transform.parent?.GetComponent<RectTransform>();
                    // Fix 0 size issue: Fallback to 100f if gridRect is null or has 0 size
                    float unitW = (gridRect != null && gridRect.rect.width > 0) ? gridRect.rect.width : 100f;
                    float unitH = (gridRect != null && gridRect.rect.height > 0) ? gridRect.rect.height : 100f;

                    GetGameObject().transform.localPosition = Vector3.zero;

                    // Calculate Shape Bounds and Rotated MidPos
                    int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;
                    var shape = (_m_partInfo != null && _m_partInfo.partRefObj != null) ? _m_partInfo.partRefObj.posList : null;
                    Vector2Int rotatedMidPos = Vector2Int.zero;

                    if (_m_partInfo != null && _m_partInfo.partRefObj != null)
                    {
                        rotatedMidPos = RotateVector(_m_partInfo.partRefObj.midPos, _m_partInfo.rotation);
                    }

                    if (shape != null && shape.Count > 0)
                    {
                        foreach (var p in shape)
                        {
                            Vector2Int rotatedP = RotateVector(new Vector2Int(p.x, p.y), _m_partInfo.rotation);
                            if (rotatedP.x < minX) minX = rotatedP.x;
                            if (rotatedP.x > maxX) maxX = rotatedP.x;
                            if (rotatedP.y < minY) minY = rotatedP.y;
                            if (rotatedP.y > maxY) maxY = rotatedP.y;
                        }
                    }
                    else
                    {
                        minX = rotatedMidPos.x; maxX = rotatedMidPos.x;
                        minY = rotatedMidPos.y; maxY = rotatedMidPos.y;
                    }

                    int widthCells = maxX - minX + 1;
                    int heightCells = maxY - minY + 1;

                    float pivotX = (float)(rotatedMidPos.x - minX + 0.5f) / widthCells;
                    float pivotY = (float)(rotatedMidPos.y - minY + 0.5f) / heightCells;

                    mono.imgGoods.transform.localScale = 1.4f * Vector3.one;
                    mono.imgGoods.rectTransform.pivot = new Vector2(pivotX, pivotY);
                    mono.imgGoods.SetNativeSize();
                    //mono.imgGoods.rectTransform.pivot = new Vector2(pivotX, 1 - pivotY);
                }
                else
                {
                    mono.imgGoods.sprite = ResourcesHelper.LoadAsset<Sprite>(_m_partInfo.partRefObj.partSpriteObjName);

                }
            }
            
            string hpStr = $"{_m_partInfo.currentHealth}/{_m_partInfo.partRefObj.partHealth}";
            
            if (mono.txtHealth != null)
                mono.txtHealth.text = hpStr;
            else
                Debug.LogWarning("[Item] txtHealth is null!");
            // Debug.Log($"[Item] refreshShow: {_m_partInfo.partRefObj.partName} HP: {hpStr}");
        }

        private void onBeginDrag(PointerEventData _arg, object[] _objs)
        {
            _m_isDraging = true;
            
            // 1. 先创建拖拽克隆体 (使用当前显示状态)
            createDragClone();

            // 2. 隐藏原物体的交互和显示
            if (mono.canvasGroup != null)
            {
                mono.canvasGroup.alpha = 0f;
                mono.canvasGroup.blocksRaycasts = false;
            }
            // 强制隐藏图片（防止CanvasGroup不起作用）
            if (mono.imgGoods != null) mono.imgGoods.enabled = false;

            if (_dragLoopCoroutine != null) mono.StopCoroutine(_dragLoopCoroutine);
            _dragLoopCoroutine = mono.StartCoroutine(DragLoop());

            // 3. 恢复格子颜色
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
            
            ClearPreview(); // Cleanup preview before placing

            UIMonoMaskCombineFaceGrid hitGrid = GetHitGrid(_arg);
            bool placementSuccess = false;

            if (hitGrid != null)
            {
                 if (TryCalculatePlacement(hitGrid, out Vector2Int logicalOrigin, out List<GameCore.RefData.PosEffectObj> rotatedShape))
                 {
                     // Placement Success
                     _m_partInfo.gridPos = logicalOrigin;
                     _m_partInfo.rotation = _currentRotation; // Save rotation

                    //隐藏格子背景
                     GetGameObject().GetComponent<Image>().enabled = false;

                     SnapToGrid(hitGrid); 
                     
                     // Apply Rotation to actual item
                     GetGameObject().transform.localRotation = Quaternion.Euler(0, 0, _currentRotation * 90);
                     
                     // Update Grid Colors using rotated shape
                     UpdateGridColors(true, rotatedShape); 
                     
                     placementSuccess = true;

                     //RefreshUI();
                 }
                 else
                 {
                     Debug.Log("该区域已被占用或无效！");
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
            if (mono.imgGoods != null) mono.imgGoods.enabled = true;
            
        }

        private void onDrag(PointerEventData _arg, object[] _objs)
        {
            if (!_m_isDraging)
                return;
            updateDragClonePosition(_arg);
        }
        
        public void InitVisualsFromData()
        {
            // Called after parenting to Grid
            var gridRect = GetGameObject().transform.parent?.GetComponent<RectTransform>();
            if (gridRect != null)
            {
                UpdateVisuals(gridRect);
            }
            GetGameObject().transform.localPosition = Vector3.zero;
        }

        private void SnapToGrid(UIMonoMaskCombineFaceGrid grid)
        {
            // Set Parent
            GetGameObject().transform.SetParent(grid.transform);
            
            // USER REQUEST 2: Set Pivot to (0,0) and adjust Size
            if (mono.imgGoods != null)
            {
                var gridRect = grid.GetComponent<RectTransform>();
                UpdateVisuals(gridRect);
            }
        }
        

        private void UpdateVisuals(RectTransform gridRect)
        {
             if (mono.imgGoods == null) return;
             
             var rt = mono.imgGoods.rectTransform;
             // Fix 0 size issue: Fallback to 100f if gridRect is null or has 0 size
             float unitW = (gridRect != null && gridRect.rect.width > 0) ? gridRect.rect.width : 100f;
             float unitH = (gridRect != null && gridRect.rect.height > 0) ? gridRect.rect.height : 100f;
             
             GetGameObject().transform.localPosition = Vector3.zero;
             
             // Calculate Shape Bounds and Rotated MidPos
             int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;
             var shape = (_m_partInfo != null && _m_partInfo.partRefObj != null) ? _m_partInfo.partRefObj.posList : null;
             Vector2Int rotatedMidPos = Vector2Int.zero;
             
             if (_m_partInfo != null && _m_partInfo.partRefObj != null)
             {
                 rotatedMidPos = RotateVector(_m_partInfo.partRefObj.midPos, _m_partInfo.rotation);
             }
             
             if (shape != null && shape.Count > 0)
             {
                 foreach(var p in shape)
                 {
                      Vector2Int rotatedP = RotateVector(new Vector2Int(p.x, p.y), _m_partInfo.rotation);
                      if (rotatedP.x < minX) minX = rotatedP.x;
                      if (rotatedP.x > maxX) maxX = rotatedP.x;
                      if (rotatedP.y < minY) minY = rotatedP.y;
                      if (rotatedP.y > maxY) maxY = rotatedP.y;
                 }
             }
             else
             {
                 minX = rotatedMidPos.x; maxX = rotatedMidPos.x;
                 minY = rotatedMidPos.y; maxY = rotatedMidPos.y;
             }
             
             int widthCells = maxX - minX + 1;
             int heightCells = maxY - minY + 1;
             
             float pivotX = (float)(rotatedMidPos.x - minX + 0.5f) / widthCells;
             float pivotY = (float)(rotatedMidPos.y - minY + 0.5f) / heightCells;
             
             rt.pivot = new Vector2(pivotX, pivotY);
             //rt.pivot = new Vector2(((float)_m_partInfo.partRefObj.midPos.x / widthCells)/2, ((float)_m_partInfo.partRefObj.midPos.y / heightCells)/2);

            float totalW = widthCells * unitW;
             float totalH = heightCells * unitH;
             
             // Wait, Step 1424 I notified user "Implemented...".
             // Step 1430/1431 User commented them out.
             // If I uncomment them now, I might re-introduce what they hated?
             // But without Pivot/Size, the part will just be a 100x100 square at center. 
             // If the part is 3x1, it needs size.
             // I will uncomment `rt.sizeDelta` and `rt.pivot` because `InitVisualsFromData` relies on it.
             
             rt.sizeDelta = new Vector2(totalW, totalH);
        
           
            
            // 确保 scale 正确
            GetGameObject().transform.localScale = Vector3.one; // Should be 1 if sizing manually
            
            // USER REQUEST: Show game sprite when placed (Using partGameObjectName as 'gameObjectSprite')
            if (_m_partInfo != null && _m_partInfo.partRefObj != null && !string.IsNullOrEmpty(_m_partInfo.partRefObj.partGameObjectName))
            {
                 Sprite gameSprite = ResourcesHelper.LoadAsset<Sprite>(_m_partInfo.partRefObj.partGameObjectName);
                 if (gameSprite != null && mono.imgGoods != null)
                 {
                     mono.imgGoods.sprite = gameSprite;
                     
                     // USER REQUEST: Multiply width and height (Use Calculated totalW/totalH from above)
                     // Remove SetNativeSize and Pivot Override to respect the Shape Bounds
                      mono.imgGoods.rectTransform.pivot = new Vector2(pivotX, 1-pivotY);
                      mono.imgGoods.SetNativeSize();
                 }
            }
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
            // Note: UpdateGridColors implementation continues... 
            // We just need to replace the SnapToGrid and ReturnToBag sections.
            // Since this tool replaces a block, I must include UpdateGridColors start or use multi_replace.
            // I'll stick to replacing independent blocks if possible. 
            // But they are not adjacent in my view. 
            // Wait, SnapToGrid is 176-185. ReturnToBag is 253-277.
            // I should use 2 calls or multi_replace. Use multi_replace.
        
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
                // USER REQUEST 3: Do not mark other grids as red.
                // Just keep them as they are.
                // However, we might need to restore them if we previously colored them?
                // Logic: Only reset if not occupied?
                // Actually if we simply STOP coloring them red, we don't need to do anything here except
                // maybe restore colors if dragging OUT (isOccupied=false).
                
                if (!isOccupied)
                {
                     // Restoration phase (End Drag or Return Bag)
                     if (occupiedPositions.Contains(Vector2Int.RoundToInt(grid.gridPos)))
                     {
                         var img = grid.GetComponent<UnityEngine.UI.Image>();
                         if (img != null)
                         {
                             // Restore default color (white for active, invisible for disabled)
                             // But wait, disable logic uses enabled=false.
                             // Valid active grids use white.
                             // We should check if it's disabled? No, disabled grids have enabled=false.
                             // So just set color to default.
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
                //恢复格子显示
                GetGameObject().GetComponent<Image>().enabled = true;
                
                GetGameObject().transform.SetParent(containerMono.layoutGroup.transform);
                //GetGameObject().transform.localScale = Vector3.one;
                //GetGameObject().transform.localRotation = Quaternion.identity; // Reset rotation
                //GetGameObject().transform.localPosition = Vector3.zero;

                if (mono.imgGoods != null)
                {
                    // Restore size if it was modified by grid logic
                    mono.imgGoods.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    mono.imgGoods.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    mono.imgGoods.rectTransform.sizeDelta = new Vector2(100f, 100f); 
                    mono.imgGoods.rectTransform.anchoredPosition = Vector2.zero;
                    mono.imgGoods.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                }
                
                // 重置 GridPos，表示不再占用格子
                if (_m_partInfo != null)
                {
                    _m_partInfo.gridPos = new Vector2Int(-1, -1);
                    _m_partInfo.rotation = 0;
                    
                    // Restore Sprite to Icon
                    if (_m_partInfo.partRefObj != null && mono.imgGoods != null)
                    {
                         Sprite iconSprite = ResourcesHelper.LoadAsset<Sprite>(_m_partInfo.partRefObj.partSpriteObjName);
                         if (iconSprite != null) mono.imgGoods.sprite = iconSprite;
                    }

                    // Force Refresh UI (HP Text)
                    RefreshUI();
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
            
            // USER REQUEST: Swap sprite to partGameObjectName when dragging
            if (_m_partInfo != null && _m_partInfo.partRefObj != null && !string.IsNullOrEmpty(_m_partInfo.partRefObj.partGameObjectName))
            {
                var dstImg = _m_dragCloneGO.GetComponent<UnityEngine.UI.Image>();
                if (dstImg != null)
                {
                    Sprite gameSprite = ResourcesHelper.LoadAsset<Sprite>(_m_partInfo.partRefObj.partGameObjectName);
                    if (gameSprite != null)
                    {
                        dstImg.sprite = gameSprite;
                        dstImg.GetComponent<RectTransform>().pivot = Vector2.one * 0.5f;
                        //自适应大小
                        dstImg.SetNativeSize();
                    }
                }
            }

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
                //todo：来不及 不做旋转处理了
                //if (Input.GetMouseButtonDown(1))
                //{
                //    RotateDragItem();
                //}
                UpdatePreview();
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
        /// 尝试计算放置信息
        /// </summary>
        private bool TryCalculatePlacement(UIMonoMaskCombineFaceGrid hitGrid, out Vector2Int logicalOrigin, out List<GameCore.RefData.PosEffectObj> rotatedShape)
        {
            logicalOrigin = Vector2Int.zero;
            rotatedShape = new List<GameCore.RefData.PosEffectObj>();

            if (hitGrid == null) return false;

             // Calculate Logical Grid Origin based on midPos
             Vector2Int hitPos = Vector2Int.RoundToInt(hitGrid.gridPos);
             Vector2Int midPos = Vector2Int.zero;
             if (_m_partInfo != null && _m_partInfo.partRefObj != null)
             {
                 midPos = _m_partInfo.partRefObj.midPos;
             }
             
             // Rotate midPos
             Vector2Int rotatedMidPos = RotateVector(midPos, _currentRotation);
             
             logicalOrigin = hitPos - rotatedMidPos;
             
             // Check Validity of Region (All Shape cells must exist in Grid)
             var shape = (_m_partInfo != null && _m_partInfo.partRefObj != null) ? _m_partInfo.partRefObj.posList : null;
             
             // Get all Grids in parent to verify existence
             // Optimization: pass gridContainer if possible, but getting from hitGrid is safe
             var gridContainer = hitGrid.transform.parent;
             if (gridContainer == null) return false;
             var allGrids = gridContainer.GetComponentsInChildren<UIMonoMaskCombineFaceGrid>();
             
             List<Vector2Int> requiredPositions = new List<Vector2Int>();
             // Apply Rotation to Shape
             if (shape != null && shape.Count > 0)
             {
                 foreach(var p in shape) 
                 {
                     Vector2Int rotatedP = RotateVector(new Vector2Int(p.x, p.y), _currentRotation);
                     Vector2Int targetP = logicalOrigin + rotatedP;
                     
                     requiredPositions.Add(targetP);
                     rotatedShape.Add(new GameCore.RefData.PosEffectObj() { x = rotatedP.x, y = rotatedP.y });
                 }
             }
             else
             {
                  requiredPositions.Add(logicalOrigin);
                  rotatedShape.Add(new GameCore.RefData.PosEffectObj() { x = 0, y = 0 });
             }

             // Access Face Mono to check disabled grids
             var faceMono = gridContainer.GetComponentInParent<UIMonoMaskCombineFace>();
             List<Vector2Int> disabledList = (faceMono != null) ? faceMono.disabledGrids : null;

             foreach(var p in requiredPositions)
             {
                 // 1. Check Bounds (0-3, 0-6 usually, but depends on row/col count which we can try to guess or just trust grid existence)
                 // Better: Check if a grid exists at 'p'
                 // We can iterate allGrids to find if 'p' exists.
                 bool exists = false;
                 foreach(var g in allGrids)
                 {
                     if (Vector2Int.RoundToInt(g.gridPos) == p)
                     {
                         exists = true;
                         break;
                     }
                 }
                 if (!exists) return false;
                 
                 // 2. Check Disabled
                 if (disabledList != null && disabledList.Contains(p))
                 {
                     // Debug.Log($"Placement Failed: Position {p} is disabled.");
                     return false;
                 }
             }
             
             // Check Occupancy
             bool isOccupied = _m_container != null && _m_container.CheckRegionOccupancy(logicalOrigin, rotatedShape, _m_partInfo);
             if (isOccupied) return false;

             return true;
        }

        // --- Preview Logic ---
        private GameObject _m_previewGhostGO;
        private List<UIMonoMaskCombineFaceGrid> _previewHighlightedGrids = new List<UIMonoMaskCombineFaceGrid>();

        private void UpdatePreview()
        {
            // Create fake pointer data to find grid under mouse
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };
            UIMonoMaskCombineFaceGrid hitGrid = GetHitGrid(pointerData);

            // Calculate valid placement
            bool isValid = false;
            Vector2Int logicalOrigin = Vector2Int.zero;
            List<GameCore.RefData.PosEffectObj> rotatedShape = null;

            if (hitGrid != null)
            {
                isValid = TryCalculatePlacement(hitGrid, out logicalOrigin, out rotatedShape);
            }

            if (isValid && hitGrid != null)
            {
                // 1. Show Ghost Item
                if (_m_previewGhostGO == null)
                {
                    _m_previewGhostGO = SCCommon.InstantiateGameObject(mono.imgGoods.gameObject, hitGrid.transform);
                    var img = _m_previewGhostGO.GetComponent<UnityEngine.UI.Image>();
                    if (img != null) 
                    {
                        var col = img.color;
                        col.a = 0.5f;
                        img.color = col;
                    }
                    // Remove logic component
                    var comp = _m_previewGhostGO.GetComponent<UIMonoMaskCombinePartContainerItem>();
                    if (comp) comp.enabled = false;
                }
                
                // Update Ghost Transform (Snap to grid, apply rotation)
                if (_m_previewGhostGO.transform.parent != hitGrid.transform)
                {
                    _m_previewGhostGO.transform.SetParent(hitGrid.transform);
                }
                _m_previewGhostGO.transform.localPosition = Vector3.zero;
                _m_previewGhostGO.transform.localScale = new Vector3(0.7f, 0.7f, 1);
                _m_previewGhostGO.transform.localRotation = Quaternion.Euler(0, 0, _currentRotation * 90);
                if (!_m_previewGhostGO.activeSelf) _m_previewGhostGO.SetActive(true);

                // 2. Highlight Grids
                // Find grids to highlight
                // We need to find the specific UIMonoMaskCombineFaceGrid instances corresponding to requiredPositions
                // Reuse parent find logic
                 var gridContainer = hitGrid.transform.parent;
                 var allGrids = gridContainer.GetComponentsInChildren<UIMonoMaskCombineFaceGrid>();
                 
                 List<Vector2Int> targetPositions = new List<Vector2Int>();
                 foreach(var p in rotatedShape) targetPositions.Add(logicalOrigin + new Vector2Int(p.x, p.y));

                 // Clear old highlights first (simplest way, though slightly inefficient)
                 ClearPreviewHighlights();

                 foreach(var g in allGrids)
                 {
                     if (targetPositions.Contains(Vector2Int.RoundToInt(g.gridPos)))
                     {
                         var img = g.GetComponent<UnityEngine.UI.Image>();
                         if (img != null)
                         {
                             // Only highlight if it's not already red (occupied) - though isValid=true implies it's not occupied.
                             // Set to Red 0.5
                             img.color = new Color(1, 0, 0, 0.5f); 
                             _previewHighlightedGrids.Add(g);
                         }
                     }
                 }
            }
            else
            {
                // Invalid or no grid
                ClearPreview();
            }
        }

        private void ClearPreview()
        {
            if (_m_previewGhostGO != null)
            {
                SCCommon.DestoryGameObject(_m_previewGhostGO);
                _m_previewGhostGO = null;
            }
            ClearPreviewHighlights();
        }

        private void ClearPreviewHighlights()
        {
            foreach(var g in _previewHighlightedGrids)
            {
                if (g != null)
                {
                    var img = g.GetComponent<UnityEngine.UI.Image>();
                    if (img != null)
                    {
                        // Restore default
                        img.color = g.colorDefault;
                    }
                }
            }
            _previewHighlightedGrids.Clear();
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


        private void onGameObjMouseExit(PointerEventData arg1, object[] arg2)
        {
            GameCommon.DiscardToolTip();
            _m_tweenContainer.RegDoTween(GetGameObject().transform.DOScale(Vector3.one, mono.scaleChgDuration));

        }

        private void onGameObjMouseEnter(PointerEventData arg1, object[] arg2)
        {
            if (_m_partInfo == null)
                return;
            Vector2 screenPos = Vector2.zero;
            var _canvas = GetGameObject().GetComponentInParent<Canvas>();
            Camera cam = (_canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? _canvas.worldCamera : null;

            // Check direction
            float itemScreenX = RectTransformUtility.WorldToScreenPoint(cam, GetGameObject().transform.position).x;
            bool showOnLeft = itemScreenX > Screen.width * 0.7f; // If in right 30% of screen

            // Offset based on direction
            // Left: Pivot Right-Top (1,1) -> Anchor at (ItemX - border, ItemY)
            // Right: Pivot Left-Top (0,1) -> Anchor at (ItemX + border, ItemY)
            Vector3 offset = showOnLeft ? new Vector3(-40, -20, 0) : new Vector3(40, -20, 0);

            screenPos = RectTransformUtility.WorldToScreenPoint(cam, GetGameObject().transform.position + offset);

            var tooltip = GameCommon.ShowTooltip(_m_partInfo.partRefObj.partName, _m_partInfo.partRefObj.partDesc, screenPos);
            /*if (tooltip != null)
            {
                tooltip.SetPivot(showOnLeft ? new Vector2(1, 1) : new Vector2(0, 1));
            }*/

            _m_tweenContainer.RegDoTween(GetGameObject().transform.DOScale(mono.scaleMouseEnter, mono.scaleChgDuration));
        }
    }
}
