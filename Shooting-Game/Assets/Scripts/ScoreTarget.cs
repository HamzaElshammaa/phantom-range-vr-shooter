using UnityEngine;

namespace VRShootingGame
{
    /// <summary>
    /// Marks a collider (or parent chain) as a valid training target for scoring.
    /// </summary>
    public class ScoreTarget : MonoBehaviour
    {
        [SerializeField]
        bool m_ScoreOnce = true;

        bool m_AlreadyScored;

        /// <returns>True if this hit should increase the valid hit counter.</returns>
        public bool TryConsumeScore()
        {
            if (m_ScoreOnce && m_AlreadyScored)
                return false;
            if (m_ScoreOnce)
                m_AlreadyScored = true;
            return true;
        }
    }
}
