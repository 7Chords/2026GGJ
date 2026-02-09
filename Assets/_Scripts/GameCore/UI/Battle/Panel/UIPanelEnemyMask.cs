using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using SCFrame;
using UnityEngine;

namespace GameCore.UI
{
    public class UIPanelEnemyMask : _ASCUIPanelBase<UIMonoEnemyMask>
    {
        private List<UIPanelEnemyMaskGrid> _m_gridPanelList;
        private List<FaceGridInfo> _m_gridInfoList;
        public UIPanelEnemyMask(UIMonoEnemyMask _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_gridPanelList = new List<UIPanelEnemyMaskGrid>();
            _m_gridInfoList = new List<FaceGridInfo>();
            createGrids();
        }

        public override void BeforeDiscard()
        {
            if (_m_gridPanelList != null)
            {
                foreach (var grid in _m_gridPanelList)
                    grid?.Discard();
            }
        }

        public override void OnHidePanel()
        {
            if (_m_gridPanelList != null)
            {
                foreach (var grid in _m_gridPanelList)
                    grid?.HidePanel();
            }
        }

        public override void OnShowPanel()
        {
            if(_m_gridPanelList != null)
            {
                foreach (var grid in _m_gridPanelList)
                    grid?.ShowPanel();
            }
        }
        private void createGrids()
        {
            Vector2Int tmp = Vector2Int.zero;
            UIPanelEnemyMaskGrid panel;
            UIMonoEnemyMaskGrid gridMono;
            for (int i = 0; i < mono.column; i++)//4
            {
                for (int j = 0; j < mono.row; j++)//7
                {
                    tmp.x = i;
                    tmp.y = j;
                    GameObject go = ResourcesHelper.LoadGameObject(mono.gridPrefabName, mono.layoutGrid.transform);

                    gridMono = go.GetComponent<UIMonoEnemyMaskGrid>();
                    if (gridMono != null)
                    {
                        panel = new UIPanelEnemyMaskGrid(gridMono, SCUIShowType.INTERNAL);

                        if (mono.disabledGrids.Contains(tmp))
                        {
                            panel.ShowPanel();//单独show一下 失活状态会影响布局
                            panel.SetDisable();
                        }
                        else
                        {
                            FaceGridInfo info = new FaceGridInfo(tmp, false);
                            panel.SetInfo(info);
                            _m_gridInfoList.Add(info);
                            _m_gridPanelList.Add(panel);
                            //_m_gridGOList.Add(go);
                        }
                    }


                }
            }

        }
    }
}
