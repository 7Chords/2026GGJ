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
    public class UIPanelHistoryItem : _ASCUIPanelBase<UIMonoHistoryItem>
    {
        private UIPanelCommonPartContainer _m_partContainer;
        private GameBattleHistory.BattleHistoryEntry _m_entry;
        private bool _m_expanded;
        private Coroutine _m_layoutRebuildRoutine;

        public UIPanelHistoryItem(UIMonoHistoryItem _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            if (mono.monoPartContainer != null)
                _m_partContainer = new UIPanelCommonPartContainer(mono.monoPartContainer, SCUIShowType.INTERNAL);
        }

        public override void BeforeDiscard()
        {
            cancelDeferredItemLayoutRebuild();
            _m_partContainer?.Discard();
            _m_partContainer = null;
        }

        public override void OnHidePanel()
        {
            cancelDeferredItemLayoutRebuild();
            ResetToCollapsed();
            if (mono.btnToggle != null)
                mono.btnToggle.RemoveClickDown(onBtnToggleClickDown);
            _m_partContainer?.HidePanel();
        }

        public override void OnShowPanel()
        {
            if (mono.btnToggle != null)
                mono.btnToggle.AddMouseLeftClickDown(onBtnToggleClickDown);
            refreshExpandedView(false);
            _m_partContainer?.ShowPanel();
        }

        public void SetInfo(GameBattleHistory.BattleHistoryEntry entry)
        {
            _m_entry = entry;
            ResetToCollapsed();
            refreshHeader();
            refreshExpandedView(false);
        }

        public void ResetToCollapsed()
        {
            cancelDeferredItemLayoutRebuild();
            _m_expanded = false;
            if (mono.goPartRoot != null)
                mono.goPartRoot.SetActive(false);
            if (mono.txtExpandHint != null)
                mono.txtExpandHint.text = "展开器官库";
        }

        public void RebuildItemLayout()
        {
            rebuildItemLayoutImmediate();
        }

        public void RebuildParentListLayout()
        {
            RectTransform itemRect = GetGameObject().GetComponent<RectTransform>();
            if (itemRect == null)
                return;

            RectTransform listRect = itemRect.parent as RectTransform;
            if (listRect == null)
                return;

            LayoutRebuilder.ForceRebuildLayoutImmediate(listRect);
            Canvas.ForceUpdateCanvases();
        }

        private void refreshHeader()
        {
            if (_m_entry == null)
                return;

            if (mono.txtResult != null)
                mono.txtResult.text = GameBattleHistory.FormatResultText(_m_entry);
            if (mono.txtTime != null)
                mono.txtTime.text = GameBattleHistory.FormatRecordedTime(_m_entry);

            string loseText = GameBattleHistory.FormatLoseLocation(_m_entry);
            if (mono.txtLoseLocation != null)
            {
                mono.txtLoseLocation.text = loseText;
                mono.txtLoseLocation.gameObject.SetActive(!_m_entry.isWin && !string.IsNullOrEmpty(loseText));
            }
        }

        private void refreshExpandedView(bool rebuildParentList)
        {
            cancelDeferredItemLayoutRebuild();

            if (mono.goPartRoot != null)
                mono.goPartRoot.SetActive(_m_expanded);
            if (mono.txtExpandHint != null)
                mono.txtExpandHint.text = _m_expanded ? "收起器官库" : "展开器官库";

            if (_m_expanded)
                _m_partContainer?.SetListInfo(GameBattleHistory.DeserializeEndParts(_m_entry));

            rebuildItemLayoutImmediate();

            if (rebuildParentList)
            {
                RebuildParentListLayout();
                if (_m_expanded)
                    scheduleDeferredItemLayoutRebuild(true);
            }
        }

        private void rebuildItemLayoutImmediate()
        {
            if (_m_expanded && mono.monoPartContainer != null && mono.monoPartContainer.layoutGroup != null)
            {
                RectTransform partLayoutRect = mono.monoPartContainer.layoutGroup.transform as RectTransform;
                if (partLayoutRect != null)
                    LayoutRebuilder.ForceRebuildLayoutImmediate(partLayoutRect);
            }

            if (_m_expanded && mono.goPartRoot != null)
            {
                RectTransform partRootRect = mono.goPartRoot.GetComponent<RectTransform>();
                if (partRootRect != null)
                    LayoutRebuilder.ForceRebuildLayoutImmediate(partRootRect);
            }

            RectTransform itemRect = GetGameObject().GetComponent<RectTransform>();
            if (itemRect == null)
                return;

            LayoutRebuilder.ForceRebuildLayoutImmediate(itemRect);
        }

        private void scheduleDeferredItemLayoutRebuild(bool rebuildParentList)
        {
            if (mono == null)
                return;

            _m_layoutRebuildRoutine = this.StartCoroutine(coDeferredItemLayoutRebuild(rebuildParentList));
        }

        private IEnumerator coDeferredItemLayoutRebuild(bool rebuildParentList)
        {
            yield return null;
            yield return new WaitForEndOfFrame();

            _m_layoutRebuildRoutine = null;
            if (!_m_expanded)
                yield break;

            rebuildItemLayoutImmediate();
            if (rebuildParentList)
                RebuildParentListLayout();
        }

        private void cancelDeferredItemLayoutRebuild()
        {
            if (_m_layoutRebuildRoutine == null)
                return;

            this.StopCoroutine(_m_layoutRebuildRoutine);
            _m_layoutRebuildRoutine = null;
        }

        private void onBtnToggleClickDown(PointerEventData _data, object[] _objs)
        {
            AudioMgr.instance.PlaySfx("sfx_click");
            _m_expanded = !_m_expanded;
            refreshExpandedView(true);
        }
    }
}
