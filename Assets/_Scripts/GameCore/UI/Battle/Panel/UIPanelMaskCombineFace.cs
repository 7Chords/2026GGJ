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
        private List<GameObject> _m_gridGOList;
        public UIPanelMaskCombineFace(UIMonoMaskCombineFace _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {

            _m_gridList = new List<UIPanelMaskCombineFaceGrid>();
            _m_gridInfoList = new List<FaceGridInfo>();
            _m_gridGOList = new List<GameObject>();

            createGrids();

        }

        private void createGrids()
        {
            Vector2Int tmp = Vector2Int.zero;
            UIPanelMaskCombineFaceGrid panel;
            UIMonoMaskCombineFaceGrid gridMono;
            for (int i =0;i<mono.columnCount; i++)//4
            {
                for(int j =0;j<mono.rowCount;j++)//7
                {
                    tmp.x = i;
                    tmp.y = j;
                    GameObject go = ResourcesHelper.LoadGameObject(mono.gridPrefabName, mono.girdLayoutGroup.transform);

                    gridMono = go.GetComponent<UIMonoMaskCombineFaceGrid>();
                    if (gridMono != null)
                    {
                        panel = new UIPanelMaskCombineFaceGrid(gridMono, SCUIShowType.INTERNAL);

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
                            _m_gridList.Add(panel);
                            _m_gridGOList.Add(go);
                        }
                    }


                }
            }
            GameModel.instance.faceGridInfoList = _m_gridInfoList;
            GameModel.instance.faceGOList = _m_gridGOList;

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
