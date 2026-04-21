using SCFrame;
using UnityEngine;

namespace GameCore.Battle
{
    /// <summary>
    /// 有SEND_MOLD_2_BY_GET_HIT的器官 受伤优先扣除菌团处理器
    /// </summary>
    public static class GermMassDamageAbsorption
    {
        public static bool PartHasSendMoldGetHitEntry(PartInfo part)
        {
            if (part?.entryInfoList == null) return false;
            for (int i = 0; i < part.entryInfoList.Count; i++)
            {
                var e = part.entryInfoList[i];
                if (e == null) continue;
                if (e.triggerPointType == EAttributeTriggerPointType.GET_HIT
                    && e.attributeType == EAttributeType.SEND_MOLD_2_BY_GET_HIT)
                    return true;
            }
            return false;
        }

        /// <summary> Reduces <paramref name="damage"/> using germ stacks; each layer absorbs 1 damage. </summary>
        public static void AbsorbDamageThroughGerms(PartInfo part, ref int damage)
        {
            if (part?.buffLogic == null || damage <= 0) return;
            int d = damage;
            d = AbsorbOneType(part, EBuffType.HEAL_MASS, d);
            d = AbsorbOneType(part, EBuffType.ATTACK_MASS, d);
            d = AbsorbOneType(part, EBuffType.BREEDING_MASS, d);
            damage = d;
        }

        static int AbsorbOneType(PartInfo part, EBuffType type, int damage)
        {
            if (damage <= 0) return 0;
            var buff = part.GetBuff(type);
            if (buff == null || buff.buffLayer <= 0) return damage;
            int take = Mathf.Min(damage, buff.buffLayer);
            if (take <= 0) return damage;
            buff.ReduceBuffLayer(take);
            if (buff.buffLayer == 0)
                part.buffLogic.RemoveBuff(buff);
            else
                SCMsgCenter.SendMsg(SCMsgConst.PART_BUFF_UPDATE, buff);
            return damage - take;
        }
    }
}
