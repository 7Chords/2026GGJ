using GameCore.Data;
using GameCore.Helpers;
using GameCore.RefData;
using SCFrame;
using SCFrame.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIPanelBookEnemyTurnLayoutItem : _ASCUIPanelBase<UIMonoBookEnemyTurnLayoutItem>
    {
        private UIPanelBookEnemyMaskPreview _m_maskPreview;

        public UIPanelBookEnemyTurnLayoutItem(UIMonoBookEnemyTurnLayoutItem mono, SCUIShowType showType) : base(mono, showType)
        {
        }

        public override void AfterInitialize()
        {
            if (mono.monoEnemyMask != null)
            {
                _m_maskPreview = new UIPanelBookEnemyMaskPreview(mono.monoEnemyMask, SCUIShowType.INTERNAL);
                _m_maskPreview.Initialize();
            }
        }

        public override void BeforeDiscard()
        {
            _m_maskPreview?.Discard();
            _m_maskPreview = null;
        }

        public override void OnHidePanel()
        {
            _m_maskPreview?.HidePanel();
        }

        public override void OnShowPanel()
        {
            _m_maskPreview?.ShowPanel();
        }

        public void SetInfo(
            EnemyRefObj enemyRef,
            EnemyBookPreviewHelper.TurnLayoutPreviewEntry entry,
            List<PartInfo> deckPool,
            EnemyLayoutPreset preset)
        {
            if (mono.txtTurnLabel != null)
                mono.txtTurnLabel.text = entry.label ?? "";

            List<PartInfo> faceParts = EnemyBookPreviewHelper.BuildFacePartsForLayout(entry.layout, deckPool);
            _m_maskPreview?.SetPreview(enemyRef, faceParts, preset);
        }

        public void RebuildLayout()
        {
            _m_maskPreview?.RebuildLayout();

            RectTransform itemRect = GetGameObject()?.GetComponent<RectTransform>();
            if (itemRect != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(itemRect);
        }
    }
}
