using System.Collections.Generic;
using GameCore;
using GameCore.RefData;
using SCFrame;
using UnityEngine;

namespace GameCore.Helpers
{
    /// <summary>
    /// Collects buff types for tooltip side icons: current stacks on the part plus types referenced by its entries (grants, multipliers, bleed/fat/burn chains, etc.).
    /// </summary>
    public static class PartTooltipBuffSideHintCollector
    {
        // Keep in sync with effect handlers that use literal buff ids.
        private const long BleedBuffId = 100001;
        private const long FatBuffId = 100002;
        private const long BurnBuffId = 100003;

        public static List<EBuffType> CollectSideHintBuffTypes(PartInfo part)
        {
            var result = new List<EBuffType>();
            var seen = new HashSet<EBuffType>();

            if (part?.buffLogic?.buffList != null)
            {
                for (int i = 0; i < part.buffLogic.buffList.Count; i++)
                {
                    var bi = part.buffLogic.buffList[i];
                    if (bi == null)
                        continue;
                    if (seen.Add(bi.buffType))
                        result.Add(bi.buffType);
                }
            }

            if (part?.entryInfoList != null)
            {
                for (int i = 0; i < part.entryInfoList.Count; i++)
                {
                    var e = part.entryInfoList[i];
                    if (e == null)
                        continue;
                    foreach (var t in GetBuffTypesReferencedByEntry(e))
                    {
                        if (seen.Add(t))
                            result.Add(t);
                    }
                }
            }

            return result;
        }

        private static IEnumerable<EBuffType> GetBuffTypesReferencedByEntry(EntryInfo e)
        {
            var list = e.attributeValueList;

            switch (e.attributeType)
            {
                case EAttributeType.SELF_GET_BUFF:
                case EAttributeType.ENEMY_GET_BUFF:
                case EAttributeType.SELF_MOUTH_GET_BUFF:
                case EAttributeType.ENEMY_MOUTH_GET_BUFF:
                case EAttributeType.SELF_BUFF_MULTIPLIER:
                case EAttributeType.ENEMY_BUFF_MULTIPLIER:
                    if (list != null && list.Count >= 1)
                    {
                        if (TryGetBuffTypeFromId(ToLong(list[0]), out var t))
                            yield return t;
                    }
                    break;

                case EAttributeType.SEND_BLEED_BY_GET_HIT:
                    if (TryGetBuffTypeFromId(BleedBuffId, out var bleed))
                        yield return bleed;
                    break;

                case EAttributeType.SEND_ALL_FAT_BY_GET_HIT:
                    if (TryGetBuffTypeFromId(FatBuffId, out var fat))
                        yield return fat;
                    break;

                case EAttributeType.CHANGE_FAT_2_BURN:
                    if (TryGetBuffTypeFromId(FatBuffId, out var fat2))
                        yield return fat2;
                    if (TryGetBuffTypeFromId(BurnBuffId, out var burn))
                        yield return burn;
                    break;

                case EAttributeType.SPREAD_BURN:
                    if (TryGetBuffTypeFromId(BurnBuffId, out var burn2))
                        yield return burn2;
                    break;

                case EAttributeType.ATTACK_BY_ENEMY_BLEED:
                    yield return EBuffType.BLEED;
                    break;

                case EAttributeType.CLEAR_ENEMY_BLEED_AND_HEAL_PART:
                case EAttributeType.CLEAR_SELF_BLEED_AND_HEAL_SELF:
                    yield return EBuffType.BLEED;
                    break;

                case EAttributeType.CLEAR_ALL_HATE_AND_MOUTH_GET_STRONG:
                    yield return EBuffType.HATE;
                    yield return EBuffType.STRONG;
                    break;

                case EAttributeType.USE_HEAT_2_ATTACK_AGAIN:
                    yield return EBuffType.STRONG;
                    break;

                case EAttributeType.INCREASE_ADD_BURN:
                    yield return EBuffType.BURN;
                    break;
            }
        }

        private static long ToLong(float v)
        {
            return (long)v;
        }

        private static bool TryGetBuffTypeFromId(long buffId, out EBuffType type)
        {
            type = default;
            var refs = SCRefDataMgr.instance?.buffRefList?.refDataList;
            if (refs == null)
                return false;
            for (int i = 0; i < refs.Count; i++)
            {
                if (refs[i] != null && refs[i].id == buffId)
                {
                    type = refs[i].buffType;
                    return true;
                }
            }
            return false;
        }
    }
}
