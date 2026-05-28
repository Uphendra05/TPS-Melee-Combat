using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;



[CustomEditor(typeof(BaseAnimationEventSO), editorForChildClasses: true)]
public class AnimationEventEditor : Editor
{
    private BaseAnimationEventSO _data;

    private PreviewRenderUtility _previewRenderer;
    private GameObject _previewInstance;
    private float _previewTime = 0f;
    private bool _isPlaying = false;
    private double _lastEditorTime;
    private bool _showTooltip = false;
    private Rect _tooltipRect;

    // Styles
    private GUIStyle _notifyTagStyle;
    private GUIStyle _stateTagStyle;
    private GUIStyle _headerStyle;
    private GUIStyle _titleStyle;
    private GUIStyle _targetStyle;
    private GUIStyle _infoIconStyle; 

    private SerializedProperty _clipProp;
    private List<Texture2D> _createdTextures = new();


    private static readonly System.Type[] NotifyTypes = TypeCache.GetTypesDerivedFrom<BaseAnimationNotify>()
        .Where(t => !t.IsAbstract).ToArray();

    private static readonly System.Type[] NotifyStateTypes = TypeCache.GetTypesDerivedFrom<BaseAnimationNotifyState>()
        .Where(t => !t.IsAbstract).ToArray();


    private void OnEnable()
    {
        _data = (BaseAnimationEventSO)target;

        _clipProp = serializedObject.FindProperty("clip");


        InitStyles();
    }

    private void OnDisable()
    {
        foreach (var tex in _createdTextures)
            if (tex != null) DestroyImmediate(tex);
        _createdTextures.Clear();

        _notifyTagStyle = null; 
        _stateTagStyle = null;
        _headerStyle = null;

        StopAnimationMode();
        CleanupPreview();
    }

    private void InitStyles()
    {
        if (_notifyTagStyle != null ) return;

        _notifyTagStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            normal = { textColor = Color.white, background = MakeTex(1, 1, new Color(0.3f, 0.3f, 0.3f)) },
            padding = new RectOffset(2, 2, 1, 1),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        _stateTagStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            normal = { textColor = Color.white, background = MakeTex(1, 1, new Color(0.3f, 0.3f, 0.3f)) },
            padding = new RectOffset(2, 2, 1, 1),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        _headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 11
        };

