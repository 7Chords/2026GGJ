using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using SCFrame;
using UnityEngine;
using System;

namespace GameCore.UI
{
    public class UIPanelMaskCombineFace : _ASCUIPanelBase<UIMonoMaskCombineFace>
    {
        private List<UIPanelMaskCombineFaceGrid> _m_gridPanelList;

        private List<FaceGridInfo> _m_gridInfoList;
        private List<GameObject> _m_gridGOList;
        public UIPanelMaskCombineFace(UIMonoMaskCombineFace _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {

            _m_gridPanelList = new List<UIPanelMaskCombineFaceGrid>();
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
                            _m_gridPanelList.Add(panel);
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
            if (_m_gridPanelList != null)
            {
                foreach (var grid in _m_gridPanelList)
                {
                    grid?.Discard();
                }
                _m_gridPanelList.Clear();
                _m_gridPanelList = null;
            }
        }

        public override void OnHidePanel()
        {
            SCMsgCenter.UnregisterMsg(SCMsgConst.PLACE_PART_SUCCESS, onPlacePartSuccess);

            if (_m_gridPanelList != null)
            {
                foreach (var grid in _m_gridPanelList)
                {
                    grid?.HidePanel();
                }
            }
        }

        public override void OnShowPanel()
        {
            SCMsgCenter.RegisterMsg(SCMsgConst.PLACE_PART_SUCCESS, onPlacePartSuccess);

            if (_m_gridPanelList != null)
            {
                foreach (var grid in _m_gridPanelList)
                {
                    grid?.ShowPanel();
                }
            }
        }

        private void onPlacePartSuccess(object[] _objs)
        {
            if (_objs == null || _objs.Length == 0)
                return;
            List<Vector2Int> gridPosList = (List<Vector2Int>)_objs[0];
            if (gridPosList == null)
                return;
            UIPanelMaskCombineFaceGrid tmpGrid = null;
            FaceGridInfo tmpInfo = null;
            GameObject tmpGO = null;
            List<GameObject> tmpGOList = new List<GameObject>();
            for(int i =0;i<gridPosList.Count;i++)
            {
                tmpInfo = _m_gridInfoList.Find(x => x.pos == gridPosList[i]);
                if (tmpInfo == null)
                    continue;
                tmpInfo.hasPart = true;
                int index = _m_gridInfoList.IndexOf(tmpInfo);
                tmpGOList.Add(_m_gridGOList[index]);
            }
            Vector3 placeWorldPos = Vector2.zero;
            for (int i = 0; i < tmpGOList.Count; i++)
            {
                placeWorldPos += tmpGOList[i].transform.position;
            }
            placeWorldPos /= tmpGOList.Count;

        }
    }
}
