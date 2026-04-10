using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;


public class FixGunAttachRotation
{
    // The M1911 world rotation is (0, 270, 90)
    // When grabbed, XR maps the controller forward (+Z) to the gun's local axes
    // We need the barrel (+Z local of gun) to point AWAY from the player (forward)
    // Try different attach offsets with menu items to find the right one

    [MenuItem("Tools/Gun Attach/Rotate Y+180")]
    public static void RotateY180() => ApplyAttach(new Vector3(0, 180, 0));

    [MenuItem("Tools/Gun Attach/Rotate Y+0 (reset)")]
    public static void RotateY0() => ApplyAttach(new Vector3(0, 0, 0));

    [MenuItem("Tools/Gun Attach/Rotate X+180")]
    public static void RotateX180() => ApplyAttach(new Vector3(180, 0, 0));

    [MenuItem("Tools/Gun Attach/Rotate Z+180")]
    public static void RotateZ180() => ApplyAttach(new Vector3(0, 0, 180));

    [MenuItem("Tools/Gun Attach/Rotate Y-90")]
    public static void RotateYMinus90() => ApplyAttach(new Vector3(0, -90, 0));

    [MenuItem("Tools/Gun Attach/Rotate Y+90")]
    public static void RotateYPlus90() => ApplyAttach(new Vector3(0, 90, 0));

    static void ApplyAttach(Vector3 euler)
    {
        string[] gunNames = { "M1911", "M1911 (1)", "M1911 (2)" };
        foreach (string gunName in gunNames)
        {
            GameObject gun = GameObject.Find(gunName);
            if (gun == null) continue;

            UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab = gun.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (grab == null) grab = gun.GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (grab == null) continue;

            Transform attachTransform = gun.transform.Find("AttachPoint");
            if (attachTransform == null)
            {
                GameObject attachGO = new GameObject("AttachPoint");
                attachGO.transform.SetParent(gun.transform);
                attachTransform = attachGO.transform;
            }

            attachTransform.localPosition = Vector3.zero;
            attachTransform.localEulerAngles = euler;
            grab.attachTransform = attachTransform;

            EditorUtility.SetDirty(gun);
            EditorUtility.SetDirty(grab);
        }

        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[GunAttach] Applied rotation {euler} to all M1911s");
    }
}
