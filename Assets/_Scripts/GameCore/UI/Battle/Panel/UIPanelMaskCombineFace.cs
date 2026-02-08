using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using SCFrame;
using UnityEngine;

namespace GameCore.UI
{
    public class UIPanelMaskCombineFace : _ASCUIPanelBase<UIMonoMaskCombineFace>
    {
        private List<UIMonoMaskCombineFaceGrid> _m_gridList;

        public UIPanelMaskCombineFace(UIMonoMaskCombineFace _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            CreateGrids();
        }

        private void CreateGrids()
        {
            
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
        
    }
}
