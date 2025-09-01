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
            if (!IsOwner || playerCamera == null) return;

            playerCamera.transform.position = playerBoardPosition;
            playerCamera.transform.rotation = Quaternion.Euler(playerBoardRotation);
            isLookingAtPlayerBoard = true;
        }

        public void SetAttackBoardView()
        {
            if (!IsOwner || playerCamera == null) return;

            playerCamera.transform.position = attackBoardPosition;
            playerCamera.transform.rotation = Quaternion.Euler(attackBoardRotation);
            isLookingAtPlayerBoard = false;
        }

        public void ToggleCameraView()
        {
            if (!IsOwner) return;

            if (isLookingAtPlayerBoard)
            {
                SetAttackBoardView();
            }
            else
            {
                SetPlayerBoardView();
            }
        }

        // Smooth transition between views
        public void SmoothTransitionTo(Vector3 targetPosition, Vector3 targetRotation)
        {
            if (!IsOwner || playerCamera == null) return;

            StartCoroutine(SmoothCameraTransition(targetPosition, targetRotation));
        }

        private System.Collections.IEnumerator SmoothCameraTransition(Vector3 targetPos, Vector3 targetRot)
        {
            Vector3 startPos = playerCamera.transform.position;
            Quaternion startRot = playerCamera.transform.rotation;
            Quaternion targetRotQuat = Quaternion.Euler(targetRot);

            float elapsedTime = 0;
            float transitionDuration = 1f / turnSpeed;

            while (elapsedTime < transitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / transitionDuration;
                t = Mathf.SmoothStep(0f, 1f, t);

                playerCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
                playerCamera.transform.rotation = Quaternion.Lerp(startRot, targetRotQuat, t);

                yield return null;
            }

            playerCamera.transform.position = targetPos;
            playerCamera.transform.rotation = targetRotQuat;
        }
    }
}