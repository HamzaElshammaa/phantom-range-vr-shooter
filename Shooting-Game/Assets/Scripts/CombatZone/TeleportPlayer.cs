using UnityEngine;

public class TeleportPlayer : MonoBehaviour
{
    public Transform player;
    public Transform teleportTarget;

    public void Teleport()
    {
        Vector3 offset = player.position - Camera.main.transform.position;
        player.position = teleportTarget.position + offset;
    }
}