using SCFrame.UI;
using UnityEngine;

namespace GameCore.UI
{
    public class UIPanelBookEnemyMaskGrid : _ASCUIPanelBase<UIMonoBookEnemyMaskGrid>
    {
        private FaceGridInfo _m_faceGridInfo;

        public FaceGridInfo gridInfo => _m_faceGridInfo;

        public UIPanelBookEnemyMaskGrid(UIMonoBookEnemyMaskGrid mono, SCUIShowType showType) : base(mono, showType)
        {
        }

        public override void AfterInitialize()
        {
        }

        public override void BeforeDiscard()
        {
        }

        public override void OnHidePanel()
        {
        }

        public override void OnShowPanel()
        {
            ApplyDefaultVisual();
        }

        public void SetInfo(FaceGridInfo info)
        {
            _m_faceGridInfo = info;
            ApplyDefaultVisual();
        }

        public void ApplyDefaultVisual()
        {
            if (mono?.imgGrid != null)
                mono.imgGrid.color = mono.colorDefault;
        }

        public void SetDisable()
        {
            mono.canvasGroup.alpha = 0;
            mono.canvasGroup.interactable = false;
            mono.canvasGroup.blocksRaycasts = false;
        }

        public void SetOccupyHighlight()
        {
            mono.imgGrid.color = mono.colorOccupy;
        }

        public void SetEffectHighlight()
        {
            mono.imgGrid.color = mono.colorEffect;
        }

        public void SetOverlapHighlight()
        {
            mono.imgGrid.color = mono.colorOverlap;
        }

        public void ClearHighlight()
        {
            ApplyDefaultVisual();
        }
    }
}
