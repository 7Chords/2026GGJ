using GameCore;
using GameCore.Battle;
using SCFrame;
using SCFrame.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.UI
{
    public class UIPanelMaskCombineFace : _ASCUIPanelBase<UIMonoMaskCombineFace>
    {
        private List<UIPanelMaskCombineFaceGrid> _m_gridPanelList;

        private List<FaceGridInfo> _m_gridInfoList;
        private List<GameObject> _m_gridGOList;

        private List<UIPanelPlayerFacePart> _m_facePartPanelList;
        private List<PartInfo> _m_playerBattlePartInfoList;
        public UIPanelMaskCombineFace(UIMonoMaskCombineFace _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {

            _m_gridPanelList = new List<UIPanelMaskCombineFaceGrid>();
            _m_gridInfoList = new List<FaceGridInfo>();
            _m_gridGOList = new List<GameObject>();
            _m_facePartPanelList = new List<UIPanelPlayerFacePart>();
            _m_playerBattlePartInfoList = new List<PartInfo>();

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
                            SCCommon.SetGameObjectEnable(go, true);
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
            GameModel.instance.playerFaceGridInfoList = _m_gridInfoList;
            GameModel.instance.playerFaceGridGOList = _m_gridGOList;
            GameModel.instance.playerInfo.battlePartInfoList = _m_playerBattlePartInfoList;

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
            onClearPreview();

            SCMsgCenter.UnregisterMsg(SCMsgConst.PLACE_PART_SUCCESS, onPlacePartSuccess);
            SCMsgCenter.UnregisterMsg(SCMsgConst.REPLACE_PART_POS_SUCCESS, onReplacePartPosSuccess);
            SCMsgCenter.UnregisterMsg(SCMsgConst.REPLACE_PART_POS_FAIL, onReplacePartPosFail);
            SCMsgCenter.UnregisterMsg(SCMsgConst.PLACE_PART_PREVIEW, onPlacePartPreview);
            SCMsgCenter.UnregisterMsgAct(SCMsgConst.CLEAR_PLAYER_PREVIEW, onClearPreview);
            SCMsgCenter.UnregisterMsg(SCMsgConst.PLAYER_FACE_PART_RANGE_HIGHLIGHT, onFacePartRangeHighlight);

            //???????????
            SCMsgCenter.UnregisterMsg(SCMsgConst.ENEMY_FACE_PART_RANGE_HIGHLIGHT, onFacePartRangeHighlight);
            SCMsgCenter.UnregisterMsgAct(SCMsgConst.CLEAR_ENEMY_PREVIEW, onClearPreview);
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
            onClearPreview();

            SCMsgCenter.RegisterMsg(SCMsgConst.PLACE_PART_SUCCESS, onPlacePartSuccess);
            SCMsgCenter.RegisterMsg(SCMsgConst.REPLACE_PART_POS_SUCCESS, onReplacePartPosSuccess);
            SCMsgCenter.RegisterMsg(SCMsgConst.REPLACE_PART_POS_FAIL, onReplacePartPosFail);
            SCMsgCenter.RegisterMsg(SCMsgConst.PLACE_PART_PREVIEW, onPlacePartPreview);
            SCMsgCenter.RegisterMsgAct(SCMsgConst.CLEAR_PLAYER_PREVIEW, onClearPreview);
            SCMsgCenter.RegisterMsg(SCMsgConst.PLAYER_FACE_PART_RANGE_HIGHLIGHT, onFacePartRangeHighlight);

            //???????????
            SCMsgCenter.RegisterMsg(SCMsgConst.ENEMY_FACE_PART_RANGE_HIGHLIGHT, onFacePartRangeHighlight);
            SCMsgCenter.RegisterMsgAct(SCMsgConst.CLEAR_ENEMY_PREVIEW, onClearPreview);

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
                    grid?.HidePanel();
                    grid?.Discard();
                }
                _m_facePartPanelList.Clear();
            }
        }

        /// <summary> 放置成功后拖放预览格、目标高亮与数值预览需立刻清掉，否则会残留到进战再返回。 </summary>
        private void clearPlacementPreviewUi()
        {
            onClearPreview();
            SCMsgCenter.SendMsg(SCMsgConst.FACE_PART_TARTGET_PREVIEW_CANCEL);
            PlacementPreviewHelper.BroadcastClear();
        }

        private void onPlacePartSuccess(object[] _objs)
        {
            if (_objs == null || _objs.Length < 3)
                return;
            clearPlacementPreviewUi();

            PartInfo partInfo = _objs[0] as PartInfo;
            List<Vector2Int> occupyPosList = _objs[1] as List<Vector2Int>;
            List<Vector2Int> effectPosList = _objs[2] as List<Vector2Int>;

            if (occupyPosList == null || effectPosList == null)
                return;


            _m_playerBattlePartInfoList.Add(partInfo);

            //??????????????????????????
            partInfo.curOccupyFacePosList = occupyPosList;
            partInfo.curEffectFacePosList = effectPosList;
            partInfo.isOnFace = true;

            FaceGridInfo tmpInfo = null;
            List<Vector3> tmpGOList = new List<Vector3>();
            for(int i =0;i<occupyPosList.Count;i++)
            {
                tmpInfo = _m_gridInfoList.Find(x => x.pos == occupyPosList[i]);
                if (tmpInfo == null)
                    continue;
                tmpInfo.SetOwnerPart(partInfo);
                int index = _m_gridInfoList.IndexOf(tmpInfo);
                tmpGOList.Add(_m_gridGOList[index].transform.localPosition);
            }
            //?????????????
            Vector2 placeWorldPos = GameCommon.CalculateStandardCenterPos(tmpGOList);
            GameObject partGO = ResourcesHelper.LoadGameObject(GameConst.PREFAB_PLAYER_FACE_PART, mono.tranParentPart);
            UIMonoPlayerFacePart monoFacePart = partGO.GetComponent<UIMonoPlayerFacePart>();
            UIPanelPlayerFacePart panel = new UIPanelPlayerFacePart(monoFacePart, SCUIShowType.INTERNAL);
            panel.SetLocalPos(placeWorldPos);
            panel.SetInfo(partInfo);
            panel.ShowPanel();
            _m_facePartPanelList.Add(panel);

            SCMsgCenter.SendMsg(SCMsgConst.FACE_PART_ORDER_CHG);

        }
        private void onReplacePartPosSuccess(object[] _objs)
        {
            if (_objs == null || _objs.Length < 3)
                return;
            clearPlacementPreviewUi();

            UIPanelPlayerFacePart panel = _objs[0] as UIPanelPlayerFacePart;
            List<Vector2Int> occupyPosList = _objs[1] as List<Vector2Int>;
            List<Vector2Int> effectPosList = _objs[2] as List<Vector2Int>;
            if (occupyPosList == null || effectPosList == null)
                return;

            //??????????????????????????
            panel.partInfo.curOccupyFacePosList = occupyPosList;
            panel.partInfo.curEffectFacePosList = effectPosList;
            panel.partInfo.isOnFace = true;

            FaceGridInfo tmpInfo = null;
            List<Vector3> tmpGOList = new List<Vector3>();
            for (int i = 0; i < occupyPosList.Count; i++)
            {
                tmpInfo = _m_gridInfoList.Find(x => x.pos == occupyPosList[i]);
                if (tmpInfo == null)
                    continue;
                tmpInfo.SetOwnerPart(panel.partInfo);
                int index = _m_gridInfoList.IndexOf(tmpInfo);
                tmpGOList.Add(_m_gridGOList[index].transform.localPosition);
            }
            //?????????????
            Vector2 placeWorldPos = GameCommon.CalculateStandardCenterPos(tmpGOList);
            panel.SetLocalPos(placeWorldPos);
            SCMsgCenter.SendMsg(SCMsgConst.FACE_PART_ORDER_CHG);
        }
        private void onReplacePartPosFail(object[] _objs)
        {
            if (_objs == null || _objs.Length == 0)
                return;
            PartInfo partInfo = _objs[0] as PartInfo;
            _m_playerBattlePartInfoList.Remove(partInfo);

            UIPanelPlayerFacePart panel = _m_facePartPanelList.Find(x => x.partInfo == partInfo);
            _m_facePartPanelList.Remove(panel);
            SCMsgCenter.SendMsg(SCMsgConst.FACE_PART_ORDER_CHG);

        }
        private void onPlacePartPreview(object[] _objs)
        {
            if (_objs == null || _objs.Length < 2)
                return;
            List<Vector2Int> occupyPosList = _objs[0] as List<Vector2Int>;
            List<Vector2Int> effectPosList = _objs[1] as List<Vector2Int>;
            if (occupyPosList == null || effectPosList == null)
                return;
            foreach (var panel in _m_gridPanelList)
                panel.SetNoPreview();

            bool canPlace = GameModel.instance.CanPlacePart(occupyPosList);
            var occSet = GameCommon.ToPositionSet(occupyPosList);
            var effSet = GameCommon.ToPositionSet(effectPosList);
            var union = GameCommon.UnionSortedGridPositions(occupyPosList, effectPosList);
            UIPanelMaskCombineFaceGrid tmpGrid = null;
            for (int i = 0; i < union.Count; i++)
            {
                Vector2Int p = union[i];
                tmpGrid = _m_gridPanelList.Find(x => x.gridInfo.pos == p);
                if (tmpGrid == null)
                    continue;
                switch (GameCommon.GetOccupyEffectCellType(p, occSet, effSet))
                {
                    case EGridPosType.OCCUPY:
                        tmpGrid.SetOccupyPreview(canPlace);
                        break;
                    case EGridPosType.EFFECT:
                        if (canPlace)
                            tmpGrid.SetEffectPreview();
                        break;
                    case EGridPosType.BOTH:
                        if (canPlace)
                            tmpGrid.SetOverlapPreview();
                        else
                            tmpGrid.SetOccupyPreview(false);
                        break;
                }
            }
        }
        private void onClearPreview()
        {
            if (_m_gridPanelList == null)
                return;
            foreach (var gridPanel in _m_gridPanelList)
                gridPanel?.SetNoPreview();
        }
        private void onFacePartRangeHighlight(object[] _objs)
        {
            if (_objs == null || _objs.Length == 0)
                return;
            PartInfo partInfo = _objs[0] as PartInfo;
            if (partInfo == null)
                return;
            var occSet = GameCommon.ToPositionSet(partInfo.curOccupyFacePosList);
            var effSet = GameCommon.ToPositionSet(partInfo.curEffectFacePosList);
            var union = GameCommon.UnionSortedGridPositions(partInfo.curOccupyFacePosList, partInfo.curEffectFacePosList);
            UIPanelMaskCombineFaceGrid grid = null;
            for (int i = 0; i < union.Count; i++)
            {
                Vector2Int p = union[i];
                grid = _m_gridPanelList.Find(x => x.gridInfo.pos == p);
                if (grid == null)
                    continue;
                switch (GameCommon.GetOccupyEffectCellType(p, occSet, effSet))
                {
                    case EGridPosType.OCCUPY:
                        grid.SetOccupyPreview(true);
                        break;
                    case EGridPosType.EFFECT:
                        grid.SetEffectPreview();
                        break;
                    case EGridPosType.BOTH:
                        grid.SetOverlapPreview();
                        break;
                }
            }
        }

    }
}
