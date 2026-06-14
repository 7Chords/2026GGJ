using GameCore;
using GameCore.Helpers;
using GameCore.RefData;
using SCFrame;
using SCFrame.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameCore.UI
{
    public class UIPanelPartSelect : _ASCUIPanelBase<UIMonoPartSelect>
    {
        public static UIPanelBattleWin pendingBattleWinHost;
        public static int pendingOptionIndex = -1;

        private readonly List<UIPanelPartSelectItem> _m_itemList = new List<UIPanelPartSelectItem>();
        private List<PartInfo> _m_offerList;
        private bool _m_finished;

        public UIPanelPartSelect(UIMonoPartSelect mono, SCUIShowType showType) : base(mono, showType)
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
            if (mono.btnSkip != null)
                mono.btnSkip.RemoveClickDown(onBtnSkipClickDown);
            hideAllItems();
        }

        public override void OnShowPanel()
        {
            _m_finished = false;

            if (mono.btnSkip != null)
            {
                mono.btnSkip.interactable = true;
                mono.btnSkip.RemoveClickDown(onBtnSkipClickDown);
                mono.btnSkip.AddMouseLeftClickDown(onBtnSkipClickDown);
            }

            refreshOfferList();
        }

        private void refreshOfferList()
        {
            hideAllItems();

            if (pendingBattleWinHost != null)
                _m_offerList = pendingBattleWinHost.GetOrRollBootyOffers(pendingOptionIndex);
            else
            {
                EnemyRefObj enemyRef = resolveWinEnemyRef();
                int offerCount = mono != null && mono.offerCount > 0
                    ? mono.offerCount
                    : BattleBootyHelper.DefaultOfferCount;
                _m_offerList = BattleBootyHelper.RollBootyOffers(enemyRef, offerCount);
            }

            if (_m_offerList == null || _m_offerList.Count == 0)
            {
                closeWithoutSelection();
                return;
            }

            if (mono.monoContainer == null || mono.monoContainer.layoutGroup == null)
                return;

            for (int i = 0; i < _m_offerList.Count; i++)
            {
                UIPanelPartSelectItem itemPanel = getOrCreateItem(i);
                if (itemPanel == null)
                    continue;

                itemPanel.onSelected = onItemSelected;
                itemPanel.SetInfo(_m_offerList[i], false);
                if (!itemPanel.hasShowed)
                    itemPanel.ShowPanel();
            }

            for (int i = _m_offerList.Count; i < _m_itemList.Count; i++)
                _m_itemList[i]?.HidePanel();
        }

        private static EnemyRefObj resolveWinEnemyRef()
        {
            long winEnemyId = GameModel.instance.LastWinEnemyRefId;
            if (winEnemyId == 0)
                return null;

            return SCRefDataMgr.instance.enemyRefList.refDataList.Find(x => x.id == winEnemyId);
        }

        private UIPanelPartSelectItem getOrCreateItem(int index)
        {
            if (index < _m_itemList.Count)
                return _m_itemList[index];

            if (mono.monoContainer == null)
                return null;

            GameObject itemGO = ResourcesHelper.LoadGameObject(
                mono.monoContainer.prefabItemObjName,
                mono.monoContainer.layoutGroup.transform);
            if (itemGO == null)
                return null;

            UIMonoPartSelectItem itemMono = itemGO.GetComponent<UIMonoPartSelectItem>();
            if (itemMono == null)
            {
                Debug.LogError("prefab missing UIMonoPartSelectItem: " + mono.monoContainer.prefabItemObjName);
                return null;
            }

            var itemPanel = new UIPanelPartSelectItem(itemMono, SCUIShowType.INTERNAL);
            itemPanel.Initialize();
            _m_itemList.Add(itemPanel);
            return itemPanel;
        }

        private void hideAllItems()
        {
            for (int i = 0; i < _m_itemList.Count; i++)
                _m_itemList[i]?.HidePanel();
        }

        private void lockAllItems()
        {
            for (int i = 0; i < _m_itemList.Count; i++)
                _m_itemList[i]?.SetLocked(true);
        }

        private void onItemSelected(PartInfo selectedPart)
        {
            if (_m_finished)
                return;

            AudioMgr.instance.PlaySfx("sfx_buy");
            finishSelection(selectedPart);
        }

        private void onBtnSkipClickDown(PointerEventData data, object[] objs)
        {
            if (_m_finished)
                return;

            AudioMgr.instance.PlaySfx("sfx_click");
            closeWithoutSelection();
        }

        private void closeWithoutSelection()
        {
            pendingBattleWinHost?.OnPartSelectClosed();
            UICoreMgr.instance.CloseTopNode();
        }

        private void finishSelection(PartInfo selectedPart)
        {
            if (_m_finished || selectedPart == null)
                return;

            _m_finished = true;
            lockAllItems();

            if (mono.btnSkip != null)
                mono.btnSkip.interactable = false;

            GameModel.instance.playerInfo.bagPartInfoList.Add(selectedPart);

            var battleWinHost = pendingBattleWinHost;
            int optionIndex = pendingOptionIndex;
            pendingBattleWinHost = null;
            pendingOptionIndex = -1;
            battleWinHost?.OnBootySelected(optionIndex, selectedPart);
            UICoreMgr.instance.CloseTopNode();
        }
    }
}
