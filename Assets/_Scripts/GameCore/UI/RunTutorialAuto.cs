using GameCore;
using SCFrame;
using SCFrame.UI;

namespace GameCore.UI
{

    public static class RunTutorialAuto
    {
        public static void TryShowBattleGuideFirstTimeInRun()
        {
            var gm = GameModel.instance;
            if (gm == null || gm.RunTutorialBattleAutoShown)
                return;
            gm.MarkRunTutorialBattleAutoShown();
            UICoreMgr.instance.AddNode(new UINodeGuideBattle(SCUIShowType.ADDITION));
            GameRunSave.SaveFromGameModel();
        }
    }
}
