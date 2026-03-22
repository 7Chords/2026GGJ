using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Battle.Effects
{
    public class ClearDebuffEffectHandler : IPartEffectHandler
    {
        public void Execute(PartInfo _caster, EntryInfo _entry, PartEffectContext _ctx)
        {
            var battleCtx = BattleContext.current;
            if (battleCtx == null) return;

            int reduceLayer = Mathf.RoundToInt(_entry.attributeValueList[0]);
            var partInfoList = GameModel.instance.GetEntryRealTargetPartList(_caster, _entry, _ctx);
            if (partInfoList == null)
                return;

            foreach (var part in partInfoList)
            {
                battleCtx.ApplyReduceAllBuffLayerToPart(part, reduceLayer);
            }
        }
    }
}
