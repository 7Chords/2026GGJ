using GameCore;

namespace GameCore.Battle.Effects
{
    /// <summary>
    /// ENEMY_MOUTH_GET_BUFF: 与 ENEMY_GET_BUFF 相同目标范围，但仅对敌方「嘴巴」部位上 buff。
    /// attributeValueList[0]=buffId, [1]=layer
    /// </summary>
    public class EnemyMouthGetBuffEffectHandler : IPartEffectHandler
    {
        public void Execute(PartInfo _caster, EntryInfo _entry, PartEffectContext _ctx)
        {
            var battleCtx = BattleContext.current;
            if (battleCtx == null) return;

            long buffId = (long)_entry.attributeValueList[0];
            int buffLayer = (int)_entry.attributeValueList[1];

            var partInfoList = GameModel.instance.GetEntryRealTargetPartList(_caster, _entry, _ctx);
            if (partInfoList == null)
                return;

            for (int i = 0; i < partInfoList.Count; i++)
            {
                var part = partInfoList[i];
                if (part == null || part.partRefObj == null)
                    continue;
                if (part.partRefObj.partType != EPartType.MOUTH)
                    continue;
                battleCtx.ApplyBuffToPart(part, _caster, buffId, buffLayer);
            }
        }
    }
}
