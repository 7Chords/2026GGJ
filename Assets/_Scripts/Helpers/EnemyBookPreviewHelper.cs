using GameCore;
using GameCore.Data;
using GameCore.Helpers;
using GameCore.RefData;
using SCFrame;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Helpers
{
    public static class EnemyBookPreviewHelper
    {
        public struct TurnLayoutPreviewEntry
        {
            public string label;
            public EnemyTurnFaceLayout layout;
            public bool enemyActsFirst;
        }

        public struct PartReserveSummaryEntry
        {
            public string partName;
            public int count;
        }

        public static List<PartReserveSummaryEntry> BuildPartReserveSummaries(EnemyRefObj enemyRef)
        {
            var result = new List<PartReserveSummaryEntry>();
            if (enemyRef?.initPartList == null || enemyRef.initPartList.Count == 0)
                return result;

            var indexByLevelId = new Dictionary<long, int>();

            for (int i = 0; i < enemyRef.initPartList.Count; i++)
            {
                PartLevelEffectObj entry = enemyRef.initPartList[i];
                if (entry == null || entry.partLevelId == 0)
                    continue;

                PartLevelRefObj levelRef = SCRefDataMgr.instance.partLevelRefList.refDataList
                    .Find(x => x.id == entry.partLevelId);
                if (levelRef == null)
                    continue;

                PartRefObj partRef = SCRefDataMgr.instance.partRefList.refDataList
                    .Find(x => x.id == levelRef.partId);
                if (partRef == null)
                    continue;

                int amount = Mathf.Max(0, entry.partAmount);
                if (amount <= 0)
                    continue;

                if (indexByLevelId.TryGetValue(entry.partLevelId, out int existingIndex))
                {
                    var existing = result[existingIndex];
                    existing.count += amount;
                    result[existingIndex] = existing;
                    continue;
                }

                indexByLevelId[entry.partLevelId] = result.Count;
                result.Add(new PartReserveSummaryEntry
                {
                    partName = partRef.partName ?? "",
                    count = amount
                });
            }

            return result;
        }

        public static List<PartInfo> BuildDeckParts(EnemyRefObj enemyRef)
        {
            var list = new List<PartInfo>();
            if (enemyRef?.initPartList == null || enemyRef.initPartList.Count == 0)
                return list;

            for (int i = 0; i < enemyRef.initPartList.Count; i++)
            {
                PartLevelEffectObj entry = enemyRef.initPartList[i];
                if (entry == null)
                    continue;

                PartLevelRefObj levelRef = SCRefDataMgr.instance.partLevelRefList.refDataList
                    .Find(x => x.id == entry.partLevelId);
                if (levelRef == null)
                    continue;

                PartRefObj partRef = SCRefDataMgr.instance.partRefList.refDataList
                    .Find(x => x.id == levelRef.partId);
                if (partRef == null)
                    continue;

                int amount = Mathf.Max(0, entry.partAmount);
                for (int j = 0; j < amount; j++)
                    list.Add(new PartInfo(partRef, true, levelRef.partLevel));
            }

            return list;
        }

        public static List<PartInfo> CloneDeckParts(List<PartInfo> source)
        {
            var clones = new List<PartInfo>();
            if (source == null)
                return clones;

            for (int i = 0; i < source.Count; i++)
            {
                PartInfo part = source[i];
                if (part?.partRefObj == null)
                    continue;
                clones.Add(new PartInfo(part.partRefObj, true, part.partLevel));
            }

            return clones;
        }

        public static List<PartInfo> BuildFacePartsForLayout(EnemyTurnFaceLayout layout, List<PartInfo> deckPool)
        {
            var faceParts = new List<PartInfo>();
            if (layout?.slots == null || layout.slots.Count == 0)
                return faceParts;

            var remainingDeck = CloneDeckParts(deckPool);
            for (int i = 0; i < layout.slots.Count; i++)
            {
                EnemyLayoutSlot slot = layout.slots[i];
                if (slot == null)
                    continue;

                int idx = remainingDeck.FindIndex(p => p != null && p.GetPartLevelRefId() == slot.partLevelRefId);
                if (idx < 0)
                    continue;

                PartInfo template = remainingDeck[idx];
                remainingDeck.RemoveAt(idx);

                var part = new PartInfo(template.partRefObj, true, template.partLevel);
                part.ResetToBusy();
                placePartOnFace(part, slot.originFacePosition, slot.rotationSteps);
                faceParts.Add(part);
            }

            BattleOrderHelper.SortBattleOrder(faceParts);
            return faceParts;
        }

        public static List<TurnLayoutPreviewEntry> CollectTurnLayoutEntries(EnemyLayoutPreset preset)
        {
            var entries = new List<TurnLayoutPreviewEntry>();
            if (preset == null)
                return entries;

            appendTurnLayouts(entries, preset.turnLayoutsEnemyActsFirst, true);
            appendTurnLayouts(entries, preset.turnLayoutsEnemyActsSecond, false);
            return entries;
        }

        public static EnemyLayoutPreset LoadLayoutPreset(EnemyRefObj enemyRef)
        {
            if (enemyRef == null || string.IsNullOrEmpty(enemyRef.layoutPresetName))
                return null;

            return ResourcesHelper.LoadAsset<EnemyLayoutPreset>(enemyRef.layoutPresetName);
        }

        public static List<EnemyRefObj> BuildSortedEnemyBookList()
        {
            var result = new List<EnemyRefObj>();
            var enemyRefs = SCRefDataMgr.instance?.enemyRefList?.refDataList;
            if (enemyRefs == null)
                return result;

            var seenIds = new HashSet<long>();
            for (int i = 0; i < enemyRefs.Count; i++)
            {
                EnemyRefObj enemyRef = enemyRefs[i];
                if (enemyRef == null || seenIds.Contains(enemyRef.id))
                    continue;
                if (enemyRef.battleType == EBattleType.EVENT)
                    continue;

                seenIds.Add(enemyRef.id);
                result.Add(enemyRef);
            }

            result.Sort((a, b) =>
            {
                int floorCmp = a.floor.CompareTo(b.floor);
                if (floorCmp != 0)
                    return floorCmp;
                int columnCmp = a.column.CompareTo(b.column);
                if (columnCmp != 0)
                    return columnCmp;
                return a.id.CompareTo(b.id);
            });
            return result;
        }

        private static void appendTurnLayouts(
            List<TurnLayoutPreviewEntry> entries,
            List<EnemyTurnFaceLayout> layouts,
            bool enemyActsFirst)
        {
            if (layouts == null || layouts.Count == 0)
                return;

            for (int i = 0; i < layouts.Count; i++)
            {
                EnemyTurnFaceLayout layout = layouts[i];
                if (layout == null)
                    continue;

                string orderLabel = enemyActsFirst ? "Enemy First" : "Player First";
                entries.Add(new TurnLayoutPreviewEntry
                {
                    label = $"Round {i + 1} ({orderLabel})",
                    layout = layout,
                    enemyActsFirst = enemyActsFirst
                });
            }
        }

        private static void placePartOnFace(PartInfo part, Vector2Int origin, int rotationSteps)
        {
            if (part == null)
                return;

            for (int i = 0; i < rotationSteps; i++)
                part.RotateOnce();

            part.curOccupyFacePosList = new List<Vector2Int>();
            part.curEffectFacePosList = new List<Vector2Int>();

            for (int i = 0; i < part.localOccupyPosList.Count; i++)
                part.curOccupyFacePosList.Add(origin + part.localOccupyPosList[i]);
            for (int i = 0; i < part.localEffectPosList.Count; i++)
                part.curEffectFacePosList.Add(origin + part.localEffectPosList[i]);

            part.isOnFace = true;
        }
    }
}
