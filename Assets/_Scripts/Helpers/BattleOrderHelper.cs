using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Helpers
{
    /// <summary>
    /// 战斗顺序：按脸部格子位置排序，以及根据部位查序号（1-based）。
    /// </summary>
    public static class BattleOrderHelper
    {
        /// <summary> 按占格最小坐标 (y 优先，再 x) 排序 </summary>
        public static void SortBattleOrder(List<PartInfo> battlePartList)
        {
            if (battlePartList == null || battlePartList.Count == 0) return;
            battlePartList.Sort((a, b) =>
            {
                Vector2Int aPos = a.GetMinGridPos();
                Vector2Int bPos = b.GetMinGridPos();
                if (aPos.y != bPos.y) return aPos.y.CompareTo(bPos.y);
                return aPos.x.CompareTo(bPos.x);
            });
        }

        /// <summary> 获取部位在战斗列表中的显示序号（1-based）；不含则返回 -1。会先按位置排序再查。 </summary>
        public static int GetBattleOrderByPartInfo(List<PartInfo> battlePartList, PartInfo info)
        {
            if (info == null || battlePartList == null || !battlePartList.Contains(info))
                return -1;
            SortBattleOrder(battlePartList);
            return battlePartList.IndexOf(info) + 1;
        }
    }
}
