using GameCore;
using UnityEngine;

namespace GameCore.Editor
{
    /// <summary>
    /// 在编辑器中加载 Resources/RefData/ExportTxt 配表（与运行时 SCRefDataMgr 一致）。
    /// </summary>
    public static class EnemyLayoutEditorRefDataUtility
    {
        static bool _loaded;

        public static void EnsureRefDataLoaded()
        {
            if (_loaded) return;
            try
            {
                SCRefDataMgr.instance.OnInitialize();
                _loaded = true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[EnemyLayoutEditor] 配表加载失败：{e.Message}");
            }
        }

        public static void ForceReload()
        {
            _loaded = false;
            EnsureRefDataLoaded();
        }
    }
}
