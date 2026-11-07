using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public struct TeleportWaypoints
{
    public string name;
    public Transform waypointTransform;
}

public class TeleportWaypoint : MonoBehaviour
{
    [Header("Target to Teleport")]
    public Transform playerTransform;

    [Header("Waypoints (Assign up to 9 positions)")]
    public Transform[] teleportWaypoints = new Transform[9];

    void Update()
    {
        if (Keyboard.current == null) return;

        for (int i = 0; i < 10; i++)
        {
            bool keyPressed = false;

            switch (i)
            {
                case 0: keyPressed = Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame; break;
                case 1: keyPressed = Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame; break;
                case 2: keyPressed = Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame; break;
                case 3: keyPressed = Keyboard.current.digit4Key.wasPressedThisFrame || Keyboard.current.numpad4Key.wasPressedThisFrame; break;
                case 4: keyPressed = Keyboard.current.digit5Key.wasPressedThisFrame || Keyboard.current.numpad5Key.wasPressedThisFrame; break;
                case 5: keyPressed = Keyboard.current.digit6Key.wasPressedThisFrame || Keyboard.current.numpad6Key.wasPressedThisFrame; break;
                case 6: keyPressed = Keyboard.current.digit7Key.wasPressedThisFrame || Keyboard.current.numpad7Key.wasPressedThisFrame; break;
                case 7: keyPressed = Keyboard.current.digit8Key.wasPressedThisFrame || Keyboard.current.numpad8Key.wasPressedThisFrame; break;
                case 8: keyPressed = Keyboard.current.digit9Key.wasPressedThisFrame || Keyboard.current.numpad9Key.wasPressedThisFrame; break;
                case 9: keyPressed = Keyboard.current.digit9Key.wasPressedThisFrame || Keyboard.current.numpad0Key.wasPressedThisFrame; break;
            }

            if (keyPressed)
            {
                TeleportToWaypoint(i);
                break;
            }
        }
    }

    void TeleportToWaypoint(int index)
    {
        if (index < teleportWaypoints.Length && teleportWaypoints[index] != null)
        {
            if (playerTransform != null)
            {
                CharacterController cc = playerTransform.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;

                playerTransform.position = teleportWaypoints[index].position;
                playerTransform.rotation = teleportWaypoints[index].rotation;

                if (cc != null) cc.enabled = true;

                Debug.Log($"Teleported to Waypoint {index + 1} ({teleportWaypoints[index].name})");
            }
            else
            {
                Debug.LogWarning("Player Transform is not assigned on the MultiTeleporter script!");
            }
        }
        else
        {
            Debug.Log($"Waypoint {index + 1} is not assigned.");
        }
    }
}
