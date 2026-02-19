namespace GameCore.Battle.Effects
{
    public class HealPartEffectHandler : IPartEffectHandler
    {
        public void Execute(PartInfo caster, EntryInfo entry, PartEffectContext ctx)
        {
            PartLogicHandler.DealHealPart(caster, entry);
        }
    }
}
