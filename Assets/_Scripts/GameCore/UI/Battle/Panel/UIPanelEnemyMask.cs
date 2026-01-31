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
            
            int columns = 6; 
            int rows = 7;
            

            // Use content_grid if available
            Transform parent = mono.content_grid != null ? mono.content_grid : mono.transform.Find("GridContainer");
            if (parent == null) parent = mono.transform;
            
            for (int i = 0; i < 42; i++)
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
            
            // Read from Model
            var enemyData = GameModel.instance.currentEnemy;
            if (enemyData == null)
            {
                Debug.LogWarning("[UIPanelEnemyMask] No enemy data in Model!");
                return;
            }
            
            Debug.Log($"[EnemyGen] Displaying Enemy: {enemyData.enemyRef.enemyName}");

            // 3. Placement Visualization
            bool[,] occupiedGrid = new bool[6, 7]; 
            
            foreach(var partInfo in enemyData.parts)
            {
                // Mark occupancy
                MarkOccupancy(occupiedGrid, partInfo.partRefObj, partInfo.gridPos, partInfo.rotation);
                // Create UI
                CreatePartItem(partInfo.partRefObj, partInfo.gridPos, partInfo.rotation);
            }
        }
        
        // Helper methods for visual occupancy marking (same as before)
        private void MarkOccupancy(bool[,] grid, GameCore.RefData.PartRefObj part, Vector2Int origin, int rot)
        {
            List<Vector2Int> shape = GetRotatedShape(part, rot);
            foreach(var offset in shape)
            {
                Vector2Int p = origin + offset;
                if (p.x >= 0 && p.x < 6 && p.y >= 0 && p.y < 7) 
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

        private void CreatePartItem(GameCore.RefData.PartRefObj part, Vector2Int gridPos, int rot)
        {
            // Instantiate Item
            if (mono.monoGridList == null || mono.monoGridList.Count == 0) return;
            
            int index = gridPos.y * 6 + gridPos.x;
            if (index < 0 || index >= mono.monoGridList.Count) return;
            
            // Position: Match the grid cell position
            var targetGrid = mono.monoGridList[index];

            // Parent to the specific grid cell as requested
            var itemGO = ResourcesHelper.LoadGameObject("prefab_mask_combine_part_item", targetGrid.transform); 
            if (itemGO == null) return;
            
            _spawnedParts.Add(itemGO);

            // Position: Local zero since parented to grid
            itemGO.transform.localPosition = Vector3.zero;
            
            // Rotation
            itemGO.transform.localRotation = Quaternion.Euler(0, 0, rot * 90);
            
            itemGO.transform.localScale = new Vector3(0.7f, 0.7f, 1);
            
            // Mark occupied grids as red
            List<Vector2Int> shape = GetRotatedShape(part, rot);
            foreach(var offset in shape)
            {
                Vector2Int p = gridPos + offset;
                if (p.x >= 0 && p.x < 6 && p.y >= 0 && p.y < 7)
                {
                    int occIndex = p.y * 6 + p.x;
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

            // Refine: We should instantiate a simplistic view.
            var partScript = itemGO.GetComponent<UIMonoMaskCombinePartContainerItem>();
            if (partScript != null)
            {
                // Init info manually if possible
                if (!string.IsNullOrEmpty(part.partSpriteObjName))
                {
                     partScript.imgGoods.sprite = ResourcesHelper.LoadAsset<Sprite>(part.partSpriteObjName);
                }
            }
        }
    }
}
