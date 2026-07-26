using DG.Tweening;
using GameCore;
using SCFrame;
using SCFrame.UI;
using System.Collections.Generic;
using UnityEngine;


namespace GameCore.UI
{
    public class UIPanelCommonPartContainer : UIPanelContainerBase<UIMonoCommonContainer, UIPanelCommonPartItem, UIMonoCommonPartItem>
    {
        private List<UIPanelCommonPartItem> _m_itemList;
        private bool _m_enableLevelPreviewCycle;

        public UIPanelCommonPartContainer(UIMonoCommonContainer _mono, SCUIShowType _showType = SCUIShowType.INTERNAL) : base(_mono, _showType)
        {
        }

        public void SetEnableLevelPreviewCycle(bool enabled)
        {
            _m_enableLevelPreviewCycle = enabled;
            if (_m_itemList == null)
                return;
            for (int i = 0; i < _m_itemList.Count; i++)
                _m_itemList[i]?.SetEnableLevelPreviewCycle(enabled);
        }

        public override void AfterInitialize()
        {
            _m_itemList = new List<UIPanelCommonPartItem>();
        }

        public override void BeforeDiscard()
        {
            foreach (var item in _m_itemList)
            {
                item?.Discard();
            }
        }

        public override void OnHidePanel()
        {
            foreach (var item in _m_itemList)
            {
                item?.HidePanel();
            }
        }

        public override void OnShowPanel()
        {
        }

        protected override GameObject creatItemGO()
        {
            return ResourcesHelper.LoadGameObject(mono.prefabItemObjName, mono.layoutGroup.transform);
        }

        protected override UIPanelCommonPartItem creatItemPanel(UIMonoCommonPartItem _mono)
        {
            return new UIPanelCommonPartItem(_mono, SCUIShowType.INTERNAL);
        }

        public void SetListInfo(List<PartInfo> _infoList)
        {
            if (_infoList == null)
                return;
            if (_m_itemList == null)
                return;

            int i = 0, count = 0;
            UIPanelCommonPartItem item = null;
            for (i = 0; i < _infoList.Count; i++)
            {
                if (i < _m_itemList.Count)
                {
                    item = _m_itemList[i];
                }
                else
                {
                    GameObject itemGO = creatItemGO();
                    item = creatItemPanel(itemGO.GetComponent<UIMonoCommonPartItem>());

                    _m_itemList.Add(item);
                }
                if (item == null)
                    continue;
                item.SetEnableLevelPreviewCycle(_m_enableLevelPreviewCycle);
                item.SetInfo(_infoList[i]);
                item.ShowPanel();
                count++;
            }
            for (i = count; i < _m_itemList.Count; i++)
            {
                item = _m_itemList[i];
                if (item == null)
                    continue;
                item.HidePanel();
            }
        }

        public void SetListInfoAnimated(
            List<PartInfo> _infoList,
            float interval,
            float popDuration,
            float overshoot)
        {
            if (_infoList == null)
                return;
            if (_m_itemList == null)
                return;

            interval = Mathf.Max(0f, interval);
            popDuration = Mathf.Max(0.01f, popDuration);
            overshoot = Mathf.Clamp(overshoot, 0.1f, 3.5f);

            int i = 0, count = 0;
            UIPanelCommonPartItem item = null;
            for (i = 0; i < _infoList.Count; i++)
            {
                if (i < _m_itemList.Count)
                {
                    item = _m_itemList[i];
                }
                else
                {
                    GameObject itemGO = creatItemGO();
                    item = creatItemPanel(itemGO.GetComponent<UIMonoCommonPartItem>());
                    _m_itemList.Add(item);
                }
                if (item == null)
                    continue;

                item.SetEnableLevelPreviewCycle(_m_enableLevelPreviewCycle);
                item.SetInfo(_infoList[i]);
                item.ShowPanel();

                var tr = item.GetGameObject().transform;
                tr.DOKill(false);
                tr.localScale = Vector3.zero;
                tr.DOScale(Vector3.one, popDuration)
                    .SetDelay(interval * count)
                    .SetEase(Ease.OutBack, overshoot);

                count++;
            }
            for (i = count; i < _m_itemList.Count; i++)
            {
                item = _m_itemList[i];
                if (item == null)
                    continue;
                item.HidePanel();
            }
        }
    }
}
