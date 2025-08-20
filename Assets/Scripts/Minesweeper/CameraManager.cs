using UnityEngine;
using System;

namespace Games.Minesweeper
{
    public class CameraManager : MonoBehaviour
    {
        // Start is called before the first frame update
        [SerializeField]
        private float UpDownSpeed = .07f;
        [SerializeField]
        private float LeftRightSpeed = .07f;
        [SerializeField]
        private float ScrollSpeed = 5f;
        [SerializeField]
        private float dragMultiplier = -0.1f;
        [SerializeField]
        private bool InvertHorizontal = false;
        [SerializeField]
        private bool InvertVertical = false;
        [SerializeField]
        private bool InvertScroll = false;
        [SerializeField]
        private bool InvertDrag = false;

        private bool dragging = false;
        private float dragSumDistance;
        private bool exceededDragThreshold = false;
        public static event Action cameraDragExceededThreshold;

        const float CAMERA_MARGIN = -1;
        const float SQUARE_TO_HEX_ZOOM_RATIO = 1.5f;
        const float CLICK_OR_DRAG_THRESHOLD = 2f;

        Vector3 boundedMins; // min X, Y and Z
        Vector3 boundedMaxes; // max X, Y and Z
        public static CameraManager Instance { get; private set; }
        void Start()
        {
            if (Instance == null)
            {
                Instance = this;
                cameraDragExceededThreshold += () => { };
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }

            // TODO - we can change these "on the fly" from a settings menu if needed
            UpDownSpeed = InvertVertical ? -UpDownSpeed : UpDownSpeed;
            LeftRightSpeed = InvertHorizontal ? -LeftRightSpeed : LeftRightSpeed;
            ScrollSpeed = InvertScroll ? -ScrollSpeed : ScrollSpeed;
            dragMultiplier = InvertDrag ? -dragMultiplier : dragMultiplier;
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                Debug.Log("Camera Position: " + transform.position);
                Debug.Log("Camera Rotation: " + transform.rotation.eulerAngles);
            }
            /*UpDownSpeed = InvertVertical ? -UpDownSpeedOriginal : UpDownSpeedOriginal;
            LeftRightSpeed = InvertHorizontal ? -LeftRightSpeedOriginal : LeftRightSpeedOriginal;
            ScrollSpeed = InvertScroll ? -ScrollSpeedOriginal : ScrollSpeedOriginal;*/
            if (Input.GetKey(KeyCode.W))
            {
                transform.position = boundedCameraPosition(transform.position.x, transform.position.y, transform.position.z + UpDownSpeed);
            }
            if (Input.GetKey(KeyCode.S))
            {
                transform.position = boundedCameraPosition(transform.position.x, transform.position.y, transform.position.z - UpDownSpeed);
            }
            if (Input.GetKey(KeyCode.A))
            {
                transform.position = boundedCameraPosition(transform.position.x - LeftRightSpeed, transform.position.y, transform.position.z);
            }
            if (Input.GetKey(KeyCode.D))
            {
                transform.position = boundedCameraPosition(transform.position.x + LeftRightSpeed, transform.position.y, transform.position.z);
            }
            if (Input.GetAxisRaw("Mouse ScrollWheel") < 0)
            {
                transform.position = boundedCameraPosition(transform.position.x, transform.position.y + ScrollSpeed, transform.position.z);

            }
            if (Input.GetAxisRaw("Mouse ScrollWheel") > 0)
            {
                transform.position = boundedCameraPosition(transform.position.x, transform.position.y - ScrollSpeed, transform.position.z);
            }
            if (Input.GetMouseButtonDown(0))
            {
                dragging = true;
                dragSumDistance = 0;
                exceededDragThreshold = false;
            }
            if(Input.GetMouseButtonUp(0))
            {
                dragging = false;
            }
            if(dragging)
            {
                Vector2 delta = ((Vector2)Input.mousePositionDelta) * dragMultiplier;

                // keep track of how far we drag. past a certain point we do not count anything as a "click".
                if(!exceededDragThreshold)
                {
                    dragSumDistance += delta.magnitude;
                    if (dragSumDistance > CLICK_OR_DRAG_THRESHOLD)
                    {
                        exceededDragThreshold = true;
                        cameraDragExceededThreshold.Invoke();
                    }
                }
                
                transform.position = boundedCameraPosition(transform.position.x + delta.x, transform.position.y, transform.position.z + delta.y);
            }
        }

        /// <summary>
        /// returns true if the player did not drag the camera far enough to count as "dragging", it will instead be a single click.
        /// </summary>
        public bool countedAsTilePress()
        {
            return dragSumDistance < CLICK_OR_DRAG_THRESHOLD;
        }

        /// <summary>
        /// Given a proposed new position, will return the position clamped to known mins and maxes
        /// </summary>
        private Vector3 boundedCameraPosition(float proposedX, float proposedY, float proposedZ)
        {
            return new Vector3(
                Mathf.Clamp(proposedX, boundedMins.x, boundedMaxes.x),
                Mathf.Clamp(proposedY, boundedMins.y, boundedMaxes.y),
                Mathf.Clamp(proposedZ, boundedMins.z, boundedMaxes.z)
            );
        }

        public void SetPositionSquare(float rowCount, float colCount)
        {
            transform.position = new Vector3(5.4f, 10f, 4.7f);
            float maxYZoom = 5 + ((int)Mathf.Max(rowCount, colCount) / 5) * 5;

            boundedMins = new Vector3(-CAMERA_MARGIN, 5, -CAMERA_MARGIN);
            boundedMaxes = new Vector3(rowCount + CAMERA_MARGIN, maxYZoom, colCount + CAMERA_MARGIN);
        }

        public void SetPositionHex(float rowCount, float colCount)
        {
            transform.position = new Vector3(10f, 15f, 8.5f);
            float maxYZoom = 5 + ((int)Mathf.Max(rowCount, colCount) / 5) * 5 * SQUARE_TO_HEX_ZOOM_RATIO;

            boundedMins = new Vector3(-CAMERA_MARGIN, 5, -CAMERA_MARGIN);
            boundedMaxes = new Vector3(rowCount + CAMERA_MARGIN, maxYZoom, colCount + CAMERA_MARGIN);
        }
    }

}