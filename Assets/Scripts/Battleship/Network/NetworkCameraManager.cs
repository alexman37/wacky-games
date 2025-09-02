using UnityEngine;
using Unity.Netcode;

namespace Games.Battleship
{
    public class NetworkCameraManager : NetworkBehaviour
    {
        [Header("Camera Settings")]
        public Camera playerCamera;
        public float turnSpeed = 2f;

        [Header("Camera Positions")]
        public Vector3 playerBoardPosition = new Vector3(5, 13, -4);
        public Vector3 playerBoardRotation = new Vector3(90, 0, 0);
        public Vector3 attackBoardPosition = new Vector3(5, 5, -10);
        public Vector3 attackBoardRotation = new Vector3(0, 0, 0);

        private bool isLookingAtPlayerBoard = true;

        public override void OnNetworkSpawn()
        {
            // Only initialize for the local player's camera
            if (!IsOwner)
            {
                // Disable camera and audio listener for non-local players
                if (playerCamera != null)
                {
                    playerCamera.enabled = false;
                    var audioListener = playerCamera.GetComponent<AudioListener>();
                    if (audioListener != null)
                    {
                        audioListener.enabled = false;
                    }
                }
                return;
            }

            InitializeCamera();
        }

        private void InitializeCamera()
        {
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }

            if (playerCamera == null)
            {
                Debug.LogError($"No camera found for player {GetComponent<NetworkPlayer>().PlayerName}");
                return;
            }

            if (playerCamera != null)
            {
                playerCamera.enabled = true;
                SetPlayerBoardView();
            }
            else
            {
                Debug.LogError($"No camera found for player {GetComponent<NetworkPlayer>().PlayerName}");
            }
        }

        public void SetPlayerBoardView()
        {
            if (!IsOwner || playerCamera == null)
            {
                if(!IsOwner) Debug.Log("Not the owner, no set player board view");
                if(playerCamera == null) Debug.Log("No player camera, no set player board view");
            }

            playerCamera.transform.position = playerBoardPosition;
            playerCamera.transform.rotation = Quaternion.Euler(playerBoardRotation);
            isLookingAtPlayerBoard = true;
        }

        public void SetAttackBoardView()
        {
            if (!IsOwner || playerCamera == null)
            {
                if (!IsOwner) Debug.Log("Not the owner, no set attack board view");
                if (playerCamera == null) Debug.Log("No player camera, no set attack board view");
            }

            playerCamera.transform.position = attackBoardPosition;
            playerCamera.transform.rotation = Quaternion.Euler(attackBoardRotation);
            isLookingAtPlayerBoard = false;
        }

        public void ToggleCameraView()
        {
            Debug.Log("Toggling camera view");
            if (!IsOwner)
            {
                Debug.Log("Not the owner, no toggle camera view" );
                return;
            }

            if (isLookingAtPlayerBoard)
            {
                Debug.Log("Switching to attack board view");
                SetAttackBoardView();
            }
            else
            {
                Debug.Log("Switching to player board view");
                SetPlayerBoardView();
            }
        }
    }
}