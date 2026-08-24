using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EffectNode), true)]
public class EffectNodeDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);

        var node = property.managedReferenceValue as EffectNode;

        float lineHeight = EditorGUIUtility.singleLineHeight;
        Rect headerRect = new Rect(position.x, position.y, position.width, lineHeight);

        if (node != null) {
            // HEADER NAO EDITAVEL
            GUI.enabled = false;
            EditorGUI.TextField(headerRect, node.HeaderText);
            GUI.enabled = true;
        }

        Rect contentRect = new Rect(
            position.x,
            position.y + lineHeight + 2,
            position.width,
            position.height - lineHeight - 2
        );

        EditorGUI.PropertyField(contentRect, property, true);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return EditorGUI.GetPropertyHeight(property, true) + EditorGUIUtility.singleLineHeight + 2;
    }
}
