using System;
using System.Collections.Generic;
using UnityEngine;

public enum EModuleType
{
    MAP,
    COMBAT,
    EVENT,
    LOOT
}

namespace GameCore
{
    public static class RandomUtility
    {

        private const string _m_globalSeed = "FACEMASK";

        private static Dictionary<EModuleType, System.Random> _randomGenerators = new Dictionary<EModuleType, System.Random>();

        static RandomUtility()
        {
            InitializeGenerators();
        }

        private static void InitializeGenerators()
        {
            int seedHash = _m_globalSeed.GetHashCode();
            foreach (EModuleType type in Enum.GetValues(typeof(EModuleType)))
            {
                _randomGenerators[type] = new System.Random(seedHash + (int)type);
            }
        }

        public static System.Random GetRandomGenerator(EModuleType module)
        {
            if (!_randomGenerators.ContainsKey(module))
            {
                _randomGenerators[module] = new System.Random();
            }
            return _randomGenerators[module];
        }

        /// <summary>
        /// 替换指定模块的随机数生成器（用于新游戏时重新随机地图路线等，避免固定全局种子导致地图总相同）。
        /// </summary>
        public static void ReseedModule(EModuleType module, int seed)
        {
            _randomGenerators[module] = new System.Random(seed);
        }
    }
}
