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
                    grid?.ShowPanel();
                }
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
                            panel.ShowPanel();//单独show一下 失活状态会影响布局
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
                infoList = GameModel.instance.curEnemyInfo.battleParts;
            }
            else
            {
                _m_gridInfoList = GameModel.instance.playerFaceGridInfoList;
                infoList = GameModel.instance.battlePartInfoList;
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
                Vector2 placeWorldPos = GameCommon.CalculateWorldCenterPos(tmpGOList);
                GameObject partGO = ResourcesHelper.LoadGameObject(GameConst.PREFAB_BATTLE_PART, mono.tranParentPart);
                UIMonoBattlePart monoFacePart = partGO.GetComponent<UIMonoBattlePart>();
                UIPanelBattlePart panel = new UIPanelBattlePart(monoFacePart, SCUIShowType.INTERNAL);
                panel.SetLocalPos(placeWorldPos);
                panel.SetInfo(partInfo);
                panel.ShowPanel();
                _m_partPanelList.Add(panel);
            }
        }
    }
}
