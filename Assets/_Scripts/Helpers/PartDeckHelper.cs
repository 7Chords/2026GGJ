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
        public static void DrawParts(List<PartInfo> _deck, List<PartInfo> _busy, int _count, int _maxBusy)
        {
            if (_deck == null || _deck.Count == 0) return;
            if (_busy == null) return;
            for (int i = 0; i < _count; i++)
            {
                if (_deck.Count == 0) break;
                if (_busy.Count >= _maxBusy) break;
                int idx = Random.Range(0, _deck.Count);
                PartInfo drawn = _deck[idx];
                _deck.RemoveAt(idx);
                _busy.Add(drawn);
            }
        }

        /// <summary> 将 busy 中所有部位 ResetToDeck 后全部移入 deck，并清空 busy。 </summary>
        public static void RecycleBusyToDeck(List<PartInfo> _deck, List<PartInfo> _busy)
        {
            if (_busy == null) return;
            if (_deck == null) return;
            for (int i = 0; i < _busy.Count; i++)
                _busy[i].ResetToDeck();
            _deck.AddRange(_busy);
            _busy.Clear();
        }

        /// <summary> 将 battle 中所有部位 ResetToBusy 后全部移入 busy，并清空 battle。 </summary>
        public static void RecycleBattleToBusy(List<PartInfo> _battle, List<PartInfo> _busy)
        {
            if (_battle == null || _busy == null) return;
            for (int i = 0; i < _battle.Count; i++)
                _battle[i].ResetToBusy();
            _busy.AddRange(_battle);
            _battle.Clear();
        }
    }
}
