using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FixEnemyAnimations
{
    [MenuItem("Tools/Fix Enemy Animation Components")]
    public static void FixEnemies()
    {
        Animation[] allAnimations = GameObject.FindObjectsOfType<Animation>(true);
        int removed = 0;

        foreach (Animation anim in allAnimations)
        {
            GameObject go = anim.gameObject;
            string goName = go.name;
            Object.DestroyImmediate(anim);
            EditorUtility.SetDirty(go);
            Debug.Log($"[FixEnemies] Removed Animation from: {goName}");
            removed++;
        }

        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[FixEnemies] Done! Removed {removed} legacy Animation components.");
    }
}
