using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using SCFrame;
using UnityEngine;

namespace GameCore.UI
{
    public class UIPanelEnemyMask : _ASCUIPanelBase<UIMonoEnemyMask>
    {
        public UIPanelEnemyMask(UIMonoEnemyMask _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        private List<GameObject> _spawnedParts = new List<GameObject>();

        public override void AfterInitialize()
        {
            CreateGrids();
        }

        private void CreateGrids()
        {
            if (mono.gridPrefab == null)
            {
                Debug.LogError("Grid Prefab is null in UIMonoEnemyMask!");
                return;
            }

            mono.monoGridList = new List<UIMonoEnemyMaskGrid>();
            
            int columns = 4; 
            int rows = 7;
            

            // Use content_grid if available
            Transform parent = mono.content_grid != null ? mono.content_grid : mono.transform.Find("GridContainer");
            if (parent == null) parent = mono.transform;
            
            Debug.Log($"[EnemyMask] Creating Grids: {columns}x{rows}, Disabled={(mono.disabledGrids!=null?mono.disabledGrids.Count:0)}");
            
            for (int i = 0; i < columns * rows; i++)
            {
                GameObject go = SCCommon.InstantiateGameObject(mono.gridPrefab, parent);
                go.SetActive(true);
                
                var gridMono = go.GetComponent<UIMonoEnemyMaskGrid>();
                if (gridMono != null)
                {
                    // Calculate GridPos (6x7)
                    int x = i % columns;
                    int y = i / columns;
                    gridMono.gridPos = new Vector2(x, y);
                    
                    if (mono.disabledGrids != null && mono.disabledGrids.Contains(new Vector2Int(x, y)))
                    {
                        if (gridMono.imgBg != null)
                        {
                            gridMono.imgBg.enabled = false; // Hide completely
                        }
                    }
                    else
                    {
                         // Reset
                         if (gridMono.imgBg != null)
                         {
                              gridMono.imgBg.enabled = true;
                              // Respect prefab color
                         }
                    }
                    
                    mono.monoGridList.Add(gridMono);
                }
            }

            if (mono.gridPrefab.activeSelf) mono.gridPrefab.SetActive(false);
        }

        public override void BeforeDiscard()
        {
            ClearItems();
        }

        public override void OnHidePanel()
        {
             mono.btnClose.onClick.RemoveAllListeners();
        }

        public override void OnShowPanel()
        {
            mono.btnClose.onClick.RemoveAllListeners();
            mono.btnClose.onClick.AddListener(() =>
            {
                // Use Manager to close properly so it can be reopened
                UICoreMgr.instance.CloseTopNode();
            });
            InitializeEnemy();
        }
        
        private void ClearItems()
        {
            if (_spawnedParts != null)
            {
                foreach(var go in _spawnedParts)
                {
                    if (go != null) Object.Destroy(go);
                }
                _spawnedParts.Clear();
            }

            // Robust Clear: Destroy all children of the grid parent to ensure no leftovers
            Transform parent = mono.content_grid != null ? mono.content_grid : mono.transform.Find("GridContainer");
            if (parent == null) parent = mono.transform;
            
            if (parent.childCount > 0)
            {
                 // Note: If reusing grids is desired, we should distinguish between clearing items vs grids. 
                 // But since CreateGrids is called on AfterInitialize, and usually one-time, 
                 // we might need to handle this carefully. 
                 // Actually this method is 'ClearItems' called on BeforeDiscard.
                 // If we destroy grids here, we must re-create them next time. 
                 // CreateGrids is called in AfterInitialize.
                 // For now let's just clear items (spawnedParts).
                 // IF the user sees 42 grids, it's because they were instantiated.
                 // We should destroy them if we want to reset count.
                 // BUT `BeforeDiscard` is called on Destroy/Close.
                 
                 // Let's add specific Grid Clear logic if checking monoGridList
                 if (mono.monoGridList != null)
                 {
                     foreach(var g in mono.monoGridList)
                     {
                         if (g!=null) Object.Destroy(g.gameObject);
                     }
                     mono.monoGridList.Clear();
                 }
                 
                 // Also fallback destroy children if list was lost but objects exist
                 for(int i = parent.childCount - 1; i >= 0; i--)
                 {
                     var child = parent.GetChild(i);
                     if (mono.gridPrefab != null && child == mono.gridPrefab.transform) continue;
                     
                     Object.Destroy(child.gameObject);
                 }
            }

            // Reset Grid Colors
            if (mono.monoGridList != null)
            {
                foreach(var grid in mono.monoGridList)
                {
                    if (grid != null && grid.imgBg != null)
                    {
                        grid.imgBg.color = Color.black;
                    }
                }
            }
        }

        private void InitializeEnemy()
        {
            ClearItems();
            CreateGrids(); // Re-create grids since ClearItems destroyed them
            
            // Read from Model
            var enemyData = GameModel.instance.currentEnemy;
            if (enemyData == null)
            {
                Debug.LogWarning("[UIPanelEnemyMask] No enemy data in Model!");
                return;
            }
            
            // Check if parts need layout (e.g. if they are all at 0,0 or marked as invalid)
            // Assuming GameModel no longer does layout, or we force it here.
            // But if we want to respect disabledGrids, we should probably regenerate or ensure it's valid.
            // If GameModel already generated it avoiding collisions, we are good.
            // But the user explicitly asked to "Add judgment in UIPanelEnemyMask".
            // So we will Generate Layout here.
            GenerateLayout(enemyData);
            
            Debug.Log($"[EnemyGen] Displaying Enemy: {enemyData.enemyRef.enemyName}");

            // 3. Placement Visualization
            bool[,] occupiedGrid = new bool[4, 7]; 
            
            foreach(var partInfo in enemyData.parts)
            {
                // Mark occupancy
                MarkOccupancy(occupiedGrid, partInfo.partRefObj, partInfo.gridPos, partInfo.rotation);
                // Create UI
                CreatePartItem(partInfo);
            }
        }

        private void GenerateLayout(GameCore.EnemyData enemy)
        {
            // 6x7 Grid Logic (matching GameModel logic but using View data)
             bool[,] occupiedGrid = new bool[4, 7];
             
             // Reset parts? Or try to place them? 
             // We assume we need to place ALL parts.
             
             foreach (var part in enemy.parts)
             {
                 // Start fresh or keep? 
                 // If we re-generate, we overlook previous positions.
                 
                 if (TryFindValidPlacement(occupiedGrid, part.partRefObj, out Vector2Int pos, out int rot))
                 {
                     part.gridPos = pos;
                     part.rotation = rot;
                     MarkOccupancy(occupiedGrid, part.partRefObj, pos, rot);
                 }
                 else
                 {
                      Debug.LogWarning($"[UIPanelEnemyMask] Could not fit enemy part {part.partRefObj.partName}");
                 }
             }
        }
        
        private bool TryFindValidPlacement(bool[,] grid, GameCore.RefData.PartRefObj part, out Vector2Int resultPos, out int resultRot)
        {
            resultPos = Vector2Int.zero;
            resultRot = 0;
            for (int i = 0; i < 50; i++)
            {
                int rot = Random.Range(0, 4);
                int x = Random.Range(0, 4);
                int y = Random.Range(0, 7);
                Vector2Int origin = new Vector2Int(x, y);
                if (IsValidPlacement(grid, part, origin, rot))
                {
                    resultPos = origin;
                    resultRot = rot;
                    return true;
                }
            }
            return false;
        }

        private bool IsValidPlacement(bool[,] grid, GameCore.RefData.PartRefObj part, Vector2Int origin, int rot)
        {
            List<Vector2Int> shape = GetRotatedShape(part, rot);
            foreach (var offset in shape)
            {
                Vector2Int p = origin + offset;
                if (p.x < 0 || p.x >= 4 || p.y < 0 || p.y >= 7) return false;
                if (grid[p.x, p.y]) return false;
                
                // Check Disabled Grids from Mono
                if (mono.disabledGrids != null && mono.disabledGrids.Contains(p))
                {
                    return false;
                }
            }
            return true;
        }
        
        // Helper methods for visual occupancy marking (same as before)
        private void MarkOccupancy(bool[,] grid, GameCore.RefData.PartRefObj part, Vector2Int origin, int rot)
        {
            List<Vector2Int> shape = GetRotatedShape(part, rot);
            foreach(var offset in shape)
            {
                Vector2Int p = origin + offset;
                if (p.x >= 0 && p.x < 4 && p.y >= 0 && p.y < 7) 
                    grid[p.x, p.y] = true;
            }
        }

        private List<Vector2Int> GetRotatedShape(GameCore.RefData.PartRefObj part, int rot)
        {
            List<Vector2Int> list = new List<Vector2Int>();
            
            if (part.posList != null && part.posList.Count > 0)
            {
                foreach(var pObj in part.posList)
                {
                    Vector2Int p = new Vector2Int(pObj.x, pObj.y);
                    // Rotate
                    for(int k=0; k<rot; k++) p = new Vector2Int(-p.y, p.x);
                    list.Add(p);
                }
            }
            else
            {
                 list.Add(Vector2Int.zero); 
            }
            return list;
        }

        private void CreatePartItem(PartInfo partInfo)
        {
            // Instantiate Item
            if (mono.monoGridList == null || mono.monoGridList.Count == 0) return;
            
            Vector2Int gridPos = partInfo.gridPos;
            int rot = partInfo.rotation;
            
            int index = gridPos.y * 4 + gridPos.x;
            if (index < 0 || index >= mono.monoGridList.Count) return;
            
            // Position: Match the grid cell position
            var targetGrid = mono.monoGridList[index];

            // Parent to the specific grid cell as requested
            var itemGO = ResourcesHelper.LoadGameObject("prefab_mask_combine_part_item", targetGrid.transform);
            UIMonoMaskCombinePartContainerItem item = itemGO.GetComponent<UIMonoMaskCombinePartContainerItem>();
            item.useGameObjSpr = true;
            if (itemGO == null) return;
            
            _spawnedParts.Add(itemGO);

            // Position: Local zero since parented to grid
            itemGO.transform.localPosition = Vector3.zero;
            
            // Rotation
            itemGO.transform.localRotation = Quaternion.Euler(0, 0, rot * 90);
            
            itemGO.transform.localScale = new Vector3(0.7f, 0.7f, 1);
            
            // Mark occupied grids as red
            List<Vector2Int> shape = GetRotatedShape(partInfo.partRefObj, rot);
            foreach(var offset in shape)
            {
                Vector2Int p = gridPos + offset;
                if (p.x >= 0 && p.x < 4 && p.y >= 0 && p.y < 7)
                {
                    int occIndex = p.y * 4 + p.x;
                    if (occIndex >= 0 && occIndex < mono.monoGridList.Count)
                    {
                        var gridMono = mono.monoGridList[occIndex];
                        if (gridMono != null && gridMono.imgBg != null)
                        {
                            gridMono.imgBg.color = new Color(1f, 0.3f, 0.3f, 0.8f); // Red tint
                        }
                    }
                }
            }

            // Using Helper Logic for Display
            var itemMono = itemGO.GetComponent<UIMonoMaskCombinePartContainerItem>();
            if (itemMono != null)
            {
                var itemPanel = new UIPanelMaskCombinePartContainerItem(itemMono, showType);
                itemPanel.AfterInitialize();
                itemPanel.SetInfo(partInfo); 
                
                // USER REQUEST: Match OnDragEnd logic (Init Visuals from data for Pivot/Size)
                itemPanel.InitVisualsFromData();
            }
            
            // Re-apply Transform settings (Scale/Rotation) AFTER InitVisuals might have reset them (e.g. scale to 1)
            // Position: Local zero since parented to grid
            itemGO.transform.localPosition = Vector3.zero;
            
            // Rotation
            itemGO.transform.localRotation = Quaternion.Euler(0, 0, rot * 90);
            
            // Scale (Enemy Mask uses 0.7f)
            itemGO.transform.localScale = new Vector3(0.7f, 0.7f, 1);
            
            // USER REQUEST: Set Sorting Order to 6
            if (itemMono.imgCanvas != null)
            {
                itemMono.imgCanvas.overrideSorting = true;
                itemMono.imgCanvas.sortingOrder = 6;
            }
        }
    }
}
