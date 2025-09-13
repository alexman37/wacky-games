using System.Collections.Generic;
using UnityEngine;

namespace Games.Battleship
{
    /// Camera for battleship
    public static class BattleshipCameraManager
    {
        public static Camera MainCamera { get; private set; }
        static bool LookingAtPlayerBoard = true;
        public static float turnSpeed = 0.1f;
        public static void Initialize()
        {
            MainCamera = Camera.main;
            if (MainCamera == null)
            {
                Debug.LogError("Main camera not found. Please ensure there is a camera tagged as 'MainCamera' in the scene.");
            }
            else
            {
                MainCamera.transform.position = new Vector3(5, 13, -4);
                MainCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
            }
        }

        // TODO will probably have to change these positions.
        public static void SetCameraPlayer1View()
        {
            if (MainCamera != null)
            {
                MainCamera.transform.position = new Vector3(5, 13, -4);
                MainCamera.transform.rotation = Quaternion.Euler(90,0,0);
            }
        }

        public static void SetCameraPlayer2View()
        {
            if (MainCamera != null)
            {
                MainCamera.transform.position = new Vector3(5, 13, 6);
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