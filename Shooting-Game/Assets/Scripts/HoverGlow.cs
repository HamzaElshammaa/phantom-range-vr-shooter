using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class HoverGlow : MonoBehaviour
{
    public Color glowColor = new Color(1f, 0.92f, 0.016f, 1f); // bright yellow

    Renderer rend;
    Color originalColor;
    static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend == null) { enabled = false; return; }

        // Store original color (URP uses _BaseColor, fallback to _Color)
        if (rend.material.HasProperty(BaseColor))
            originalColor = rend.material.GetColor(BaseColor);
        else
            originalColor = rend.material.color;

        // Auto-hook XRSimpleInteractable events if present
        var interactable = GetComponent<XRBaseInteractable>();
        if (interactable != null)
        {
            interactable.hoverEntered.AddListener(OnHoverEnter);
            interactable.hoverExited.AddListener(OnHoverExit);
        }
    }

    public void OnHoverEnter(HoverEnterEventArgs args)
    {
        SetColor(glowColor);
    }

    public void OnHoverExit(HoverExitEventArgs args)
    {
        SetColor(originalColor);
    }

    void SetColor(Color c)
    {
        if (rend == null) return;
        if (rend.material.HasProperty(BaseColor))
            rend.material.SetColor(BaseColor, c);
        else
            rend.material.color = c;
    }
}
