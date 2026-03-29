using System.Collections.Generic;
using GameCore.RefData;
using UnityEngine;

namespace GameCore.Helpers
{
    /// <summary>
    /// Weighted pick: sum positive dropChance, sample proportionally (same idea as battle win booty);
    /// if total weight &lt;= 0, pick uniformly among valid entries.
    /// </summary>
    public static class WeightedBootyPickHelper
    {
        public static bool TryPickOne(IReadOnlyList<BootyEffectObj> items, out BootyEffectObj selected)
        {
            selected = null;
            if (items == null || items.Count == 0)
                return false;

            List<BootyEffectObj> usable = null;
            for (int i = 0; i < items.Count; i++)
            {
                var b = items[i];
                if (b != null && b.partLevelId > 0)
                {
                    usable ??= new List<BootyEffectObj>();
                    usable.Add(b);
                }
            }

            if (usable == null || usable.Count == 0)
                return false;

            float totalChance = 0f;
            foreach (var booty in usable)
                totalChance += Mathf.Max(0f, booty.dropChance);

            if (totalChance <= 0f)
            {
                selected = usable[Random.Range(0, usable.Count)];
                return true;
            }

            float randomValue = Random.Range(0f, totalChance);
            float currentChance = 0f;
            for (int j = 0; j < usable.Count; j++)
            {
                float chance = Mathf.Max(0f, usable[j].dropChance);
                currentChance += chance;
                if (randomValue <= currentChance)
                {
                    selected = usable[j];
                    return true;
                }
            }

            selected = usable[usable.Count - 1];
            return true;
        }
    }
}
