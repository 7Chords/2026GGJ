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

        }
        private void onEndDrag(PointerEventData _arg, object[] _objs)
        {
            _m_isDraging = false;

            UIMonoMaskCombineFaceGrid hitGrid = GetHitGrid(_arg);
            bool placementSuccess = false;

            if (hitGrid != null)
            {
                 Vector2Int targetGridPos = Vector2Int.RoundToInt(hitGrid.gridPos);
                 
                 // 检查占用
                 bool isOccupied = _m_container != null && _m_container.CheckOccupancy(targetGridPos, _m_partInfo);
                 
                 if (!isOccupied)
                 {
                     // 放置成功
                     _m_partInfo.gridPos = targetGridPos;
                     
                     // 视觉放置：将 Item 移动到 Grid 的位置
                     // 注意：这里需要根据具体的层级结构来决定是 SetParent 还是 SetPosition
                     // 假设我们只是将 Item 移动到 Grid 的位置 (Position snap)
                     // 如果 Grid 和 Item 在同一个 Canvas 下，可以直接转换坐标
                     SnapToGrid(hitGrid);
                     placementSuccess = true;
                 }
                 else
                 {
                     Debug.Log("该位置已被占用！");
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

        private void ReturnToBag()
        {
            if (_m_container == null) return;
            
            // 将 item 的父物体设置回容器的布局组件
            // 注意：_m_container.GetMono() 可以获取 Mono 引用
            var containerMono = _m_container.mono;
            if (containerMono != null && containerMono.layoutGroup != null)
            {
                GetGameObject().transform.SetParent(containerMono.layoutGroup.transform);
                GetGameObject().transform.localScale = Vector3.one;
                
                // 重置 GridPos，表示不再占用格子
                if (_m_partInfo != null)
                {
                    _m_partInfo.gridPos = new Vector2Int(-1, -1);
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

            // 创建克隆体
            _m_dragCloneGO = SCCommon.InstantiateGameObject(GetGameObject(), SCGame.instance.fullLayerRoot.transform);
            
            // 确保克隆体不阻挡射线，否则 RaycastAll 可能会先打到克隆体
            var canvasGroup = _m_dragCloneGO.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = _m_dragCloneGO.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.7f;

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

            Vector2 localPointerPosition = SCUICommon.ScreenPointToUIPoint(SCGame.instance.fullLayerRoot.transform as RectTransform, eventData.position);
            _m_dragCloneGO.transform.localPosition = localPointerPosition;
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
