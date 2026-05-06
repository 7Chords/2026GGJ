using GameCore;
using UnityEngine;

namespace GameCore.Battle.Effects
{
    /// <summary>
    /// USE_HEAT_2_ATTACK_AGAIN: 自身强壮层数 &gt; n 时，将强壮降到 n，一次消耗 (旧层数 - n) 层；
    /// 只做一次概率判定：成功率为 (消耗层数 × x%)，封顶 100%；成功则再插入一次本部位行动。
    /// attributeValueList[0]=n（阈值层数），attributeValueList[1]=x（单层基础概率，0~100）
    /// </summary>
    public class UseHeat2AttackAgainEffectHandler : IPartEffectHandler
    {
        public void Execute(PartInfo _caster, EntryInfo _entry, PartEffectContext _ctx)
        {
            var battleCtx = BattleContext.current;
            if (battleCtx == null || _caster == null) return;
            if (_entry.attributeValueList == null || _entry.attributeValueList.Count < 2)
                return;

            int n = Mathf.Max(0, (int)_entry.attributeValueList[0]);
            float xPercent = _entry.attributeValueList[1];
            if (xPercent <= 1f && xPercent > 0f)
                xPercent *= 100f;
            xPercent = Mathf.Clamp(xPercent, 0f, 100f);

            BuffInfo strong = _caster.GetBuff(EBuffType.STRONG);
            if (strong == null || strong.buffLayer <= n)
                return;

            int excess = strong.buffLayer - n;
            long buffId = strong.buffRefObj.id;
            battleCtx.ApplyReduceBuffLayerToPart(_caster, buffId, excess);

            float combinedChance = Mathf.Clamp(excess * xPercent, 0f, 100f);
            float roll = UnityEngine.Random.Range(0f, 100f);
            if (roll >= combinedChance)
                return;

            bool isPlayer = !_caster.isEnemyPart;
            battleCtx.InsertPartAfterInQueue(isPlayer, _caster, _caster);
        }
    }
}
