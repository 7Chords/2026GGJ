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
            
            // 1. Random Enemy
            var enemies = SCRefDataMgr.instance.enemyRefList.refDataList;
            if (enemies == null || enemies.Count == 0) return;
            var enemy = enemies[Random.Range(0, enemies.Count)];
            
            Debug.Log($"[EnemyGen] Selected Enemy: {enemy.enemyName}");

            // 2. Random Parts (Pick 2)
            if (enemy.initPartList == null || enemy.initPartList.Count == 0) return;
            List<GameCore.RefData.PartEffectObj> selectedEffects = new List<GameCore.RefData.PartEffectObj>();
            
            // Fisher-Yates shuffle for randomness or simple loop if small count
            List<GameCore.RefData.PartEffectObj> pool = new List<GameCore.RefData.PartEffectObj>(enemy.initPartList);
            int pickCount = Mathf.Min(2, pool.Count);
            
            for(int k=0; k<pickCount; k++)
            {
               int idx = Random.Range(0, pool.Count);
               selectedEffects.Add(pool[idx]);
               pool.RemoveAt(idx); // No duplicate picking of the exact same entry obj (unless defined multiple times)
            }

            // 3. Placement
            bool[,] occupiedGrid = new bool[6, 7]; // 6 Width, 7 Height
            
            foreach(var effect in selectedEffects)
            {
                var partRef = SCRefDataMgr.instance.partRefList.refDataList.Find(x => x.id == effect.partId);
                if (partRef == null) continue;
                
                // Try to place
                if (TryFindValidPlacement(occupiedGrid, partRef, out Vector2Int pos, out int rot))
                {
                    // Mark occupancy
                    MarkOccupancy(occupiedGrid, partRef, pos, rot);
                    // Create UI
                    CreatePartItem(partRef, pos, rot);
                }
                else
                {
                    Debug.LogWarning($"[EnemyGen] Could not fit part {partRef.partName}");
                }
            }
        }
        
        private bool TryFindValidPlacement(bool[,] grid, GameCore.RefData.PartRefObj part, out Vector2Int resultPos, out int resultRot)
        {
            resultPos = Vector2Int.zero;
            resultRot = 0;
            
            int maxAttempts = 50;
            for(int i=0; i<maxAttempts; i++)
            {
                // Random Rotation: 0, 1, 2, 3 (x90)
                int rot = Random.Range(0, 4);
                // Random Pos
                int x = Random.Range(0, 6);
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
            foreach(var offset in shape)
            {
                Vector2Int p = origin + offset;
                // Check Bounds
                if (p.x < 0 || p.x >= 6 || p.y < 0 || p.y >= 7) return false;
                // Check Occupancy
                if (grid[p.x, p.y]) return false;
            }
            return true;
        }

        private void MarkOccupancy(bool[,] grid, GameCore.RefData.PartRefObj part, Vector2Int origin, int rot)
        {
            List<Vector2Int> shape = GetRotatedShape(part, rot);
            foreach(var offset in shape)
            {
                Vector2Int p = origin + offset;
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
