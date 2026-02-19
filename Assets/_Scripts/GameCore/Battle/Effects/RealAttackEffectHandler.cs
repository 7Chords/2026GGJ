
namespace GameCore.Battle.Effects
{
    public class RealAttackEffectHandler : IPartEffectHandler
    {
        public void Execute(PartInfo caster, EntryInfo entry, PartEffectContext ctx)
        {
            PartLogicHandler.DealRealAttack(caster, entry);
        }
    }
}
