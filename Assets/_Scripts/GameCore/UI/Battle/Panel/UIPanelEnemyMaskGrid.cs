using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GameCore.UI
{
    public class UIPanelEnemyMaskGrid : _ASCUIPanelBase<UIMonoEnemyMaskGrid>
    {
        private FaceGridInfo _m_faceGridInfo;
        public FaceGridInfo gridInfo => _m_faceGridInfo;
        public UIPanelEnemyMaskGrid(UIMonoEnemyMaskGrid _mono, SCUIShowType _showType) : base(_mono, _showType)
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
            _m_faceGridInfo = _info;
        }
        public void SetDisable()
        {
            mono.canvasGroup.alpha = 0;
            mono.canvasGroup.interactable = false;
            mono.canvasGroup.blocksRaycasts = false;
        }
        public void SetOccupyPreview(bool _canPlace)
        {
            mono.imgGrid.color = _canPlace ? mono.colorCanPlace : mono.colorCanNotPlace;
        }

        public void SetEffectPreview()
        {
            mono.imgGrid.color = mono.colorIsEffective;
        }

        public void SetOverlapPreview()
        {
            mono.imgGrid.color = mono.colorOverlap;
        }

        public void SetNoPreview()
        {
            mono.imgGrid.color = mono.colorDefault;
        }
    }
}
