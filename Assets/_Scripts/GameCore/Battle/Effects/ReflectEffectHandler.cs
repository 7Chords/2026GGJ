
namespace GameCore.Battle.Effects
{
    public class ReflectEffectHandler : IPartEffectHandler
    {
        public void Execute(PartInfo caster, EntryInfo entry, PartEffectContext ctx)
        {
            PartLogicHandler.DealReflect(caster, entry, ctx.SenderPart, ctx.Damage);
        }
    }
}
