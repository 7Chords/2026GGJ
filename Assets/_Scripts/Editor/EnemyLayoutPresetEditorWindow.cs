using GameCore;
using GameCore.Data;
using GameCore.Helpers;
using GameCore.RefData;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameCore.Editor
{
    /// <summary>
    /// 可视化编辑 <see cref="EnemyLayoutPreset"/>：网格、禁用格、部位原点与旋转、重叠检测。
    /// </summary>
    public class EnemyLayoutPresetEditorWindow : EditorWindow
    {
        const float CellPx = 26f;

        EnemyLayoutPreset _preset;
        SerializedObject _serializedObject;
        Vector2 _scroll;
        int _turnIndex;
        int _editVariant; // 0 = enemy acts first (先手), 1 = enemy acts second (后手)
        int _selectedSlotIndex = -1;
        long _newSlotPartLevelId;
        int _newSlotRotation;
        long _filterEnemyTableId; // 0 = 显示全部 part_level；否则按敌人 initPart 过滤

        [MenuItem("Tools/Game/敌人脸部布局编辑器")]
        public static void OpenFromMenu()
        {
            // 停靠在 Scene 窗口旁（单参数重载：params Type[] desiredDockNextTo）
            var w = GetWindow<EnemyLayoutPresetEditorWindow>(typeof(SceneView));
            w.titleContent = new GUIContent("敌人脸部布局");
            w.minSize = new Vector2(520, 460);
            w.Show();
        }

        public static void Open(EnemyLayoutPreset preset)
        {
            var w = GetWindow<EnemyLayoutPresetEditorWindow>(typeof(SceneView));
            w.titleContent = new GUIContent("敌人脸部布局");
            w.minSize = new Vector2(520, 460);
            w._preset = preset;
            w._serializedObject = preset != null ? new SerializedObject(preset) : null;
            w.Show();
        }

        void OnEnable()
        {
            if (_preset != null)
                _serializedObject = new SerializedObject(_preset);
        }

        void OnGUI()
        {
            EnemyLayoutEditorRefDataUtility.EnsureRefDataLoaded();

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("敌人脸部布局预设", EditorStyles.boldLabel);

            var newPreset = (EnemyLayoutPreset)EditorGUILayout.ObjectField("预设资源", _preset, typeof(EnemyLayoutPreset), false);
            if (newPreset != _preset)
            {
                _preset = newPreset;
                _serializedObject = _preset != null ? new SerializedObject(_preset) : null;
                _selectedSlotIndex = -1;
            }

            if (_preset == null || _serializedObject == null)
            {
                EditorGUILayout.HelpBox("请拖入或选择 EnemyLayoutPreset 资源。", MessageType.Info);
                return;
            }

            _serializedObject.Update();

            EditorGUILayout.PropertyField(_serializedObject.FindProperty("gridSize"), true);
            EditorGUILayout.PropertyField(_serializedObject.FindProperty("disabledGridPositions"), true);

            SerializedProperty firstProp = _serializedObject.FindProperty("turnLayoutsEnemyActsFirst");
            SerializedProperty secondProp = _serializedObject.FindProperty("turnLayoutsEnemyActsSecond");
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("回合布局（先手 / 后手）", EditorStyles.boldLabel);
            _editVariant = GUILayout.Toolbar(_editVariant, new[] { "敌人先手（敌方先动）", "敌人后手（玩家先动）" });
            SerializedProperty turnLayoutsProp = _editVariant == 0 ? firstProp : secondProp;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("＋ 回合", GUILayout.Width(72)))
            {
                firstProp.arraySize++;
                secondProp.arraySize++;
                _turnIndex = firstProp.arraySize - 1;
                _selectedSlotIndex = -1;
            }
            if (GUILayout.Button("－ 回合", GUILayout.Width(72)) && firstProp.arraySize > 0)
            {
                int idx = Mathf.Clamp(_turnIndex, 0, firstProp.arraySize - 1);
                firstProp.DeleteArrayElementAtIndex(idx);
                if (secondProp.arraySize > idx)
                    secondProp.DeleteArrayElementAtIndex(idx);
                _turnIndex = Mathf.Clamp(_turnIndex, 0, Mathf.Max(0, firstProp.arraySize - 1));
                _selectedSlotIndex = -1;
            }
            if (GUILayout.Button("后手 ← 全部复制先手", GUILayout.Width(160)))
            {
                DuplicateFirstLayoutsOntoSecond(firstProp, secondProp);
                _selectedSlotIndex = -1;
            }
            EditorGUILayout.EndHorizontal();

            if (firstProp.arraySize == 0)
            {
                EditorGUILayout.HelpBox("请先添加至少一个「回合」布局（先手、后手列表会同步长度）。", MessageType.Warning);
                _serializedObject.ApplyModifiedProperties();
                return;
            }

            if (secondProp.arraySize != firstProp.arraySize)
                EditorGUILayout.HelpBox(
                    $"先手 {firstProp.arraySize} 回合，后手 {secondProp.arraySize} 回合：运行时缺索引会回退为先手；可用「后手 ← 全部复制先手」对齐。",
                    MessageType.Info);

            _turnIndex = Mathf.Clamp(_turnIndex, 0, firstProp.arraySize - 1);
            _turnIndex = EditorGUILayout.IntSlider("当前编辑回合索引", _turnIndex, 0, firstProp.arraySize - 1);

            if (_turnIndex >= turnLayoutsProp.arraySize)
            {
                EditorGUILayout.HelpBox("当前列表在此索引无条目，正在编辑先手条目的对应索引。", MessageType.Warning);
                _serializedObject.ApplyModifiedProperties();
                return;
            }

            SerializedProperty turnProp = turnLayoutsProp.GetArrayElementAtIndex(_turnIndex);
            SerializedProperty slotsProp = turnProp.FindPropertyRelative("slots");

            EditorGUILayout.Space(6);
            DrawEnemyFilterToolbar();
            DrawNewSlotToolbar(slotsProp);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawFaceGrid(slotsProp);
            EditorGUILayout.Space(8);
            DrawSlotList(slotsProp);
            EditorGUILayout.EndScrollView();

            DrawValidationHelp(slotsProp);

            _serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(_preset);
            }
        }

        void DrawEnemyFilterToolbar()
        {
            EditorGUILayout.LabelField("部位列表（可选：按敌人 initPart 过滤）", EditorStyles.miniBoldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("敌人表 id（0=显示全部 part_level）", GUILayout.Width(200));
            _filterEnemyTableId = EditorGUILayout.LongField(_filterEnemyTableId);
            EditorGUILayout.EndHorizontal();
        }

        void DrawNewSlotToolbar(SerializedProperty slotsProp)
        {
            EditorGUILayout.BeginHorizontal();
            var plList = BuildPartLevelDropdownList();
            int selected = 0;
            for (int i = 0; i < plList.Count; i++)
            {
                if (plList[i].id == _newSlotPartLevelId) { selected = i; break; }
            }
            if (plList.Count == 0)
            {
                EditorGUILayout.HelpBox("未加载到 part_level 配表。", MessageType.Warning);
                EditorGUILayout.EndHorizontal();
                return;
            }

            string[] names = new string[plList.Count];
            for (int i = 0; i < plList.Count; i++)
                names[i] = $"{plList[i].id} · {GetPartName(plList[i])} Lv{plList[i].partLevel}";

            selected = EditorGUILayout.Popup("添加部位", selected, names);
            _newSlotPartLevelId = plList[selected].id;
            _newSlotRotation = EditorGUILayout.IntSlider("旋转", _newSlotRotation, 0, 3);

            if (GUILayout.Button("添加槽位", GUILayout.Width(80)))
            {
                slotsProp.arraySize++;
                var slot = slotsProp.GetArrayElementAtIndex(slotsProp.arraySize - 1);
                slot.FindPropertyRelative("partLevelRefId").longValue = _newSlotPartLevelId;
                slot.FindPropertyRelative("originFacePosition").vector2IntValue = Vector2Int.zero;
                slot.FindPropertyRelative("rotationSteps").intValue = _newSlotRotation;
                _selectedSlotIndex = slotsProp.arraySize - 1;
            }
            EditorGUILayout.EndHorizontal();
        }

        List<PartLevelRefObj> BuildPartLevelDropdownList()
        {
            var all = SCRefDataMgr.instance.partLevelRefList.refDataList;
            if (all == null || all.Count == 0) return new List<PartLevelRefObj>();

            if (_filterEnemyTableId <= 0)
                return new List<PartLevelRefObj>(all);

            var enemy = SCRefDataMgr.instance.enemyRefList.refDataList.Find(e => e.id == _filterEnemyTableId);
            if (enemy == null || enemy.initPartList == null)
                return new List<PartLevelRefObj>(all);

            var filtered = new List<PartLevelRefObj>();
            foreach (var pe in enemy.initPartList)
            {
                for (int n = 0; n < pe.partAmount; n++)
                {
                    var pl = all.Find(x => x.id == pe.partLevelId);
                    if (pl != null && !filtered.Exists(x => x.id == pl.id))
                        filtered.Add(pl);
                }
            }
            return filtered.Count > 0 ? filtered : new List<PartLevelRefObj>(all);
        }

        static string GetPartName(PartLevelRefObj pl)
        {
            var pr = SCRefDataMgr.instance.partRefList.refDataList.Find(x => x.id == pl.partId);
            return pr != null ? pr.partName : "?";
        }

        void DrawFaceGrid(SerializedProperty slotsProp)
        {
            int cols = Mathf.Max(1, _preset.gridSize.x);
            int rows = Mathf.Max(1, _preset.gridSize.y);
            var disabled = new HashSet<Vector2Int>(_preset.disabledGridPositions ?? new List<Vector2Int>());

            var occMap = BuildOccupancyMap(slotsProp, out var overlapCells);

            EditorGUILayout.LabelField("脸部网格（点击格子：为当前选中槽位设置原点）", EditorStyles.boldLabel);
            if (_selectedSlotIndex < 0)
                EditorGUILayout.HelpBox("请先在下方列表选中一个槽位，再点击网格设置原点。", MessageType.None);

            float w = cols * CellPx + 8;
            float h = rows * CellPx + 8;
            Rect outer = GUILayoutUtility.GetRect(w, h);
            EditorGUI.DrawRect(outer, new Color(0.15f, 0.15f, 0.15f));

            // 与运行时脸部网格一致：x 向右为正，y 向下为正（屏幕上方一行 y=0，向下递增）
            Event e = Event.current;
            for (int gy = 0; gy < rows; gy++)
            {
                int y = gy;
                for (int x = 0; x < cols; x++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    Rect cellRect = new Rect(outer.x + 4 + x * CellPx, outer.y + 4 + gy * CellPx, CellPx - 2, CellPx - 2);

                    Color bg = new Color(0.35f, 0.35f, 0.38f);
                    if (disabled.Contains(pos))
                        bg = new Color(0.12f, 0.12f, 0.12f);
                    else if (overlapCells.Contains(pos))
                        bg = new Color(0.85f, 0.2f, 0.2f);
                    else if (occMap.TryGetValue(pos, out int slotIndx))
                        bg = SlotColor(slotIndx);

                    EditorGUI.DrawRect(cellRect, bg);

                    if (occMap.TryGetValue(pos, out int sidx) && !overlapCells.Contains(pos))
                    {
                        var c = Color.white * 0.3f;
                        EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.y, cellRect.width, 1), c);
                        EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.yMax - 1, cellRect.width, 1), c);
                        EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.y, 1, cellRect.height), c);
                        EditorGUI.DrawRect(new Rect(cellRect.xMax - 1, cellRect.y, 1, cellRect.height), c);
                    }

                    if (e.type == EventType.MouseDown && e.button == 0 && cellRect.Contains(e.mousePosition))
                    {
                        if (_selectedSlotIndex >= 0 && _selectedSlotIndex < slotsProp.arraySize)
                        {
                            var slotProp = slotsProp.GetArrayElementAtIndex(_selectedSlotIndex);
                            slotProp.FindPropertyRelative("originFacePosition").vector2IntValue = pos;
                            e.Use();
                            GUI.changed = true;
                        }
                    }
                }
            }

            EditorGUILayout.LabelField("图例：灰=空 深灰=禁用 红=重叠 彩色=部位占用 · 坐标 y 向下为正（顶行 y=0）", EditorStyles.miniLabel);
        }

        static Color SlotColor(int slotIndex)
        {
            Color[] palette =
            {
                new Color(0.2f, 0.65f, 0.85f, 0.85f),
                new Color(0.35f, 0.8f, 0.4f, 0.85f),
                new Color(0.95f, 0.75f, 0.25f, 0.85f),
                new Color(0.85f, 0.45f, 0.85f, 0.85f),
                new Color(0.55f, 0.55f, 0.9f, 0.85f),
            };
            return palette[slotIndex % palette.Length];
        }

        Dictionary<Vector2Int, int> BuildOccupancyMap(SerializedProperty slotsProp, out HashSet<Vector2Int> overlapCells)
        {
            overlapCells = new HashSet<Vector2Int>();
            var map = new Dictionary<Vector2Int, int>();
            var count = new Dictionary<Vector2Int, int>();

            for (int i = 0; i < slotsProp.arraySize; i++)
            {
                var slotProp = slotsProp.GetArrayElementAtIndex(i);
                long plId = slotProp.FindPropertyRelative("partLevelRefId").longValue;
                Vector2Int origin = slotProp.FindPropertyRelative("originFacePosition").vector2IntValue;
                int rot = slotProp.FindPropertyRelative("rotationSteps").intValue;

                var partRef = EnemyLayoutGeometryHelper.ResolvePartRefForLevel(
                    plId,
                    SCRefDataMgr.instance.partLevelRefList.refDataList,
                    SCRefDataMgr.instance.partRefList.refDataList);
                if (partRef == null) continue;

                var cells = EnemyLayoutGeometryHelper.GetOccupiedFaceCells(origin, partRef, rot);
                foreach (var p in cells)
                {
                    if (!count.ContainsKey(p)) count[p] = 0;
                    count[p]++;
                    if (!map.ContainsKey(p))
                        map[p] = i;
                }
            }

            foreach (var kv in count)
            {
                if (kv.Value > 1)
                    overlapCells.Add(kv.Key);
            }

            return map;
        }

        void DrawSlotList(SerializedProperty slotsProp)
        {
            EditorGUILayout.LabelField("槽位列表", EditorStyles.boldLabel);
            for (int i = 0; i < slotsProp.arraySize; i++)
            {
                var slotProp = slotsProp.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                bool sel = _selectedSlotIndex == i;
                if (GUILayout.Toggle(sel, $"槽位 {i}", "Button", GUILayout.Width(72)))
                {
                    _selectedSlotIndex = i;
                }
                if (GUILayout.Button("删除", GUILayout.Width(48)))
                {
                    slotsProp.DeleteArrayElementAtIndex(i);
                    if (_selectedSlotIndex == i) _selectedSlotIndex = -1;
                    break;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(slotProp.FindPropertyRelative("partLevelRefId"), new GUIContent("part_level id"));
                EditorGUILayout.PropertyField(slotProp.FindPropertyRelative("originFacePosition"), new GUIContent("原点"));
                EditorGUILayout.PropertyField(slotProp.FindPropertyRelative("rotationSteps"), new GUIContent("旋转(0~3)"));
                EditorGUILayout.EndVertical();
            }
        }

        void DrawValidationHelp(SerializedProperty slotsProp)
        {
            var slots = new List<EnemyLayoutSlot>();
            for (int i = 0; i < slotsProp.arraySize; i++)
            {
                var sp = slotsProp.GetArrayElementAtIndex(i);
                slots.Add(new EnemyLayoutSlot
                {
                    partLevelRefId = sp.FindPropertyRelative("partLevelRefId").longValue,
                    originFacePosition = sp.FindPropertyRelative("originFacePosition").vector2IntValue,
                    rotationSteps = sp.FindPropertyRelative("rotationSteps").intValue
                });
            }

            if (slots.Count == 0)
            {
                EditorGUILayout.HelpBox("当前回合无槽位（可为空布局）。", MessageType.Info);
                return;
            }

            var disabled = new HashSet<Vector2Int>(_preset.disabledGridPositions ?? new List<Vector2Int>());
            bool ok = EnemyLayoutGeometryHelper.ValidateLayout(
                slots,
                _preset.gridSize.x,
                _preset.gridSize.y,
                disabled,
                plId => EnemyLayoutGeometryHelper.ResolvePartRefForLevel(
                    plId,
                    SCRefDataMgr.instance.partLevelRefList.refDataList,
                    SCRefDataMgr.instance.partRefList.refDataList),
                out string err);

            if (ok)
                EditorGUILayout.HelpBox("校验通过：无越界、禁用格冲突与占用重叠。", MessageType.Info);
            else
                EditorGUILayout.HelpBox("校验失败：" + err, MessageType.Error);
        }

        static void DuplicateFirstLayoutsOntoSecond(SerializedProperty firstList, SerializedProperty secondList)
        {
            if (firstList == null || secondList == null)
                return;
            secondList.arraySize = 0;
            secondList.arraySize = firstList.arraySize;
            for (int i = 0; i < firstList.arraySize; i++)
                CopyTurnFaceLayoutSerialized(firstList.GetArrayElementAtIndex(i), secondList.GetArrayElementAtIndex(i));
        }

        static void CopyTurnFaceLayoutSerialized(SerializedProperty src, SerializedProperty dst)
        {
            SerializedProperty srcSlots = src.FindPropertyRelative("slots");
            SerializedProperty dstSlots = dst.FindPropertyRelative("slots");
            if (srcSlots == null || dstSlots == null)
                return;
            dstSlots.arraySize = 0;
            dstSlots.arraySize = srcSlots.arraySize;
            for (int i = 0; i < srcSlots.arraySize; i++)
            {
                SerializedProperty s = srcSlots.GetArrayElementAtIndex(i);
                SerializedProperty d = dstSlots.GetArrayElementAtIndex(i);
                d.FindPropertyRelative("partLevelRefId").longValue = s.FindPropertyRelative("partLevelRefId").longValue;
                d.FindPropertyRelative("originFacePosition").vector2IntValue = s.FindPropertyRelative("originFacePosition").vector2IntValue;
                d.FindPropertyRelative("rotationSteps").intValue = s.FindPropertyRelative("rotationSteps").intValue;
            }
        }
    }
}
