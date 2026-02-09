using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.UI
{
    public class UIPanelMaskCombineFaceGrid : _ASCUIPanelBase<UIMonoMaskCombineFaceGrid>
    {

        private FaceGridInfo _m_gridInfo;
        public FaceGridInfo gridInfo => _m_gridInfo;
        public UIPanelMaskCombineFaceGrid(UIMonoMaskCombineFaceGrid _mono, SCUIShowType _showType) : base(_mono, _showType)
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

        }

        public void SetInfo(FaceGridInfo _info)
        {
            _m_gridInfo = _info;
        }

        public void SetDisable()
        {
            mono.canvasGroup.alpha = 0;
            mono.canvasGroup.interactable = false;
            mono.canvasGroup.blocksRaycasts = false;
        }

        public void SetOccupyPreview(bool _canPlace)
        {
            mono.imgGrid.color = _canPlace?mono.colorCanPlace:mono.colorCanNotPlace;
        }

        public void SetEffectPreview()
        {
            mono.imgGrid.color = mono.colorIsEffective;
        }

        public void SetNoPreview()
        {
            mono.imgGrid.color = mono.colorDefault;
        }

    }

}
