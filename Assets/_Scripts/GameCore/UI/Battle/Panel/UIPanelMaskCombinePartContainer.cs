using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.UI
{
    public class UIPanelMaskCombinePartContainer : UIPanelContainerBase<UIMonoCommonContainer, UIPanelMaskCombinePartContainerItem, UIMonoMaskCombinePartContainerItem>
    {
        public UIPanelMaskCombinePartContainer(UIMonoCommonContainer _mono, SCUIShowType _showType = SCUIShowType.INTERNAL) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            throw new System.NotImplementedException();
        }

        public override void BeforeDiscard()
        {
            throw new System.NotImplementedException();
        }

        public override void OnHidePanel()
        {
            throw new System.NotImplementedException();
        }

        public override void OnShowPanel()
        {
            throw new System.NotImplementedException();
        }

        protected override GameObject creatItemGO()
        {
            throw new System.NotImplementedException();
        }


        protected override UIPanelMaskCombinePartContainerItem creatItemPanel(UIMonoMaskCombinePartContainerItem _mono)
        {
            throw new System.NotImplementedException();
        }
    }
}
