using UnityEngine;
namespace Games.Battleship
{
    public class BattleshipMouseFollower : MonoBehaviour
    {
        [SerializeField] private float fixedYPosition = 1f; // The fixed Y position (distance above the grid)

        void Update()
        {
            // Get the current mouse position in screen coordinates
            Vector3 mouseScreenPosition = Input.mousePosition;

            // Calculate the distance from camera to the desired Y position
            float distanceFromCamera = Camera.main.transform.position.y - fixedYPosition;

            // Convert the screen position to world coordinates using the correct distance
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, distanceFromCamera));

            // Set the object's position with fixed Y coordinate
            transform.position = new Vector3(mouseWorldPosition.x, fixedYPosition, mouseWorldPosition.z);
        }
    }
}