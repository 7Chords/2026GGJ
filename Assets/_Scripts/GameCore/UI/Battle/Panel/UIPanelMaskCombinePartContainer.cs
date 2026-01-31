using SCFrame;
using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.UI
{
    public class UIPanelMaskCombinePartContainer : UIPanelContainerBase<UIMonoCommonContainer, UIPanelMaskCombinePartContainerItem, UIMonoMaskCombinePartContainerItem>
    {
        private List<UIPanelMaskCombinePartContainerItem> _m_partItemList;//item列表
        public UIMonoCommonContainer mono;

        public UIPanelMaskCombinePartContainer(UIMonoCommonContainer _mono, SCUIShowType _showType = SCUIShowType.INTERNAL) : base(_mono, _showType)
        {
            mono = _mono;
        }

        public override void AfterInitialize()
        {
            _m_partItemList = new List<UIPanelMaskCombinePartContainerItem>();
        }

        public override void BeforeDiscard()
        {
            foreach(var item in _m_partItemList)
            {
                item?.Discard();
            }
        }

        public override void OnHidePanel()
        {
            foreach (var item in _m_partItemList)
            {
                item?.HidePanel();
            }
        }

        public override void OnShowPanel()
        {
        }

        protected override GameObject creatItemGO()
        {
            return ResourcesHelper.LoadGameObject(mono.prefabItemObjName,mono.layoutGroup.transform);
        }


        protected override UIPanelMaskCombinePartContainerItem creatItemPanel(UIMonoMaskCombinePartContainerItem _mono)
        {
            var item = new UIPanelMaskCombinePartContainerItem(_mono, SCUIShowType.INTERNAL);
            item.SetContainer(this);
            return item;
        }

        // Helper to rotate vector around (0,0) logic
        private Vector2Int RotateVector(Vector2Int v, int rotationSteps)
        {
            Vector2Int ret = v;
            for(int i=0; i<rotationSteps; i++)
            {
                // (x, y) -> (-y, x) for 90 degrees counter-clockwise
                ret = new Vector2Int(-ret.y, ret.x);
            }
            return ret;
        }

        public bool CheckOccupancy(Vector2Int targetPos, PartInfo exceptInfo)
        {
            if (_m_partItemList == null) return false;
            foreach (var item in _m_partItemList)
            {
                var info = item.GetPartInfo();
                if (info == null || info == exceptInfo) continue;
                if (info.gridPos == targetPos) return true;
                
                // Check shape overlap
                if (info.partRefObj != null && info.partRefObj.posList != null && info.gridPos.x != -1)
                {
                    foreach (var p in info.partRefObj.posList)
                    {
                         // Apply rotation !
                         Vector2Int rotatedP = RotateVector(new Vector2Int(p.x, p.y), info.rotation);
                         Vector2Int occupied = info.gridPos + rotatedP;
                         if (occupied == targetPos) return true;
                    }
                }
            }
            return false;
        }

        public bool CheckRegionOccupancy(Vector2Int logicalOrigin, List<GameCore.RefData.PosEffectObj> shape, PartInfo exceptInfo)
        {
            // Calculate all required positions
            List<Vector2Int> required = new List<Vector2Int>();
            if (shape == null || shape.Count == 0)
            {
                required.Add(logicalOrigin);
            }
            else
            {
                // shape coming in is already rotated if needed (calling context handles it for the dragging item)
                foreach(var p in shape) required.Add(logicalOrigin + new Vector2Int(p.x, p.y));
            }
            
            // Check against all items
            if (_m_partItemList == null) return false;
            
            foreach (var item in _m_partItemList)
            {
                var info = item.GetPartInfo();
                if (info == null || info == exceptInfo) continue;
                if (info.gridPos.x == -1) continue; // In bag
                
                // Get existing item's occupied cells
                List<Vector2Int> existingOccupied = new List<Vector2Int>();
                if (info.partRefObj != null && info.partRefObj.posList != null && info.partRefObj.posList.Count > 0)
                {
                    foreach (var p in info.partRefObj.posList) 
                    {
                        // Apply existing item's rotation
                        Vector2Int rotatedP = RotateVector(new Vector2Int(p.x, p.y), info.rotation);
                        existingOccupied.Add(info.gridPos + rotatedP);
                    }
                }
                else
                {
                    existingOccupied.Add(info.gridPos);
                }
                
                // Intersect
                foreach(var r in required)
                {
                    if (existingOccupied.Contains(r)) return true;
                }
            }
            return false;
        }

        public List<PartInfo> GetPlacedParts()
        {
            List<PartInfo> list = new List<PartInfo>();
            if (_m_partItemList == null) return list;

            foreach (var item in _m_partItemList)
            {
                var info = item.GetPartInfo();
                if (info == null) continue;
                if (info.gridPos.x != -1) // -1 means in bag
                {
                    list.Add(info);
                }
            }
            return list;
        }

        public void SetListInfo(List<PartInfo> _infoList)
        {
            if (_infoList == null)
                return;
            if (_m_partItemList == null)
                return;

            int i = 0, count = 0;
            UIPanelMaskCombinePartContainerItem item = null;
            for (i = 0; i < _infoList.Count; i++)
            {
                if (i < _m_partItemList.Count)
                {
                    item = _m_partItemList[i];
                }
                else
                {
                    GameObject itemGO = creatItemGO();
                    item = creatItemPanel(itemGO.GetComponent<UIMonoMaskCombinePartContainerItem>());
                    _m_partItemList.Add(item);
                }
                if (item == null)
                    continue;
                item.SetInfo(_infoList[i]);
                item.ShowPanel();
                count++;
            }
            //隐藏多余的
            for (i = count; i < _m_partItemList.Count; i++)
            {
                item = _m_partItemList[i];
                if (item == null)
                    continue;
                item.HidePanel();
            }

        }

        public void RefreshShow(List<PartInfo> _infoList)
        {
            if (_infoList == null)
                return;
            if (_m_partItemList == null)
                return;

            int i = 0, count = 0;
            UIPanelMaskCombinePartContainerItem item = null;
            for (i = 0; i < _infoList.Count; i++)
            {
                if (i < _m_partItemList.Count)
                {
                    item = _m_partItemList[i];
                }
                else
                {
                    GameObject itemGO = creatItemGO();
                    item = creatItemPanel(itemGO.GetComponent<UIMonoMaskCombinePartContainerItem>());

                    _m_partItemList.Add(item);
                }
                if (item == null)
                    continue;
                item.SetInfo(_infoList[i]);
                count++;
            }
            //隐藏多余的
            for (i = count; i < _m_partItemList.Count; i++)
            {
                item = _m_partItemList[i];
                if (item == null)
                    continue;
                item.HidePanel();
            }
        }
    }
}
