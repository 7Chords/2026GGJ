using DG.Tweening;
using GameCore;
using GameCore.RefData;
using SCFrame;
using SCFrame.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIPanelBook : _ASCUIPanelBase<UIMonoBook>
    {
        private UIPanelCommonPartContainer _m_partContainer;
        private TweenContainer _m_tweenContainer;
        private EPartType _m_curFilterType = EPartType.EYE;

        public UIPanelBook(UIMonoBook _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_tweenContainer = new TweenContainer();
            _m_partContainer = new UIPanelCommonPartContainer(mono.monoContainer, SCUIShowType.INTERNAL);
        }

        public override void BeforeDiscard()
        {
            _m_tweenContainer?.KillAllDoTween();
            _m_tweenContainer = null;
            _m_partContainer?.Discard();
            _m_partContainer = null;
        }

        public override void OnHidePanel()
        {
            if (mono.btnClose != null)
                mono.btnClose.RemoveClickDown(onBtnCloseClickDown);
            unbindFilterButton(mono.btnEye, onBtnEyeClickDown, onBtnEyeMouseEnter, onBtnEyeMouseExit);
            unbindFilterButton(mono.btnNose, onBtnNoseClickDown, onBtnNoseMouseEnter, onBtnNoseMouseExit);
            unbindFilterButton(mono.btnMouth, onBtnMouthClickDown, onBtnMouthMouseEnter, onBtnMouthMouseExit);
            unbindFilterButton(mono.btnSkin, onBtnSkinClickDown, onBtnSkinMouseEnter, onBtnSkinMouseExit);
            _m_partContainer?.HidePanel();
        }

        public override void OnShowPanel()
        {
            _m_curFilterType = EPartType.EYE;

            if (mono.btnClose != null)
                mono.btnClose.AddMouseLeftClickDown(onBtnCloseClickDown);
            bindFilterButton(mono.btnEye, onBtnEyeClickDown, onBtnEyeMouseEnter, onBtnEyeMouseExit);
            bindFilterButton(mono.btnNose, onBtnNoseClickDown, onBtnNoseMouseEnter, onBtnNoseMouseExit);
            bindFilterButton(mono.btnMouth, onBtnMouthClickDown, onBtnMouthMouseEnter, onBtnMouthMouseExit);
            bindFilterButton(mono.btnSkin, onBtnSkinClickDown, onBtnSkinMouseEnter, onBtnSkinMouseExit);

            _m_partContainer?.ShowPanel();
            refreshFilterButtonState();
            refreshPartList();
        }

        private void bindFilterButton(
            Button _btn,
            System.Action<PointerEventData, object[]> _click,
            System.Action<PointerEventData, object[]> _enter,
            System.Action<PointerEventData, object[]> _exit)
        {
            if (_btn == null)
                return;
            _btn.AddMouseLeftClickDown(_click);
            _btn.AddMouseEnter(_enter);
            _btn.AddMouseExit(_exit);
        }

        private void unbindFilterButton(
            Button _btn,
            System.Action<PointerEventData, object[]> _click,
            System.Action<PointerEventData, object[]> _enter,
            System.Action<PointerEventData, object[]> _exit)
        {
            if (_btn == null)
                return;
            _btn.RemoveClickDown(_click);
            _btn.RemoveMouseEnter(_enter);
            _btn.RemoveMouseExit(_exit);
        }

        private void onFilterButtonClick(EPartType _type)
        {
            if (_m_curFilterType == _type)
                return;
            AudioMgr.instance.PlaySfx("sfx_click");
            _m_curFilterType = _type;
            refreshFilterButtonState();
            refreshPartList();
        }

        private void refreshFilterButtonState()
        {
            setFilterButtonSelected(mono.btnEye, _m_curFilterType == EPartType.EYE);
            setFilterButtonSelected(mono.btnNose, _m_curFilterType == EPartType.NOSE);
            setFilterButtonSelected(mono.btnMouth, _m_curFilterType == EPartType.MOUTH);
            setFilterButtonSelected(mono.btnSkin, _m_curFilterType == EPartType.SKIN);
        }

        private static void setFilterButtonSelected(Button _btn, bool _selected)
        {
            if (_btn == null)
                return;
            _btn.interactable = !_selected;
        }

        private void refreshPartList()
        {
            _m_partContainer?.SetListInfo(buildBookPartList(_m_curFilterType));
        }

        private static List<PartInfo> buildBookPartList(EPartType _filterType)
        {
            var result = new List<PartInfo>();
            var partRefs = SCRefDataMgr.instance?.partRefList?.refDataList;
            if (partRefs == null)
                return result;

            for (int i = 0; i < partRefs.Count; i++)
            {
                PartRefObj partRef = partRefs[i];
                if (partRef == null || partRef.partType != _filterType)
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

        private static PartLevelRefObj findLowestLevelRowForPart(long _partId)
        {
            var rows = SCRefDataMgr.instance?.partLevelRefList?.refDataList;
            if (rows == null)
                return null;

            PartLevelRefObj best = null;
            for (int i = 0; i < rows.Count; i++)
            {
                PartLevelRefObj row = rows[i];
                if (row == null || row.partId != _partId)
                    continue;
                if (best == null || row.partLevel < best.partLevel)
                    best = row;
            }
            return best;
        }

        private void onBtnCloseClickDown(PointerEventData _data, object[] _objs)
        {
            AudioMgr.instance.PlaySfx("sfx_click");
            UICoreMgr.instance.CloseTopNode();
        }

        private void onBtnEyeClickDown(PointerEventData _data, object[] _objs) => onFilterButtonClick(EPartType.EYE);
        private void onBtnNoseClickDown(PointerEventData _data, object[] _objs) => onFilterButtonClick(EPartType.NOSE);
        private void onBtnMouthClickDown(PointerEventData _data, object[] _objs) => onFilterButtonClick(EPartType.MOUTH);
        private void onBtnSkinClickDown(PointerEventData _data, object[] _objs) => onFilterButtonClick(EPartType.SKIN);

        private void onBtnEyeMouseEnter(PointerEventData _data, object[] _objs) => onFilterButtonMouseEnter(mono.btnEye);
        private void onBtnEyeMouseExit(PointerEventData _data, object[] _objs) => onFilterButtonMouseExit(mono.btnEye);
        private void onBtnNoseMouseEnter(PointerEventData _data, object[] _objs) => onFilterButtonMouseEnter(mono.btnNose);
        private void onBtnNoseMouseExit(PointerEventData _data, object[] _objs) => onFilterButtonMouseExit(mono.btnNose);
        private void onBtnMouthMouseEnter(PointerEventData _data, object[] _objs) => onFilterButtonMouseEnter(mono.btnMouth);
        private void onBtnMouthMouseExit(PointerEventData _data, object[] _objs) => onFilterButtonMouseExit(mono.btnMouth);
        private void onBtnSkinMouseEnter(PointerEventData _data, object[] _objs) => onFilterButtonMouseEnter(mono.btnSkin);
        private void onBtnSkinMouseExit(PointerEventData _data, object[] _objs) => onFilterButtonMouseExit(mono.btnSkin);

        private void onFilterButtonMouseEnter(Button _btn)
        {
            if (_btn == null || !_btn.interactable)
                return;
            _m_tweenContainer?.RegDoTween(_btn.transform.DOScale(mono.btnEnterScale, mono.btnScaleChgTime));
        }

        private void onFilterButtonMouseExit(Button _btn)
        {
            if (_btn == null)
                return;
            _m_tweenContainer?.RegDoTween(_btn.transform.DOScale(Vector3.one, mono.btnScaleChgTime));
        }
    }
}
