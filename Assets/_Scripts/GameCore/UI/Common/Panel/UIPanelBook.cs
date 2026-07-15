using DG.Tweening;
using GameCore;
using GameCore.Helpers;
using GameCore.RefData;
using SCFrame;
using SCFrame.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameCore.UI
{
    public enum EBookCategory
    {
        None = 0,
        Part = 1,
        Enemy = 2,
    }

    public class UIPanelBook : _ASCUIPanelBase<UIMonoBook>
    {
        private UIPanelCommonPartContainer _m_partContainer;
        private readonly List<UIPanelBookEnemyItem> _m_enemyItemList = new List<UIPanelBookEnemyItem>();
        private TweenContainer _m_tweenContainer;
        private EBookCategory _m_curCategory = EBookCategory.Part;
        private bool _m_suppressToggleCallback;

        public UIPanelBook(UIMonoBook _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_tweenContainer = new TweenContainer();
            _m_partContainer = new UIPanelCommonPartContainer(mono.monoPartContainer, SCUIShowType.INTERNAL);
        }

        public override void BeforeDiscard()
        {
            _m_tweenContainer?.KillAllDoTween();
            _m_tweenContainer = null;
            _m_partContainer?.Discard();
            _m_partContainer = null;
            for (int i = 0; i < _m_enemyItemList.Count; i++)
                _m_enemyItemList[i]?.Discard();
            _m_enemyItemList.Clear();
        }

        public override void OnHidePanel()
        {
            if (mono.btnClose != null)
                mono.btnClose.RemoveClickDown(onBtnCloseClickDown);
            unbindCategoryButton(mono.btnPart, onBtnPartClickDown, onBtnPartMouseEnter, onBtnPartMouseExit);
            unbindCategoryButton(mono.btnEnemy, onBtnEnemyClickDown, onBtnEnemyMouseEnter, onBtnEnemyMouseExit);
            unbindPartFilterToggles();
            unbindEnemyFloorToggles();
            _m_partContainer?.HidePanel();
            hideEnemyItems();
            resetOnHide();
        }

        public override void OnShowPanel()
        {
            if (mono.btnClose != null)
                mono.btnClose.AddMouseLeftClickDown(onBtnCloseClickDown);
            bindCategoryButton(mono.btnPart, onBtnPartClickDown, onBtnPartMouseEnter, onBtnPartMouseExit);
            bindCategoryButton(mono.btnEnemy, onBtnEnemyClickDown, onBtnEnemyMouseEnter, onBtnEnemyMouseExit);
            bindPartFilterToggles();
            bindEnemyFloorToggles();

            showDefaultOpenState();
        }

        private void showDefaultOpenState()
        {
            _m_curCategory = EBookCategory.Part;

            if (mono.goIndexes != null)
                mono.goIndexes.SetActive(true);
            if (mono.goPageEnemy != null)
                mono.goPageEnemy.SetActive(false);

            refreshCategoryView();
        }

        private void resetOnHide()
        {
            _m_curCategory = EBookCategory.None;

            if (mono.goPagePart != null)
                mono.goPagePart.SetActive(false);
            if (mono.goPageEnemy != null)
                mono.goPageEnemy.SetActive(false);
        }

        private void selectCategory(EBookCategory category)
        {
            if (_m_curCategory == category)
                return;

            AudioMgr.instance.PlaySfx("sfx_click");
            _m_curCategory = category;
            refreshCategoryView();
        }

        private void refreshCategoryView()
        {
            refreshCategoryButtonState();

            bool showPartPage = _m_curCategory == EBookCategory.Part;
            bool showEnemyPage = _m_curCategory == EBookCategory.Enemy && hasEnemyBookPage();

            if (mono.goPagePart != null)
                mono.goPagePart.SetActive(showPartPage);
            if (mono.goPageEnemy != null)
                mono.goPageEnemy.SetActive(showEnemyPage);

            setPartFiltersVisible(showPartPage);
            setEnemyFloorFiltersVisible(showEnemyPage);

            if (showPartPage)
            {
                applyDefaultPartFilterToggles();
                _m_partContainer?.ShowPanel();
                hideEnemyItems();
                refreshPartList();
                return;
            }

            _m_partContainer?.HidePanel();
            if (showEnemyPage)
            {
                applyDefaultEnemyFloorToggles();
                refreshEnemyList();
            }
            else
                hideEnemyItems();
        }

        private bool hasEnemyFloorFilters()
        {
            return mono.toggleEnemyFloor1 != null || mono.toggleEnemyFloor2 != null;
        }

        private bool hasEnemyBookPage()
        {
            return mono.monoEnemyContainer != null;
        }

        private void setPartFiltersVisible(bool visible)
        {
            setToggleVisible(mono.toggleEye, visible);
            setToggleVisible(mono.toggleNose, visible);
            setToggleVisible(mono.toggleMouth, visible);
            setToggleVisible(mono.toggleSkin, visible);
            setToggleVisible(mono.toggleEnemyPart, visible);
        }

        private void setEnemyFloorFiltersVisible(bool visible)
        {
            if (!hasEnemyFloorFilters())
                return;

            setToggleVisible(mono.toggleEnemyFloor1, visible);
            setToggleVisible(mono.toggleEnemyFloor2, visible);
        }

        private static void setToggleVisible(Toggle toggle, bool visible)
        {
            if (toggle != null)
                toggle.gameObject.SetActive(visible);
        }

        private void refreshCategoryButtonState()
        {
            if (mono.btnPart != null)
                mono.btnPart.interactable = true;

            if (mono.btnEnemy != null)
                mono.btnEnemy.interactable = hasEnemyBookPage();
        }

        private void bindCategoryButton(
            Button btn,
            System.Action<PointerEventData, object[]> click,
            System.Action<PointerEventData, object[]> enter,
            System.Action<PointerEventData, object[]> exit)
        {
            if (btn == null)
                return;
            btn.AddMouseLeftClickDown(click);
            btn.AddMouseEnter(enter);
            btn.AddMouseExit(exit);
        }

        private void unbindCategoryButton(
            Button btn,
            System.Action<PointerEventData, object[]> click,
            System.Action<PointerEventData, object[]> enter,
            System.Action<PointerEventData, object[]> exit)
        {
            if (btn == null)
                return;
            btn.RemoveClickDown(click);
            btn.RemoveMouseEnter(enter);
            btn.RemoveMouseExit(exit);
        }

        private void bindPartFilterToggles()
        {
            bindFilterToggle(mono.toggleEye, onToggleEyeChanged, onToggleEyeMouseEnter, onToggleEyeMouseExit);
            bindFilterToggle(mono.toggleNose, onToggleNoseChanged, onToggleNoseMouseEnter, onToggleNoseMouseExit);
            bindFilterToggle(mono.toggleMouth, onToggleMouthChanged, onToggleMouthMouseEnter, onToggleMouthMouseExit);
            bindFilterToggle(mono.toggleSkin, onToggleSkinChanged, onToggleSkinMouseEnter, onToggleSkinMouseExit);
            bindFilterToggle(mono.toggleEnemyPart, onToggleEnemyPartChanged, onToggleEnemyPartMouseEnter, onToggleEnemyPartMouseExit);
        }

        private void unbindPartFilterToggles()
        {
            unbindFilterToggle(mono.toggleEye, onToggleEyeChanged, onToggleEyeMouseEnter, onToggleEyeMouseExit);
            unbindFilterToggle(mono.toggleNose, onToggleNoseChanged, onToggleNoseMouseEnter, onToggleNoseMouseExit);
            unbindFilterToggle(mono.toggleMouth, onToggleMouthChanged, onToggleMouthMouseEnter, onToggleMouthMouseExit);
            unbindFilterToggle(mono.toggleSkin, onToggleSkinChanged, onToggleSkinMouseEnter, onToggleSkinMouseExit);
            unbindFilterToggle(mono.toggleEnemyPart, onToggleEnemyPartChanged, onToggleEnemyPartMouseEnter, onToggleEnemyPartMouseExit);
        }

        private void bindEnemyFloorToggles()
        {
            bindFilterToggle(mono.toggleEnemyFloor1, onToggleEnemyFloor1Changed, onToggleEnemyFloor1MouseEnter, onToggleEnemyFloor1MouseExit);
            bindFilterToggle(mono.toggleEnemyFloor2, onToggleEnemyFloor2Changed, onToggleEnemyFloor2MouseEnter, onToggleEnemyFloor2MouseExit);
        }

        private void unbindEnemyFloorToggles()
        {
            unbindFilterToggle(mono.toggleEnemyFloor1, onToggleEnemyFloor1Changed, onToggleEnemyFloor1MouseEnter, onToggleEnemyFloor1MouseExit);
            unbindFilterToggle(mono.toggleEnemyFloor2, onToggleEnemyFloor2Changed, onToggleEnemyFloor2MouseEnter, onToggleEnemyFloor2MouseExit);
        }

        private void bindFilterToggle(
            Toggle toggle,
            UnityEngine.Events.UnityAction<bool> changed,
            System.Action<PointerEventData, object[]> enter,
            System.Action<PointerEventData, object[]> exit)
        {
            if (toggle == null)
                return;
            toggle.onValueChanged.AddListener(changed);
            toggle.AddMouseEnter(enter);
            toggle.AddMouseExit(exit);
        }

        private void unbindFilterToggle(
            Toggle toggle,
            UnityEngine.Events.UnityAction<bool> changed,
            System.Action<PointerEventData, object[]> enter,
            System.Action<PointerEventData, object[]> exit)
        {
            if (toggle == null)
                return;
            toggle.onValueChanged.RemoveListener(changed);
            toggle.RemoveMouseEnter(enter);
            toggle.RemoveMouseExit(exit);
        }

        private void applyDefaultPartFilterToggles()
        {
            _m_suppressToggleCallback = true;
            setToggleIsOn(mono.toggleEye, true);
            setToggleIsOn(mono.toggleNose, false);
            setToggleIsOn(mono.toggleMouth, false);
            setToggleIsOn(mono.toggleSkin, false);
            setToggleIsOn(mono.toggleEnemyPart, false);
            _m_suppressToggleCallback = false;
        }

        private void applyDefaultEnemyFloorToggles()
        {
            _m_suppressToggleCallback = true;
            setToggleIsOn(mono.toggleEnemyFloor1, true);
            setToggleIsOn(mono.toggleEnemyFloor2, false);
            _m_suppressToggleCallback = false;
        }

        private static void setToggleIsOn(Toggle toggle, bool isOn)
        {
            if (toggle == null)
                return;
            toggle.SetIsOnWithoutNotify(isOn);
        }

        private static bool isToggleOn(Toggle toggle)
        {
            return toggle != null && toggle.isOn;
        }

        private void onPartFilterChanged(bool isOn)
        {
            if (_m_suppressToggleCallback)
                return;
            if (_m_curCategory != EBookCategory.Part)
                return;

            AudioMgr.instance.PlaySfx("sfx_click");
            refreshPartList();
        }

        private void onEnemyFloorChanged(bool isOn)
        {
            if (_m_suppressToggleCallback)
                return;
            if (_m_curCategory != EBookCategory.Enemy)
                return;

            AudioMgr.instance.PlaySfx("sfx_click");
            refreshEnemyList();
        }

        private void refreshPartList()
        {
            if (_m_curCategory != EBookCategory.Part)
                return;

            bool includeEye = isToggleOn(mono.toggleEye);
            bool includeNose = isToggleOn(mono.toggleNose);
            bool includeMouth = isToggleOn(mono.toggleMouth);
            bool includeSkin = isToggleOn(mono.toggleSkin);
            bool includeEnemyPart = isToggleOn(mono.toggleEnemyPart);
            bool anySelected = includeEye || includeNose || includeMouth || includeSkin || includeEnemyPart;

            _m_partContainer?.SetListInfo(buildBookPartList(
                includeEye,
                includeNose,
                includeMouth,
                includeSkin,
                includeEnemyPart,
                anySelected));
        }

        private static List<PartInfo> buildBookPartList(
            bool includeEye,
            bool includeNose,
            bool includeMouth,
            bool includeSkin,
            bool includeEnemyPart,
            bool anySelected)
        {
            var result = new List<PartInfo>();
            var partRefs = SCRefDataMgr.instance?.partRefList?.refDataList;
            if (partRefs == null)
                return result;

            for (int i = 0; i < partRefs.Count; i++)
            {
                PartRefObj partRef = partRefs[i];
                if (partRef == null)
                    continue;

                if (!matchPartFilter(partRef, includeEye, includeNose, includeMouth, includeSkin, includeEnemyPart, anySelected))
                    continue;

                PartLevelRefObj levelRow = findLowestLevelRowForPart(partRef.id);
                if (levelRow == null)
                    continue;

                var info = new PartInfo(partRef, false, levelRow.partLevel);
                if (info.levelRefObj == null)
                    continue;
                result.Add(info);
            }
            return result;
        }

        private static bool matchPartFilter(
            PartRefObj partRef,
            bool includeEye,
            bool includeNose,
            bool includeMouth,
            bool includeSkin,
            bool includeEnemyPart,
            bool anySelected)
        {
            if (partRef.isEnemyPart)
                return anySelected ? includeEnemyPart : false;

            if (!anySelected)
                return false;

            switch (partRef.partType)
            {
                case EPartType.EYE:
                    return includeEye;
                case EPartType.NOSE:
                    return includeNose;
                case EPartType.MOUTH:
                    return includeMouth;
                case EPartType.SKIN:
                    return includeSkin;
                default:
                    return false;
            }
        }

        private static PartLevelRefObj findLowestLevelRowForPart(long partId)
        {
            var rows = SCRefDataMgr.instance?.partLevelRefList?.refDataList;
            if (rows == null)
                return null;

            PartLevelRefObj best = null;
            for (int i = 0; i < rows.Count; i++)
            {
                PartLevelRefObj row = rows[i];
                if (row == null || row.partId != partId)
                    continue;
                if (best == null || row.partLevel < best.partLevel)
                    best = row;
            }
            return best;
        }

        private void refreshEnemyList()
        {
            hideEnemyItems();

            if (mono.monoEnemyContainer == null || mono.monoEnemyContainer.layoutGroup == null)
                return;

            List<EnemyRefObj> enemyList = buildEnemyBookList();
            for (int i = 0; i < enemyList.Count; i++)
            {
                UIPanelBookEnemyItem itemPanel = getOrCreateEnemyItem(i);
                if (itemPanel == null)
                    continue;

                itemPanel.onSelected = onEnemyItemSelected;
                itemPanel.SetInfo(enemyList[i]);
                if (!itemPanel.hasShowed)
                    itemPanel.ShowPanel();
            }

            for (int i = enemyList.Count; i < _m_enemyItemList.Count; i++)
                _m_enemyItemList[i]?.HidePanel();
        }

        private List<EnemyRefObj> buildEnemyBookList()
        {
            bool includeFloor1 = isToggleOn(mono.toggleEnemyFloor1);
            bool includeFloor2 = isToggleOn(mono.toggleEnemyFloor2);
            bool anyFloorSelected = includeFloor1 || includeFloor2;

            // No floor toggle selected -> empty list; otherwise union of selected floors.
            if (!anyFloorSelected && hasEnemyFloorFilters())
                return new List<EnemyRefObj>();

            if (includeFloor1 && includeFloor2)
            {
                var merged = EnemyBookPreviewHelper.BuildSortedEnemyBookList(1);
                List<EnemyRefObj> floor2 = EnemyBookPreviewHelper.BuildSortedEnemyBookList(2);
                for (int i = 0; i < floor2.Count; i++)
                {
                    EnemyRefObj enemy = floor2[i];
                    if (enemy == null)
                        continue;
                    bool exists = false;
                    for (int j = 0; j < merged.Count; j++)
                    {
                        if (merged[j] != null && merged[j].id == enemy.id)
                        {
                            exists = true;
                            break;
                        }
                    }
                    if (!exists)
                        merged.Add(enemy);
                }
                return merged;
            }

            if (includeFloor1)
                return EnemyBookPreviewHelper.BuildSortedEnemyBookList(1);
            if (includeFloor2)
                return EnemyBookPreviewHelper.BuildSortedEnemyBookList(2);

            // No floor filters exist on UI: show all.
            return EnemyBookPreviewHelper.BuildSortedEnemyBookList(0);
        }

        private UIPanelBookEnemyItem getOrCreateEnemyItem(int index)
        {
            if (index < _m_enemyItemList.Count)
                return _m_enemyItemList[index];

            if (mono.monoEnemyContainer == null)
                return null;

            GameObject itemGO = ResourcesHelper.LoadGameObject(
                mono.monoEnemyContainer.prefabItemObjName,
                mono.monoEnemyContainer.layoutGroup.transform);
            if (itemGO == null)
                return null;

            UIMonoBookEnemyItem itemMono = itemGO.GetComponent<UIMonoBookEnemyItem>();
            if (itemMono == null)
            {
                Debug.LogError("prefab missing UIMonoBookEnemyItem: " + mono.monoEnemyContainer.prefabItemObjName);
                return null;
            }

            var itemPanel = new UIPanelBookEnemyItem(itemMono, SCUIShowType.INTERNAL);
            itemPanel.Initialize();
            _m_enemyItemList.Add(itemPanel);
            return itemPanel;
        }

        private void hideEnemyItems()
        {
            for (int i = 0; i < _m_enemyItemList.Count; i++)
                _m_enemyItemList[i]?.HidePanel();
        }

        private void onEnemyItemSelected(EnemyRefObj enemyRef)
        {
            if (enemyRef == null)
                return;

            AudioMgr.instance.PlaySfx("sfx_click");
            UICoreMgr.instance.AddNode(new UINodeBookEnemyDetail(SCUIShowType.ADDITION, enemyRef));
        }

        private void onBtnCloseClickDown(PointerEventData data, object[] objs)
        {
            AudioMgr.instance.PlaySfx("sfx_click");
            UICoreMgr.instance.CloseTopNode();
        }

        private void onBtnPartClickDown(PointerEventData data, object[] objs) => selectCategory(EBookCategory.Part);

        private void onBtnEnemyClickDown(PointerEventData data, object[] objs)
        {
            if (!hasEnemyBookPage())
                return;
            selectCategory(EBookCategory.Enemy);
        }

        private void onToggleEyeChanged(bool isOn) => onPartFilterChanged(isOn);
        private void onToggleNoseChanged(bool isOn) => onPartFilterChanged(isOn);
        private void onToggleMouthChanged(bool isOn) => onPartFilterChanged(isOn);
        private void onToggleSkinChanged(bool isOn) => onPartFilterChanged(isOn);
        private void onToggleEnemyPartChanged(bool isOn) => onPartFilterChanged(isOn);
        private void onToggleEnemyFloor1Changed(bool isOn) => onEnemyFloorChanged(isOn);
        private void onToggleEnemyFloor2Changed(bool isOn) => onEnemyFloorChanged(isOn);

        private void onBtnPartMouseEnter(PointerEventData data, object[] objs) => onHoverEnter(mono.btnPart);
        private void onBtnPartMouseExit(PointerEventData data, object[] objs) => onHoverExit(mono.btnPart);
        private void onBtnEnemyMouseEnter(PointerEventData data, object[] objs) => onHoverEnter(mono.btnEnemy);
        private void onBtnEnemyMouseExit(PointerEventData data, object[] objs) => onHoverExit(mono.btnEnemy);

        private void onToggleEyeMouseEnter(PointerEventData data, object[] objs) => onHoverEnter(mono.toggleEye);
        private void onToggleEyeMouseExit(PointerEventData data, object[] objs) => onHoverExit(mono.toggleEye);
        private void onToggleNoseMouseEnter(PointerEventData data, object[] objs) => onHoverEnter(mono.toggleNose);
        private void onToggleNoseMouseExit(PointerEventData data, object[] objs) => onHoverExit(mono.toggleNose);
        private void onToggleMouthMouseEnter(PointerEventData data, object[] objs) => onHoverEnter(mono.toggleMouth);
        private void onToggleMouthMouseExit(PointerEventData data, object[] objs) => onHoverExit(mono.toggleMouth);
        private void onToggleSkinMouseEnter(PointerEventData data, object[] objs) => onHoverEnter(mono.toggleSkin);
        private void onToggleSkinMouseExit(PointerEventData data, object[] objs) => onHoverExit(mono.toggleSkin);
        private void onToggleEnemyPartMouseEnter(PointerEventData data, object[] objs) => onHoverEnter(mono.toggleEnemyPart);
        private void onToggleEnemyPartMouseExit(PointerEventData data, object[] objs) => onHoverExit(mono.toggleEnemyPart);
        private void onToggleEnemyFloor1MouseEnter(PointerEventData data, object[] objs) => onHoverEnter(mono.toggleEnemyFloor1);
        private void onToggleEnemyFloor1MouseExit(PointerEventData data, object[] objs) => onHoverExit(mono.toggleEnemyFloor1);
        private void onToggleEnemyFloor2MouseEnter(PointerEventData data, object[] objs) => onHoverEnter(mono.toggleEnemyFloor2);
        private void onToggleEnemyFloor2MouseExit(PointerEventData data, object[] objs) => onHoverExit(mono.toggleEnemyFloor2);

        private void onHoverEnter(Selectable selectable)
        {
            if (selectable == null || !selectable.interactable)
                return;
            _m_tweenContainer?.RegDoTween(selectable.transform.DOScale(mono.btnEnterScale, mono.btnScaleChgTime));
        }

        private void onHoverExit(Selectable selectable)
        {
            if (selectable == null)
                return;
            _m_tweenContainer?.RegDoTween(selectable.transform.DOScale(Vector3.one, mono.btnScaleChgTime));
        }
    }
}
