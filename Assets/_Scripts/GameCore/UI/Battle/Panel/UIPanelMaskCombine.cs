using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.UI
{
    public class UIPanelMaskCombine : _ASCUIPanelBase<UIMonoMaskCombine>
    {
        private UIPanelMaskCombinePartContainer _m_partContainer;
        public UIPanelMaskCombine(UIMonoMaskCombine _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_partContainer = new UIPanelMaskCombinePartContainer(mono.monoPartContainer);
        }

        public override void BeforeDiscard()
        {
            _m_partContainer?.Discard();
            _m_partContainer = null;
        }

        public override void OnHidePanel()
        {
        }

        public override void OnShowPanel()
        {
            refreshShow();
        }

        private void refreshShow()
        {
            _m_partContainer?.ShowPanel();
            _m_partContainer?.SetListInfo(GameModel.instance.partInfoList);
        }
    }
}
