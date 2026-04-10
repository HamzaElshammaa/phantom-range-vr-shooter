
using UnityEngine;
using UnityEditor;

public class DiagnoseCollision : EditorWindow
{
    [MenuItem("Tools/Diagnose Collision")]
    static void Run()
    {
        var cae = GameObject.Find("CAE");
        var xr = GameObject.Find("XR Origin (XR Rig)");
        var cc = xr?.GetComponent<CharacterController>();
        var cols = cae?.GetComponentsInChildren<Collider>(true);

        string msg = $"=== Collision Diagnosis ===\n";
        msg += $"CharacterController: {(cc != null ? "FOUND" : "MISSING")}\n";
        if (cc != null) msg += $"  enabled={cc.enabled}, height={cc.height}, radius={cc.radius}\n";
        msg += $"CAE colliders: {cols?.Length ?? 0}\n";
        if (cols != null && cols.Length > 0)
        {
            for (int i = 0; i < Mathf.Min(10, cols.Length); i++)
                msg += $"  [{i}] {cols[i].GetType().Name} on '{cols[i].gameObject.name}' isTrigger={cols[i].isTrigger} enabled={cols[i].enabled}\n";
        }
        else
        {
            msg += "  WARNING: No colliders found on CAE or its children!\n";
        }
        Debug.Log(msg);
        EditorUtility.DisplayDialog("Collision Diagnosis", msg, "OK");
    }
}
