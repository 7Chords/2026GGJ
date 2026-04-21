using GameCore;

namespace GameCore.Battle.Effects
{
    public class TriggerMaxMassEffectHandler : IPartEffectHandler
    {
        public void Execute(PartInfo _caster, EntryInfo _entry, PartEffectContext _ctx)
        {
            if (_caster == null) return;
            GermMassBuffEffects.RunTriggerMaxMassNonBreeding(_caster);
        }
    }
}
