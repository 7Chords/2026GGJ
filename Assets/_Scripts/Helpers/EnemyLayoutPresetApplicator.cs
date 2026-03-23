using GameCore.Data;
using SCFrame;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Helpers
{
    /// <summary>
    /// 根据策划配置的 <see cref="EnemyLayoutPreset"/> 从牌堆取牌并摆放敌人脸部。
    /// </summary>
    public static class EnemyLayoutPresetApplicator
    {
        /// <summary>
        /// 将敌人 busy（手牌）与 battle（脸上）全部回收进 deck，便于预设按回合从牌堆精确取牌。
        /// </summary>
        public static void MergeAllEnemyPartsIntoDeck(EnemyInfo enemy)
        {
            if (enemy == null) return;
            PartDeckHelper.RecycleBusyToDeck(enemy.deckPartInfoList, enemy.busyPartInfoList);
            PartDeckHelper.RecycleBattleToBusy(enemy.battlePartInfoList, enemy.busyPartInfoList);
            PartDeckHelper.RecycleBusyToDeck(enemy.deckPartInfoList, enemy.busyPartInfoList);
        }

        /// <summary>
        /// 按回合布局从牌堆依次取牌到手牌区。缺牌（被击杀、配表错误等）时跳过该槽位并打日志，不整盘失败。
        /// <paramref name="resolvedSlots"/> 与 <see cref="EnemyInfo.busyPartInfoList"/> 顺序一一对应。
        /// </summary>
        public static void PrepareBusyFromTurnLayoutBestEffort(EnemyInfo enemy, EnemyTurnFaceLayout layout, out List<EnemyLayoutSlot> resolvedSlots)
        {
            resolvedSlots = new List<EnemyLayoutSlot>();
            if (enemy == null)
                return;

            enemy.busyPartInfoList.Clear();

            if (layout == null || layout.slots == null || layout.slots.Count == 0)
                return;

            foreach (var slot in layout.slots)
            {
                int idx = enemy.deckPartInfoList.FindIndex(p => p != null && p.GetPartLevelRefId() == slot.partLevelRefId);
                if (idx < 0)
                {
                    SCDebugHelper.LogWarning(
                        $"[EnemyLayoutPreset] 牌堆中找不到 partLevelRefId={slot.partLevelRefId} 的部位实例（可能已被击杀或配置错误），跳过该槽位");
                    continue;
                }

                var part = enemy.deckPartInfoList[idx];
                enemy.deckPartInfoList.RemoveAt(idx);
                enemy.busyPartInfoList.Add(part);
                resolvedSlots.Add(slot);
            }
        }

        /// <summary>
        /// 将手牌区部位按 <paramref name="resolvedSlots"/> 顺序摆到脸上（与 busy 一一对应，摆完后 busy 清空）。
        /// </summary>
        public static void ApplyTurnLayoutToFace(EnemyInfo enemy, List<FaceGridInfo> faceGrids, List<EnemyLayoutSlot> resolvedSlots)
        {
            if (enemy == null || faceGrids == null)
                return;

            if (resolvedSlots == null)
                resolvedSlots = new List<EnemyLayoutSlot>();

            foreach (var g in faceGrids)
                g.SetEmpty();
            enemy.battlePartInfoList.Clear();

            int n = Mathf.Min(resolvedSlots.Count, enemy.busyPartInfoList.Count);
            if (resolvedSlots.Count != enemy.busyPartInfoList.Count)
                SCDebugHelper.LogWarning(
                    $"[EnemyLayoutPreset] resolvedSlots({resolvedSlots.Count})与手牌({enemy.busyPartInfoList.Count})数量不一致");

            for (int i = 0; i < n; i++)
            {
                var part = enemy.busyPartInfoList[0];
                enemy.busyPartInfoList.RemoveAt(0);
                var slot = resolvedSlots[i];

                if (part.GetPartLevelRefId() != slot.partLevelRefId)
                    SCDebugHelper.LogWarning($"[EnemyLayoutPreset] 第{i}格部位 id 与预设不一致");

                part.ResetToBusy();
                MarkOccupancy(part, slot.originFacePosition, slot.rotationSteps, enemy, faceGrids);
            }

            BattleOrderHelper.SortBattleOrder(enemy.battlePartInfoList);
        }

        private static void MarkOccupancy(PartInfo part, Vector2Int origin, int rot, EnemyInfo enemy, List<FaceGridInfo> faceGrids)
        {
            for (int i = 0; i < rot; i++)
                part.RotateOnce();

            foreach (var offset in part.localOccupyPosList)
            {
                Vector2Int p = origin + offset;
                FaceGridInfo gridInfo = faceGrids.Find(x => x.pos == p);
                if (gridInfo == null) continue;
                gridInfo.SetOwnerPart(part);
                part.curOccupyFacePosList.Add(p);
            }
            foreach (var offset in part.localEffectPosList)
            {
                part.curEffectFacePosList.Add(origin + offset);
            }
            part.isOnFace = true;
            enemy.battlePartInfoList.Add(part);
        }

        public static int GetClampedTurnIndex(int turnIndex, int layoutCount)
        {
            if (layoutCount <= 0) return -1;
            return Mathf.Clamp(turnIndex, 0, layoutCount - 1);
        }
    }
}
