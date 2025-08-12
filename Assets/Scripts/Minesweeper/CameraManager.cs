using UnityEngine;

namespace Games.Minesweeper
{
    public class CameraManager : MonoBehaviour
    {
        // Start is called before the first frame update
        [SerializeField]
        private float UpDownSpeed = .1f;
        [SerializeField]
        private float LeftRightSpeed = .1f;
        [SerializeField]
        private float ScrollSpeed = 5f;
        [SerializeField]
        private bool InvertHorizontal = false;
        [SerializeField]
        private bool InvertVertical = false;
        [SerializeField]
        private bool InvertScroll = false;
        private float UpDownSpeedOriginal;
        private float LeftRightSpeedOriginal;
        private float ScrollSpeedOriginal;
        public static CameraManager Instance { get; private set; }
        void Start()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
            UpDownSpeedOriginal = UpDownSpeed;
            LeftRightSpeedOriginal = LeftRightSpeed;
            ScrollSpeedOriginal = ScrollSpeed;
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                Debug.Log("Camera Position: " + transform.position);
                Debug.Log("Camera Rotation: " + transform.rotation.eulerAngles);
            }
            if (Mathf.Abs(UpDownSpeed) != UpDownSpeedOriginal)
            {
                UpDownSpeedOriginal = Mathf.Abs(UpDownSpeed);
            }
            if (Mathf.Abs(LeftRightSpeed) != LeftRightSpeedOriginal)
            {
                LeftRightSpeedOriginal = Mathf.Abs(LeftRightSpeed);
            }
            if (Mathf.Abs(ScrollSpeed) != ScrollSpeedOriginal)
            {
                ScrollSpeedOriginal = Mathf.Abs(ScrollSpeed);
            }
            UpDownSpeed = InvertHorizontal ? -UpDownSpeedOriginal : UpDownSpeedOriginal;
            LeftRightSpeed = InvertVertical ? -LeftRightSpeedOriginal : LeftRightSpeedOriginal;
            ScrollSpeed = InvertScroll ? -ScrollSpeedOriginal : ScrollSpeedOriginal;
            if (Input.GetKey(KeyCode.W))
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + UpDownSpeed);
            }
            if (Input.GetKey(KeyCode.S))
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - UpDownSpeed);
            }
            if (Input.GetKey(KeyCode.A))
            {
                transform.position = new Vector3(transform.position.x - LeftRightSpeed, transform.position.y, transform.position.z);
            }
            if (Input.GetKey(KeyCode.D))
            {
                transform.position = new Vector3(transform.position.x + LeftRightSpeed, transform.position.y, transform.position.z);
            }
            if (Input.GetAxisRaw("Mouse ScrollWheel") < 0)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y + ScrollSpeed, transform.position.z);

            }
            if (Input.GetAxisRaw("Mouse ScrollWheel") > 0)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y - ScrollSpeed, transform.position.z);

            }
        }

        public void SetPositionSquare()
        {
            transform.position = new Vector3(5.4f, 10f, 4.7f);
        }

        public void SetPositionHex()
        {
            transform.position = new Vector3(10f, 15f, 8.5f);
        }
    }

}