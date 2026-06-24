using GameCore;
using GameCore.Data;
using GameCore.Helpers;
using GameCore.RefData;
using SCFrame;
using SCFrame.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIPanelBookEnemyMaskPreview : _ASCUIPanelBase<UIMonoEnemyMask>
    {
        private readonly List<UIPanelEnemyMaskGrid> _m_gridPanelList = new List<UIPanelEnemyMaskGrid>();
        private readonly List<FaceGridInfo> _m_gridInfoList = new List<FaceGridInfo>();
        private readonly List<GameObject> _m_gridGOList = new List<GameObject>();
        private readonly List<UIPanelEnemyFacePart> _m_facePartPanelList = new List<UIPanelEnemyFacePart>();

        private EnemyLayoutPreset _m_preset;
        private bool _m_gridsBuilt;

        public UIPanelBookEnemyMaskPreview(UIMonoEnemyMask mono, SCUIShowType showType) : base(mono, showType)
        {
        }

        public override void AfterInitialize()
        {
        }

        public override void BeforeDiscard()
        {
            clearFaceParts();
            clearGridObjects();
        }

        public override void OnHidePanel()
        {
            clearFaceParts();
            for (int i = 0; i < _m_gridPanelList.Count; i++)
                _m_gridPanelList[i]?.HidePanel();
        }

        public override void OnShowPanel()
        {
            for (int i = 0; i < _m_gridPanelList.Count; i++)
                _m_gridPanelList[i]?.ShowPanel();
        }

        public void SetPreview(
            EnemyRefObj enemyRef,
            List<PartInfo> faceParts,
            EnemyLayoutPreset preset)
        {
            if (preset != _m_preset || !_m_gridsBuilt)
            {
                _m_preset = preset;
                rebuildGrids(preset);
            }

            if (mono.txtName != null)
            {
                string name = enemyRef?.enemyName;
                mono.txtName.text = string.IsNullOrEmpty(name) ? "" : name;
            }

            forceRebuildLayout();
            refreshFaceParts(faceParts);
        }

        public void RebuildLayout()
        {
            forceRebuildLayout();
        }

        private void rebuildGrids(EnemyLayoutPreset preset)
        {
            clearFaceParts();
            clearGridObjects();
            createGrids(preset);
            _m_gridsBuilt = true;
        }

        private void clearGridObjects()
        {
            for (int i = 0; i < _m_gridPanelList.Count; i++)
                _m_gridPanelList[i]?.Discard();
            _m_gridPanelList.Clear();
            _m_gridInfoList.Clear();
            _m_gridGOList.Clear();
            _m_gridsBuilt = false;

            if (mono.layoutGrid == null)
                return;

            Transform gridRoot = mono.layoutGrid.transform;
            for (int i = gridRoot.childCount - 1; i >= 0; i--)
                Object.Destroy(gridRoot.GetChild(i).gameObject);
        }

        private void createGrids(EnemyLayoutPreset preset)
        {
            if (mono.layoutGrid == null)
                return;

            int column = preset != null && preset.gridSize.x > 0 ? preset.gridSize.x : mono.column;
            int row = preset != null && preset.gridSize.y > 0 ? preset.gridSize.y : mono.row;
            List<Vector2Int> disabledGrids = preset != null
                ? preset.disabledGridPositions
                : mono.disabledGrids;

            Vector2Int tmp = Vector2Int.zero;
            for (int i = 0; i < column; i++)
            {
                for (int j = 0; j < row; j++)
                {
                    tmp.x = i;
                    tmp.y = j;
                    GameObject go = ResourcesHelper.LoadGameObject(mono.gridPrefabName, mono.layoutGrid.transform);
                    if (go == null)
                        continue;

                    UIMonoEnemyMaskGrid gridMono = go.GetComponent<UIMonoEnemyMaskGrid>();
                    if (gridMono == null)
                        continue;

                    var panel = new UIPanelEnemyMaskGrid(gridMono, SCUIShowType.INTERNAL);

                    if (isGridDisabled(disabledGrids, tmp))
                    {
                        SCCommon.SetGameObjectEnable(go, true);
                        panel.SetDisable();
                    }
                    else
                    {
                        var info = new FaceGridInfo(tmp, false);
                        panel.SetInfo(info);
                        _m_gridInfoList.Add(info);
                        _m_gridPanelList.Add(panel);
                        _m_gridGOList.Add(go);
                        panel.ShowPanel();
                    }
                }
            }
        }

        private static bool isGridDisabled(List<Vector2Int> disabledGrids, Vector2Int pos)
        {
            if (disabledGrids == null || disabledGrids.Count == 0)
                return false;

            for (int i = 0; i < disabledGrids.Count; i++)
            {
                if (disabledGrids[i] == pos)
                    return true;
            }

            return false;
        }

        private void forceRebuildLayout()
        {
            if (mono.layoutGrid == null)
                return;

            RectTransform gridRect = mono.layoutGrid.GetComponent<RectTransform>();
            if (gridRect != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(gridRect);

            Canvas.ForceUpdateCanvases();
        }

        private void refreshFaceParts(List<PartInfo> faceParts)
        {
            clearFaceParts();
            if (faceParts == null || faceParts.Count == 0)
                return;
            if (_m_gridGOList.Count == 0 || mono.tranParentPart == null)
                return;

            float cellWidth = _m_gridGOList[0].GetRectTransform().rect.width;
            float cellHeight = _m_gridGOList[0].GetRectTransform().rect.height;
            if (cellWidth <= 0.001f || cellHeight <= 0.001f)
                return;

            var parentRect = mono.tranParentPart.GetComponent<RectTransform>();
            float parentWidth = parentRect != null ? parentRect.rect.width : 0f;
            float parentHeight = parentRect != null ? parentRect.rect.height : 0f;

            for (int i = 0; i < faceParts.Count; i++)
            {
                PartInfo partInfo = faceParts[i];
                if (partInfo == null)
                    continue;

                var tmpGOList = new List<Vector3>();
                for (int j = 0; j < partInfo.curOccupyFacePosList.Count; j++)
                {
                    Vector2Int pos = partInfo.curOccupyFacePosList[j];
                    tmpGOList.Add(new Vector3(
                        cellWidth * pos.x + cellWidth / 2f - parentWidth / 2f,
                        -cellHeight * pos.y - cellHeight / 2f + parentHeight / 2f,
                        0f));
                }

                if (tmpGOList.Count == 0)
                    continue;

                Vector2 placeWorldPos = GameCommon.CalculateStandardCenterPos(tmpGOList);
                GameObject partGO = ResourcesHelper.LoadGameObject(GameConst.PREFAB_ENEMY_FACE_PART, mono.tranParentPart);
                if (partGO == null)
                    continue;

                UIMonoEnemyFacePart monoFacePart = partGO.GetComponent<UIMonoEnemyFacePart>();
                if (monoFacePart == null)
                    continue;

                var panel = new UIPanelEnemyFacePart(monoFacePart, SCUIShowType.INTERNAL);
                panel.SetLocalPos(placeWorldPos);
                panel.SetInfo(partInfo, faceParts);
                panel.ShowPanel();
                _m_facePartPanelList.Add(panel);
            }
        }

        private void clearFaceParts()
        {
            for (int i = 0; i < _m_facePartPanelList.Count; i++)
            {
                _m_facePartPanelList[i]?.HidePanel();
                _m_facePartPanelList[i]?.Discard();
            }
            _m_facePartPanelList.Clear();
        }
    }
}
