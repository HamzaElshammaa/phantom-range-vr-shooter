using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HoverGlow : MonoBehaviour
{
    private Renderer rend;
    private Material mat;

    public Color hoverColor = Color.cyan;
    public float intensity = 2.5f;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        mat = rend.material;

        var interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        interactable.hoverEntered.AddListener(OnHoverEnter);
        interactable.hoverExited.AddListener(OnHoverExit);
    }

    void OnHoverEnter(HoverEnterEventArgs args)
    {
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", hoverColor * intensity);
    }

    void OnHoverExit(HoverExitEventArgs args)
    {
        mat.SetColor("_EmissionColor", Color.black);
    }
}