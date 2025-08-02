using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the camera movement to follow the player with smooth horizontal offset based on direction.
/// </summary>
public class CameraController : MonoBehaviour
{
    public GameObject player;          // Reference to the player GameObject
    public float offset;              // Horizontal offset from the player (depends on direction)
    public float offsetSmoothing;     // How quickly the camera follows the player
    private Vector3 playerPosition;   // Calculated target position for the camera

    // Start is called before the first frame update
    void Start()
    {
        // Currently no initialization is needed
    }

    // Update is called once per frame
    void Update()
    {
        // Set target camera position to match player X and Y, but keep original camera Z
        playerPosition = new Vector3(player.transform.position.x, player.transform.position.y, transform.position.z);

        // Check player's facing direction using scale.x and apply offset accordingly
        if (player.transform.localScale.x > 0f)
        {
            // Player facing right move camera ahead
            playerPosition = new Vector3(playerPosition.x + offset, playerPosition.y, playerPosition.z);
        }
        else
        {
            // Player facing left move camera behind
            playerPosition = new Vector3(playerPosition.x - offset, playerPosition.y, playerPosition.z);
        }

        // Smoothly move the camera toward the target position
        transform.position = Vector3.Lerp(transform.position, playerPosition, offsetSmoothing * Time.deltaTime);
    }
}
