using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GameCore.RefData;
using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// 局内存档：JSON 文件，默认写在 StreamingAssets/Saves 下（部分平台不可写时会回退到 persistentDataPath/Saves）。
    /// </summary>
    public static class GameRunSave
    {
        public const string SaveFileName = "run_save.json";
        public const string SavesSubFolder = "Saves";

        private static bool _allowSaveAfterMapEntered;
        private static string _resolvedSavesDirectory;

        [Serializable]
        public class RunSaveData
        {
            public bool mapVisited;
            public int mapX, mapY;
            public int pendingX, pendingY;
            public int floor;
            public int health, maxHealth, money;
            public long rollStoreId, rollEventId;
            public PartSaveEntry[] bagParts;
            public PartSaveEntry[] deckParts;
            public PartSaveEntry[] busyParts;
            public PartSaveEntry[] battleParts;
            /// <summary> 为 true 时 mapLayoutSeed 有效，继续游戏时用同一种子复现地图。 </summary>
            public bool mapLayoutFromSave;
            public int mapLayoutSeed;
        }

        [Serializable]
        public class PartSaveEntry
        {
            public long partRefId;
            public int level;
            public int currentHealth;
        }

        public static string GetSaveFilePath()
        {
            return Path.Combine(GetResolvedSavesDirectory(), SaveFileName);
        }

        /// <summary>
        /// 优先使用 StreamingAssets/Saves；不可写时回退到 persistentDataPath/Saves（Android 等只读 StreamingAssets）。
        /// </summary>
        public static string GetResolvedSavesDirectory()
        {
            if (!string.IsNullOrEmpty(_resolvedSavesDirectory))
                return _resolvedSavesDirectory;

            string streamingSaves = Path.Combine(Application.streamingAssetsPath, SavesSubFolder);
            try
            {
                if (!Directory.Exists(streamingSaves))
                    Directory.CreateDirectory(streamingSaves);
                string testFile = Path.Combine(streamingSaves, ".write_test");
                File.WriteAllText(testFile, "ok", Encoding.UTF8);
                File.Delete(testFile);
                _resolvedSavesDirectory = streamingSaves;
                return _resolvedSavesDirectory;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GameRunSave] StreamingAssets 下 Saves 不可写，改用 persistentDataPath。{e.Message}");
                string pd = Path.Combine(Application.persistentDataPath, SavesSubFolder);
                if (!Directory.Exists(pd))
                    Directory.CreateDirectory(pd);
                _resolvedSavesDirectory = pd;
                return _resolvedSavesDirectory;
            }
        }

        public static void InvalidateResolvedSavesDirectory()
        {
            _resolvedSavesDirectory = null;
        }

        public static bool HasSavedRun()
        {
            try
            {
                if (!TryReadSaveJson(out string json) || string.IsNullOrEmpty(json))
                    return false;
                RunSaveData data = JsonUtility.FromJson<RunSaveData>(json);
                return data != null && data.mapVisited;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GameRunSave] 读取存档失败: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 优先读「当前写入路径」GetSaveFilePath()，再查 persistent、最后 StreamingAssets。
        /// 若先读 StreamingAssets，可能读到仓库里旧版/无 mapLayoutSeed 的示例档，而真实进度写在 persistent，导致继续游戏地图重随机、坐标与格子不匹配。
        /// </summary>
        static bool TryReadSaveJson(out string json)
        {
            json = null;
            try
            {
                string primary = GetSaveFilePath();
                if (File.Exists(primary))
                {
                    json = File.ReadAllText(primary, Encoding.UTF8);
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GameRunSave] 读取主存档路径失败: {e.Message}");
            }

            string persistentPath = Path.Combine(Application.persistentDataPath, SavesSubFolder, SaveFileName);
            if (File.Exists(persistentPath))
            {
                json = File.ReadAllText(persistentPath, Encoding.UTF8);
                return true;
            }
            string streamingPath = Path.Combine(Application.streamingAssetsPath, SavesSubFolder, SaveFileName);
            if (File.Exists(streamingPath))
            {
                json = File.ReadAllText(streamingPath, Encoding.UTF8);
                return true;
            }
            return false;
        }

        public static void DeleteSave()
        {
            try
            {
                string streamingPath = Path.Combine(Application.streamingAssetsPath, SavesSubFolder, SaveFileName);
                if (File.Exists(streamingPath))
                    File.Delete(streamingPath);
                string persistentPath = Path.Combine(Application.persistentDataPath, SavesSubFolder, SaveFileName);
                if (File.Exists(persistentPath))
                    File.Delete(persistentPath);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GameRunSave] 删除存档失败: {e.Message}");
            }
            finally
            {
                _allowSaveAfterMapEntered = false;
                InvalidateResolvedSavesDirectory();
            }
        }

        /// <summary> 进入地图 UI 后，才允许写入有效存档。 </summary>
        public static void NotifyEnteredMapOnce()
        {
            _allowSaveAfterMapEntered = true;
        }

        public static void SaveFromGameModel()
        {
            if (!_allowSaveAfterMapEntered)
                return;

            var gm = GameModel.instance;
            if (gm?.playerInfo == null)
                return;

            var p = gm.playerInfo;
            var mm = MapManager.instance;
            bool hasMapSeed = mm != null && mm.LastMapLayoutSeed >= 0;

            var data = new RunSaveData
            {
                mapVisited = true,
                mapX = p.playerMapPosition.x,
                mapY = p.playerMapPosition.y,
                pendingX = p.pendingMapTargetPosition.x,
                pendingY = p.pendingMapTargetPosition.y,
                floor = p.playerFloor,
                health = p.currentHealth,
                maxHealth = p.maxHealth,
                money = p.playerMoney,
                rollStoreId = gm.rollStoreId,
                rollEventId = gm.rollEventId,
                bagParts = SerializeParts(p.bagPartInfoList),
                deckParts = SerializeParts(p.deckPartInfoList),
                busyParts = SerializeParts(p.busyPartInfoList),
                battleParts = SerializeParts(p.battlePartInfoList),
                mapLayoutFromSave = hasMapSeed,
                mapLayoutSeed = hasMapSeed ? mm.LastMapLayoutSeed : 0,
            };

            try
            {
                string path = GetSaveFilePath();
                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.WriteAllText(path, JsonUtility.ToJson(data), Encoding.UTF8);
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameRunSave] 写入存档失败: {e.Message}");
            }
        }

        static PartSaveEntry[] SerializeParts(List<PartInfo> list)
        {
            if (list == null || list.Count == 0)
                return new PartSaveEntry[0];
            var tmp = new List<PartSaveEntry>();
            foreach (var info in list)
            {
                if (info?.partRefObj == null)
                    continue;
                tmp.Add(new PartSaveEntry
                {
                    partRefId = info.partRefObj.id,
                    level = info.partLevel,
                    currentHealth = info.currentHealth
                });
            }
            return tmp.ToArray();
        }

        public static bool TryLoadIntoGameModel()
        {
            try
            {
                if (!TryReadSaveJson(out string json) || string.IsNullOrEmpty(json))
                    return false;
                RunSaveData data = JsonUtility.FromJson<RunSaveData>(json);
                if (data == null || !data.mapVisited)
                    return false;
                GameModel.instance.ApplyRunSaveData(data);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GameRunSave] 读档失败: {e.Message}");
                return false;
            }
        }
    }
}
