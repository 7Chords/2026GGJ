using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Data
{
    /// <summary>
    /// 集中登记所有「敌人 encounter 布局预设」，运行时按 enemyRefId 查找。
    /// 请将资源放在 Resources 下路径 <see cref="ResourcesLoadPath"/> 以便自动加载。
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyLayoutPresetDatabase", menuName = "Game/Enemy Layout Preset Database", order = 1)]
    public class EnemyLayoutPresetDatabase : ScriptableObject
    {
        public const string ResourcesLoadPath = "GameData/EnemyLayoutPresetDatabase";

        [Tooltip("可在此拖入多个 EnemyEncounterLayoutPreset")]
        public List<EnemyEncounterLayoutPreset> presets = new List<EnemyEncounterLayoutPreset>();

        private static EnemyLayoutPresetDatabase _cached;

        public static EnemyLayoutPresetDatabase LoadOrNull()
        {
            if (_cached == null)
                _cached = Resources.Load<EnemyLayoutPresetDatabase>(ResourcesLoadPath);
            return _cached;
        }

        /// <summary> 编辑器下可清空缓存以便重新加载 </summary>
        public static void ClearRuntimeCache()
        {
            _cached = null;
        }

        public EnemyEncounterLayoutPreset GetPreset(long enemyRefId)
        {
            if (presets == null) return null;
            return presets.Find(p => p != null && p.enemyRefId == enemyRefId);
        }
    }
}
