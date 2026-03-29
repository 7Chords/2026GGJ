using GameCore;

namespace GameCore.Battle.Effects
{
    /// <summary>
    /// CLEAR_ALL_HATE_AND_MOUTH_GET_STRONG: strip all HATE from ally parts in the effect area, then each mouth in that area gains
    /// (totalHateConsumed / hatePerGrant) * strongPerGrant layers of STRONG. Params: [0]=hatePerGrant, [1]=strongPerGrant. Uses <see cref="GameConst.BUFF_ID_STRONG"/>.
    /// </summary>
    public class ClearAllHateAndMouthGetStrongEffectHandler : IPartEffectHandler
    {
        public void Execute(PartInfo _caster, EntryInfo _entry, PartEffectContext _ctx)
        {
            var battleCtx = BattleContext.current;
            if (battleCtx == null)
                return;

            if (_entry.attributeValueList == null || _entry.attributeValueList.Count < 2)
                return;

            int hatePerGrant = (int)_entry.attributeValueList[0];
            int strongPerGrant = (int)_entry.attributeValueList[1];

            var partInfoList = GameModel.instance.GetEntryRealTargetPartList(_caster, _entry, _ctx);
            if (partInfoList == null || partInfoList.Count == 0)
                return;

            int totalHate = 0;
            for (int i = 0; i < partInfoList.Count; i++)
            {
                var part = partInfoList[i];
                if (part?.buffLogic == null)
                    continue;
                var hate = part.GetBuff(EBuffType.HATE);
                if (hate == null)
                    continue;
                totalHate += hate.buffLayer;
                part.buffLogic.ClearBuff(EBuffType.HATE);
            }

            if (hatePerGrant <= 0 || totalHate <= 0)
                return;

            int strongLayers = totalHate / hatePerGrant * strongPerGrant;
            if (strongLayers == 0)
                return;

            for (int i = 0; i < partInfoList.Count; i++)
            {
                var part = partInfoList[i];
                if (part?.partRefObj == null || part.partRefObj.partType != EPartType.MOUTH)
                    continue;
                battleCtx.ApplyBuffToPart(part, _caster, GameConst.BUFF_ID_STRONG, strongLayers);
            }
        }
    }
}
