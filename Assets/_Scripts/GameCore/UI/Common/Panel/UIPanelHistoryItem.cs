using GameCore;
using SCFrame;
using SCFrame.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameCore.UI
{
    public class UIPanelHistoryItem : _ASCUIPanelBase<UIMonoHistoryItem>
    {
        private UIPanelCommonPartContainer _m_partContainer;
        private GameBattleHistory.BattleHistoryEntry _m_entry;
        private bool _m_expanded;

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
            _m_partContainer?.Discard();
            _m_partContainer = null;
        }

        public override void OnHidePanel()
        {
            if (mono.btnToggle != null)
                mono.btnToggle.RemoveClickDown(onBtnToggleClickDown);
            _m_partContainer?.HidePanel();
        }

        public override void OnShowPanel()
        {
            if (mono.btnToggle != null)
                mono.btnToggle.AddMouseLeftClickDown(onBtnToggleClickDown);
            refreshExpandedView();
            _m_partContainer?.ShowPanel();
        }

        public void SetInfo(GameBattleHistory.BattleHistoryEntry entry)
        {
            _m_entry = entry;
            _m_expanded = false;
            refreshHeader();
            refreshExpandedView();
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

        private void refreshExpandedView()
        {
            if (mono.goPartRoot != null)
                mono.goPartRoot.SetActive(_m_expanded);
            if (mono.txtExpandHint != null)
                mono.txtExpandHint.text = _m_expanded ? "收起器官库" : "展开器官库";

            RectTransform itemRect = GetGameObject().GetComponent<RectTransform>();
            if (itemRect != null)
                itemRect.sizeDelta = new Vector2(itemRect.sizeDelta.x, _m_expanded ? 360f : 120f);

            if (!_m_expanded)
                return;

            _m_partContainer?.SetListInfo(GameBattleHistory.DeserializeEndParts(_m_entry));
        }

        private void onBtnToggleClickDown(PointerEventData _data, object[] _objs)
        {
            AudioMgr.instance.PlaySfx("sfx_click");
            _m_expanded = !_m_expanded;
            refreshExpandedView();
        }
    }
}
