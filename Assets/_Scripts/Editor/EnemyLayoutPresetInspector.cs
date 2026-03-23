using GameCore.Data;
using UnityEditor;
using UnityEngine;

namespace GameCore.Editor
{
    [CustomEditor(typeof(EnemyLayoutPreset))]
    public class EnemyLayoutPresetInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("打开脸部布局编辑器", GUILayout.Height(28)))
            {
                EnemyLayoutPresetEditorWindow.Open((EnemyLayoutPreset)target);
            }
            EditorGUILayout.Space(4);
            DrawDefaultInspector();
        }
    }
}
