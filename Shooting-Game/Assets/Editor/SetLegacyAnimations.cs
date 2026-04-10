using UnityEngine;
using UnityEditor;

public class SetLegacyAnimations
{
    [MenuItem("Tools/Set Skeleton Animations to Legacy")]
    public static void SetLegacy()
    {
        string assetPath = "Assets/skeleton_-_low_tier_enemy/scene.gltf";
        Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        int count = 0;
        foreach (Object obj in allAssets)
        {
            AnimationClip clip = obj as AnimationClip;
            if (clip == null) continue;
            SerializedObject so = new SerializedObject(clip);
            SerializedProperty legacyProp = so.FindProperty("m_Legacy");
            if (legacyProp != null)
            {
                legacyProp.boolValue = true;
                so.ApplyModifiedProperties();
                count++;
                Debug.Log("Marked Legacy: " + clip.name);
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Done. Marked " + count + " clips as Legacy.");
    }
}