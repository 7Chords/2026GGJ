using SCFrame;
using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameCore.UI
{
    public class UIPanelStart : _ASCUIPanelBase<UIMonoStart>
    {
        public UIPanelStart(UIMonoStart _mono, SCUIShowType _showType) : base(_mono, _showType)
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
            mono.btnStart.RemoveClickDown(onBtnStartClickDown);
            //mono.btnSetting.RemoveClickDown(onBtnSettingClickDown);
            mono.btnExit.RemoveClickDown(onBtnExitClickDown);
        }

        public override void OnShowPanel()
        {
            mono.btnStart.AddMouseLeftClickDown(onBtnStartClickDown);
            //mono.btnSetting.AddMouseLeftClickDown(onBtnSettingClickDown);
            mono.btnExit.AddMouseLeftClickDown(onBtnExitClickDown);
        }

        private void onBtnExitClickDown(PointerEventData arg1, object[] arg2)
        {
            Application.Quit();
        }
        private void onBtnStartClickDown(PointerEventData arg1, object[] arg2)
        {
            UICoreMgr.instance.CloseTopNode();

            //todo:
        }
    }


}