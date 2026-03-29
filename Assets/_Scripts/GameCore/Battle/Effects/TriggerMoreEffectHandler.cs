using SCFrame;

namespace GameCore.Battle.Effects
{
    public class TriggerMoreEffectHandler : IPartEffectHandler
    {
        public void Execute(PartInfo _caster, EntryInfo _entry, PartEffectContext _ctx)
        {
            var battleCtx = BattleContext.current;
            if (battleCtx == null) return;

            int triggerMoreTimes = (int)_entry.attributeValueList[0];
            var partInfoList = GameModel.instance.GetEntryRealTargetPartList(_caster, _entry, _ctx);
            if (partInfoList == null)
                return;

            for (int j = 0; j < triggerMoreTimes; j++)
            {
                foreach (var part in partInfoList)
                {
                    SCMsgCenter.SendMsg(SCMsgConst.PART_TRIGGER_EFFECT, part, _caster);
                    int index = battleCtx.GetPartIndexInQueue(part, !_caster.isEnemyPart);
                    if (index >= 0)
                        battleCtx.InsertPartAtInQueue(!_caster.isEnemyPart, index, part);
                }
            }
        }
    }
}
