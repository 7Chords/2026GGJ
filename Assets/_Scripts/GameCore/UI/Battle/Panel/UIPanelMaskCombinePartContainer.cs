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

        public UIPanelMaskCombinePartContainer(UIMonoCommonContainer _mono, SCUIShowType _showType = SCUIShowType.INTERNAL) : base(_mono, _showType)
        {
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
            return ResourcesHelper.LoadGameObject(mono.prefabItemObjName);
        }


        protected override UIPanelMaskCombinePartContainerItem creatItemPanel(UIMonoMaskCombinePartContainerItem _mono)
        {
            var item = new UIPanelMaskCombinePartContainerItem(_mono, SCUIShowType.INTERNAL);
            item.SetContainer(this);
            return item;
        }

        public bool CheckOccupancy(Vector2Int targetPos, PartInfo exceptInfo)
        {
            if (_m_partItemList == null) return false;
            foreach (var item in _m_partItemList)
            {
                var info = item.GetPartInfo();
                if (info == null || info == exceptInfo) continue;
                if (info.gridPos == targetPos) return true;
                // TODO: Add multi-cell check if parts have size
            }
            return false;
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
                    itemGO.transform.SetParent(mono.layoutGroup.transform);
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
                    itemGO.transform.SetParent(mono.layoutGroup.transform);
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
