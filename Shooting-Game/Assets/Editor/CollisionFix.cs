
using UnityEngine;
using UnityEditor;

public class CollisionFix : EditorWindow
{
    [MenuItem("Tools/Check Collision Setup")]
    static void Check()
    {
        string log = "=== COLLISION CHECK ===\n\n";

        // Check XR Rig
        var xrRig = GameObject.Find("XR Origin (XR Rig)");
        if (xrRig == null) { log += "XR Rig: NOT FOUND\n"; }
        else
        {
            var cc = xrRig.GetComponent<CharacterController>();
            log += cc != null
                ? $"CharacterController: enabled={cc.enabled}, height={cc.height:F2}, radius={cc.radius:F2}, skinWidth={cc.skinWidth:F3}\n"
                : "CharacterController: MISSING!\n";
        }

        // Check CAE colliders
        var cae = GameObject.Find("CAE");
        if (cae == null) { log += "CAE: NOT FOUND\n"; }
        else
        {
            var allCols = cae.GetComponentsInChildren<Collider>(true);
            log += $"\nCAE total colliders: {allCols.Length}\n";
            int triggers = 0, disabled = 0;
            foreach (var c in allCols)
            {
                if (c.isTrigger) triggers++;
                if (!c.enabled) disabled++;
            }
            log += $"  isTrigger: {triggers}, disabled: {disabled}\n";
            if (allCols.Length > 0)
            {
                log += "  First 5:\n";
                for (int i = 0; i < Mathf.Min(5, allCols.Length); i++)
                    log += $"    [{i}] {allCols[i].GetType().Name} on '{allCols[i].gameObject.name}' trigger={allCols[i].isTrigger}\n";
            }
            else log += "  WARNING: NO COLLIDERS on CAE!\n";
        }

        // Check Physics layer matrix - layer 0 vs layer 2 (XR Rig is layer 2)
        bool layer0vs0 = !Physics.GetIgnoreLayerCollision(0, 0);
        bool layer0vs2 = !Physics.GetIgnoreLayerCollision(0, 2);
        bool layer2vs2 = !Physics.GetIgnoreLayerCollision(2, 2);
        log += $"\nPhysics Layer Collisions:\n";
        log += $"  Layer 0 vs 0: {(layer0vs0 ? "ON" : "IGNORED")}\n";
        log += $"  Layer 0 vs 2: {(layer0vs2 ? "ON" : "IGNORED")} (CAE vs XR Rig)\n";
        log += $"  Layer 2 vs 2: {(layer2vs2 ? "ON" : "IGNORED")}\n";

        Debug.Log(log);
        EditorUtility.DisplayDialog("Collision Check", log, "OK");
    }
}
