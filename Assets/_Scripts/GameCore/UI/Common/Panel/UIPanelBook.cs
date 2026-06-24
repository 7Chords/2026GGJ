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
        private EPartType _m_curFilterType = EPartType.EYE;

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
            unbindFilterButton(mono.btnEye, onBtnEyeClickDown, onBtnEyeMouseEnter, onBtnEyeMouseExit);
            unbindFilterButton(mono.btnNose, onBtnNoseClickDown, onBtnNoseMouseEnter, onBtnNoseMouseExit);
            unbindFilterButton(mono.btnMouth, onBtnMouthClickDown, onBtnMouthMouseEnter, onBtnMouthMouseExit);
            unbindFilterButton(mono.btnSkin, onBtnSkinClickDown, onBtnSkinMouseEnter, onBtnSkinMouseExit);
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
            bindFilterButton(mono.btnEye, onBtnEyeClickDown, onBtnEyeMouseEnter, onBtnEyeMouseExit);
            bindFilterButton(mono.btnNose, onBtnNoseClickDown, onBtnNoseMouseEnter, onBtnNoseMouseExit);
            bindFilterButton(mono.btnMouth, onBtnMouthClickDown, onBtnMouthMouseEnter, onBtnMouthMouseExit);
            bindFilterButton(mono.btnSkin, onBtnSkinClickDown, onBtnSkinMouseEnter, onBtnSkinMouseExit);

            showDefaultOpenState();
        }

        private void showDefaultOpenState()
        {
            _m_curCategory = EBookCategory.Part;
            _m_curFilterType = EPartType.EYE;

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

            if (showPartPage)
            {
                _m_curFilterType = EPartType.EYE;
                _m_partContainer?.ShowPanel();
                hideEnemyItems();
                refreshPartList();
                return;
            }

            _m_partContainer?.HidePanel();
            if (showEnemyPage)
                refreshEnemyList();
            else
                hideEnemyItems();
        }

        private bool hasEnemyBookPage()
        {
            return mono.monoEnemyContainer != null;
        }

        private void setPartFiltersVisible(bool visible)
        {
            if (mono.btnEye != null)
                mono.btnEye.gameObject.SetActive(visible);
            if (mono.btnNose != null)
                mono.btnNose.gameObject.SetActive(visible);
            if (mono.btnMouth != null)
                mono.btnMouth.gameObject.SetActive(visible);
            if (mono.btnSkin != null)
                mono.btnSkin.gameObject.SetActive(visible);
        }

        private void refreshCategoryButtonState()
        {
            if (mono.btnPart != null)
                mono.btnPart.interactable = true;

            if (mono.btnEnemy != null)
                mono.btnEnemy.interactable = hasEnemyBookPage();

            if (mono.btnEye != null)
                mono.btnEye.interactable = true;
            if (mono.btnNose != null)
                mono.btnNose.interactable = true;
            if (mono.btnMouth != null)
                mono.btnMouth.interactable = true;
            if (mono.btnSkin != null)
                mono.btnSkin.interactable = true;
        }

        private void bindCategoryButton(
            Button btn,
            System.Action<PointerEventData, object[]> click,
            System.Action<PointerEventData, object[]> enter,
            System.Action<PointerEventData, object[]> exit)
        {
            bindFilterButton(btn, click, enter, exit);
        }

        private void unbindCategoryButton(
            Button btn,
            System.Action<PointerEventData, object[]> click,
            System.Action<PointerEventData, object[]> enter,
            System.Action<PointerEventData, object[]> exit)
        {
            unbindFilterButton(btn, click, enter, exit);
        }

        private void bindFilterButton(
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

        private void unbindFilterButton(
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

        private void onFilterButtonClick(EPartType type)
        {
            if (_m_curCategory != EBookCategory.Part)
                return;
            if (_m_curFilterType == type)
                return;

            AudioMgr.instance.PlaySfx("sfx_click");
            _m_curFilterType = type;
            refreshPartList();
        }

        private void refreshPartList()
        {
            if (_m_curCategory != EBookCategory.Part)
                return;

            _m_partContainer?.SetListInfo(buildBookPartList(_m_curFilterType));
        }

        private static List<PartInfo> buildBookPartList(EPartType filterType)
        {
            var result = new List<PartInfo>();
            var partRefs = SCRefDataMgr.instance?.partRefList?.refDataList;
            if (partRefs == null)
                return result;

            for (int i = 0; i < partRefs.Count; i++)
            {
                PartRefObj partRef = partRefs[i];
                if (partRef == null || partRef.partType != filterType || partRef.isEnemyPart)
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

            List<EnemyRefObj> enemyList = EnemyBookPreviewHelper.BuildSortedEnemyBookList();
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

        private void onBtnEyeClickDown(PointerEventData data, object[] objs) => onFilterButtonClick(EPartType.EYE);
        private void onBtnNoseClickDown(PointerEventData data, object[] objs) => onFilterButtonClick(EPartType.NOSE);
        private void onBtnMouthClickDown(PointerEventData data, object[] objs) => onFilterButtonClick(EPartType.MOUTH);
        private void onBtnSkinClickDown(PointerEventData data, object[] objs) => onFilterButtonClick(EPartType.SKIN);

        private void onBtnPartMouseEnter(PointerEventData data, object[] objs) => onCategoryButtonMouseEnter(mono.btnPart);
        private void onBtnPartMouseExit(PointerEventData data, object[] objs) => onCategoryButtonMouseExit(mono.btnPart);
        private void onBtnEnemyMouseEnter(PointerEventData data, object[] objs) => onCategoryButtonMouseEnter(mono.btnEnemy);
        private void onBtnEnemyMouseExit(PointerEventData data, object[] objs) => onCategoryButtonMouseExit(mono.btnEnemy);

        private void onBtnEyeMouseEnter(PointerEventData data, object[] objs) => onFilterButtonMouseEnter(mono.btnEye);
        private void onBtnEyeMouseExit(PointerEventData data, object[] objs) => onFilterButtonMouseExit(mono.btnEye);
        private void onBtnNoseMouseEnter(PointerEventData data, object[] objs) => onFilterButtonMouseEnter(mono.btnNose);
        private void onBtnNoseMouseExit(PointerEventData data, object[] objs) => onFilterButtonMouseExit(mono.btnNose);
        private void onBtnMouthMouseEnter(PointerEventData data, object[] objs) => onFilterButtonMouseEnter(mono.btnMouth);
        private void onBtnMouthMouseExit(PointerEventData data, object[] objs) => onFilterButtonMouseExit(mono.btnMouth);
        private void onBtnSkinMouseEnter(PointerEventData data, object[] objs) => onFilterButtonMouseEnter(mono.btnSkin);
        private void onBtnSkinMouseExit(PointerEventData data, object[] objs) => onFilterButtonMouseExit(mono.btnSkin);

        private void onCategoryButtonMouseEnter(Button btn) => onFilterButtonMouseEnter(btn);

        private void onCategoryButtonMouseExit(Button btn) => onFilterButtonMouseExit(btn);

        private void onFilterButtonMouseEnter(Button btn)
        {
            if (btn == null || !btn.interactable)
                return;
            _m_tweenContainer?.RegDoTween(btn.transform.DOScale(mono.btnEnterScale, mono.btnScaleChgTime));
        }

        private void onFilterButtonMouseExit(Button btn)
        {
            if (btn == null)
                return;
            _m_tweenContainer?.RegDoTween(btn.transform.DOScale(Vector3.one, mono.btnScaleChgTime));
        }
    }
}
