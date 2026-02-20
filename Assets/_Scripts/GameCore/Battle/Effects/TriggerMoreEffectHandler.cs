using SCFrame;

namespace GameCore.Battle.Effects
{
    public class TriggerMoreEffectHandler : IPartEffectHandler
    {
        public void Execute(PartInfo _caster, EntryInfo _entry, PartEffectContext _ctx)
        {
            var battleCtx = BattleContext.current;
            if (battleCtx == null) return;

            int triggerMoreTimes = (int)_entry.attributeValue;
            var gridInfoList = _caster.isEnemyPart 
                ? GameModel.instance.enemyFaceGridInfoList 
                : GameModel.instance.playerFaceGridInfoList;
            var partInfoList = new System.Collections.Generic.List<PartInfo>();

            foreach (var pos in _caster.curEffectFacePosList)
            {
                var gridInfo = gridInfoList?.Find(x => x.pos == pos);
                if (gridInfo?.hasPart == true && gridInfo.ownerPart != null && !partInfoList.Contains(gridInfo.ownerPart))
                    partInfoList.Add(gridInfo.ownerPart);
            }

            for (int j = 0; j < triggerMoreTimes - 1; j++)
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
