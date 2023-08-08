using UnityEngine;

namespace RTSSystem.Cameras
{
    public class PullCameraFocus : MonoBehaviour
    {
        [SerializeField] bool _performAction = false;
        [SerializeField] RTSCamera _linkedCamera;

        private void Update()
        {
            if (_performAction)
            {
                _performAction = false;
                _linkedCamera.FocusCameraOn(transform.position);
            }
        }
    }
}
