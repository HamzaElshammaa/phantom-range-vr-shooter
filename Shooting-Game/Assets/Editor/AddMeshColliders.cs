using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class AddMeshColliders
{
    [MenuItem("Tools/Add Colliders To All Scene Objects")]
    public static void AddColliders()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        int added = 0;
        int skipped = 0;

        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var mf in root.GetComponentsInChildren<MeshFilter>(true))
            {
                var go = mf.gameObject;
                if (go.GetComponent<Collider>() != null) { skipped++; continue; }
                if (mf.sharedMesh == null) { skipped++; continue; }

                var col = go.AddComponent<MeshCollider>();
                col.sharedMesh = mf.sharedMesh;
                col.convex = false;
                EditorUtility.SetDirty(go);
                added++;
            }
        }

        EditorSceneManager.MarkAllScenesDirty();
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[AddMeshColliders] Done. Added: {added}, Skipped (already had collider or no mesh): {skipped}");
    }
}
