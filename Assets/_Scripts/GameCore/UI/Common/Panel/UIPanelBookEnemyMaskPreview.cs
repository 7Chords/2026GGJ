using GameCore;
using GameCore.Data;
using GameCore.Helpers;
using GameCore.RefData;
using SCFrame;
using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIPanelBookEnemyMaskPreview : _ASCUIPanelBase<UIMonoBookEnemyMask>
    {
        private readonly List<UIPanelBookEnemyMaskGrid> _m_gridPanelList = new List<UIPanelBookEnemyMaskGrid>();
        private readonly List<FaceGridInfo> _m_gridInfoList = new List<FaceGridInfo>();
        private readonly List<GameObject> _m_gridGOList = new List<GameObject>();
        private readonly List<UIPanelBookEnemyFacePart> _m_facePartPanelList = new List<UIPanelBookEnemyFacePart>();

        private List<PartInfo> _m_cachedFaceParts;
        private string _m_gridLayoutKey = string.Empty;
        private bool _m_gridsBuilt;
        private int _m_deferredRebuildAttempts;
        private Coroutine _m_layoutRebuildRoutine;

        public UIPanelBookEnemyMaskPreview(UIMonoBookEnemyMask mono, SCUIShowType showType) : base(mono, showType)
        {
        }

        public override void AfterInitialize()
        {
        }

        public override void BeforeDiscard()
        {
            cancelDeferredPreviewRebuild();
            clearFaceParts();
            clearGridObjects();
        }

        public override void OnHidePanel()
        {
            cancelDeferredPreviewRebuild();
            GameCommon.DiscardToolTip();
            clearGridHighlight();
            clearFaceParts();
            for (int i = 0; i < _m_gridPanelList.Count; i++)
                _m_gridPanelList[i]?.HidePanel();
        }

        public override void OnShowPanel()
        {
            for (int i = 0; i < _m_gridPanelList.Count; i++)
                _m_gridPanelList[i]?.ShowPanel();

            rebuildPreviewLayout(true);
            if (!tryRefreshFaceParts(_m_cachedFaceParts))
                scheduleDeferredPreviewRebuild();
        }

        public void SetPreview(
            EnemyRefObj enemyRef,
            List<PartInfo> faceParts,
            EnemyLayoutPreset preset)
        {
            _m_cachedFaceParts = faceParts;
            _m_deferredRebuildAttempts = 0;

            if (needsGridRebuild(preset))
                rebuildGrids(preset);

            if (mono.txtName != null)
            {
                string name = enemyRef?.enemyName;
                mono.txtName.text = string.IsNullOrEmpty(name) ? "" : name;
            }

            rebuildPreviewLayout(true);
            if (!tryRefreshFaceParts(faceParts))
                scheduleDeferredPreviewRebuild();
        }

        public void RebuildLayout()
        {
            rebuildPreviewLayout(true);
            if (!tryRefreshFaceParts(_m_cachedFaceParts))
                scheduleDeferredPreviewRebuild();
        }

        private void rebuildGrids(EnemyLayoutPreset preset)
        {
            clearFaceParts();
            clearGridObjects();
            createGrids(preset);
            _m_gridsBuilt = true;
        }

        private bool needsGridRebuild(EnemyLayoutPreset preset)
        {
            string layoutKey = buildGridLayoutKey(preset);
            if (_m_gridsBuilt && layoutKey == _m_gridLayoutKey)
                return false;

            _m_gridLayoutKey = layoutKey;
            return true;
        }

        private string buildGridLayoutKey(EnemyLayoutPreset preset)
        {
            int column = getColumnCount(preset);
            int row = getRowCount(preset);
            List<Vector2Int> disabledGrids = getDisabledGrids(preset);

            int hash = 17;
            hash = hash * 31 + column;
            hash = hash * 31 + row;
            if (disabledGrids != null)
            {
                for (int i = 0; i < disabledGrids.Count; i++)
                    hash = hash * 31 + disabledGrids[i].GetHashCode();
            }

            return hash.ToString();
        }

        private int getColumnCount(EnemyLayoutPreset preset)
        {
            return preset != null && preset.gridSize.x > 0 ? preset.gridSize.x : mono.column;
        }

        private int getRowCount(EnemyLayoutPreset preset)
        {
            return preset != null && preset.gridSize.y > 0 ? preset.gridSize.y : mono.row;
        }

        private List<Vector2Int> getDisabledGrids(EnemyLayoutPreset preset)
        {
            return preset != null ? preset.disabledGridPositions : mono.disabledGrids;
        }

        private void clearGridObjects()
        {
            for (int i = 0; i < _m_gridPanelList.Count; i++)
                _m_gridPanelList[i]?.Discard();
            _m_gridPanelList.Clear();
            _m_gridInfoList.Clear();
            _m_gridGOList.Clear();
            _m_gridsBuilt = false;
            _m_gridLayoutKey = string.Empty;

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

            if (string.IsNullOrEmpty(mono.gridPrefabName))
            {
                Debug.LogError("UIMonoBookEnemyMask.gridPrefabName is empty.");
                return;
            }

            int column = getColumnCount(preset);
            int row = getRowCount(preset);
            List<Vector2Int> disabledGrids = getDisabledGrids(preset);

            Vector2Int tmp = Vector2Int.zero;
            for (int i = 0; i < column; i++)
            {
                for (int j = 0; j < row; j++)
                {
                    tmp.x = i;
                    tmp.y = j;
                    GameObject go = ResourcesHelper.LoadGameObject(mono.gridPrefabName, mono.layoutGrid.transform);
                    if (go == null)
                    {
                        Debug.LogError("Failed to load book enemy mask grid prefab: " + mono.gridPrefabName);
                        continue;
                    }

                    UIMonoBookEnemyMaskGrid gridMono = go.GetComponent<UIMonoBookEnemyMaskGrid>();
                    if (gridMono == null)
                    {
                        Debug.LogError("Book enemy mask grid prefab missing UIMonoBookEnemyMaskGrid: " + mono.gridPrefabName);
                        Object.Destroy(go);
                        continue;
                    }

                    var panel = new UIPanelBookEnemyMaskGrid(gridMono, SCUIShowType.INTERNAL);

                    if (isGridDisabled(disabledGrids, tmp))
                    {
                        SCCommon.SetGameObjectEnable(go, true);
                        panel.SetDisable();
                    }
                    else
                    {
                        var info = new FaceGridInfo(tmp, false);
                        panel.SetInfo(info);
                        panel.ApplyDefaultVisual();
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

        private void rebuildPreviewLayout(bool refreshParts)
        {
            forceRebuildLayout();
            if (refreshParts && _m_cachedFaceParts != null)
                tryRefreshFaceParts(_m_cachedFaceParts);
        }

        private void forceRebuildLayout()
        {
            RectTransform maskRect = mono != null ? mono.transform as RectTransform : null;
            if (maskRect != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(maskRect);

            if (mono?.tranParentPart is RectTransform partRoot)
                LayoutRebuilder.ForceRebuildLayoutImmediate(partRoot);

            if (mono?.layoutGrid != null)
            {
                RectTransform gridRect = mono.layoutGrid.transform as RectTransform;
                if (gridRect != null)
                    LayoutRebuilder.ForceRebuildLayoutImmediate(gridRect);
            }

            Canvas.ForceUpdateCanvases();
        }

        private bool tryRefreshFaceParts(List<PartInfo> faceParts)
        {
            if (faceParts == null || faceParts.Count == 0)
            {
                clearFaceParts();
                return true;
            }

            if (_m_gridPanelList.Count == 0 || mono.tranParentPart == null)
                return false;

            if (string.IsNullOrEmpty(mono.facePartPrefabName))
                return false;

            if (!canPlacePartsFromGrids())
                return false;

            refreshFaceParts(faceParts);
            return true;
        }

        private bool canPlacePartsFromGrids()
        {
            for (int i = 0; i < _m_gridPanelList.Count; i++)
            {
                RectTransform gridRect = _m_gridPanelList[i]?.GetGameObject()?.GetComponent<RectTransform>();
                if (gridRect != null && gridRect.rect.width > 0.001f && gridRect.rect.height > 0.001f)
                    return true;
            }

            return getCellWidth() > 0.001f && getCellHeight() > 0.001f;
        }

        private float getCellWidth()
        {
            if (mono?.layoutGrid != null && mono.layoutGrid.cellSize.x > 0.001f)
                return mono.layoutGrid.cellSize.x;

            if (_m_gridGOList.Count > 0)
            {
                float rectWidth = _m_gridGOList[0].GetRectTransform().rect.width;
                if (rectWidth > 0.001f)
                    return rectWidth;
            }

            return 0f;
        }

        private float getCellHeight()
        {
            if (mono?.layoutGrid != null && mono.layoutGrid.cellSize.y > 0.001f)
                return mono.layoutGrid.cellSize.y;

            if (_m_gridGOList.Count > 0)
            {
                float rectHeight = _m_gridGOList[0].GetRectTransform().rect.height;
                if (rectHeight > 0.001f)
                    return rectHeight;
            }

            return 0f;
        }

        private void refreshFaceParts(List<PartInfo> faceParts)
        {
            clearFaceParts();
            if (faceParts == null || faceParts.Count == 0)
                return;

            for (int i = 0; i < faceParts.Count; i++)
            {
                PartInfo partInfo = faceParts[i];
                if (partInfo == null)
                    continue;

                Vector2? placePos = calculatePartCenterInPartParent(partInfo);
                if (!placePos.HasValue)
                    continue;

                GameObject partGO = ResourcesHelper.LoadGameObject(mono.facePartPrefabName, mono.tranParentPart);
                if (partGO == null)
                {
                    Debug.LogError("Failed to load book enemy face part prefab: " + mono.facePartPrefabName);
                    continue;
                }

                UIMonoBookEnemyFacePart monoFacePart = partGO.GetComponent<UIMonoBookEnemyFacePart>();
                if (monoFacePart == null)
                {
                    Debug.LogError("Book enemy face part prefab missing UIMonoBookEnemyFacePart: " + mono.facePartPrefabName);
                    Object.Destroy(partGO);
                    continue;
                }

                var panel = new UIPanelBookEnemyFacePart(monoFacePart, SCUIShowType.INTERNAL);
                panel.BindHoverCallbacks(highlightPartRange, clearGridHighlight);
                panel.SetTooltipScreenRatio(mono.facePartTooltipScreenRatio);
                panel.SetLocalPos(placePos.Value);
                panel.SetInfo(partInfo, faceParts);
                panel.ShowPanel();
                _m_facePartPanelList.Add(panel);
            }
        }

        private Vector2? calculatePartCenterInPartParent(PartInfo partInfo)
        {
            if (mono.tranParentPart == null || partInfo?.curOccupyFacePosList == null || partInfo.curOccupyFacePosList.Count == 0)
                return null;

            Transform partParent = mono.tranParentPart;
            Vector2 sum = Vector2.zero;
            int count = 0;

            for (int i = 0; i < partInfo.curOccupyFacePosList.Count; i++)
            {
                UIPanelBookEnemyMaskGrid grid = findGridPanel(partInfo.curOccupyFacePosList[i]);
                if (!tryGetGridCenterInPartParent(grid, partParent, out Vector2 center))
                    continue;

                sum += center;
                count++;
            }

            if (count > 0)
                return sum / count;

            return calculatePartCenterFallback(partInfo);
        }

        private static bool tryGetGridCenterInPartParent(
            UIPanelBookEnemyMaskGrid grid,
            Transform partParent,
            out Vector2 localCenter)
        {
            localCenter = Vector2.zero;
            if (grid?.GetGameObject() == null || partParent == null)
                return false;

            RectTransform gridRect = grid.GetGameObject().GetComponent<RectTransform>();
            if (gridRect == null)
                return false;

            Vector3 worldCenter = gridRect.TransformPoint(gridRect.rect.center);
            Vector3 local = partParent.InverseTransformPoint(worldCenter);
            localCenter = new Vector2(local.x, local.y);
            return true;
        }

        private Vector2? calculatePartCenterFallback(PartInfo partInfo)
        {
            float cellWidth = getCellWidth();
            float cellHeight = getCellHeight();
            if (cellWidth <= 0.001f || cellHeight <= 0.001f)
                return null;

            RectTransform parentRect = mono.tranParentPart.GetComponent<RectTransform>();
            float parentWidth = parentRect != null ? parentRect.rect.width : 0f;
            float parentHeight = parentRect != null ? parentRect.rect.height : 0f;

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
                return null;

            return GameCommon.CalculateStandardCenterPos(tmpGOList);
        }

        private void highlightPartRange(PartInfo partInfo)
        {
            clearGridHighlight();
            if (partInfo == null)
                return;

            var occSet = GameCommon.ToPositionSet(partInfo.curOccupyFacePosList);
            var effSet = GameCommon.ToPositionSet(partInfo.curEffectFacePosList);
            var union = GameCommon.UnionSortedGridPositions(partInfo.curOccupyFacePosList, partInfo.curEffectFacePosList);

            for (int i = 0; i < union.Count; i++)
            {
                Vector2Int pos = union[i];
                UIPanelBookEnemyMaskGrid grid = findGridPanel(pos);
                if (grid == null)
                    continue;

                switch (GameCommon.GetOccupyEffectCellType(pos, occSet, effSet))
                {
                    case EGridPosType.OCCUPY:
                        grid.SetOccupyHighlight();
                        break;
                    case EGridPosType.EFFECT:
                        grid.SetEffectHighlight();
                        break;
                    case EGridPosType.BOTH:
                        grid.SetOverlapHighlight();
                        break;
                }
            }
        }

        private UIPanelBookEnemyMaskGrid findGridPanel(Vector2Int pos)
        {
            for (int i = 0; i < _m_gridPanelList.Count; i++)
            {
                UIPanelBookEnemyMaskGrid grid = _m_gridPanelList[i];
                if (grid?.gridInfo != null && grid.gridInfo.pos == pos)
                    return grid;
            }

            return null;
        }

        private void clearGridHighlight()
        {
            for (int i = 0; i < _m_gridPanelList.Count; i++)
                _m_gridPanelList[i]?.ApplyDefaultVisual();
        }

        private void clearFaceParts()
        {
            GameCommon.DiscardToolTip();
            clearGridHighlight();

            for (int i = 0; i < _m_facePartPanelList.Count; i++)
            {
                _m_facePartPanelList[i]?.HidePanel();
                _m_facePartPanelList[i]?.Discard();
            }
            _m_facePartPanelList.Clear();
        }

        private void scheduleDeferredPreviewRebuild()
        {
            if (_m_deferredRebuildAttempts >= 5)
                return;

            cancelDeferredPreviewRebuild();
            if (mono == null)
                return;

            _m_deferredRebuildAttempts++;
            _m_layoutRebuildRoutine = this.StartCoroutine(coDeferredPreviewRebuild());
        }

        private IEnumerator coDeferredPreviewRebuild()
        {
            yield return null;
            yield return new WaitForEndOfFrame();

            _m_layoutRebuildRoutine = null;
            rebuildPreviewLayout(true);
            if (!tryRefreshFaceParts(_m_cachedFaceParts) && _m_deferredRebuildAttempts < 5)
                scheduleDeferredPreviewRebuild();
        }

        private void cancelDeferredPreviewRebuild()
        {
            if (_m_layoutRebuildRoutine == null)
                return;

            this.StopCoroutine(_m_layoutRebuildRoutine);
            _m_layoutRebuildRoutine = null;
        }
    }
}
