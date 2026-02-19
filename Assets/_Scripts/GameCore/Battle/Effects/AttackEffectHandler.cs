namespace GameCore.Battle.Effects
{
    public class AttackEffectHandler : IPartEffectHandler
    {
        public void Execute(PartInfo caster, EntryInfo entry, PartEffectContext ctx)
        {
            PartLogicHandler.DealAttack(caster, entry);
        }
    }
}
