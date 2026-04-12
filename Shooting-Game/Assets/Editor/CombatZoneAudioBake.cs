#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Bakes AudioClip references onto <see cref="CombatZoneAudioManager"/> in CombatZone.unity so player builds hear audio.
/// Run once: <b>Combat Zone ▸ Bake audio into CombatZone scene</b>.
/// </summary>
public static class CombatZoneAudioBake
{
    const string ScenePath = "Assets/Scenes/CombatZone.unity";
    const string SoundsRoot = "Assets/Scripts/CombatZone/Sounds/";

    [MenuItem("Combat Zone/Bake audio into CombatZone scene")]
    public static void Bake()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var roots = scene.GetRootGameObjects();
        GameObject managers = null;
        foreach (var go in roots)
        {
            if (go.name == "GameManagers")
            {
                managers = go;
                break;
            }
        }
        if (managers == null)
        {
            Debug.LogError("[CombatZone] No root GameObject named 'GameManagers' in CombatZone.unity.");
            return;
        }

        var audio = managers.GetComponent<CombatZoneAudioManager>();
        if (audio == null)
            audio = managers.AddComponent<CombatZoneAudioManager>();

        var so = new SerializedObject(audio);
        Assign(so, "backgroundMusic", "97868e1f0d00d9343a00b866fcad6cb1", SoundsRoot + "Best songs to play shooting games! - Trips (128k).wav");
        Assign(so, "gunshot", "ebaff7c25ae4d5c4ca765f9e31a40d97", SoundsRoot + "Gunshot Sound Effect Single Shot - Birthday Song (128k).wav");
        Assign(so, "sadTrombone", "fe3ef97ac202a3146870e7b66810022d", SoundsRoot + "Sad Trombone - Sound Effect (HD) - Gaming Sound FX (128k).wav");
        Assign(so, "victory", "9f8d382acf92b784da7f6cbc17484cb2", SoundsRoot + "Victory Sound Effect - Der Test (128k).wav");
        Assign(so, "fireworks", "849b2c469a3d9764999fa764df9688f3", SoundsRoot + "Sound Effects - Fireworks - Effecism (128k).wav");
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[CombatZone] Audio clips baked onto GameManagers / CombatZoneAudioManager.");
    }

    static void Assign(SerializedObject so, string propName, string guid, string fallbackAssetPath)
    {
        var path = AssetDatabase.GUIDToAssetPath(guid);
        var clip = !string.IsNullOrEmpty(path) ? LoadClip(path) : null;
        if (clip == null)
            clip = LoadClip(fallbackAssetPath);
        if (clip == null)
        {
            Debug.LogWarning("[CombatZone] Missing clip guid=" + guid + " path=" + fallbackAssetPath);
            return;
        }
        so.FindProperty(propName).objectReferenceValue = clip;
    }

    static AudioClip LoadClip(string assetPath)
    {
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
        if (clip != null) return clip;
        foreach (var o in AssetDatabase.LoadAllAssetsAtPath(assetPath))
        {
            if (o is AudioClip ac) return ac;
        }
        return null;
    }
}
#endif
