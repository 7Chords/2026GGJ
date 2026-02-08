using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.UI
{
    public class UIPanelMaskCombineFaceGrid : _ASCUIPanelBase<UIMonoMaskCombineFaceGrid>
    {

        private FaceGridInfo _m_info;
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
            _m_info = _info;
        }

        public void SetDisable()
        {
            mono.canvasGroup.alpha = 0;
            mono.canvasGroup.interactable = false;
            mono.canvasGroup.blocksRaycasts = false;
        }

    }

}
