using GameCore;
using SCFrame;
using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameCore.UI
{
    public enum EHistoryListMode
    {
        All = 0,
        Favorite = 1,
    }

    public class UIPanelHistory : _ASCUIPanelBase<UIMonoHistory>
    {
        private readonly List<UIPanelHistoryItem> _m_itemList = new List<UIPanelHistoryItem>();
        private Coroutine _m_listLayoutRebuildRoutine;
        private EHistoryListMode _m_listMode = EHistoryListMode.All;
        private bool _m_isRefreshingList;

        public UIPanelHistory(UIMonoHistory _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
        }

        public override void BeforeDiscard()
        {
            cancelDeferredListLayoutRebuild();
            for (int i = 0; i < _m_itemList.Count; i++)
                _m_itemList[i]?.Discard();
            _m_itemList.Clear();
        }

        public override void OnHidePanel()
        {
            cancelDeferredListLayoutRebuild();
            if (mono.btnClose != null)
                mono.btnClose.RemoveClickDown(onBtnCloseClickDown);
            unbindTabButton(mono.btnAll, onBtnAllClickDown);
            unbindTabButton(mono.btnFavorite, onBtnFavoriteTabClickDown);
            for (int i = 0; i < _m_itemList.Count; i++)
            {
                _m_itemList[i]?.ResetToCollapsed();
                _m_itemList[i]?.HidePanel();
            }
        }

        public override void OnShowPanel()
        {
            _m_listMode = EHistoryListMode.All;

            if (mono.btnClose != null)
                mono.btnClose.AddMouseLeftClickDown(onBtnCloseClickDown);
            bindTabButton(mono.btnAll, onBtnAllClickDown);
            bindTabButton(mono.btnFavorite, onBtnFavoriteTabClickDown);

            refreshTabButtonState();
            refreshHistoryList();
        }

        private void bindTabButton(Button btn, System.Action<PointerEventData, object[]> click)
        {
            if (btn == null)
                return;
            btn.AddMouseLeftClickDown(click);
        }

        private void unbindTabButton(Button btn, System.Action<PointerEventData, object[]> click)
        {
            if (btn == null)
                return;
            btn.RemoveClickDown(click);
        }

        private void selectListMode(EHistoryListMode mode)
        {
            if (_m_listMode == mode)
                return;

            AudioMgr.instance.PlaySfx("sfx_click");
            _m_listMode = mode;
            refreshTabButtonState();
            refreshHistoryList();
        }

        private void refreshTabButtonState()
        {
            setTabButtonSelected(mono.btnAll, _m_listMode == EHistoryListMode.All);
            setTabButtonSelected(mono.btnFavorite, _m_listMode == EHistoryListMode.Favorite);
        }

        private static void setTabButtonSelected(Button btn, bool selected)
        {
            if (btn == null)
                return;
            btn.interactable = !selected;
        }

        private void refreshHistoryList()
        {
            if (_m_isRefreshingList)
                return;

            _m_isRefreshingList = true;
            try
            {
                IReadOnlyList<GameBattleHistory.BattleHistoryEntry> entries =
                    _m_listMode == EHistoryListMode.Favorite
                        ? GameBattleHistory.GetFavoriteEntries()
                        : GameBattleHistory.GetEntries();

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

                    itemPanel.onFavoriteStateChanged = onItemFavoriteStateChanged;
                    itemPanel.onExpandStateChanged = onItemExpandStateChanged;
                    itemPanel.SetInfo(entries[i]);
                    if (!itemPanel.hasShowed)
                        itemPanel.ShowPanel();
                }

                for (int i = showCount; i < _m_itemList.Count; i++)
                {
                    _m_itemList[i]?.ResetToCollapsed();
                    _m_itemList[i]?.HidePanel();
                }

                rebuildHistoryListLayout();
                scheduleDeferredListLayoutRebuild();
            }
            finally
            {
                _m_isRefreshingList = false;
            }
        }

        private void onItemFavoriteStateChanged()
        {
            if (_m_listMode != EHistoryListMode.Favorite)
                return;

            refreshHistoryList();
        }

        private void onItemExpandStateChanged()
        {
            rebuildHistoryListLayout();
            scheduleDeferredListLayoutRebuild();
        }

        private void rebuildHistoryListLayout()
        {
            if (mono.monoListContainer == null || mono.monoListContainer.layoutGroup == null)
                return;

            for (int i = 0; i < _m_itemList.Count; i++)
            {
                UIPanelHistoryItem itemPanel = _m_itemList[i];
                if (itemPanel == null || !itemPanel.hasShowed)
                    continue;
                itemPanel.RebuildItemLayout();
            }

            RectTransform listRect = mono.monoListContainer.layoutGroup.transform as RectTransform;
            if (listRect == null)
                return;

            rebuildLayoutRect(listRect);

            ScrollRect scroll = listRect.GetComponentInParent<ScrollRect>();
            if (scroll?.content != null && scroll.content != listRect)
                rebuildLayoutRect(scroll.content);

            Canvas.ForceUpdateCanvases();
        }

        private static void rebuildLayoutRect(RectTransform rect)
        {
            if (rect != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        }

        private void scheduleDeferredListLayoutRebuild()
        {
            cancelDeferredListLayoutRebuild();
            if (mono == null)
                return;

            _m_listLayoutRebuildRoutine = this.StartCoroutine(coDeferredListLayoutRebuild());
        }

        private IEnumerator coDeferredListLayoutRebuild()
        {
            yield return null;
            yield return new WaitForEndOfFrame();

            _m_listLayoutRebuildRoutine = null;
            rebuildHistoryListLayout();
        }

        private void cancelDeferredListLayoutRebuild()
        {
            if (_m_listLayoutRebuildRoutine == null)
                return;

            this.StopCoroutine(_m_listLayoutRebuildRoutine);
            _m_listLayoutRebuildRoutine = null;
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

        private void onBtnCloseClickDown(PointerEventData data, object[] objs)
        {
            AudioMgr.instance.PlaySfx("sfx_click");
            UICoreMgr.instance.CloseTopNode();
        }

        private void onBtnAllClickDown(PointerEventData data, object[] objs) => selectListMode(EHistoryListMode.All);

        private void onBtnFavoriteTabClickDown(PointerEventData data, object[] objs) => selectListMode(EHistoryListMode.Favorite);
    }
}
