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
        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < celestialBodyManager.noiseTextures.Length; i++)
        {
            TextureField("Noise_" + i, celestialBodyManager.noiseTextures[i]);
        }
        EditorGUILayout.EndHorizontal();

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

    private static Texture2D TextureField(string name, Texture2D texture)
    {
        GUILayout.BeginVertical();
        var style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.UpperCenter;
        style.fixedWidth = 70;
        GUILayout.Label(name, style);
        var result = (Texture2D)EditorGUILayout.ObjectField(texture, typeof(Texture2D), false, GUILayout.Width(70), GUILayout.Height(70));
        GUILayout.EndVertical();
        return result;
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
