/*
 *  Comment     :
 */

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
namespace RapidFramework
{
    [CustomPropertyDrawer(typeof(Observable<>), true)]
    public class ObservableDrawer : PropertyDrawer
    {
        public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label
        )
        {
            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            SerializedProperty valueProp = property.FindPropertyRelative("_value");

            if (valueProp != null)
                EditorGUI.PropertyField(position, valueProp, GUIContent.none, true);
            else
                EditorGUI.LabelField(position, label.text, "Use [SerializeField] on _value field.");

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty valueProp = property.FindPropertyRelative("_value");

            return valueProp != null 
                ? EditorGUI.GetPropertyHeight(valueProp, label, true) 
                : EditorGUIUtility.singleLineHeight;
        }
    }
}
#endif