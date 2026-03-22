using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Battle.Effects
{
    public class ClearSelfBleedAndHealSelfEffectHandler : IPartEffectHandler
    {
        public void Execute(PartInfo _caster, EntryInfo _entry, PartEffectContext _ctx)
        {
            var battleCtx = BattleContext.current;
            if (battleCtx == null) return;

            int healthBleedUnit = (int)_entry.attributeValueList[0];
            int healthAmount = (int)_entry.attributeValueList[1];

            var partInfoList = GameModel.instance.GetEntryRealTargetPartList(_caster, _entry, _ctx);
            if (partInfoList == null)
                return;

            int layer = 0;
            foreach (var part in partInfoList)
            {
                BuffInfo bleedBuffInfo = part.GetBuff(EBuffType.BLEED);
                if (bleedBuffInfo == null)
                    continue;
                layer += bleedBuffInfo.buffLayer;
                part.buffLogic.ClearBuff(EBuffType.BLEED);
            }
            layer /= healthBleedUnit;
            if(!_caster.isEnemyPart)
                battleCtx.ApplyHealToPlayer(layer * healthAmount);
            else
                battleCtx.ApplyHealToEnemy(layer * healthAmount);

        }
    }
}
