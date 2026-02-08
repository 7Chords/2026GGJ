using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using SCFrame;
using UnityEngine;

namespace GameCore.UI
{
    public class UIPanelMaskCombineFace : _ASCUIPanelBase<UIMonoMaskCombineFace>
    {
        private List<UIPanelMaskCombineFaceGrid> _m_gridList;

        private List<FaceGridInfo> _m_gridInfoList;

        public UIPanelMaskCombineFace(UIMonoMaskCombineFace _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {

            _m_gridList = new List<UIPanelMaskCombineFaceGrid>();
            _m_gridInfoList = new List<FaceGridInfo>();

            createGrids();

        }

        private void createGrids()
        {
            for(int i =0;i<mono.columnCount;i++)
            {
                for(int j =0;j<mono.rowCount;j++)
                {
                    GameObject go = ResourcesHelper.LoadGameObject(mono.gridPrefabName, mono.girdLayoutGroup.transform);
                    UIMonoMaskCombineFaceGrid gridMono = go.GetComponent<UIMonoMaskCombineFaceGrid>();
                    if (gridMono != null)
                    {
                        UIPanelMaskCombineFaceGrid panel = new UIPanelMaskCombineFaceGrid(gridMono, SCUIShowType.INTERNAL);
                        FaceGridInfo info = new FaceGridInfo(new Vector2Int(i, j), false);
                        _m_gridInfoList.Add(info);
                        _m_gridList.Add(panel);
                    }
                }
            }
            GameModel.instance.faceGridInfoList = _m_gridInfoList;
        }

        public override void BeforeDiscard()
        {
            if (_m_gridList != null)
            {
                foreach (var grid in _m_gridList)
                {
                    grid?.Discard();
                }
                _m_gridList.Clear();
                _m_gridList = null;
            }
        }

        public override void OnHidePanel()
        {
            if (_m_gridList != null)
            {
                foreach (var grid in _m_gridList)
                {
                    grid?.HidePanel();
                }
            }
        }

        public override void OnShowPanel()
        {
            if (_m_gridList != null)
            {
                foreach (var grid in _m_gridList)
                {
                    grid?.ShowPanel();
                }
            }
        }
        
    }
}
