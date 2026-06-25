using GameCore;
using GameCore.Data;
using GameCore.Helpers;
using GameCore.RefData;
using SCFrame;
using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIPanelBookEnemyDetail : _ASCUIPanelBase<UIMonoBookEnemyDetail>
    {
        private readonly List<UIPanelBookEnemyPartReserveItem> _m_reserveItemList = new List<UIPanelBookEnemyPartReserveItem>();
        private readonly List<UIPanelBookEnemyTurnLayoutItem> _m_turnItemList = new List<UIPanelBookEnemyTurnLayoutItem>();

        private EnemyRefObj _m_enemyRef;
        private List<PartInfo> _m_deckParts;
        private List<EnemyBookPreviewHelper.PartReserveSummaryEntry> _m_reserveSummaries;
        private EnemyLayoutPreset _m_layoutPreset;
        private Coroutine _m_layoutRebuildRoutine;

        public UIPanelBookEnemyDetail(UIMonoBookEnemyDetail mono, SCUIShowType showType) : base(mono, showType)
        {
        }

        public override void AfterInitialize()
        {
        }

        public override void BeforeDiscard()
        {
            cancelDeferredDetailLayoutRebuild();
            discardReserveItems();
            discardTurnItems();
        }

        public override void OnHidePanel()
        {
            cancelDeferredDetailLayoutRebuild();
            if (mono.btnClose != null)
                mono.btnClose.RemoveClickDown(onBtnCloseClickDown);
            hideReserveItems();
            hideTurnItems();
        }

        public override void OnShowPanel()
        {
            if (mono.btnClose != null)
                mono.btnClose.AddMouseLeftClickDown(onBtnCloseClickDown);
        }

        public void SetInfo(EnemyRefObj enemyRef)
        {
            _m_enemyRef = enemyRef;
            _m_deckParts = EnemyBookPreviewHelper.BuildDeckParts(enemyRef);
            _m_reserveSummaries = EnemyBookPreviewHelper.BuildPartReserveSummaries(enemyRef);
            _m_layoutPreset = EnemyBookPreviewHelper.LoadLayoutPreset(enemyRef);
            refreshShow();
        }

        private void refreshShow()
        {
            if (_m_enemyRef == null)
                return;

            if (mono.txtEnemyName != null)
                mono.txtEnemyName.text = string.IsNullOrEmpty(_m_enemyRef.enemyName) ? "" : _m_enemyRef.enemyName;

            if (mono.txtEnemyHealth != null)
                mono.txtEnemyHealth.text = PartHealthDisplay.FormatMaxOnly(_m_enemyRef.enemyHealth);

            refreshReserveItems();
            refreshTurnLayoutItems();
            scheduleDeferredDetailLayoutRebuild();
        }

        private void refreshReserveItems()
        {
            hideReserveItems();

            if (mono.monoPartReserveContainer == null || mono.monoPartReserveContainer.layoutGroup == null)
                return;

            if (_m_reserveSummaries == null || _m_reserveSummaries.Count == 0)
                return;

            for (int i = 0; i < _m_reserveSummaries.Count; i++)
            {
                UIPanelBookEnemyPartReserveItem itemPanel = getOrCreateReserveItem(i);
                if (itemPanel == null)
                    continue;

                EnemyBookPreviewHelper.PartReserveSummaryEntry entry = _m_reserveSummaries[i];
                itemPanel.SetInfo(entry.partName, entry.count);
                if (!itemPanel.hasShowed)
                    itemPanel.ShowPanel();
            }

            for (int i = _m_reserveSummaries.Count; i < _m_reserveItemList.Count; i++)
                _m_reserveItemList[i]?.HidePanel();
        }

        private void refreshTurnLayoutItems()
        {
            hideTurnItems();

            if (mono.monoTurnLayoutContainer == null || mono.monoTurnLayoutContainer.layoutGroup == null)
                return;

            List<EnemyBookPreviewHelper.TurnLayoutPreviewEntry> entries =
                EnemyBookPreviewHelper.CollectTurnLayoutEntries(_m_layoutPreset);
            if (entries == null || entries.Count == 0)
                return;

            removeOrphanTurnLayoutChildren();

            for (int i = 0; i < entries.Count; i++)
            {
                UIPanelBookEnemyTurnLayoutItem itemPanel = getOrCreateTurnItem(i);
                if (itemPanel == null)
                    continue;

                itemPanel.SetInfo(_m_enemyRef, entries[i], _m_deckParts, _m_layoutPreset);
                itemPanel.ShowPanel();
            }

            for (int i = entries.Count; i < _m_turnItemList.Count; i++)
                _m_turnItemList[i]?.HidePanel();
        }

        private void removeOrphanTurnLayoutChildren()
        {
            Transform parent = mono.monoTurnLayoutContainer.layoutGroup.transform;
            var managedRoots = new HashSet<GameObject>();
            for (int i = 0; i < _m_turnItemList.Count; i++)
            {
                GameObject go = _m_turnItemList[i]?.GetGameObject();
                if (go != null)
                    managedRoots.Add(go);
            }

            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                GameObject child = parent.GetChild(i).gameObject;
                if (!managedRoots.Contains(child))
                    Object.Destroy(child);
            }
        }

        private UIPanelBookEnemyPartReserveItem getOrCreateReserveItem(int index)
        {
            if (index < _m_reserveItemList.Count)
                return _m_reserveItemList[index];

            GameObject itemGO = ResourcesHelper.LoadGameObject(
                mono.monoPartReserveContainer.prefabItemObjName,
                mono.monoPartReserveContainer.layoutGroup.transform);
            if (itemGO == null)
                return null;

            UIMonoBookEnemyPartReserveItem itemMono = itemGO.GetComponent<UIMonoBookEnemyPartReserveItem>();
            if (itemMono == null)
            {
                Debug.LogError("prefab missing UIMonoBookEnemyPartReserveItem: " + mono.monoPartReserveContainer.prefabItemObjName);
                return null;
            }

            var itemPanel = new UIPanelBookEnemyPartReserveItem(itemMono, SCUIShowType.INTERNAL);
            itemPanel.Initialize();
            _m_reserveItemList.Add(itemPanel);
            return itemPanel;
        }

        private UIPanelBookEnemyTurnLayoutItem getOrCreateTurnItem(int index)
        {
            if (index < _m_turnItemList.Count)
                return _m_turnItemList[index];

            GameObject itemGO = ResourcesHelper.LoadGameObject(
                mono.monoTurnLayoutContainer.prefabItemObjName,
                mono.monoTurnLayoutContainer.layoutGroup.transform);
            if (itemGO == null)
                return null;

            UIMonoBookEnemyTurnLayoutItem itemMono = itemGO.GetComponent<UIMonoBookEnemyTurnLayoutItem>();
            if (itemMono == null)
            {
                Debug.LogError("prefab missing UIMonoBookEnemyTurnLayoutItem: " + mono.monoTurnLayoutContainer.prefabItemObjName);
                return null;
            }

            var itemPanel = new UIPanelBookEnemyTurnLayoutItem(itemMono, SCUIShowType.INTERNAL);
            itemPanel.Initialize();
            _m_turnItemList.Add(itemPanel);
            return itemPanel;
        }

        private void hideReserveItems()
        {
            for (int i = 0; i < _m_reserveItemList.Count; i++)
                _m_reserveItemList[i]?.HidePanel();
        }

        private void hideTurnItems()
        {
            for (int i = 0; i < _m_turnItemList.Count; i++)
                _m_turnItemList[i]?.HidePanel();
        }

        private void discardReserveItems()
        {
            for (int i = 0; i < _m_reserveItemList.Count; i++)
                _m_reserveItemList[i]?.Discard();
            _m_reserveItemList.Clear();
        }

        private void discardTurnItems()
        {
            for (int i = 0; i < _m_turnItemList.Count; i++)
                _m_turnItemList[i]?.Discard();
            _m_turnItemList.Clear();
        }

        private void scheduleDeferredDetailLayoutRebuild()
        {
            cancelDeferredDetailLayoutRebuild();
            if (mono == null)
                return;

            _m_layoutRebuildRoutine = this.StartCoroutine(coDeferredDetailLayoutRebuild());
        }

        private IEnumerator coDeferredDetailLayoutRebuild()
        {
            yield return null;
            yield return new WaitForEndOfFrame();

            _m_layoutRebuildRoutine = null;
            rebuildDetailLayout();
        }

        private void cancelDeferredDetailLayoutRebuild()
        {
            if (_m_layoutRebuildRoutine == null)
                return;

            this.StopCoroutine(_m_layoutRebuildRoutine);
            _m_layoutRebuildRoutine = null;
        }

        private void rebuildDetailLayout()
        {
            rebuildVisibleTurnItemLayouts();
            rebuildContainerLayout(mono.monoTurnLayoutContainer);

            ScrollRect scroll = getTurnLayoutScrollRect();
            if (scroll != null && scroll.content != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content);

            rebuildContainerLayout(mono.monoPartReserveContainer);

            RectTransform contentRoot = getDetailContentRoot();
            if (contentRoot != null)
            {
                VerticalLayoutGroup verticalLayout = contentRoot.GetComponent<VerticalLayoutGroup>();
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot);
                fitScrollViewHeight(scroll, contentRoot, verticalLayout);
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot);
            }

            if (scroll != null)
                scroll.verticalNormalizedPosition = 1f;

            Canvas.ForceUpdateCanvases();
        }

        private void rebuildVisibleTurnItemLayouts()
        {
            for (int i = 0; i < _m_turnItemList.Count; i++)
            {
                UIPanelBookEnemyTurnLayoutItem itemPanel = _m_turnItemList[i];
                if (itemPanel == null || !itemPanel.hasShowed)
                    continue;

                itemPanel.RebuildLayout();
            }
        }

        private static void rebuildContainerLayout(UIMonoCommonContainer container)
        {
            if (container?.layoutGroup == null)
                return;

            RectTransform layoutRect = container.layoutGroup.transform as RectTransform;
            if (layoutRect != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRect);
        }

        private ScrollRect getTurnLayoutScrollRect()
        {
            if (mono.monoTurnLayoutContainer?.layoutGroup == null)
                return null;

            return mono.monoTurnLayoutContainer.layoutGroup.GetComponentInParent<ScrollRect>();
        }

        private RectTransform getDetailContentRoot()
        {
            ScrollRect scroll = getTurnLayoutScrollRect();
            return scroll != null ? scroll.transform.parent as RectTransform : null;
        }

        private void fitScrollViewHeight(
            ScrollRect scroll,
            RectTransform contentRoot,
            VerticalLayoutGroup verticalLayout)
        {
            if (scroll == null || contentRoot == null)
                return;

            RectTransform scrollRect = scroll.transform as RectTransform;
            if (scrollRect == null)
                return;

            RectTransform partBagRect = mono.monoPartReserveContainer?.layoutGroup?.transform as RectTransform;

            float spacing = verticalLayout != null ? verticalLayout.spacing : 0f;
            float padding = 0f;
            if (verticalLayout != null)
                padding = verticalLayout.padding.top + verticalLayout.padding.bottom;

            float partBagHeight = 0f;
            if (partBagRect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(partBagRect);
                partBagHeight = LayoutUtility.GetPreferredHeight(partBagRect);
            }

            float scrollHeight = contentRoot.rect.height - partBagHeight - spacing - padding;
            if (scrollHeight > 0f)
                scrollRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, scrollHeight);
        }

        private void onBtnCloseClickDown(PointerEventData data, object[] objs)
        {
            AudioMgr.instance.PlaySfx("sfx_click");
            UICoreMgr.instance.CloseTopNode();
        }
    }
}
