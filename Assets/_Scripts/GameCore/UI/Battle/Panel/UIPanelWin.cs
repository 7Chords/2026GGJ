using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.UI
{
    public class UIPanelWin : _ASCUIPanelBase<UIMonoWin>
    {
        public UIPanelWin(UIMonoWin _mono, SCUIShowType _showType) : base(_mono, _showType)
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
            mono.btnExit.onClick.RemoveAllListeners();
        }

        public override void OnShowPanel()
        {
            mono.btnExit.onClick.AddListener(() =>
            {
                Application.Quit();
            });
        }
    }
}
