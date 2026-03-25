using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameCore.RefData;
using SCFrame;

namespace GameCore.UI
{
    public class UIPanelBattleFace : _ASCUIPanelBase<UIMonoBattleFace>
    {
        private List<UIPanelBattleFaceGrid> _m_gridPanelList;
        private List<FaceGridInfo> _m_gridInfoList;
        private List<GameObject> _m_gridGOList;

        private List<UIPanelBattlePart> _m_partPanelList;
        public UIPanelBattleFace(UIMonoBattleFace _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_gridPanelList = new List<UIPanelBattleFaceGrid>();
            _m_gridGOList = new List<GameObject>();
            _m_partPanelList = new List<UIPanelBattlePart>();
            createGrids();
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
            if (_m_partPanelList != null)
            {
                foreach (var grid in _m_partPanelList)
                {
                    grid?.Discard();
                }
                _m_partPanelList.Clear();
                _m_partPanelList = null;
            }
        }

        public override void OnHidePanel()
        {
            SCMsgCenter.UnregisterMsg(SCMsgConst.PART_DIE, onPartDie);
            if (_m_gridPanelList != null)
            {
                foreach (var grid in _m_gridPanelList)
                {
                    grid?.HidePanel();
                }
            }
            if (_m_partPanelList != null)
            {
                foreach (var grid in _m_partPanelList)
                {
                    grid?.HidePanel();
                }
            }
        }

        public override void OnShowPanel()
        {
            SCMsgCenter.RegisterMsg(SCMsgConst.PART_DIE, onPartDie);
            if (_m_gridPanelList != null)
            {
                foreach (var grid in _m_gridPanelList)
                {
                    grid?.ShowPanel();
                }
            }
            if (_m_partPanelList != null)
            {
                foreach (var grid in _m_partPanelList)
                {
                    grid?.HidePanel();
                    grid?.Discard();
                }
                _m_partPanelList.Clear();
            }

            SCTimeCaller.instance.CallDealy(0.5f, () =>
            {
                refreshShow();
            });
        }
        private void createGrids()
        {
            Vector2Int tmpPos = Vector2Int.zero;
            UIPanelBattleFaceGrid panel;
            UIMonoBattleFaceGrid gridMono;

            for (int i = 0; i < mono.columnCount; i++)//4
            {
                for (int j = 0; j < mono.rowCount; j++)//7
                {
                    tmpPos.x = i;
                    tmpPos.y = j;
                    GameObject go = ResourcesHelper.LoadGameObject(mono.gridPrefabName, mono.girdLayoutGroup.transform);

                    gridMono = go.GetComponent<UIMonoBattleFaceGrid>();
                    if (gridMono != null)
                    {
                        panel = new UIPanelBattleFaceGrid(gridMono, SCUIShowType.INTERNAL);

                        if (mono.disabledGrids.Contains(tmpPos))
                        {
                            SCCommon.SetGameObjectEnable(go, true);
                            panel.SetDisable();
                        }
                        else
                        {
                            if(!mono.isEnemyFace)
                            {
                                FaceGridInfo info = GameModel.instance.playerFaceGridInfoList.Find(x=>x.pos == tmpPos);
                                panel.SetInfo(info);
                                _m_gridPanelList.Add(panel);
                                _m_gridGOList.Add(go);
                            }
                            else
                            {
                                FaceGridInfo info = GameModel.instance.enemyFaceGridInfoList.Find(x => x.pos == tmpPos);
                                panel.SetInfo(info);
                                _m_gridPanelList.Add(panel);
                                _m_gridGOList.Add(go);
                            }
                        }
                    }
                }
            }

        }

        private void refreshShow()
        {

            List<PartInfo> infoList = null;
            if (mono.isEnemyFace)
            {
                _m_gridInfoList = GameModel.instance.enemyFaceGridInfoList;
                infoList = GameModel.instance.curEnemyInfo.battlePartInfoList;
            }
            else
            {
                _m_gridInfoList = GameModel.instance.playerFaceGridInfoList;
                infoList = GameModel.instance.playerInfo.battlePartInfoList;
            }


            if (infoList == null)
                return;
            PartInfo partInfo = null;
            for (int i = 0; i < infoList.Count; i++)
            {
                partInfo = infoList[i];
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
                    tmpGOList.Add(_m_gridGOList[index].transform.localPosition);
                }
                //计算生成的位置
                Vector2 placeWorldPos = GameCommon.CalculateStandardCenterPos(tmpGOList);
                GameObject partGO = ResourcesHelper.LoadGameObject(GameConst.PREFAB_BATTLE_PART, mono.tranParentPart);
                UIMonoBattlePart monoFacePart = partGO.GetComponent<UIMonoBattlePart>();
                UIPanelBattlePart panel = new UIPanelBattlePart(monoFacePart, SCUIShowType.INTERNAL);
                panel.SetLocalPos(placeWorldPos);
                panel.SetInfo(partInfo);
                panel.ShowPanel();
                _m_partPanelList.Add(panel);
            }
        }

        private void onPartDie(object[] _objs)
        {
            if (_objs == null || _objs.Length == 0)
                return;
            PartInfo info = _objs[0] as PartInfo;

            UIPanelBattlePart part = _m_partPanelList.Find(x => x.partInfo == info);
            if (part != null)
            {
                part.HidePanel();
            }
        }

        public UIPanelBattlePart FindPartPanel(PartInfo _info)
        {
            if (_info == null || _m_partPanelList == null) return null;
            return _m_partPanelList.Find(x => x.partInfo == _info);
        }

        /// <summary> World-space centroid of the given face grid positions (for mouth lunge target). </summary>
        public Vector3 GetWorldCenterForGridPositions(List<Vector2Int> _facePositions)
        {
            if (mono.tranParentPart != null && (_facePositions == null || _facePositions.Count == 0))
                return mono.tranParentPart.position;

            if (_m_gridInfoList == null || _m_gridGOList == null || _facePositions == null || _facePositions.Count == 0)
                return mono.tranParentPart != null ? mono.tranParentPart.position : Vector3.zero;

            var acc = new List<Vector3>();
            for (int i = 0; i < _facePositions.Count; i++)
            {
                var info = _m_gridInfoList.Find(x => x.pos == _facePositions[i]);
                if (info == null) continue;
                int idx = _m_gridInfoList.IndexOf(info);
                if (idx >= 0 && idx < _m_gridGOList.Count)
                    acc.Add(_m_gridGOList[idx].transform.position);
            }
            if (acc.Count == 0)
                return mono.tranParentPart != null ? mono.tranParentPart.position : Vector3.zero;
            Vector3 sum = Vector3.zero;
            for (int i = 0; i < acc.Count; i++)
                sum += acc[i];
            return sum / acc.Count;
        }
    }
}
