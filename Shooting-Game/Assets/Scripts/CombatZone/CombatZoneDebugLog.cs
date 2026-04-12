using UnityEngine;

public static class CombatZoneDebugLog
{
    public static bool Enabled = true;

    public static void Log(string action, string detail = null)
    {
        if (!Enabled) return;
        if (string.IsNullOrEmpty(detail))
            Debug.Log($"[CombatZone] {action}");
        else
            Debug.Log($"[CombatZone] {action} | {detail}");
    }
}
