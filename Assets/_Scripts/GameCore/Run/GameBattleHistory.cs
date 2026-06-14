using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GameCore.RefData;
using UnityEngine;

namespace GameCore
{
    public enum ERunEndReason
    {
        Unknown = 0,
        Battle = 1,
        BossBattle = 2,
        Event = 3,
        Trial = 4,
    }

    /// <summary>
    /// Persistent run history (win/loss, lose location, end-of-run part library).
    /// </summary>
    public static class GameBattleHistory
    {
        public const string HistoryFileName = "battle_history.json";
        public const int MaxHistoryCount = 20;
        public const int MaxFavoriteCount = 10;

        [Serializable]
        public class BattleHistoryStore
        {
            public List<BattleHistoryEntry> entries = new List<BattleHistoryEntry>();
            public List<long> favoriteRecordedAtTicks = new List<long>();
        }

        [Serializable]
        public class BattleHistoryEntry
        {
            public bool isWin;
            public int floor;
            public int endReason;
            public long enemyRefId;
            public string enemyName;
            public long recordedAtTicks;
            public GameRunSave.PartSaveEntry[] endParts;
        }

        public static IReadOnlyList<BattleHistoryEntry> GetEntries()
        {
            return LoadStore().entries;
        }

        public static IReadOnlyList<BattleHistoryEntry> GetFavoriteEntries()
        {
            var store = LoadStore();
            var result = new List<BattleHistoryEntry>();
            if (store.favoriteRecordedAtTicks == null || store.entries == null)
                return result;

            for (int i = 0; i < store.favoriteRecordedAtTicks.Count; i++)
            {
                long ticks = store.favoriteRecordedAtTicks[i];
                BattleHistoryEntry entry = findEntryByRecordedAtTicks(store.entries, ticks);
                if (entry != null)
                    result.Add(entry);
            }
            return result;
        }

        public static bool IsFavorite(BattleHistoryEntry entry)
        {
            if (entry == null || entry.recordedAtTicks <= 0)
                return false;

            var ticksList = LoadStore().favoriteRecordedAtTicks;
            return ticksList != null && ticksList.Contains(entry.recordedAtTicks);
        }

        /// <summary>
        /// Toggle favorite by recordedAtTicks. Returns true if now favorited.
        /// </summary>
        public static bool ToggleFavorite(long recordedAtTicks)
        {
            if (recordedAtTicks <= 0)
                return false;

            var store = LoadStore();
            if (findEntryByRecordedAtTicks(store.entries, recordedAtTicks) == null)
                return false;

            if (store.favoriteRecordedAtTicks == null)
                store.favoriteRecordedAtTicks = new List<long>();

            int index = store.favoriteRecordedAtTicks.IndexOf(recordedAtTicks);
            if (index >= 0)
            {
                store.favoriteRecordedAtTicks.RemoveAt(index);
                SaveStore(store);
                return false;
            }

            store.favoriteRecordedAtTicks.Insert(0, recordedAtTicks);
            while (store.favoriteRecordedAtTicks.Count > MaxFavoriteCount)
                store.favoriteRecordedAtTicks.RemoveAt(store.favoriteRecordedAtTicks.Count - 1);

            SaveStore(store);
            return true;
        }

        public static void TryRecordPendingRunEndFromGameModel()
        {
            var gm = GameModel.instance;
            if (gm == null || !gm.HasPendingRunEndSnapshot)
                return;

            var entry = new BattleHistoryEntry
            {
                isWin = gm.PendingRunEndIsWin,
                floor = gm.PendingRunEndFloor,
                endReason = (int)gm.PendingRunEndReason,
                enemyRefId = gm.PendingRunEndEnemyRefId,
                enemyName = gm.PendingRunEndEnemyName ?? string.Empty,
                recordedAtTicks = DateTime.UtcNow.Ticks,
                endParts = SerializeAllPlayerParts(gm.playerInfo),
            };

            var store = LoadStore();
            store.entries.Insert(0, entry);
            if (store.entries.Count > MaxHistoryCount)
                store.entries.RemoveRange(MaxHistoryCount, store.entries.Count - MaxHistoryCount);
            pruneFavorites(store);
            SaveStore(store);
        }

        public static List<PartInfo> DeserializeEndParts(BattleHistoryEntry entry)
        {
            if (entry?.endParts == null)
                return new List<PartInfo>();

            var list = new List<PartInfo>();
            foreach (var e in entry.endParts)
            {
                if (e == null)
                    continue;
                PartRefObj partRefObj = SCRefDataMgr.instance.partRefList.refDataList.Find(x => x.id == e.partRefId);
                if (partRefObj == null)
                    continue;
                var pi = new PartInfo(partRefObj, false, e.level);
                pi.currentHealth = Mathf.Clamp(e.currentHealth, 0, pi.maxHealth);
                list.Add(pi);
            }
            return list;
        }

