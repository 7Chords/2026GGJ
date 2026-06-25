namespace GameCore
{
    public partial class GameModel
    {
        public int RunBattlesCleared { get; private set; }
        public int RunEventsCleared { get; private set; }
        public int RunShopsCleared { get; private set; }
        public int RunStrengthenCleared { get; private set; }
        public int RunTotalGoldEarned { get; private set; }
        public int RunTotalDamageDealt { get; private set; }

        public void ResetRunStatistics()
        {
            RunBattlesCleared = 0;
            RunEventsCleared = 0;
            RunShopsCleared = 0;
            RunStrengthenCleared = 0;
            RunTotalGoldEarned = 0;
            RunTotalDamageDealt = 0;
        }

        public void OnEncounterCleared()
        {
            switch (RunEncounterRoomType)
            {
                case ERoomType.ENEMY:
                case ERoomType.BOSS:
                    RunBattlesCleared++;
                    break;
                case ERoomType.EVENT:
                    RunEventsCleared++;
                    break;
                case ERoomType.SHOP:
                    RunShopsCleared++;
                    break;
                case ERoomType.STRENGTHEN:
                    RunStrengthenCleared++;
                    break;
            }

            RunEncounterRoomType = ERoomType.NONE;
        }

        public void AddRunGoldEarned(int amount)
        {
            if (amount > 0)
                RunTotalGoldEarned += amount;
        }

        public void AddRunDamageDealt(int amount)
        {
            if (amount > 0)
                RunTotalDamageDealt += amount;
        }
    }
}
