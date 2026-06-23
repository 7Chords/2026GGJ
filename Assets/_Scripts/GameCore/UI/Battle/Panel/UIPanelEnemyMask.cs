using GameCore;
using GameCore.RefData;
using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using SCFrame;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

namespace GameCore.UI
{
    public class UIPanelEnemyMask : _ASCUIPanelBase<UIMonoEnemyMask>
    {
        private List<UIPanelEnemyMaskGrid> _m_gridPanelList;
        private List<FaceGridInfo> _m_gridInfoList;
        private List<GameObject> _m_gridGOList;

        private EnemyInfo _m_curEnemyInfo;
        private List<UIPanelEnemyFacePart> _m_facePartPanelList;
        private List<UIPanelPassiveItem> _m_passiveItemPanelList;
        public UIPanelEnemyMask(UIMonoEnemyMask _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_gridPanelList = new List<UIPanelEnemyMaskGrid>();
            _m_gridInfoList = new List<FaceGridInfo>();
            _m_gridGOList = new List<GameObject>();
            _m_facePartPanelList = new List<UIPanelEnemyFacePart>();
            _m_passiveItemPanelList = new List<UIPanelPassiveItem>();
            createGrids();
        }

        public override void BeforeDiscard()
        {
            if (_m_gridPanelList != null)
            {
                foreach (var grid in _m_gridPanelList)
                    grid?.Discard();
                _m_gridPanelList.Clear();
            }

            if (_m_facePartPanelList != null)
            {
                foreach (var panel in _m_facePartPanelList)
                    panel?.Discard();
                _m_facePartPanelList.Clear();
            }
            clearPassiveItems();
        }

        public override void OnHidePanel()
        {
            onClearPreview();

            SCMsgCenter.UnregisterMsgAct(SCMsgConst.NEW_GANE_START, onNewGameStart);
            SCMsgCenter.UnregisterMsg(SCMsgConst.ENEMY_FACE_PART_RANGE_HIGHLIGHT, onEnemyFacePartRangeHighlight);
            SCMsgCenter.UnregisterMsgAct(SCMsgConst.CLEAR_ENEMY_PREVIEW, onClearPreview);

            //??????????
            SCMsgCenter.UnregisterMsg(SCMsgConst.PLAYER_FACE_PART_RANGE_HIGHLIGHT, onEnemyFacePartRangeHighlight);
            SCMsgCenter.UnregisterMsgAct(SCMsgConst.CLEAR_PLAYER_PREVIEW, onClearPreview);
            SCMsgCenter.UnregisterMsg(SCMsgConst.PLACE_PART_PREVIEW, onPlacePartPreview);

            if (_m_gridPanelList != null)
            {
                foreach (var grid in _m_gridPanelList)
                    grid?.HidePanel();
            }
            if (_m_facePartPanelList != null)
            {
                foreach (var panel in _m_facePartPanelList)
                    panel?.HidePanel();
            }
            if (_m_passiveItemPanelList != null)
            {
                foreach (var p in _m_passiveItemPanelList)
                    p?.HidePanel();
            }
        }

        public override void OnShowPanel()
        {
            onClearPreview();

            SCMsgCenter.RegisterMsgAct(SCMsgConst.NEW_GANE_START, onNewGameStart);
            SCMsgCenter.RegisterMsg(SCMsgConst.ENEMY_FACE_PART_RANGE_HIGHLIGHT, onEnemyFacePartRangeHighlight);
            SCMsgCenter.RegisterMsgAct(SCMsgConst.CLEAR_ENEMY_PREVIEW, onClearPreview);

            //??????????
            SCMsgCenter.RegisterMsg(SCMsgConst.PLAYER_FACE_PART_RANGE_HIGHLIGHT, onEnemyFacePartRangeHighlight);
            SCMsgCenter.RegisterMsgAct(SCMsgConst.CLEAR_PLAYER_PREVIEW, onClearPreview);
            SCMsgCenter.RegisterMsg(SCMsgConst.PLACE_PART_PREVIEW, onPlacePartPreview);


            if (_m_gridPanelList != null)
            {
                foreach (var grid in _m_gridPanelList)
                    grid?.ShowPanel();
            }
            refreshShow();
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
            GameModel.instance.enemyFaceGridInfoList = _m_gridInfoList;
            //LayoutRebuilder.ForceRebuildLayoutImmediate(mono.layoutGrid.gameObject.GetRectTransform());
            //LayoutRebuilder.ForceRebuildLayoutImmediate(mono.layoutGrid.gameObject.GetRectTransform());
            //Canvas.ForceUpdateCanvases();
        }

