using SCFrame;

namespace GameCore.Battle.Effects
{
    public class DamageMultiplierEffectHandler : IPartEffectHandler
    {
        public void Execute(PartInfo _caster, EntryInfo _entry, PartEffectContext _ctx)
        {
            var battleCtx = BattleContext.current;
            if (battleCtx == null) return;

            float multiplier = _entry.attributeValueList[0];
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

            foreach (var part in partInfoList)
            {
                if (part.partRefObj.partType == EPartType.MOUTH)
                {
                    var attackEntry = part.entryInfoList.Find(x => 
                        x.attributeType == EAttributeType.ATTACK || x.attributeType == EAttributeType.REAL_ATTACK);
                    if (attackEntry != null)
                    {
                        SCMsgCenter.SendMsg(SCMsgConst.PART_TRIGGER_EFFECT, part, _caster);
                        attackEntry.attributeValueList[0] *= multiplier;
                    }
                }
            }
        }
    }
}
