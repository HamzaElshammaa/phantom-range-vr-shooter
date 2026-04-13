using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace VRShootingGame
{
    /// <summary>
    /// Snaps a grabbed weapon to a placeholder transform (e.g. on the camera)
    /// and keeps it locked there through all movement until dropped.
    /// </summary>
    public class WeaponSocketHandler : MonoBehaviour
    {
        [Tooltip("The transform the gun snaps to when grabbed (e.g. POV_Placeholder on Main Camera).")]
        public Transform weaponPlaceholder;

        XRGrabInteractable m_Grab;
        Rigidbody m_Rb;
        bool m_IsHeld;
        private Vector3 m_StartPos;
private Quaternion m_StartRot;

// 2. Record the position in Start()
void Start()
{
    m_StartPos = transform.position;
    m_StartRot = transform.rotation;
}

        void Awake()
        {
            m_Grab = GetComponent<XRGrabInteractable>();
            m_Rb = GetComponent<Rigidbody>();

            if (m_Grab == null) { enabled = false; return; }

            m_Grab.selectEntered.AddListener(OnSelectEntered);
            m_Grab.selectExited.AddListener(OnSelectExited);
        }

        void OnDestroy()
        {
            if (m_Grab != null)
            {
                m_Grab.selectEntered.RemoveListener(OnSelectEntered);
                m_Grab.selectExited.RemoveListener(OnSelectExited);
            }
        }

        void OnSelectEntered(SelectEnterEventArgs args)
        {
            if (m_Rb != null) m_Rb.isKinematic = true;

            m_Grab.trackPosition = false;
            m_Grab.trackRotation = false;

            m_IsHeld = true;
        }

        void OnSelectExited(SelectExitEventArgs args)
        {
    m_IsHeld = false;
    transform.SetParent(null);

    // Re-enable tracking for the next grab
    m_Grab.trackPosition = true;
    m_Grab.trackRotation = true;

    if (m_Rb != null)
    {
        m_Rb.isKinematic = true; // Keep it kinematic so it doesn't fall over
        m_Rb.linearVelocity = Vector3.zero;
        m_Rb.angularVelocity = Vector3.zero;
    }

    // TELEPORT BACK HOME
    transform.position = m_StartPos;
    transform.rotation = m_StartRot;
}

        void LateUpdate()
        {
            if (!m_IsHeld || weaponPlaceholder == null) return;

            transform.position = weaponPlaceholder.position;
            transform.rotation = weaponPlaceholder.rotation * Quaternion.Euler(0f, 90f, 0f);
        }
    }
}
