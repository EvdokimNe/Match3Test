using System.Collections.Generic;
using Match3.Board.View;
using UnityEditor;
using UnityEngine;

namespace Match3.Configs.Editor
{
    [CustomEditor(typeof(LevelConfig))]
    public sealed class LevelConfigEditor : UnityEditor.Editor
    {
        private const float CellSize = 30f;
        private static readonly Color EmptyCellColor = new(0.22f, 0.22f, 0.22f);
        private static readonly Color EraserColor = new(0.35f, 0.35f, 0.35f);
        private static readonly Color GridLineColor = new(0.12f, 0.12f, 0.12f);
        private static readonly Color SelectionColor = new(1f, 0.85f, 0.2f);

        private SerializedProperty _width;
        private SerializedProperty _height;
        private SerializedProperty _entries;

        private readonly List<BlockConfig> _palette = new();
        private readonly Dictionary<BlockConfig, Color> _paletteColors = new();
        private readonly Dictionary<Vector2Int, BlockConfig> _cellLookup = new();

        private BlockConfig _brush;

        private void OnEnable()
        {
            _width = serializedObject.FindProperty("_width");
            _height = serializedObject.FindProperty("_height");
            _entries = serializedObject.FindProperty("_entries");
            ReloadPalette();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_width);
            EditorGUILayout.PropertyField(_height);

            EditorGUILayout.Space();
            DrawPalette();

            EditorGUILayout.Space();
            DrawGrid();

            EditorGUILayout.Space();
            DrawTools();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawPalette()
        {
            EditorGUILayout.LabelField("Кисть", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawBrushButton(null, "Ластик", EraserColor);

                for (var i = 0; i < _palette.Count; i++)
                {
                    var block = _palette[i];
                    DrawBrushButton(block, block.name, BlockColor(block));
                }
            }

            if (GUILayout.Button("Обновить палитру", GUILayout.Width(160f)))
                ReloadPalette();
        }

        private void DrawBrushButton(BlockConfig block, string label, Color color)
        {
            var rect = GUILayoutUtility.GetRect(new GUIContent(label), EditorStyles.miniButton, GUILayout.Height(26f));

            EditorGUI.DrawRect(rect, color);

            if (_brush == block)
                DrawOutline(rect, SelectionColor, 2f);

            GUI.Label(rect, label, LabelStyle(color));

            if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
                _brush = block;
        }

        private void DrawGrid()
        {
            var width = _width.intValue;
            var height = _height.intValue;
            if (width <= 0 || height <= 0)
                return;

            RebuildLookup();

            for (var y = height - 1; y >= 0; y--)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    for (var x = 0; x < width; x++)
                        DrawCell(new Vector2Int(x, y));
                }
            }
        }

        private void DrawCell(Vector2Int cell)
        {
            _cellLookup.TryGetValue(cell, out var block);

            var rect = GUILayoutUtility.GetRect(CellSize, CellSize, GUILayout.Width(CellSize), GUILayout.Height(CellSize));

            EditorGUI.DrawRect(rect, GridLineColor);
            EditorGUI.DrawRect(Inset(rect, 1f), block != null ? BlockColor(block) : EmptyCellColor);

            if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
                Paint(cell);
        }

        private static Rect Inset(Rect rect, float amount) =>
            new(rect.x + amount, rect.y + amount, rect.width - amount * 2f, rect.height - amount * 2f);

        private static void DrawOutline(Rect rect, Color color, float thickness)
        {
            EditorGUI.DrawRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
            EditorGUI.DrawRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
        }

        private static GUIStyle LabelStyle(Color background)
        {
            var luminance = background.r * 0.299f + background.g * 0.587f + background.b * 0.114f;

            return new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = luminance > 0.55f ? Color.black : Color.white },
            };
        }

        private void DrawTools()
        {
            var outOfBounds = CountOutOfBounds();
            if (outOfBounds > 0 && GUILayout.Button($"Убрать блоки вне поля ({outOfBounds})"))
                PruneOutOfBounds();

            if (_entries.arraySize > 0 && GUILayout.Button("Очистить поле"))
                _entries.ClearArray();
        }

        private void Paint(Vector2Int cell)
        {
            var index = FindEntryIndex(cell);

            if (_brush == null)
            {
                if (index >= 0)
                    _entries.DeleteArrayElementAtIndex(index);
                return;
            }

            if (index < 0)
            {
                index = _entries.arraySize;
                _entries.InsertArrayElementAtIndex(index);
                var entry = _entries.GetArrayElementAtIndex(index);
                entry.FindPropertyRelative("Pos").vector2IntValue = cell;
                entry.FindPropertyRelative("Block").objectReferenceValue = _brush;
                return;
            }

            _entries.GetArrayElementAtIndex(index).FindPropertyRelative("Block").objectReferenceValue = _brush;
        }

        private int FindEntryIndex(Vector2Int cell)
        {
            for (var i = 0; i < _entries.arraySize; i++)
            {
                if (_entries.GetArrayElementAtIndex(i).FindPropertyRelative("Pos").vector2IntValue == cell)
                    return i;
            }

            return -1;
        }

        private void RebuildLookup()
        {
            _cellLookup.Clear();

            for (var i = 0; i < _entries.arraySize; i++)
            {
                var entry = _entries.GetArrayElementAtIndex(i);
                var pos = entry.FindPropertyRelative("Pos").vector2IntValue;
                _cellLookup[pos] = entry.FindPropertyRelative("Block").objectReferenceValue as BlockConfig;
            }
        }

        private int CountOutOfBounds()
        {
            var width = _width.intValue;
            var height = _height.intValue;
            var count = 0;

            for (var i = 0; i < _entries.arraySize; i++)
            {
                if (IsOutOfBounds(_entries.GetArrayElementAtIndex(i).FindPropertyRelative("Pos").vector2IntValue, width, height))
                    count++;
            }

            return count;
        }

        private void PruneOutOfBounds()
        {
            var width = _width.intValue;
            var height = _height.intValue;

            for (var i = _entries.arraySize - 1; i >= 0; i--)
            {
                if (IsOutOfBounds(_entries.GetArrayElementAtIndex(i).FindPropertyRelative("Pos").vector2IntValue, width, height))
                    _entries.DeleteArrayElementAtIndex(i);
            }
        }

        private static bool IsOutOfBounds(Vector2Int pos, int width, int height) =>
            pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height;

        private Color BlockColor(BlockConfig block) =>
            _paletteColors.TryGetValue(block, out var color) ? color : Color.magenta;

        private void ReloadPalette()
        {
            _palette.Clear();
            _paletteColors.Clear();

            foreach (var guid in AssetDatabase.FindAssets("t:BlockConfig"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var block = AssetDatabase.LoadAssetAtPath<BlockConfig>(path);
                if (block != null)
                    _palette.Add(block);
            }

            foreach (var guid in AssetDatabase.FindAssets("t:BlockVisualCatalog"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var catalog = AssetDatabase.LoadAssetAtPath<BlockVisualCatalog>(path);
                if (catalog == null)
                    continue;

                var entries = catalog.Entries;
                for (var i = 0; i < entries.Count; i++)
                {
                    if (entries[i].Block != null)
                        _paletteColors[entries[i].Block] = entries[i].DebugColor;
                }
            }
        }
    }
}
