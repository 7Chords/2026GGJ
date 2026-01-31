using System;
using System.Collections.Generic;
using GameCore.Logic.Parts;
using UnityEngine; // 引用具体逻辑类命名空间

namespace GameCore.Logic
{
    public static class PartLogicFactory
    {
        private static Dictionary<string, Type> _logicTypeMap;

        public static void Initialize()
        {
            if (_logicTypeMap != null) return;

            _logicTypeMap = new Dictionary<string, Type>();
            
            // 2. 注册逻辑类
            // 建议：后续可以使用反射自动注册所有继承自 BasePartLogic 的类
            RegisterLogic("NoseLogic", typeof(NoseLogic));
            RegisterLogic("MouthLogic", typeof(MouthLogic));
            RegisterLogic("EyeLogic", typeof(EyeLogic));
        }

        private static void RegisterLogic(string name, Type type)
        {
            if (!_logicTypeMap.ContainsKey(name))
            {
                _logicTypeMap.Add(name, type);
            }
        }

        public static BasePartLogic CreateLogic(string logicName)
        {
            if (_logicTypeMap == null) Initialize();

            if (string.IsNullOrEmpty(logicName)) return null;

            if (_logicTypeMap.TryGetValue(logicName, out Type type))
            {
                return Activator.CreateInstance(type) as BasePartLogic;
            }
            
            Debug.LogWarning($"[PartLogicFactory] Logic class not found in registry: {logicName}");
            
            // 如果字典里没找到，尝试通过反射查找（可选保底）
            // Type t = Type.GetType("GameCore.Logic.Parts." + logicName);
            // if (t != null) return Activator.CreateInstance(t) as BasePartLogic;

            return null;
        }
    }
}
