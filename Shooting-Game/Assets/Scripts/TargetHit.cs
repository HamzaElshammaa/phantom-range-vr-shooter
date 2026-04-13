using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace VRShootingGame
{
    /// <summary>
    /// Visual hit feedback for training targets: flashes red, spawns a floating "+1" marker,
    /// then reverts after a cooldown.
    /// </summary>
    public class TargetHit : MonoBehaviour
    {
        [SerializeField] float m_CooldownSeconds = 2f;
        [SerializeField] Color m_HitColor = Color.red;

        /// <summary>Callback invoked on each valid hit (used by TutorialManager).</summary>
        public System.Action onHitCallback;

        Renderer m_Renderer;
        Color m_OriginalColor;
        bool m_InCooldown;

        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        void Awake()
        {
            m_Renderer = GetComponent<Renderer>();
            if (m_Renderer != null)
            {
                m_OriginalColor = m_Renderer.material.HasProperty(BaseColor)
                    ? m_Renderer.material.GetColor(BaseColor)
                    : m_Renderer.material.color;
            }
        }

        /// <summary>Called by GrabbableGunFire when the raycast hits this target.</summary>
        public void OnHit()
        {
            if (m_InCooldown) return;
            onHitCallback?.Invoke();
            StartCoroutine(HitRoutine());
        }

        IEnumerator HitRoutine()
        {
            m_InCooldown = true;

            // Flash red
            SetColor(m_HitColor);

            // Spawn floating +1
            SpawnHitMarker();

            // Wait cooldown
            yield return new WaitForSeconds(m_CooldownSeconds);

            // Revert color
            SetColor(m_OriginalColor);
            m_InCooldown = false;
        }

        void SetColor(Color c)
        {
            if (m_Renderer == null) return;
            if (m_Renderer.material.HasProperty(BaseColor))
                m_Renderer.material.SetColor(BaseColor, c);
            else
                m_Renderer.material.color = c;
        }

        void SpawnHitMarker()
        {
            // Create a tiny world-space canvas
            var markerGo = new GameObject("HitMarker+1");
            var canvas = markerGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 100;

            var rt = markerGo.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(1f, 0.4f);

            // Position in front of the target (toward camera) and slightly above center
            var cam = Camera.main;
            Vector3 toCamera = cam != null
                ? (cam.transform.position - transform.position).normalized
                : transform.forward;
            markerGo.transform.position = transform.position + toCamera * 0.4f + Vector3.up * 0.3f;
            markerGo.transform.localScale = Vector3.one * 0.01f;

            // Text
            var textGo = new GameObject("Text");
            textGo.transform.SetParent(markerGo.transform, false);
            var textRt = textGo.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            var text = textGo.AddComponent<Text>();
            text.text = "+1";
            text.fontSize = 64;
            text.color = Color.yellow;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Animate: float up and fade out over 1 second
            var anim = markerGo.AddComponent<HitMarkerAnimation>();
            anim.duration = 1f;
            anim.floatSpeed = 0.5f;
        }
    }

    /// <summary>
    /// Simple animation: floats upward and fades out, then self-destructs.
    /// </summary>
    public class HitMarkerAnimation : MonoBehaviour
    {
        public float duration = 1f;
        public float floatSpeed = 0.5f;

        float m_Elapsed;
        Text m_Text;
        Transform m_Cam;

        void Start()
        {
            m_Text = GetComponentInChildren<Text>();
            if (Camera.main != null)
                m_Cam = Camera.main.transform;
        }

        void LateUpdate()
        {
            m_Elapsed += Time.deltaTime;
            float t = m_Elapsed / duration;

            // Float upward
            transform.position += Vector3.up * floatSpeed * Time.deltaTime;

            // Always face the camera (billboard)
            if (m_Cam != null)
            {
                Vector3 dir = transform.position - m_Cam.position;
                if (dir.sqrMagnitude > 0.001f)
                    transform.rotation = Quaternion.LookRotation(dir);
            }

            // Fade out
            if (m_Text != null)
            {
                var c = m_Text.color;
                c.a = Mathf.Lerp(1f, 0f, t);
                m_Text.color = c;
            }

            if (t >= 1f)
                Destroy(gameObject);
        }
    }
}
