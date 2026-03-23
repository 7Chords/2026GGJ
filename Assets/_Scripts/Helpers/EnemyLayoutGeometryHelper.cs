using GameCore.Data;
using GameCore.RefData;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Helpers
{
    /// <summary>
    /// 敌人脸部预设布局的几何计算（与摆脸逻辑一致：占用格来自 PartRef 形状 + 旋转）。
    /// </summary>
    public static class EnemyLayoutGeometryHelper
    {
        public static List<Vector2Int> GetRotatedOccupyOffsets(PartRefObj partRef, int rotationSteps)
        {
            if (partRef == null) return new List<Vector2Int>();
            return GameCommon.RotateShapeAndMove2Zero(partRef.GetOccupyPosList(), rotationSteps);
        }

        public static List<Vector2Int> GetOccupiedFaceCells(Vector2Int origin, PartRefObj partRef, int rotationSteps)
        {
            var offsets = GetRotatedOccupyOffsets(partRef, rotationSteps);
            var cells = new List<Vector2Int>(offsets.Count);
            foreach (var o in offsets)
                cells.Add(origin + o);
            return cells;
        }

        /// <summary>
        /// 根据 part_level 表 id 解析部位形状（占用格）。
        /// </summary>
        public static PartRefObj ResolvePartRefForLevel(long partLevelRefId, List<PartLevelRefObj> partLevelList, List<PartRefObj> partRefList)
        {
            if (partLevelList == null || partRefList == null) return null;
            var pl = partLevelList.Find(x => x.id == partLevelRefId);
            if (pl == null) return null;
            return partRefList.Find(x => x.id == pl.partId);
        }

        /// <summary>
        /// 校验：越界、禁用格、部位占用格之间重叠。
        /// </summary>
        public static bool ValidateLayout(
            IList<EnemyLayoutSlot> slots,
            int gridColumns,
            int gridRows,
            HashSet<Vector2Int> disabledCells,
            System.Func<long, PartRefObj> resolvePartRef,
            out string errorMessage)
        {
            errorMessage = null;
            if (slots == null || slots.Count == 0)
            {
                errorMessage = null;
                return true;
            }

            var used = new HashSet<Vector2Int>();
            for (int s = 0; s < slots.Count; s++)
            {
                EnemyLayoutSlot slot = slots[s];
                var partRef = resolvePartRef(slot.partLevelRefId);
                if (partRef == null)
                {
                    errorMessage = $"槽位 {s}: 找不到 partLevelRefId={slot.partLevelRefId} 对应的 PartRef";
                    return false;
                }

                var cells = GetOccupiedFaceCells(slot.originFacePosition, partRef, slot.rotationSteps);
                foreach (var p in cells)
                {
                    if (p.x < 0 || p.x >= gridColumns || p.y < 0 || p.y >= gridRows)
                    {
                        errorMessage = $"槽位 {s}: 占用格 {p} 超出网格 [0,{gridColumns}) x [0,{gridRows})";
                        return false;
                    }
                    if (disabledCells != null && disabledCells.Contains(p))
                    {
                        errorMessage = $"槽位 {s}: 占用格 {p} 位于禁用格";
                        return false;
                    }
                    if (used.Contains(p))
                    {
                        errorMessage = $"占用格重叠: {p}";
                        return false;
                    }
                    used.Add(p);
                }
            }
            return true;
        }
    }
}
