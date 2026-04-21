namespace GameCore
{
    /// <summary> Per-battle runtime for enemy passives (owned by EnemyInfo). </summary>
    public sealed class EnemyPassiveBattleState
    {
        public int bodyDamageAccumulator;
        public int enemyPhaseCounter;
        public EEnemyOutgoingPartDamageMod outgoingPartDamageMod;
        public int outgoingPreyBonus;
        public int outgoingPartPenalty;
        public bool germsAtTheReadyApplied;
    }
}
