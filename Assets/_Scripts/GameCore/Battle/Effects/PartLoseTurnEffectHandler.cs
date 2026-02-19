namespace GameCore.Battle.Effects
{
    public class PartLoseTurnEffectHandler : IPartEffectHandler
    {
        public void Execute(PartInfo caster, EntryInfo entry, PartEffectContext ctx)
        {
            PartLogicHandler.DealPartLoseTurn(caster, entry);
        }
    }
}
