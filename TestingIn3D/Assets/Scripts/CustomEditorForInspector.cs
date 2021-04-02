using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CelestialBodyManager))]
public class CustomEditorForInspector : Editor
{
    CelestialBodyManager celestialBodyManager;
    Editor colorEditor;
    Editor sphereEditor;
    Editor terrainEditor;
    Editor faunaEditor;

    public override void OnInspectorGUI()
    {
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            base.OnInspectorGUI();
            if (check.changed)
            {
                celestialBodyManager.GenerateCelestialBody();
            }
        }

        DrawSettingsEditor(celestialBodyManager.colorSettings, celestialBodyManager.OnColorSettingsUpdated, ref celestialBodyManager.colorSettingsFoldout, ref colorEditor);
        DrawSettingsEditor(celestialBodyManager.sphereSettings, celestialBodyManager.OnSphereSettingsUpdated, ref celestialBodyManager.sphereSettingsFoldout, ref sphereEditor);
        DrawSettingsEditor(celestialBodyManager.shapeSettings, celestialBodyManager.OnTerrainSettingsUpdated, ref celestialBodyManager.terrainSettingsFoldout, ref terrainEditor);
        DrawSettingsEditor(celestialBodyManager.faunaSettings, celestialBodyManager.OnFaunaSettingsUpdated, ref celestialBodyManager.faunaSettingsFoldout, ref faunaEditor);
    }

    void DrawSettingsEditor(Object settings, System.Action onSettingsUpdated, ref bool foldout, ref Editor editor)
    {
        if (settings != null)
        {
            foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                if (foldout)
                {
                    CreateCachedEditor(settings, null, ref editor);
                    editor.OnInspectorGUI();

                    if (check.changed)
                    {
                        if (onSettingsUpdated != null)
                        {
                            onSettingsUpdated();
                        }
                    }
                }
            }
        }
    }

    private void OnEnable()
    {
        celestialBodyManager = (CelestialBodyManager)target;
    }
}