        private void refreshShow()
        {
            if (_m_facePartPanelList != null)
            {
                foreach (var panel in _m_facePartPanelList)
                {
                    panel?.HidePanel();
                    panel?.Discard();
                }
                _m_facePartPanelList.Clear();
            }

            clearPassiveItems();

            _m_curEnemyInfo = GameModel.instance.curEnemyInfo;
            refreshEnemyName();

            if (_m_curEnemyInfo == null)
            {
                refreshPassiveItems();
                return;
            }

            PartInfo partInfo = null;

            float cellWidth = _m_gridGOList[0].GetRectTransform().rect.width;
            float cellHeight = _m_gridGOList[0].GetRectTransform().rect.height;
            float parentWidth = mono.tranParentPart.GetComponent<RectTransform>().rect.width;
            float parentHeight = mono.tranParentPart.GetComponent<RectTransform>().rect.height;

            for (int i =0;i<_m_curEnemyInfo.battlePartInfoList.Count;i++)
            {
                partInfo = _m_curEnemyInfo.battlePartInfoList[i];
                if (partInfo == null)
                    continue;

                FaceGridInfo tmpInfo = null;
                List<Vector3> tmpGOList = new List<Vector3>();
                for (int j = 0; j < partInfo.curOccupyFacePosList.Count; j++)
                {
                    tmpInfo = _m_gridInfoList.Find(x => x.pos == partInfo.curOccupyFacePosList[j]);
                    if (tmpInfo == null)
                        continue;
                    int index = _m_gridInfoList.IndexOf(tmpInfo);
                    tmpGOList.Add(new Vector3(cellWidth * partInfo.curOccupyFacePosList[j].x + cellWidth/2 - parentWidth/2,
                        -cellHeight * partInfo.curOccupyFacePosList[j].y - cellHeight/2 + parentHeight/2,
                        0));
                }
                //?????????????
                Vector2 placeWorldPos = GameCommon.CalculateStandardCenterPos(tmpGOList);
                GameObject partGO = ResourcesHelper.LoadGameObject(GameConst.PREFAB_ENEMY_FACE_PART, mono.tranParentPart);
                UIMonoEnemyFacePart monoFacePart = partGO.GetComponent<UIMonoEnemyFacePart>();
                UIPanelEnemyFacePart panel = new UIPanelEnemyFacePart(monoFacePart, SCUIShowType.INTERNAL);
                panel.SetLocalPos(placeWorldPos);
                panel.SetInfo(partInfo);
                panel.ShowPanel();
                _m_facePartPanelList.Add(panel);
            }

            refreshPassiveItems();
        }

        private void clearPassiveItems()
        {
            if (_m_passiveItemPanelList == null)
                return;
            foreach (var p in _m_passiveItemPanelList)
            {
                p?.HidePanel();
                p?.Discard();
            }
            _m_passiveItemPanelList.Clear();
        }

        private void refreshEnemyName()
        {
            if (mono.txtName == null)
                return;
            string name = _m_curEnemyInfo?.enemyRefObj?.enemyName;
            mono.txtName.text = string.IsNullOrEmpty(name) ? "" : name;
        }

        private bool hasValidPassive()
        {
            if (_m_curEnemyInfo?.enemyRefObj?.passiveIdList == null || _m_curEnemyInfo.enemyRefObj.passiveIdList.Count == 0)
                return false;
            var passiveTable = SCRefDataMgr.instance.enemyPassiveRefList.refDataList;
            if (passiveTable == null)
                return false;
            for (int i = 0; i < _m_curEnemyInfo.enemyRefObj.passiveIdList.Count; i++)
            {
                long pid = _m_curEnemyInfo.enemyRefObj.passiveIdList[i];
                if (passiveTable.Find(x => x.id == pid) != null)
                    return true;
            }
            return false;
        }

        private void refreshPassiveItems()
        {
            clearPassiveItems();
            _m_curEnemyInfo = GameModel.instance.curEnemyInfo;
            bool hasPassive = hasValidPassive();
            if (mono.tranPassiveContainer != null)
                SCCommon.SetGameObjectEnable(mono.tranPassiveContainer.gameObject, hasPassive);
            if (!hasPassive || mono.tranPassiveContainer == null || string.IsNullOrEmpty(mono.passiveItemPrefabName))
                return;
            var passiveTable = SCRefDataMgr.instance.enemyPassiveRefList.refDataList;
            if (passiveTable == null)
                return;
            for (int i = 0; i < _m_curEnemyInfo.enemyRefObj.passiveIdList.Count; i++)
            {
                long pid = _m_curEnemyInfo.enemyRefObj.passiveIdList[i];
                EnemyPassiveRefObj row = passiveTable.Find(x => x.id == pid);
                if (row == null)
                    continue;
                GameObject go = ResourcesHelper.LoadGameObject(mono.passiveItemPrefabName, mono.tranPassiveContainer);
                if (go == null)
                    continue;
                UIMonoPassiveItem itemMono = go.GetComponent<UIMonoPassiveItem>();
                if (itemMono == null)
                    continue;
                UIPanelPassiveItem itemPanel = new UIPanelPassiveItem(itemMono, SCUIShowType.INTERNAL);
                itemPanel.SetInfo(row);
                itemPanel.ShowPanel();
                _m_passiveItemPanelList.Add(itemPanel);
            }
        }

        private void onEnemyFacePartRangeHighlight(object[] _objs)
        {
            if (_objs == null || _objs.Length == 0)
                return;
            PartInfo partInfo = _objs[0] as PartInfo;
            if (partInfo == null)
                return;
            var occSet = GameCommon.ToPositionSet(partInfo.curOccupyFacePosList);
            var effSet = GameCommon.ToPositionSet(partInfo.curEffectFacePosList);
            var union = GameCommon.UnionSortedGridPositions(partInfo.curOccupyFacePosList, partInfo.curEffectFacePosList);
            UIPanelEnemyMaskGrid grid = null;
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
        private void onClearPreview()
        {
            if (_m_gridPanelList == null)
                return;
            foreach (var gridPanel in _m_gridPanelList)
                gridPanel?.SetNoPreview();
        }
        private void onNewGameStart()
        {
            refreshShow();
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
            UIPanelEnemyMaskGrid tmpGrid = null;
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
    }
}
