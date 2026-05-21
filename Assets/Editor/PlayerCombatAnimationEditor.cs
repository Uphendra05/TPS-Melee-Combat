using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(MonoBehaviour), true)]
[CanEditMultipleObjects]
public class PlayerDataEditor : Editor
{
    private Dictionary<string, List<SerializedProperty>> sections = new();
    private Dictionary<string, bool> sectionStates = new();

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        BuildSectionsWithInheritance();
        DrawTabBar();
        DrawActiveSections();

        serializedObject.ApplyModifiedProperties();
    }

    // ---------------- CORE MAGIC ----------------
    private void BuildSectionsWithInheritance()
    {
        sections.Clear();

        string currentSection = "Default";

        SerializedProperty prop = serializedObject.GetIterator();
        bool enterChildren = true;

        while (prop.NextVisible(enterChildren))
        {
            enterChildren = false;

            if (prop.name == "m_Script")
                continue;

            string foundSection = GetSection(prop);

            // REGION INHERITANCE LOGIC
            if (!string.IsNullOrEmpty(foundSection))
            {
                currentSection = foundSection;
            }

            if (!sections.ContainsKey(currentSection))
                sections[currentSection] = new List<SerializedProperty>();

            sections[currentSection].Add(serializedObject.FindProperty(prop.name));
        }

        // init states
        foreach (var key in sections.Keys)
        {
            if (!sectionStates.ContainsKey(key))
                sectionStates[key] = false;
        }

        if (sectionStates.Count > 0 && !AnyActive())
        {
            var first = new List<string>(sectionStates.Keys)[0];
            sectionStates[first] = true;
        }
    }

    // ---------------- TAB BAR ----------------
    private void DrawTabBar()
    {
        if (sections == null || sections.Count == 0)
            return;

        float windowWidth = EditorGUIUtility.currentViewWidth;
        float padding = 10f;
        float spacing = 6f;

        float x = 0;

        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginHorizontal();

        foreach (var section in sections)
        {
            bool isActive = sectionStates[section.Key];

            GUIStyle style = new GUIStyle(GUI.skin.button)
            {
                fontStyle = FontStyle.Bold,
                fixedHeight = 28
            };

            // estimate button width based on text
            float buttonWidth = GUI.skin.button.CalcSize(new GUIContent(section.Key)).x + 20f;

            // WRAP CHECK → move to next row
            if (x + buttonWidth > windowWidth - padding)
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                x = 0;
            }

            if (isActive)
                GUI.backgroundColor = Color.cyan;

            if (GUILayout.Button(section.Key, style, GUILayout.ExpandWidth(true)))
            {
                SetOnlyActive(section.Key);
            }

            GUI.backgroundColor = Color.white;

            x += buttonWidth + spacing;
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

    }

    // ---------------- DRAW CONTENT ----------------
    private void DrawActiveSections()
    {
        foreach (var section in sections)
        {
            if (!sectionStates.TryGetValue(section.Key, out bool active) || !active)
                continue;

            EditorGUILayout.BeginVertical("box");

            foreach (var prop in section.Value)
            {
                EditorGUILayout.PropertyField(prop, true);
            }

            EditorGUILayout.EndVertical();
        }
    }

    // ---------------- HELPERS ----------------
    private void SetOnlyActive(string key)
    {
        var keys = new List<string>(sectionStates.Keys);

        foreach (var k in keys)
            sectionStates[k] = (k == key);
    }

    private bool AnyActive()
    {
        foreach (var v in sectionStates.Values)
            if (v) return true;
        return false;
    }

    private string GetSection(SerializedProperty prop)
    {
        var field = target.GetType().GetField(prop.name,
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);

        if (field == null)
            return "";

        var attr = (SectionAttribute)System.Attribute.GetCustomAttribute(
            field, typeof(SectionAttribute));

        return attr != null ? attr.name : "";
    }
}