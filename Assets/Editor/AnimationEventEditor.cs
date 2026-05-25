using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;


[CustomEditor(typeof(AnimationEventSO))]
public class AnimationEventEditor : Editor
{
    private AnimationEventSO _data;
    private PreviewRenderUtility _previewRenderer;
    private GameObject _previewInstance;
    private Animator _previewAnimator;
    private float _previewTime = 0f;
    private bool _isPlaying = false;
    private double _lastEditorTime;
    private AnimatorController _previewController;



    private void OnEnable()
    {
        _data = (AnimationEventSO)target;
       
    }

    private void OnDisable()
    {
        StopAnimationMode();
        CleanupPreview();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw default fields
        EditorGUILayout.PropertyField(serializedObject.FindProperty("clip"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("aniamtionSpeed"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Timeline", EditorStyles.boldLabel);

        DrawTimeline();

        EditorGUILayout.Space();
        DrawEventList();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawTimeline()
    {
        if (_data.clip == null) return;
        if (_data.events == null || _data.events.Count == 0) return;

        float duration = _data.clip.length;
        float rowHeight = 18f;
        float padding = 4f;
        float labelWidth = 150f;
        float totalHeight = _data.events.Count * (rowHeight + padding) + 30f; // +30 for ruler

        // IMPORTANT: Use GUILayout to reserve space, then draw in that rect during Repaint
        Rect timelineRect = GUILayoutUtility.GetRect(0, totalHeight, GUILayout.ExpandWidth(true));

        // Only draw during repaint, not layout
        if (Event.current.type != EventType.Repaint) return;

        // Background
        EditorGUI.DrawRect(timelineRect, new Color(0.15f, 0.15f, 0.15f));

        float trackAreaX = timelineRect.x + labelWidth;
        float trackAreaWidth = timelineRect.width - labelWidth;

        // Draw each event
        for (int i = 0; i < _data.events.Count; i++)
        {
            var ev = _data.events[i];
            float y = timelineRect.y + padding + i * (rowHeight + padding);

            // Label on the left
            Rect labelRect = new Rect(timelineRect.x + 4, y, labelWidth - 8, rowHeight);
            GUI.Label(labelRect, ev.name, new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = ev.color },
                fontStyle = FontStyle.Bold
            });

            // Map times to pixel positions
            float tStart = Mathf.Clamp01(ev.startTime / duration);
            float tEnd = Mathf.Clamp01(ev.endTime / duration);

            float xStart = trackAreaX + tStart * trackAreaWidth;
            float xEnd = trackAreaX + tEnd * trackAreaWidth;
            float barWidth = Mathf.Max(xEnd - xStart, 6f); // min 6px so instant events show

            Rect barRect = new Rect(xStart, y, barWidth, rowHeight);
            EditorGUI.DrawRect(barRect, ev.color);

            // Darker border on bar
            EditorGUI.DrawRect(new Rect(xStart, y, barWidth, 1), Color.black);
            EditorGUI.DrawRect(new Rect(xStart, y + rowHeight - 1, barWidth, 1), Color.black);
        }

        // Time ruler at the bottom
        Rect rulerRect = new Rect(timelineRect.x, timelineRect.yMax - 22, timelineRect.width, 22);
        EditorGUI.DrawRect(rulerRect, new Color(0.1f, 0.1f, 0.1f));

        int steps = 10;
        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps * duration;
            float x = trackAreaX + (i / (float)steps) * trackAreaWidth;

            // Tick mark
            EditorGUI.DrawRect(new Rect(x, rulerRect.y, 1, 6), Color.gray);

            // Label
            GUI.Label(
                new Rect(x - 15, rulerRect.y + 6, 30, 14),
                t.ToString("F2"),
                EditorStyles.centeredGreyMiniLabel
            );
        }
    }

    private void DrawEventList()
    {
        EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
        var eventsProp = serializedObject.FindProperty("events");

        for (int i = 0; i < eventsProp.arraySize; i++)
        {
            var element = eventsProp.GetArrayElementAtIndex(i);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(element.FindPropertyRelative("name"), GUIContent.none, GUILayout.Width(150));
            EditorGUILayout.PropertyField(element.FindPropertyRelative("startTime"), GUIContent.none, GUILayout.Width(50));
            EditorGUILayout.PropertyField(element.FindPropertyRelative("endTime"), GUIContent.none, GUILayout.Width(50));
            EditorGUILayout.PropertyField(element.FindPropertyRelative("color"), GUIContent.none, GUILayout.Width(60));
            if (GUILayout.Button("X", GUILayout.Width(20)))
                eventsProp.DeleteArrayElementAtIndex(i);
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Add Event"))
            eventsProp.InsertArrayElementAtIndex(eventsProp.arraySize);
    }

    private void EnsurePreviewReady()
    {
        if (_previewRenderer != null && _previewInstance != null) return;

        CleanupPreview();

        _previewRenderer = new PreviewRenderUtility();
        _previewRenderer.camera.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
        _previewRenderer.camera.nearClipPlane = 0.01f;
        _previewRenderer.camera.farClipPlane = 100f;
        _previewRenderer.camera.transform.position = new Vector3(-0.8f, 1.2f, 9.5f);
        _previewRenderer.camera.transform.LookAt(new Vector3(0, 1f, 0));
        _previewRenderer.lights[0].intensity = 1f;
        _previewRenderer.lights[0].transform.rotation = Quaternion.Euler(30f, 30f, 0f);

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Adventure_Character/Prefabs/Man_01.prefab");
        if (prefab == null)
        {
            Debug.LogError("Prefab not found!");
            return;
        }

        _previewInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, _previewRenderer.camera.scene);
        _previewInstance.transform.position = Vector3.zero;
        _previewInstance.transform.rotation = Quaternion.identity;
        _previewInstance.hideFlags = HideFlags.HideAndDontSave;
    }

    private void SampleClipAtTime(float time)
    {
        if (_previewInstance == null || _data.clip == null) return;

        // AnimationMode is how Unity's own Animation window drives previews
        AnimationMode.StartAnimationMode();
        AnimationMode.BeginSampling();
        AnimationMode.SampleAnimationClip(_previewInstance, _data.clip, time);
        AnimationMode.EndSampling();
    }

    private void StopAnimationMode()
    {
        if (AnimationMode.InAnimationMode())
            AnimationMode.StopAnimationMode();
    }

    public override bool HasPreviewGUI() => _data != null && _data.clip != null;

    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        if (_data == null || _data.clip == null) return;

        if (Event.current.type == EventType.Repaint)
        {
            EnsurePreviewReady();
            if (_previewRenderer == null || _previewInstance == null) return;

            Rect previewRect = new Rect(r.x, r.y, r.width, r.height - 30);

            // This is the key — drives the pose correctly for ALL rig types
            SampleClipAtTime(_previewTime);

            _previewRenderer.BeginPreview(previewRect, background);
            _previewRenderer.camera.Render();
            var tex = _previewRenderer.EndPreview();
            GUI.DrawTexture(previewRect, tex, ScaleMode.ScaleToFit);
        }

        // Controls
        Rect controlRect = new Rect(r.x, r.yMax - 28, r.width, 28);

        if (GUI.Button(new Rect(controlRect.x + 4, controlRect.y + 4, 55, 20),
            _isPlaying ? "Pause" : "Play"))
        {
            _isPlaying = !_isPlaying;
            _lastEditorTime = EditorApplication.timeSinceStartup;
        }

        float newTime = GUI.HorizontalSlider(
            new Rect(controlRect.x + 65, controlRect.y + 8, controlRect.width - 70, 16),
            _previewTime, 0f, _data.clip.length);

        if (Mathf.Abs(newTime - _previewTime) > 0.001f)
        {
            _previewTime = newTime;
            _isPlaying = false;
        }

        if (_isPlaying)
        {
            double now = EditorApplication.timeSinceStartup;
            _previewTime += (float)(now - _lastEditorTime) * _data.aniamtionSpeed;
            _lastEditorTime = now;
            if (_previewTime > _data.clip.length) _previewTime = 0f;
            Repaint();
        }
    }

    private void CleanupPreview()
    {
        StopAnimationMode();

        if (_previewInstance != null)
        {
            DestroyImmediate(_previewInstance);
            _previewInstance = null;
        }

        _previewRenderer?.Cleanup();
        _previewRenderer = null;
    }
}

