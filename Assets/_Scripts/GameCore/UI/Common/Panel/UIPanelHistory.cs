using GameCore;
using SCFrame;
using SCFrame.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameCore.UI
{
    public class UIPanelHistory : _ASCUIPanelBase<UIMonoHistory>
    {
        private readonly List<UIPanelHistoryItem> _m_itemList = new List<UIPanelHistoryItem>();

        public UIPanelHistory(UIMonoHistory _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
        }

        public override void BeforeDiscard()
        {
            for (int i = 0; i < _m_itemList.Count; i++)
                _m_itemList[i]?.Discard();
            _m_itemList.Clear();
        }

        public override void OnHidePanel()
        {
            if (mono.btnClose != null)
                mono.btnClose.RemoveClickDown(onBtnCloseClickDown);
            for (int i = 0; i < _m_itemList.Count; i++)
                _m_itemList[i]?.HidePanel();
        }

        public override void OnShowPanel()
        {
            if (mono.btnClose != null)
                mono.btnClose.AddMouseLeftClickDown(onBtnCloseClickDown);
            refreshHistoryList();
        }

        private void refreshHistoryList()
        {
            var entries = GameBattleHistory.GetEntries();
            bool hasEntries = entries != null && entries.Count > 0;
            if (mono.goEmptyHint != null)
                mono.goEmptyHint.SetActive(!hasEntries);

            if (mono.monoListContainer == null || mono.monoListContainer.layoutGroup == null)
                return;

            int showCount = hasEntries ? entries.Count : 0;
            for (int i = 0; i < showCount; i++)
            {
                UIPanelHistoryItem itemPanel = getOrCreateItem(i);
                if (itemPanel == null)
                    continue;
                itemPanel.SetInfo(entries[i]);
                itemPanel.ShowPanel();
            }

            for (int i = showCount; i < _m_itemList.Count; i++)
                _m_itemList[i]?.HidePanel();
        }

        private UIPanelHistoryItem getOrCreateItem(int index)
        {
            if (index < _m_itemList.Count)
                return _m_itemList[index];

            GameObject itemGO = ResourcesHelper.LoadGameObject(
                mono.monoListContainer.prefabItemObjName,
                mono.monoListContainer.layoutGroup.transform);
            if (itemGO == null)
                return null;

            UIMonoHistoryItem itemMono = itemGO.GetComponent<UIMonoHistoryItem>();
            if (itemMono == null)
            {
                Debug.LogError("prefab missing UIMonoHistoryItem: " + mono.monoListContainer.prefabItemObjName);
                return null;
            }

            var itemPanel = new UIPanelHistoryItem(itemMono, SCUIShowType.INTERNAL);
            itemPanel.Initialize();
            _m_itemList.Add(itemPanel);
            return itemPanel;
        }

        private void onBtnCloseClickDown(PointerEventData _data, object[] _objs)
        {
            AudioMgr.instance.PlaySfx("sfx_click");
            UICoreMgr.instance.CloseTopNode();
        }
    }
}
