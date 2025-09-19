using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Games.Battleship
{
    /// Camera for battleship
    public class BattleshipCameraManager : MonoBehaviour
    {
        public static BattleshipCameraManager instance;

        public static Camera MainCamera { get; private set; }
        static bool LookingAtPlayerBoard = true;
        public static float turnSpeed = 0.1f;

        private static Vector3 player1Cam = new Vector3(5, 13, -4);
        private static Vector3 player2Cam = new Vector3(5, 13, 6);

        private void Start()
        {
            if (instance == null) instance = this;
            else Destroy(this.gameObject);
        }

        public static void Initialize()
        {
            MainCamera = Camera.main;
            if (MainCamera == null)
            {
                Debug.LogError("Main camera not found. Please ensure there is a camera tagged as 'MainCamera' in the scene.");
            }
            else
            {
                MainCamera.transform.position = player1Cam;
                MainCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
            }
        }

        // TODO will probably have to change these positions.
        public static void SetCameraPlayer1View()
        {
            if (MainCamera != null)
            {
                MainCamera.transform.position = player1Cam;
                MainCamera.transform.rotation = Quaternion.Euler(90,0,0);
            }
        }

        public void TransitionToCameraPlayer1View()
        {
            if (MainCamera != null)
            {
                StartCoroutine(cameraTransition(1f, MainCamera.transform.position, player1Cam));
            }
        }

        public static void SetCameraPlayer2View()
        {
            if (MainCamera != null)
            {
                MainCamera.transform.position = player2Cam;
            }
        }

        public void TransitionToCameraPlayer2View()
        {
            if (MainCamera != null)
            {
                StartCoroutine(cameraTransition(1f, MainCamera.transform.position, player2Cam));
            }
        }

        private static IEnumerator cameraTransition(float time, Vector3 start, Vector3 end)
        {
            for(float t = 0; t < time; t += Time.deltaTime)
            {
                MainCamera.transform.position = UIUtils.XerpStandard(start, end, t / time);
                yield return null;
            }
        }

        // TODO a better animation for switching between the two sides.
        public static void RotateCamera()
        {
            if (MainCamera != null)
            {
                if(LookingAtPlayerBoard)
                {
                    SetCameraPlayer2View();
                    LookingAtPlayerBoard = false;
                }
                else
                {
                    SetCameraPlayer1View();
                    LookingAtPlayerBoard = true;
                }
            }
            else
            {
                Debug.LogError("Main camera is not initialized. Please call Initialize() first.");
            }
        }
    }


}