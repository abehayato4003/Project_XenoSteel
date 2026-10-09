using UnityEditor;
using UnityEngine;
using XenoSteel.Combat;
using System;
using System.Linq;

[CustomEditor(typeof(SkillData))]
public class SkillDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawPropertiesExcluding(
            serializedObject,
            "m_Script",
            "effects"
        );

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Effects", EditorStyles.boldLabel);

        SerializedProperty effects =
            serializedObject.FindProperty("effects");

        Type[] effectTypes = TypeCache
            .GetTypesDerivedFrom<XenoSteelEffect>()
            .Where(t => !t.IsAbstract && !t.IsGenericType)
            .OrderBy(t => t.Name)
            .ToArray();

        string[] options = effectTypes
            .Select(t => t.Name)
            .ToArray();

        for (int i = 0; i < effects.arraySize; i++)
        {
            SerializedProperty element =
                effects.GetArrayElementAtIndex(i);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            Type currentType =
                element.managedReferenceValue?.GetType();

            EditorGUILayout.LabelField(
                $"Effect {i + 1}: {currentType?.Name ?? "(未設定)"}",
                EditorStyles.boldLabel
            );

            int selectedIndex = Array.FindIndex(
                effectTypes,
                t => t == currentType
            );

            int newIndex = EditorGUILayout.Popup(
                "Effect Type",
                selectedIndex,
                options
            );

            if (newIndex >= 0 && newIndex != selectedIndex)
            {
                element.managedReferenceValue =
                    Activator.CreateInstance(effectTypes[newIndex]);
            }

            if (element.managedReferenceValue != null)
            {
                EditorGUILayout.PropertyField(
                    element,
                    GUIContent.none,
                    true
                );
            }

            if (GUILayout.Button("削除"))
            {
                effects.DeleteArrayElementAtIndex(i);
                EditorGUILayout.EndVertical();
                break;
            }

            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("Effectを追加"))
        {
            int index = effects.arraySize;
            effects.InsertArrayElementAtIndex(index);
            effects.GetArrayElementAtIndex(index)
                .managedReferenceValue = null;
        }

        serializedObject.ApplyModifiedProperties();
    }
}