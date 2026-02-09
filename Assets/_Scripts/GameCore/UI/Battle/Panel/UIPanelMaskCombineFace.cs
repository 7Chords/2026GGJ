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

        private List<UIPanelFacePart> _m_facePartPanelList;
        public UIPanelMaskCombineFace(UIMonoMaskCombineFace _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {

            _m_gridPanelList = new List<UIPanelMaskCombineFaceGrid>();
            _m_gridInfoList = new List<FaceGridInfo>();
            _m_gridGOList = new List<GameObject>();
            _m_facePartPanelList = new List<UIPanelFacePart>();

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
            if (_m_facePartPanelList != null)
            {
                foreach (var grid in _m_facePartPanelList)
                {
                    grid?.Discard();
                }
                _m_facePartPanelList.Clear();
                _m_facePartPanelList = null;
            }
        }

        public override void OnHidePanel()
        {
            SCMsgCenter.UnregisterMsg(SCMsgConst.PLACE_PART_SUCCESS, onPlacePartSuccess);
            SCMsgCenter.UnregisterMsg(SCMsgConst.REPLACE_PART_POS_SUCCESS, onReplacePartPosSuccess);
            SCMsgCenter.UnregisterMsg(SCMsgConst.REPLACE_PART_POS_FAIL, onReplacePartPosFail);
            SCMsgCenter.UnregisterMsg(SCMsgConst.PLACE_PART_PREVIEW, onPlacePartPreview);

            if (_m_gridPanelList != null)
            {
                foreach (var grid in _m_gridPanelList)
                {
                    grid?.HidePanel();
                }
            }
            if (_m_facePartPanelList != null)
            {
                foreach (var grid in _m_facePartPanelList)
                {
                    grid?.HidePanel();
                }
            }

        }


        public override void OnShowPanel()
        {
            SCMsgCenter.RegisterMsg(SCMsgConst.PLACE_PART_SUCCESS, onPlacePartSuccess);
            SCMsgCenter.RegisterMsg(SCMsgConst.REPLACE_PART_POS_SUCCESS, onReplacePartPosSuccess);
            SCMsgCenter.RegisterMsg(SCMsgConst.REPLACE_PART_POS_FAIL, onReplacePartPosFail);
            SCMsgCenter.RegisterMsg(SCMsgConst.PLACE_PART_PREVIEW, onPlacePartPreview);

            if (_m_gridPanelList != null)
            {
                foreach (var grid in _m_gridPanelList)
                {
                    grid?.ShowPanel();
                }
            }
            if (_m_facePartPanelList != null)
            {
                foreach (var grid in _m_facePartPanelList)
                {
                    grid?.ShowPanel();
                }
            }
        }

        private void onPlacePartSuccess(object[] _objs)
        {
            if (_objs == null || _objs.Length < 3)
                return;
            PartInfo partInfo = _objs[0] as PartInfo;
            List<Vector2Int> occupyPosList = _objs[1] as List<Vector2Int>;
            List<Vector2Int> effectPosList = _objs[2] as List<Vector2Int>;

            if (occupyPosList == null || effectPosList == null)
                return;
            //设置部位当前占据的脸部格子信息
            partInfo.curOccupyFacePosList = occupyPosList;
            partInfo.curEffectFacePosList = effectPosList;

            UIPanelMaskCombineFaceGrid tmpGrid = null;
            FaceGridInfo tmpInfo = null;
            GameObject tmpGO = null;
            List<Vector3> tmpGOList = new List<Vector3>();
            for(int i =0;i<occupyPosList.Count;i++)
            {
                tmpInfo = _m_gridInfoList.Find(x => x.pos == occupyPosList[i]);
                if (tmpInfo == null)
                    continue;
                tmpInfo.hasPart = true;
                int index = _m_gridInfoList.IndexOf(tmpInfo);
                tmpGOList.Add(_m_gridGOList[index].transform.localPosition);
            }
            //计算生成的位置
            Vector2 placeWorldPos = GameCommon.CalculateWorldCenterPos(tmpGOList);
            GameObject partGO = ResourcesHelper.LoadGameObject(GameConst.PREFAB_FACE_PART, mono.tranParentPart);
            UIMonoFacePart monoFacePart = partGO.GetComponent<UIMonoFacePart>();
            UIPanelFacePart panel = new UIPanelFacePart(monoFacePart, SCUIShowType.INTERNAL);
            panel.SetLocalPos(placeWorldPos);
            panel.SetInfo(partInfo);
            panel.ShowPanel();
            _m_facePartPanelList.Add(panel);
        }

        private void onReplacePartPosSuccess(object[] _objs)
        {
            if (_objs == null || _objs.Length < 3)
                return;
            UIPanelFacePart panel = _objs[0] as UIPanelFacePart;
            List<Vector2Int> occupyPosList = _objs[1] as List<Vector2Int>;
            List<Vector2Int> effectPosList = _objs[2] as List<Vector2Int>;
            if (occupyPosList == null || effectPosList == null)
                return;
            //设置部位当前占据的脸部格子信息
            panel.partInfo.curOccupyFacePosList = occupyPosList;
            panel.partInfo.curEffectFacePosList = effectPosList;

            FaceGridInfo tmpInfo = null;
            List<Vector3> tmpGOList = new List<Vector3>();
            for (int i = 0; i < occupyPosList.Count; i++)
            {
                tmpInfo = _m_gridInfoList.Find(x => x.pos == occupyPosList[i]);
                if (tmpInfo == null)
                    continue;
                tmpInfo.hasPart = true;
                int index = _m_gridInfoList.IndexOf(tmpInfo);
                tmpGOList.Add(_m_gridGOList[index].transform.localPosition);
            }
            //计算生成的位置
            Vector2 placeWorldPos = GameCommon.CalculateWorldCenterPos(tmpGOList);
            panel.SetLocalPos(placeWorldPos);
        }

        private void onReplacePartPosFail(object[] _objs)
        {
            if (_objs == null || _objs.Length == 0)
                return;
            PartInfo partInfo = _objs[0] as PartInfo;

            UIPanelFacePart panel = _m_facePartPanelList.Find(x => x.partInfo == partInfo);
            _m_facePartPanelList.Remove(panel);
        }

        private void onPlacePartPreview(object[] _objs)
        {

        }
    }
}
