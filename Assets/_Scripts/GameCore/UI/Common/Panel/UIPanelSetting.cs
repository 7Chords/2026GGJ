using GameCore;
using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SCFrame;
using UnityEngine.EventSystems;
using System;

namespace GameCore.UI
{
    public class UIPanelSetting : _ASCUIPanelBase<UIMonoSetting>
    {
        public UIPanelSetting(UIMonoSetting _mono, SCUIShowType _showType) : base(_mono, _showType)
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
            mono.sldMusic.onValueChanged.RemoveAllListeners();
            mono.sldSound.onValueChanged.RemoveAllListeners();
            mono.btnClose.RemoveClickDown(onBtnCloseClickDown);
            mono.btnReturnMain.RemoveClickDown(onBtnReturnMainClickDonw);
        }

        public override void OnShowPanel()
        {
            mono.sldMusic.onValueChanged.AddListener((value) =>
            {
                AudioMgr.instance.ChangeBgmVolume(value);
            });
            mono.sldSound.onValueChanged.AddListener((value) =>
            {
                AudioMgr.instance.ChangeSfxVolume(value);
            });
            mono.btnClose.AddMouseLeftClickDown(onBtnCloseClickDown);
            mono.btnReturnMain.AddMouseLeftClickDown(onBtnReturnMainClickDonw);
            mono.sldMusic.value = AudioMgr.instance.bgmVolumeFactor;
            mono.sldSound.value = AudioMgr.instance.sfxVolumeFactor;
        }

        private void onBtnReturnMainClickDonw(PointerEventData _data, object[] _objs)
        {
            AudioMgr.instance.PlaySfx("sfx_click");
            GameRunSave.SaveFromGameModel();
            TVSwitchTransition.Run(() =>
            {
                GameModel.instance.playerInfo.ClearPendingMapMove();
                GameModel.instance.SetAllPlayerPart2Bag();
                GameModel.instance.SetEnemyEmpty();
                UICoreMgr.instance.CloseTopNode();
                UICoreMgr.instance.RemoveAllNodes(SCUINodeFuncType.BATTLE);
                UICoreMgr.instance.AddNode(new UINodeStart(SCUIShowType.FULL));
            });
        }

        private void onBtnCloseClickDown(PointerEventData _data, object[] _objs)
        {
            AudioMgr.instance.PlaySfx("sfx_click");
            UICoreMgr.instance.CloseTopNode();
        }
    }
}
