using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameCore.RefData;
using SCFrame;

namespace GameCore.UI
{
    public class UIPanelMaskCombineFace : _ASCUIPanelBase<UIMonoMaskCombineFace>
    {
        private List<UIPanelMaskCombinePartContainerItem> _partItems = new List<UIPanelMaskCombinePartContainerItem>();

        public void RefreshParts()
        {
            if (_partItems != null)
            {
                foreach(var item in _partItems)
                {
                    if (item != null) item.RefreshUI();
                }
            }
        }

      
        // ... Inside CreatePartItem ...
        
        // (See implementation below)
        private List<UIMonoMaskCombineFaceGrid> _gridList = new List<UIMonoMaskCombineFaceGrid>(); // Assuming reusing EnemyMaskGrid or similar

        public UIPanelMaskCombineFace(UIMonoMaskCombineFace _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {

        }

        public override void BeforeDiscard()
        {

        }

        public override void OnHidePanel()
        {

        }
        
        public override void OnShowPanel()
        {
            // Do nothing on default show, wait for Setup
        }

        public void Setup(List<PartInfo> parts)
        {
            Debug.Log($"[UIPanelMaskCombineFace] Setup with {parts?.Count} parts");
            ClearItems();
            CreateGrids();
            
            if (parts == null) return;
            
            foreach(var part in parts)
            {
                CreatePartItem(part);
            }
        }
        


        private void ClearItems()
        {
            if (_partItems != null) // Changed from _spawnedParts
            {
                foreach(var item in _partItems) // Changed from go in _spawnedParts
                {
                    if(item != null) // Changed from go != null
                    {
                        item.DestroySelf(); // Use public method instead of accessing protected mono
                        item.Discard(); 
                    }
                }
                _partItems.Clear(); // Changed from _spawnedParts.Clear();
            }
        
     
            
            // Robust Clear: Destroy all children to avoid duplicates if _gridList lost track
            if (mono.transform.childCount > 0)
            {
                for(int i = mono.transform.childCount - 1; i >= 0; i--)
                {
                    var child = mono.transform.GetChild(i);
                    // Protect the prefab if it's a child
                    if (mono.gridPrefab != null && child == mono.gridPrefab.transform) continue;
                    
                    Object.Destroy(child.gameObject);
                }
            }
            _gridList.Clear();
        }

        private void CreateGrids()
        {
             if (mono.gridPrefab == null) 
             {
                 Debug.LogError($"[UIPanelMaskCombineFace] gridPrefab is null in {mono.gameObject.name}!");
                 return;
             }
             
             // Fallback defaults if 0
             if (mono.rowCount == 0) mono.rowCount = 7;
             if (mono.columnCount == 0) mono.columnCount = 4;
             
             Debug.Log($"[UIPanelMaskCombineFace] Creating Grids: Rows={mono.rowCount}, Cols={mono.columnCount}, Disabled={(mono.disabledGrids!=null?mono.disabledGrids.Count:0)}");
             
             for(int y=0; y<mono.rowCount; y++)
             {
                 for(int x=0; x<mono.columnCount; x++)
                 {
                     GameObject go = SCCommon.InstantiateGameObject(mono.gridPrefab, mono.transform);
                     var script = go.GetComponent<UIMonoMaskCombineFaceGrid>(); // reusing existing grid mono
                     if (script != null)
                     {
                         script.gridPos = new Vector2(x, y);
                         // script.imgBg.color = Color.white; // Ensure white
                         
                         if (mono.disabledGrids != null && mono.disabledGrids.Contains(new Vector2Int(x, y)))
                         {
                             if (script.imgBg != null)
                             {
                                 script.imgBg.enabled = false; // Hide completely
                             }
                         }
                         else
                         {
                             // Ensure default state if re-used or pooled
                             if (script.imgBg != null)
                             {
                                 script.imgBg.enabled = true;
                                 // Respect prefab color
                             }
                         }

                         _gridList.Add(script);
                     }
                     else
                     {
                         Debug.LogWarning($"[UIPanelMaskCombineFace] Grid prefab missing UIMonoEnemyMaskGrid component!");
                     }
                 }
             }
             mono.gridPrefab.SetActive(false);
        }

        private void CreatePartItem(PartInfo part)
        {
            if (_gridList == null || _gridList.Count == 0) 
            {
                Debug.LogError("[UIPanelMaskCombineFace] CreatePartItem failed: _gridList is empty!");
                return;
            }
            
            int index = part.gridPos.y * mono.columnCount + part.gridPos.x;
            if (index < 0 || index >= _gridList.Count) 
            {
                Debug.LogError($"[UIPanelMaskCombineFace] CreatePartItem failed: Index {index} out of bounds (GridPos: {part.gridPos}, ListCount: {_gridList.Count})");
                return;
            }
            
            var targetGrid = _gridList[index];
            if (targetGrid == null)
            {
                Debug.LogError($"[UIPanelMaskCombineFace] targetGrid at index {index} is null!");
                return;
            }
            
            var itemGO = ResourcesHelper.LoadGameObject("prefab_mask_combine_part_item", targetGrid.transform);
            if (itemGO == null) 
            {
                 Debug.LogError($"[UIPanelMaskCombineFace] Failed to load 'prefab_mask_combine_part_item'. Check spelling or Addressables/Resources.");
                 return;
            }
            
            var itemMono = itemGO.GetComponent<UIMonoMaskCombinePartContainerItem>();
            if (itemMono != null)
            {
                var itemPanel = new UIPanelMaskCombinePartContainerItem(itemMono, showType);
                itemPanel.AfterInitialize();
                itemPanel.SetInfo(part); 
                _partItems.Add(itemPanel);
                
                // Set Rotation
                itemGO.transform.localRotation = Quaternion.Euler(0, 0, part.rotation * 90);
                
                itemPanel.InitVisualsFromData();
            }
            else
            {
                Debug.LogError("[UIPanelMaskCombineFace] Logic Error: Item Prefab missing Mono component");
            }

            Debug.Log($"[UIPanelMaskCombineFace] Generated Item for {part.partRefObj?.partName} at {part.gridPos}");
            
            // Re-apply Transform settings (Scale/Rotation) AFTER InitVisuals might have reset them
            // itemGO.transform.localPosition = Vector3.zero; // InitVisuals handles this
            itemGO.transform.localScale = new Vector3(0.7f, 0.7f, 1); 
            
            // USER REQUEST: Set Sorting Order to 6
            if (itemMono.imgCanvas != null)
            {
                itemMono.imgCanvas.overrideSorting = true;
                itemMono.imgCanvas.sortingOrder = 6;
            } 
            
            // Mark occupied grids as red
            if (part.partRefObj != null)
            {
                List<Vector2Int> shape = GetRotatedShape(part.partRefObj, part.rotation);
                foreach(var offset in shape)
                {
                    Vector2Int p = part.gridPos + offset;
                    if (p.x >= 0 && p.x < mono.columnCount && p.y >= 0 && p.y < mono.rowCount)
                    {
                        int occIndex = p.y * mono.columnCount + p.x;
                        if (occIndex >= 0 && occIndex < _gridList.Count)
                        {
                            var gridMono = _gridList[occIndex];
                            if (gridMono != null && gridMono.imgBg != null)
                            {
                                gridMono.imgBg.color = new Color(1f, 0.3f, 0.3f, 0.8f); // Red tint
                            }
                        }
                    }
                }
            }

            var partScript = itemGO.GetComponent<UIMonoMaskCombinePartContainerItem>();
            if (partScript != null && part.partRefObj != null)
            {
                if (!string.IsNullOrEmpty(part.partRefObj.partGameObjectName))
                {
                     partScript.imgGoods.sprite = ResourcesHelper.LoadAsset<Sprite>(part.partRefObj.partGameObjectName);
                }
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
    }
}
