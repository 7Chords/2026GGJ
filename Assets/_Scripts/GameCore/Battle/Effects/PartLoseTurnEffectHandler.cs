using SCFrame;

namespace GameCore.Battle.Effects
{
    public class PartLoseTurnEffectHandler : IPartEffectHandler
    {
        public void Execute(PartInfo _caster, EntryInfo _entry, PartEffectContext _ctx)
        {
            var battleCtx = BattleContext.current;
            if (battleCtx == null) return;

            var gridInfoList = _caster.isEnemyPart 
                ? GameModel.instance.playerFaceGridInfoList
                : GameModel.instance.enemyFaceGridInfoList;
            var partInfoList = new System.Collections.Generic.List<PartInfo>();

            foreach (var pos in _caster.curEffectFacePosList)
            {
                var gridInfo = gridInfoList?.Find(x => x.pos == pos);
                if (gridInfo?.hasPart == true && gridInfo.ownerPart != null && !partInfoList.Contains(gridInfo.ownerPart))
                    partInfoList.Add(gridInfo.ownerPart);
            }

            foreach (var part in partInfoList)
            {
                SCMsgCenter.SendMsg(SCMsgConst.PART_TRIGGER_EFFECT, part, _caster);
                if (part.isEnemyPart)
                    battleCtx.RemovePlayerPartFromBattle(part);
                else
                    battleCtx.RemoveEnemyPartFromBattle(part);
            }
        }
    }
}
