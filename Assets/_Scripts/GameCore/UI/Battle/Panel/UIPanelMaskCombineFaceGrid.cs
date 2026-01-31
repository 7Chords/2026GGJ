using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using SCFrame;
using UnityEngine;

namespace GameCore.UI
{
    public class UIPanelMaskCombineFaceGrid : _ASCUIPanelBase<UIMonoMaskCombineFace>
    {
        private List<UIMonoMaskCombineFaceGrid> _m_gridList;

        public UIPanelMaskCombineFaceGrid(UIMonoMaskCombineFace _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            CreateGrids();
        }

        private void CreateGrids()
        {
            if (mono.gridPrefab == null)
            {
                Debug.LogError("Grid Prefab is null in UIMonoMaskCombineFace!");
                return;
            }

            _m_gridList = new List<UIMonoMaskCombineFaceGrid>();
            
            int columns = 4; 
            int rows = 7;

            Transform parent = mono.transform;
            
            for (int i = 0; i < columns * rows; i++)
            {
                GameObject go = SCCommon.InstantiateGameObject(mono.gridPrefab, parent);
                go.SetActive(true);
                
                var gridMono = go.GetComponent<UIMonoMaskCombineFaceGrid>();
                if (gridMono != null)
                {
                    // 计算 GridPos (假设线性索引转二维)
                    // 以前面6x7为例
                    int x = i % columns;
                    int y = i / columns;
                    gridMono.gridPos = new Vector2(x, y);
                    
                    _m_gridList.Add(gridMono);
                }
            }

            if (mono.gridPrefab.activeSelf) mono.gridPrefab.SetActive(false);
        }

        public override void BeforeDiscard()
        {
            if (_m_gridList != null)
            {
                foreach (var grid in _m_gridList)
                {
                    if (grid != null) SCCommon.DestoryGameObject(grid.gameObject);
                }
                _m_gridList.Clear();
                _m_gridList = null;
            }
        }

        public override void OnHidePanel()
        {
        }

        public override void OnShowPanel()
        {
        }
        
        public UIMonoMaskCombineFaceGrid GetGrid(Vector2 pos)
        {
             if (_m_gridList == null) return null;
             return _m_gridList.Find(g => g.gridPos == pos);
        }
    }
}
