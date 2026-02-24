using UnityEngine;

namespace GameCore.Battle.Effects
{
    public class HealPartEffectHandler : IPartEffectHandler
    {
        public void Execute(PartInfo _caster, EntryInfo _entry, PartEffectContext _ctx)
        {
            var battleCtx = BattleContext.current;
            if (battleCtx == null) return;

            int healAmount = Mathf.RoundToInt(_entry.attributeValueList[0]);
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
                battleCtx.ApplyHealToPart(part, healAmount);
        }
    }
}
