using GameCore;
using SCFrame;
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
            mono.btnReturnMain.onClick.RemoveAllListeners();
        }

        public override void OnShowPanel()
        {
            mono.btnReturnMain.onClick.AddListener(() =>
            {
                AudioMgr.instance.PlaySfx("sfx_click");
                TVSwitchTransition.Run(() =>
                {
                    GameModel.instance.ResetRunForNewGame();
                    GameRunSave.DeleteSave();
                    MapGenerator.GetOrFind()?.GenerateMapDataOnly();
                    UICoreMgr.instance.RemoveAllNodes();
                    UICoreMgr.instance.AddNode(new UINodeStart(SCUIShowType.FULL));
                });
            });
        }
    }
}