        public static string FormatLoseLocation(BattleHistoryEntry entry)
        {
            if (entry == null || entry.isWin)
                return string.Empty;

            var reason = (ERunEndReason)entry.endReason;
            switch (reason)
            {
                case ERunEndReason.BossBattle:
                    return string.IsNullOrEmpty(entry.enemyName)
                        ? $"第{entry.floor}层 Boss"
                        : $"第{entry.floor}层 Boss：{entry.enemyName}";
                case ERunEndReason.Battle:
                    return string.IsNullOrEmpty(entry.enemyName)
                        ? $"第{entry.floor}层 战斗"
                        : $"第{entry.floor}层 战斗：{entry.enemyName}";
                case ERunEndReason.Event:
                    return $"第{entry.floor}层 事件";
                case ERunEndReason.Trial:
                    return $"第{entry.floor}层 试炼";
                default:
                    return $"第{entry.floor}层";
            }
        }

        public static string FormatResultText(BattleHistoryEntry entry)
        {
            if (entry == null)
                return string.Empty;
            return entry.isWin ? "胜利" : "失败";
        }

        public static string FormatRecordedTime(BattleHistoryEntry entry)
        {
            if (entry == null || entry.recordedAtTicks <= 0)
                return string.Empty;
            try
            {
                return new DateTime(entry.recordedAtTicks, DateTimeKind.Utc).ToLocalTime().ToString("yyyy-MM-dd HH:mm");
            }
            catch
            {
                return string.Empty;
            }
        }

        static GameRunSave.PartSaveEntry[] SerializeAllPlayerParts(PlayerInfo playerInfo)
        {
            if (playerInfo == null)
                return new GameRunSave.PartSaveEntry[0];

            var merged = new List<PartInfo>();
            AppendUniqueParts(merged, playerInfo.bagPartInfoList);
            AppendUniqueParts(merged, playerInfo.deckPartInfoList);
            AppendUniqueParts(merged, playerInfo.busyPartInfoList);
            AppendUniqueParts(merged, playerInfo.battlePartInfoList);
            AppendUniqueParts(merged, playerInfo.deadPartInfoList);
            return SerializeParts(merged);
        }

        static void AppendUniqueParts(List<PartInfo> target, List<PartInfo> source)
        {
            if (source == null)
                return;
            for (int i = 0; i < source.Count; i++)
            {
                var p = source[i];
                if (p == null || target.Contains(p))
                    continue;
                target.Add(p);
            }
        }

        static GameRunSave.PartSaveEntry[] SerializeParts(List<PartInfo> list)
        {
            if (list == null || list.Count == 0)
                return new GameRunSave.PartSaveEntry[0];

            var tmp = new List<GameRunSave.PartSaveEntry>();
            foreach (var info in list)
            {
                if (info?.partRefObj == null)
                    continue;
                tmp.Add(new GameRunSave.PartSaveEntry
                {
                    partRefId = info.partRefObj.id,
                    level = info.partLevel,
                    currentHealth = info.currentHealth,
                });
            }
            return tmp.ToArray();
        }

        static string GetHistoryFilePath()
        {
            return Path.Combine(GameRunSave.GetResolvedSavesDirectory(), HistoryFileName);
        }

        static BattleHistoryStore LoadStore()
        {
            try
            {
                string path = GetHistoryFilePath();
                if (!File.Exists(path))
                    return new BattleHistoryStore();
                string json = File.ReadAllText(path, Encoding.UTF8);
                if (string.IsNullOrEmpty(json))
                    return new BattleHistoryStore();
                var store = JsonUtility.FromJson<BattleHistoryStore>(json);
                if (store == null)
                    return new BattleHistoryStore();
                if (store.entries == null)
                    store.entries = new List<BattleHistoryEntry>();
                if (store.favoriteRecordedAtTicks == null)
                    store.favoriteRecordedAtTicks = new List<long>();
                if (store.entries.Count > MaxHistoryCount)
                    store.entries.RemoveRange(MaxHistoryCount, store.entries.Count - MaxHistoryCount);
                pruneFavorites(store);
                return store;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GameBattleHistory] load failed: {e.Message}");
                return new BattleHistoryStore();
            }
        }

        static BattleHistoryEntry findEntryByRecordedAtTicks(List<BattleHistoryEntry> entries, long recordedAtTicks)
        {
            if (entries == null)
                return null;

            for (int i = 0; i < entries.Count; i++)
            {
                BattleHistoryEntry entry = entries[i];
                if (entry != null && entry.recordedAtTicks == recordedAtTicks)
                    return entry;
            }
            return null;
        }

        static void pruneFavorites(BattleHistoryStore store)
        {
            if (store?.favoriteRecordedAtTicks == null || store.entries == null)
                return;

            for (int i = store.favoriteRecordedAtTicks.Count - 1; i >= 0; i--)
            {
                long ticks = store.favoriteRecordedAtTicks[i];
                if (findEntryByRecordedAtTicks(store.entries, ticks) == null)
                    store.favoriteRecordedAtTicks.RemoveAt(i);
            }

            while (store.favoriteRecordedAtTicks.Count > MaxFavoriteCount)
                store.favoriteRecordedAtTicks.RemoveAt(store.favoriteRecordedAtTicks.Count - 1);
        }

        static void SaveStore(BattleHistoryStore store)
        {
            if (store == null)
                return;
            try
            {
                string path = GetHistoryFilePath();
                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.WriteAllText(path, JsonUtility.ToJson(store), Encoding.UTF8);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GameBattleHistory] save failed: {e.Message}");
            }
        }
    }
}
