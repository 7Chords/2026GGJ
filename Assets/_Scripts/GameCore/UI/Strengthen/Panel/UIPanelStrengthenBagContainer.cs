using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SCFrame;
using SCFrame.UI;

namespace GameCore.UI
{
    public class UIPanelStrengthenBagContainer : UIPanelContainerBase<UIMonoCommonContainer, UIPanelStrengthenBagItem, UIMonoStrengthenBagItem>
    {
        public UIPanelStrengthenBagContainer(UIMonoCommonContainer _mono, SCUIShowType _showType = SCUIShowType.INTERNAL) : base(_mono, _showType)
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

        protected override UIPanelStrengthenBagItem creatItemPanel(UIMonoStrengthenBagItem _mono)
        {
            throw new System.NotImplementedException();
        }
    }
}
