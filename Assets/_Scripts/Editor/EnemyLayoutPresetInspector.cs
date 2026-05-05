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
            var preset = (EnemyLayoutPreset)target;
            using (new UnityEditor.EditorGUI.DisabledScope(preset.turnLayoutsEnemyActsFirst == null || preset.turnLayoutsEnemyActsFirst.Count == 0))
            {
                if (GUILayout.Button("后手 ← 复制全部先手（覆盖后手列表）", GUILayout.Height(22)))
                {
                    Undo.RecordObject(preset, "Copy enemy-first layouts to enemy-second");
                    if (preset.turnLayoutsEnemyActsSecond == null)
                        preset.turnLayoutsEnemyActsSecond = new System.Collections.Generic.List<EnemyTurnFaceLayout>();
                    preset.turnLayoutsEnemyActsSecond.Clear();
                    for (int i = 0; i < preset.turnLayoutsEnemyActsFirst.Count; i++)
                    {
                        var src = preset.turnLayoutsEnemyActsFirst[i];
                        var copy = new EnemyTurnFaceLayout();
                        copy.slots = new System.Collections.Generic.List<EnemyLayoutSlot>();
                        if (src != null && src.slots != null)
                        {
                            for (int s = 0; s < src.slots.Count; s++)
                            {
                                var a = src.slots[s];
                                copy.slots.Add(new EnemyLayoutSlot
                                {
                                    partLevelRefId = a.partLevelRefId,
                                    originFacePosition = a.originFacePosition,
                                    rotationSteps = a.rotationSteps
                                });
                            }
                        }
                        preset.turnLayoutsEnemyActsSecond.Add(copy);
                    }
                    EditorUtility.SetDirty(preset);
                }
            }
            EditorGUILayout.Space(4);
            DrawDefaultInspector();
        }
    }
}
