namespace GameCore.Helpers
{
    /// <summary>
    /// Parts with very large max HP (e.g. 培养基): show "MAX" on UI, no current/max line.
    /// </summary>
    public static class PartHealthDisplay
    {
        public const int InfiniteDisplayMaxHpThreshold = 999;
        public const string MaxHpDisplayText = "MAX";

        public static bool UseInfiniteHpDisplay(int maxHealth) => maxHealth > InfiniteDisplayMaxHpThreshold;

        public static string FormatSlashLine(int currentHp, int maxHp) =>
            UseInfiniteHpDisplay(maxHp) ? MaxHpDisplayText : currentHp + "/" + maxHp;

        public static string FormatMaxOnly(int maxHp) =>
            UseInfiniteHpDisplay(maxHp) ? MaxHpDisplayText : maxHp.ToString();
    }
}