        _titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 13,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };

        _targetStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(1f, 1f, 1f, 0.5f) }
        };

        _infoIconStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
        {
            fontSize = 12,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(1f, 1f, 1f, 0.6f) },
            hover = { textColor = Color.white },
        };



    }

    private Texture2D MakeTex(int w, int h, Color col)
    {
        var tex = new Texture2D(w, h);
        tex.SetPixel(0, 0, col);
        tex.Apply();
        _createdTextures.Add(tex);
        return tex;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_clipProp);
        
        DrawChildProperties();

        EditorGUILayout.Space(8);


        // Begin bordered container
        Rect containerRect = EditorGUILayout.BeginVertical();

        // Border (drawn 1px outside)
        EditorGUI.DrawRect(new Rect(containerRect.x - 1, containerRect.y - 1, containerRect.width + 2, containerRect.height + 2),Color.white);

        // Background fill
        EditorGUI.DrawRect(containerRect, new Color(0.15f, 0.15f, 0.15f));


        GUILayout.Space(2);
        EditorGUILayout.LabelField("Timeline", _headerStyle);
        GUILayout.Space(4);
        DrawTimeline();

        GUILayout.Space(6);
        EditorGUILayout.LabelField("Events", _headerStyle);
        
        DrawEventList();
        GUILayout.Space(6);

        DrawTitleBanner();

        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();

        if (_isPlaying) Repaint();
    }

    private void DrawChildProperties()
    {
        SerializedProperty prop = serializedObject.GetIterator();
        prop.NextVisible(true);

        while (prop.NextVisible(false))
        {
            if (prop.name == "clip") continue;
            if (prop.name == "events") continue;

            EditorGUILayout.PropertyField(prop, true);
        }
    }

    private void DrawTimeline()
    {
        if (_data.clip == null)
        {
            EditorGUILayout.HelpBox("Assign an Animation Clip to see the timeline.", MessageType.Info);
            return;
        }

        float duration = _data.clip.length;
        float rowHeight = 20f;
        float padding = 4f;
        float labelWidth = 160f;
        int count = Mathf.Max(_data.events.Count, 1);
        float totalHeight = count * (rowHeight + padding) + 36f;

        Rect timelineRect = GUILayoutUtility.GetRect(0, totalHeight, GUILayout.ExpandWidth(true));

        if (Event.current.type != EventType.Repaint)
        {
            float trackAreaXInput = timelineRect.x + labelWidth;
            float trackAreaWidthInput = timelineRect.width - labelWidth;
            Rect seekRect = new Rect(trackAreaXInput, timelineRect.y, trackAreaWidthInput, timelineRect.height - 30);
            Event e = Event.current;
            if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && seekRect.Contains(e.mousePosition))
            {
                float normalized = (e.mousePosition.x - trackAreaXInput) / trackAreaWidthInput;
                _previewTime = Mathf.Clamp(normalized * duration, 0f, duration);
                _isPlaying = false;
                e.Use();
                Repaint();
            }
            return;
        }

        float trackAreaX = timelineRect.x + labelWidth;
        float trackAreaW = timelineRect.width - labelWidth;

        EditorGUI.DrawRect(timelineRect, new Color(0.13f, 0.13f, 0.13f));
        EditorGUI.DrawRect(new Rect(trackAreaX, timelineRect.y, trackAreaW, timelineRect.height - 28), new Color(0.18f, 0.18f, 0.18f));

        if (_data.events.Count == 0)
        {
            GUI.Label(new Rect(trackAreaX + 8, timelineRect.y + 8, trackAreaW, 20), "No events — add one below", EditorStyles.centeredGreyMiniLabel);
        }

        for (int i = 0; i < _data.events.Count; i++)
        {
            var ev = _data.events[i];
            float y = timelineRect.y + padding + i * (rowHeight + padding);
            bool isState = ev.IsState();

            EditorGUI.DrawRect(new Rect(timelineRect.x, y, timelineRect.width, rowHeight),
                i % 2 == 0 ? new Color(0.17f, 0.17f, 0.17f) : new Color(0.15f, 0.15f, 0.15f));

            Rect tagRect = new Rect(timelineRect.x + 2, y + 2, 52, rowHeight - 4);
            GUI.Label(tagRect, isState ? " ■ STATE " : " ♦ NOTIFY ", isState ? _stateTagStyle : _notifyTagStyle);

            Rect nameRect = new Rect(timelineRect.x + 58, y + 2, labelWidth - 62, rowHeight - 4);

            GUI.Label(nameRect, ev.name, new GUIStyle(EditorStyles.miniLabel)
            {
                normal = { textColor = ev.color },
                fontStyle = FontStyle.Bold,
                clipping = TextClipping.Clip
            });

            float tStart = Mathf.Clamp01(ev.GetStart() / duration);
            float tEnd = Mathf.Clamp01(ev.GetEnd() / duration);
            float xStart = trackAreaX + tStart * trackAreaW;
            float xEnd = trackAreaX + tEnd * trackAreaW;

            if (isState)
            {
                float barW = Mathf.Max(xEnd - xStart, 8f);
                EditorGUI.DrawRect(new Rect(xStart, y + 2, barW, rowHeight - 4), ev.color * 0.7f);
                EditorGUI.DrawRect(new Rect(xStart, y + 2, 2, rowHeight - 4), ev.color);
                EditorGUI.DrawRect(new Rect(xStart + barW - 2, y + 2, 2, rowHeight - 4), ev.color);
            }
            else
            {
                float cx = xStart;
                float cy = y + rowHeight * 0.5f;
                float s = 6f;
                Vector3[] diamond =
                {
                    new Vector3(cx,     cy - s),
                    new Vector3(cx + s, cy),
                    new Vector3(cx,     cy + s),
                    new Vector3(cx - s, cy),
                };
                Handles.color = ev.color;
                Handles.DrawAAConvexPolygon(diamond);
            }
        }

        // Ruler
        Rect rulerRect = new Rect(timelineRect.x, timelineRect.yMax - 26, timelineRect.width, 26);
        EditorGUI.DrawRect(rulerRect, new Color(0.1f, 0.1f, 0.1f));
        int steps = 10;
        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps * duration;
            float x = trackAreaX + (i / (float)steps) * trackAreaW;
            EditorGUI.DrawRect(new Rect(x, rulerRect.y, 1, 5), Color.gray);
            GUI.Label(new Rect(x - 15, rulerRect.y + 6, 30, 14), t.ToString("F2"), EditorStyles.centeredGreyMiniLabel);
        }

        // Scrubber
        float scrubX = trackAreaX + (_previewTime / duration) * trackAreaW;
        EditorGUI.DrawRect(new Rect(scrubX, timelineRect.y, 1, timelineRect.height - 26), new Color(1f, 0.9f, 0.2f, 0.9f));
        Vector3[] scrubDiamond =
        {
            new Vector3(scrubX,     timelineRect.y),
            new Vector3(scrubX + 5, timelineRect.y + 7),
            new Vector3(scrubX,     timelineRect.y + 14),
            new Vector3(scrubX - 5, timelineRect.y + 7),
        };
        Handles.color = new Color(1f, 0.9f, 0.2f);
        Handles.DrawAAConvexPolygon(scrubDiamond);
    }

    private void DrawEventList()
    {
        var eventsProp = serializedObject.FindProperty("events");

        for (int i = 0; i < eventsProp.arraySize; i++)
        {
            var element = eventsProp.GetArrayElementAtIndex(i);
            bool isState = element.FindPropertyRelative("eventType").enumValueIndex == 1;

            Rect cardRect = EditorGUILayout.BeginVertical();
            EditorGUI.DrawRect(
                new Rect(cardRect.x - 2, cardRect.y - 2, cardRect.width + 4, cardRect.height + 4),
                isState ? new Color(0.2f, 0.25f, 0.35f) : new Color(0.2f, 0.3f, 0.2f));

            // --- Row 1: badge | name | color | delete ---
            EditorGUILayout.BeginHorizontal();

            GUIStyle badge = isState ? _stateTagStyle : _notifyTagStyle;
            if (GUILayout.Button(isState ? "■ STATE" : "♦ NOTIFY", badge, GUILayout.Width(80)))
                element.FindPropertyRelative("eventType").enumValueIndex = isState ? 0 : 1;

            EditorGUILayout.PropertyField(element.FindPropertyRelative("name"), GUIContent.none, GUILayout.ExpandWidth(true));
            EditorGUILayout.PropertyField(element.FindPropertyRelative("color"), GUIContent.none, GUILayout.Width(44));

            bool deleted = GUILayout.Button("✕", GUILayout.Width(22));

            EditorGUILayout.EndHorizontal(); // always close row 1

            if (deleted)
            {
                eventsProp.DeleteArrayElementAtIndex(i);
                EditorGUILayout.EndVertical(); // close card
                break;
            }

            // --- Row 2: timing ---
            EditorGUILayout.BeginHorizontal();
            if (isState)
            {
                EditorGUILayout.LabelField("Start", GUILayout.Width(34));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("startTime"), GUIContent.none, GUILayout.Width(44));
                EditorGUILayout.LabelField("End", GUILayout.Width(28));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("endTime"), GUIContent.none, GUILayout.Width(44));
            }
            else
            {
                EditorGUILayout.LabelField("Time", GUILayout.Width(34));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("triggerTime"), GUIContent.none, GUILayout.Width(44));
            }
            EditorGUILayout.EndHorizontal(); // always close row 2

            // --- Row 3: notify/state type picker (separate row, no extra horizontal wrapping) ---
            if (isState)
            {
                var notifyStateProp = element.FindPropertyRelative("notifyState");
                if (notifyStateProp != null)
                    DrawNotifyField(notifyStateProp, NotifyStateTypes);
            }
            else
            {
                var notifyProp = element.FindPropertyRelative("notify");
                if (notifyProp != null)
                    DrawNotifyField(notifyProp, NotifyTypes);
            }

            EditorGUILayout.EndVertical(); // close card
            EditorGUILayout.Space(3);
        }

        EditorGUILayout.Space(4);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("+ Add Notify ♦", GUILayout.Height(24)))
        {
            eventsProp.InsertArrayElementAtIndex(eventsProp.arraySize);
            var el = eventsProp.GetArrayElementAtIndex(eventsProp.arraySize - 1);
            el.FindPropertyRelative("name").stringValue = "AN_NewNotify";
            el.FindPropertyRelative("eventType").enumValueIndex = 0;
            el.FindPropertyRelative("triggerTime").floatValue = 0.1f;
            el.FindPropertyRelative("color").colorValue = new Color(0.4f, 0.9f, 0.4f);

            var n = el.FindPropertyRelative("notify");
            var ns = el.FindPropertyRelative("notifyState");
            if (n != null) n.managedReferenceValue = null;
            if (ns != null) ns.managedReferenceValue = null;
        }

        if (GUILayout.Button("+ Add State ■", GUILayout.Height(24)))
        {
            eventsProp.InsertArrayElementAtIndex(eventsProp.arraySize);
            var el = eventsProp.GetArrayElementAtIndex(eventsProp.arraySize - 1);
            el.FindPropertyRelative("name").stringValue = "ANS_NewState";
            el.FindPropertyRelative("eventType").enumValueIndex = 1;
            el.FindPropertyRelative("startTime").floatValue = 0.1f;
            el.FindPropertyRelative("endTime").floatValue = 0.4f;
            el.FindPropertyRelative("color").colorValue = new Color(0.4f, 0.6f, 1f);

            var n = el.FindPropertyRelative("notify");
            var ns = el.FindPropertyRelative("notifyState");
            if (n != null) n.managedReferenceValue = null;
            if (ns != null) ns.managedReferenceValue = null;
        }

        EditorGUILayout.EndHorizontal();
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
            SampleClipAtTime(_previewTime);
            _previewRenderer.BeginPreview(previewRect, background);
            _previewRenderer.camera.Render();
            var tex = _previewRenderer.EndPreview();
            GUI.DrawTexture(previewRect, tex, ScaleMode.ScaleToFit);
        }

        Rect controlRect = new Rect(r.x, r.yMax - 28, r.width, 28);

        if (GUI.Button(new Rect(controlRect.x + 4, controlRect.y + 4, 55, 20), _isPlaying ? "Pause" : "Play"))
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
            _previewTime += (float)(now - _lastEditorTime);
            _lastEditorTime = now;
            if (_previewTime > _data.clip.length) _previewTime = 0f;
            Repaint();
        }
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
            Debug.LogError("SOAnimationDataEditor: Prefab not found — update the path in EnsurePreviewReady()");
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

    private void DrawNotifyField(SerializedProperty prop, System.Type[] types)
    {
        EditorGUILayout.BeginHorizontal();

        // Show current type name or None
        string currentName = prop.managedReferenceValue != null
            ? prop.managedReferenceValue.GetType().Name
            : "None";

        // Dropdown to pick type
        if (EditorGUILayout.DropdownButton(new GUIContent(currentName), FocusType.Keyboard, GUILayout.Width(140)))
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("None"), prop.managedReferenceValue == null, () =>
            {
                prop.managedReferenceValue = null;
                serializedObject.ApplyModifiedProperties();
            });

            foreach (var type in types)
            {
                var capturedType = type;
                bool isSelected = prop.managedReferenceValue?.GetType() == capturedType;
                menu.AddItem(new GUIContent(capturedType.Name), isSelected, () =>
                {
                    prop.managedReferenceValue = System.Activator.CreateInstance(capturedType);
                    serializedObject.ApplyModifiedProperties();
                });
            }

            menu.ShowAsContext();
        }

        // Draw the actual fields of the selected type
        if (prop.managedReferenceValue != null)
            EditorGUILayout.PropertyField(prop, GUIContent.none, true, GUILayout.ExpandWidth(true));

        EditorGUILayout.EndHorizontal();
    }   

    private void DrawTitleBanner()
    {
        Rect bannerRect = EditorGUILayout.GetControlRect(false, 42);
        EditorGUI.DrawRect(bannerRect, new Color(0.15f, 0.15f, 0.15f));

        EditorGUI.LabelField(  new Rect(bannerRect.x, bannerRect.y + 4, bannerRect.width, 20), "CUSTOM ANIMATION EVENT", _titleStyle);

        EditorGUI.LabelField( new Rect(bannerRect.x, bannerRect.y + 22, bannerRect.width, 16),target.name, _targetStyle);

        Rect infoRect = new Rect(bannerRect.xMax - 24, bannerRect.y + 8, 18, 18);
        GUI.Label(infoRect, new GUIContent("?", ""), _infoIconStyle);

        // Track hover on info icon
        if (infoRect.Contains(Event.current.mousePosition))
        {
            _showTooltip = true;
            Repaint();
        }
        else if (!_tooltipRect.Contains(Event.current.mousePosition))
        {
            _showTooltip = false;
            Repaint();
        }

        // Draw popup
        if (_showTooltip)
            DrawTooltipPopup(new Vector2(bannerRect.xMax - 260, bannerRect.yMax + 4));
    }    

    private void DrawTooltipPopup(Vector2 position)
    {
        float width = 250f;
        float height = 200f;
        _tooltipRect = new Rect(position.x, position.y, width, height);

        // Shadow
        EditorGUI.DrawRect(
            new Rect(_tooltipRect.x + 3, _tooltipRect.y + 3, _tooltipRect.width, _tooltipRect.height),
            new Color(0f, 0f, 0f, 0.4f));

        // Border
        EditorGUI.DrawRect(_tooltipRect, new Color(0.13f, 0.13f, 0.13f));

        // Background
        EditorGUI.DrawRect(
            new Rect(_tooltipRect.x + 1, _tooltipRect.y + 1, _tooltipRect.width - 2, _tooltipRect.height - 2),
            new Color(0.22f, 0.22f, 0.22f));

        float x = _tooltipRect.x + 10;
        float y = _tooltipRect.y + 10;
        float w = _tooltipRect.width - 20;

        // Header
        EditorGUI.LabelField(new Rect(x, y, w, 18), "How to use", new GUIStyle(EditorStyles.boldLabel)
        {
            normal = { textColor = new Color(0.82f, 0.82f, 0.82f) }
        });

        y += 22;
        EditorGUI.DrawRect(new Rect(x, y, w, 1), new Color(0.13f, 0.13f, 0.13f));
        y += 8;

        // Notify row
        EditorGUI.DrawRect(new Rect(x, y, 10, 10), new Color(0.25f, 0.45f, 0.25f));
        EditorGUI.LabelField(new Rect(x + 14, y - 1, w - 14, 16), "♦ Notify — fires once at a point in time",
            new GUIStyle(EditorStyles.miniLabel)
            {
                normal = { textColor = new Color(0.82f, 0.82f, 0.82f) }
            });
        y += 18;

        // State row
        EditorGUI.DrawRect(new Rect(x, y, 10, 10), new Color(0.20f, 0.30f, 0.48f));
        EditorGUI.LabelField(new Rect(x + 14, y - 1, w - 14, 16), "■ State — has begin, tick and end",
            new GUIStyle(EditorStyles.miniLabel)
            {
                normal = { textColor = new Color(0.82f, 0.82f, 0.82f) }
            });
        y += 22;

        EditorGUI.DrawRect(new Rect(x, y, w, 1), new Color(0.13f, 0.13f, 0.13f));
        y += 8;

        // Steps
        var stepStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            normal = { textColor = new Color(0.55f, 0.55f, 0.55f) },
            wordWrap = true
        };

        EditorGUI.LabelField(new Rect(x, y, w, 16), "1. Assign an Animation Clip above", stepStyle);
        y += 16;
        EditorGUI.LabelField(new Rect(x, y, w, 16), "2. Add Notify or State events below", stepStyle);
        y += 16;
        EditorGUI.LabelField(new Rect(x, y, w, 16), "3. Set timing to match your clip", stepStyle);
        y += 16;
        EditorGUI.LabelField(new Rect(x, y, w, 32), "4. Assign an AnimationEventPlayer\n    to your character to fire them", stepStyle);
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

