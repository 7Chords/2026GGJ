using GameCore;
using SCFrame;
using SCFrame.UI;

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
            GameModel.instance.PrepareRunEndSnapshot(true, ERunEndReason.BossBattle);
            refreshRunStats();
            GameBattleHistory.TryRecordPendingRunEndFromGameModel();
            GameModel.instance.ClearRunEndSnapshot();

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

        private void refreshRunStats()
        {
            GameModel gm = GameModel.instance;
            var entry = new GameBattleHistory.BattleHistoryEntry
            {
                battlesCleared = gm.RunBattlesCleared,
                eventsCleared = gm.RunEventsCleared,
                shopsCleared = gm.RunShopsCleared,
                strengthenCleared = gm.RunStrengthenCleared,
                totalGoldEarned = gm.RunTotalGoldEarned,
                totalDamageDealt = gm.RunTotalDamageDealt
            };

            if (mono.txtBattleCount != null)
                mono.txtBattleCount.text = GameBattleHistory.FormatBattlesClearedText(entry);
            if (mono.txtEventCount != null)
                mono.txtEventCount.text = GameBattleHistory.FormatEventsClearedText(entry);
            if (mono.txtShopCount != null)
                mono.txtShopCount.text = GameBattleHistory.FormatShopsClearedText(entry);
            if (mono.txtStrengthenCount != null)
                mono.txtStrengthenCount.text = GameBattleHistory.FormatStrengthenClearedText(entry);
            if (mono.txtTotalGold != null)
                mono.txtTotalGold.text = GameBattleHistory.FormatTotalGoldText(entry);
            if (mono.txtTotalDamage != null)
                mono.txtTotalDamage.text = GameBattleHistory.FormatTotalDamageText(entry);
        }
    }
}
