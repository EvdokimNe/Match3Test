using UnityEditor;
using UnityEngine;

namespace Match3.Background.Editor
{
    [CustomEditor(typeof(BalloonZoneAuthoring))]
    public sealed class BalloonZoneAuthoringEditor : UnityEditor.Editor
    {
        private void OnSceneGUI()
        {
            var zone = (BalloonZoneAuthoring)target;

            var x = zone.transform.position.x;
            var oldBottom = zone.BottomY;
            var oldTop = zone.TopY;

            EditorGUI.BeginChangeCheck();

            var bottom = DragY(new Vector3(x, oldBottom));
            var top = DragY(new Vector3(x, oldTop));

            if (!EditorGUI.EndChangeCheck())
                return;

            Undo.RecordObject(zone, "Зона шаров");
            Undo.RecordObject(zone.transform, "Зона шаров");

            var position = zone.transform.position;
            zone.transform.position = new Vector3(position.x, (bottom + top) * 0.5f, position.z);

            serializedObject.Update();
            serializedObject.FindProperty("_height").floatValue = Mathf.Max(0f, top - bottom);
            serializedObject.ApplyModifiedProperties();
        }

        private static float DragY(Vector3 position)
        {
            var size = HandleUtility.GetHandleSize(position) * 0.08f;
            return Handles.Slider(position, Vector3.up, size, Handles.DotHandleCap, 0f).y;
        }
    }
}
