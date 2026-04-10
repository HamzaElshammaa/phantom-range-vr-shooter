using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FixXRRigLayer
{
    [MenuItem("Tools/Fix XR Rig Layer")]
    public static void Fix()
    {
        // The XR Rig is on Layer 2 (Ignore Raycast) which ignores collisions with Default layer
        // Move it to Default layer (0) so CharacterController collides with environment

        GameObject xrRig = GameObject.Find("XR Origin (XR Rig)");
        if (xrRig == null) { Debug.LogError("[FixXRRig] Could not find XR Origin (XR Rig)"); return; }

        int oldLayer = xrRig.layer;
        xrRig.layer = 0; // Default
        EditorUtility.SetDirty(xrRig);
        Debug.Log($"[FixXRRig] Changed XR Rig layer from {LayerMask.LayerToName(oldLayer)}({oldLayer}) to Default(0)");

        // Also check physics matrix - ensure Default (0) vs Default (0) collisions are ON
        bool ignored = Physics.GetIgnoreLayerCollision(0, 0);
        Debug.Log($"[FixXRRig] Default vs Default collision currently ignored: {ignored}");
        if (ignored)
        {
            Physics.IgnoreLayerCollision(0, 0, false);
            Debug.Log("[FixXRRig] Enabled Default vs Default collisions in Physics matrix");
        }

        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[FixXRRig] Done!");
    }
}
