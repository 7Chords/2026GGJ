using SCFrame;

namespace GameCore.Battle.Effects
{
    public class PartLoseTurnEffectHandler : IPartEffectHandler
    {
        public void Execute(PartInfo _caster, EntryInfo _entry, PartEffectContext _ctx)
        {
            var battleCtx = BattleContext.current;
            if (battleCtx == null) return;

            var partInfoList = GameModel.instance.GetEntryRealTargetPartList(_caster, _entry, _ctx);
            if (partInfoList == null)
                return;

            foreach (var part in partInfoList)
            {
                SCMsgCenter.SendMsg(SCMsgConst.PART_TRIGGER_EFFECT, part, _caster);
                if (_caster.isEnemyPart)
                    battleCtx.RemovePlayerPartFromBattle(part);
                else
                    battleCtx.RemoveEnemyPartFromBattle(part);
            }
        }
    }
}
