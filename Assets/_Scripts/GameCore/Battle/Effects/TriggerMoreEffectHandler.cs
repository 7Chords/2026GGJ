
namespace GameCore.Battle.Effects
{
    public class TriggerMoreEffectHandler : IPartEffectHandler
    {
        public void Execute(PartInfo caster, EntryInfo entry, PartEffectContext ctx)
        {
            PartLogicHandler.DealTriggerMore(caster, entry);
        }
    }
}
