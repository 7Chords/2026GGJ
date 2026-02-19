using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Helpers
{
    /// <summary>
    /// 部位牌堆/手牌/脸区之间的回收与抽牌逻辑，与 GameModel 解耦便于测试与复用。
    /// </summary>
    public static class PartDeckHelper
    {
        /// <summary> 从 deck 随机抽 count 张到 busy，不超过 maxBusy。直接修改两个列表。 </summary>
        public static void DrawParts(List<PartInfo> deck, List<PartInfo> busy, int count, int maxBusy)
        {
            if (deck == null || deck.Count == 0) return;
            if (busy == null) return;
            for (int i = 0; i < count; i++)
            {
                if (deck.Count == 0) break;
                if (busy.Count >= maxBusy) break;
                int idx = Random.Range(0, deck.Count);
                PartInfo drawn = deck[idx];
                deck.RemoveAt(idx);
                busy.Add(drawn);
            }
        }

        /// <summary> 将 busy 中所有部位 ResetToDeck 后全部移入 deck，并清空 busy。 </summary>
        public static void RecycleBusyToDeck(List<PartInfo> deck, List<PartInfo> busy)
        {
            if (busy == null) return;
            if (deck == null) return;
            for (int i = 0; i < busy.Count; i++)
                busy[i].ResetToDeck();
            deck.AddRange(busy);
            busy.Clear();
        }

        /// <summary> 将 battle 中所有部位 ResetToBusy 后全部移入 busy，并清空 battle。 </summary>
        public static void RecycleBattleToBusy(List<PartInfo> battle, List<PartInfo> busy)
        {
            if (battle == null || busy == null) return;
            for (int i = 0; i < battle.Count; i++)
                battle[i].ResetToBusy();
            busy.AddRange(battle);
            battle.Clear();
        }
    }
}
