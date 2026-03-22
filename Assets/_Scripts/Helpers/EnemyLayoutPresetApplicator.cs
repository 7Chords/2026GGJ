using GameCore.Data;
using SCFrame;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Helpers
{
    /// <summary>
    /// 根据策划配置的 <see cref="EnemyEncounterLayoutPreset"/> 从牌堆取牌并摆放敌人脸部。
    /// </summary>
    public static class EnemyLayoutPresetApplicator
    {
        /// <summary>
        /// 将敌人 busy（手牌）与 battle（脸上）全部回收进 deck，便于按预设重新抽选手牌。
        /// </summary>
        public static void MergeAllEnemyPartsIntoDeck(EnemyInfo enemy)
        {
            if (enemy == null) return;
            PartDeckHelper.RecycleBusyToDeck(enemy.deckPartInfoList, enemy.busyPartInfoList);
            PartDeckHelper.RecycleBattleToBusy(enemy.battlePartInfoList, enemy.busyPartInfoList);
            PartDeckHelper.RecycleBusyToDeck(enemy.deckPartInfoList, enemy.busyPartInfoList);
        }

        /// <summary>
        /// 按回合布局从牌堆依次取出部位到手牌区（顺序与 slots 一致）。
        /// </summary>
        public static bool TryPrepareBusyFromTurnLayout(EnemyInfo enemy, EnemyTurnFaceLayout layout)
        {
            if (enemy == null || layout == null || layout.slots == null)
                return false;

            enemy.busyPartInfoList.Clear();
            foreach (var slot in layout.slots)
            {
                int idx = enemy.deckPartInfoList.FindIndex(p => p != null && p.GetPartLevelRefId() == slot.partLevelRefId);
                if (idx < 0)
                {
                    SCDebugHelper.LogWarning($"[EnemyLayoutPreset] 牌堆中找不到 partLevelRefId={slot.partLevelRefId} 的部位实例");
                    return false;
                }
                var part = enemy.deckPartInfoList[idx];
                enemy.deckPartInfoList.RemoveAt(idx);
                enemy.busyPartInfoList.Add(part);
            }
            return true;
        }

        /// <summary>
        /// 将手牌区部位按 slots 顺序摆到脸上（busy 与 slots 一一对应，摆完后 busy 清空）。
        /// </summary>
        public static void ApplyTurnLayoutToFace(EnemyInfo enemy, List<FaceGridInfo> faceGrids, EnemyTurnFaceLayout layout)
        {
            if (enemy == null || faceGrids == null || layout == null || layout.slots == null)
                return;

            foreach (var g in faceGrids)
                g.SetEmpty();
            enemy.battlePartInfoList.Clear();

            int n = Mathf.Min(layout.slots.Count, enemy.busyPartInfoList.Count);
            if (layout.slots.Count != enemy.busyPartInfoList.Count)
                SCDebugHelper.LogWarning($"[EnemyLayoutPreset] slots 数量({layout.slots.Count})与手牌数量({enemy.busyPartInfoList.Count})不一致");

            for (int i = 0; i < n; i++)
            {
                var part = enemy.busyPartInfoList[0];
                enemy.busyPartInfoList.RemoveAt(0);
                var slot = layout.slots[i];

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
