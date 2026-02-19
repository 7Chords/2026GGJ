namespace GameCore.Battle.Effects
{
    public class DamageMultiplierEffectHandler : IPartEffectHandler
    {
        public void Execute(PartInfo caster, EntryInfo entry, PartEffectContext ctx)
        {
            PartLogicHandler.DealAttackMultiplier(caster, entry);
        }
    }
}
