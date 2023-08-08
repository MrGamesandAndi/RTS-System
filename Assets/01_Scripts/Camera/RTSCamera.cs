using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RTSSystem.Cameras
{
    public class RTSCamera : MonoBehaviour
    {
        [System.Serializable]
        public class CameraConfig
        {
            public float heightOffset = 20f;
            public float horizontalOffset = 20f;
        }

        [SerializeField] GameObject _cameraFocus;
        [SerializeField] Terrain _linkedTerrain;
        [SerializeField] [Range(0f, 1f)] float _initialZoomLevel = 0.5f;

        [SerializeField]
        CameraConfig _minimumZoomConfig = new CameraConfig()
        {
            heightOffset = 50f,
            horizontalOffset = 50f
        };

        [SerializeField]
        CameraConfig _maximumZoomConfig = new CameraConfig()
        {
            heightOffset = 10f,
            horizontalOffset = 5f
        };

        [SerializeField] AnimationCurve _zoomMappingCurve;
        [SerializeField] float _zoomSensitivity = 0.1f;
        [SerializeField] bool _invertZoomDirection = false;
        [SerializeField] float _cameraPanSafetyMargin = 0.05f;
        [SerializeField] float _cameraPanSpeed = 50f;

        Camera _linkedCamera;
        float _currentZoomLevel;
        Vector2 _moveInput;
        Vector3 _desiredCameraFocusLocation;

        private void Awake()
        {
            _linkedCamera = GetComponent<Camera>();
            _currentZoomLevel = _initialZoomLevel;
            _desiredCameraFocusLocation = _cameraFocus.transform.position;
        }

        private void Start()
        {
            UpdateCameraPosition();
        }

        private void Update()
        {
            if (_moveInput.sqrMagnitude > float.Epsilon)
            {
                UpdateCameraFocusLocation(_moveInput * _cameraPanSpeed * Time.deltaTime);
            }

            if ((_desiredCameraFocusLocation - _cameraFocus.transform.position).sqrMagnitude > float.Epsilon) 
            {
                Vector3 newPosition = Vector3.MoveTowards(_cameraFocus.transform.position, 
                    _desiredCameraFocusLocation, _cameraPanSpeed * Time.deltaTime);
                _cameraFocus.transform.position = GetClampedFocusLocation(newPosition);
                UpdateCameraPosition();
            }
        }

        private void UpdateCameraFocusLocation(Vector2 positionDelta)
        {
            Vector3 newPosition = _cameraFocus.transform.position;
            newPosition += _cameraFocus.transform.forward * positionDelta.y;
            newPosition += _cameraFocus.transform.right * positionDelta.x;

            _desiredCameraFocusLocation = GetClampedFocusLocation(newPosition);
        }

        private Vector3 GetClampedFocusLocation(Vector3 newPosition)
        {
            //Get normalised location clamped to the terrain bounds
            Vector3 normalisedTerrainPosition = newPosition - _linkedTerrain.transform.position;
            normalisedTerrainPosition.x = Mathf.Clamp01(normalisedTerrainPosition.x / _linkedTerrain.terrainData.size.x);
            normalisedTerrainPosition.z = Mathf.Clamp01(normalisedTerrainPosition.z / _linkedTerrain.terrainData.size.z);
            normalisedTerrainPosition.y = _linkedTerrain.terrainData.GetInterpolatedHeight(normalisedTerrainPosition.x,
                normalisedTerrainPosition.z);

            //Apply safety boundary
            normalisedTerrainPosition.x = Mathf.Clamp(normalisedTerrainPosition.x, _cameraPanSafetyMargin, 1f - _cameraPanSafetyMargin);
            normalisedTerrainPosition.z = Mathf.Clamp(normalisedTerrainPosition.z, _cameraPanSafetyMargin, 1f - _cameraPanSafetyMargin);

            //Convert back to world location
            newPosition.x = _linkedTerrain.transform.position.x + normalisedTerrainPosition.x * _linkedTerrain.terrainData.size.x;
            newPosition.y = normalisedTerrainPosition.y;
            newPosition.z = _linkedTerrain.transform.position.z + normalisedTerrainPosition.z * _linkedTerrain.terrainData.size.z;

            return newPosition;
        }

        private void UpdateCameraPosition()
        {
            Vector3 cameraLocation = _cameraFocus.transform.position;
            float workingZoomLevel = _zoomMappingCurve.Evaluate(_currentZoomLevel);
            float workingHeightOffset = Mathf.Lerp(_minimumZoomConfig.heightOffset, 
                _maximumZoomConfig.heightOffset, workingZoomLevel);
            float workingHorizontalOffset = Mathf.Lerp(_minimumZoomConfig.heightOffset,
                _maximumZoomConfig.heightOffset, workingZoomLevel);
            cameraLocation += workingHeightOffset * Vector3.up;
            cameraLocation -= workingHorizontalOffset * _cameraFocus.transform.forward;
            _linkedCamera.transform.position = cameraLocation;
            _linkedCamera.transform.LookAt(_cameraFocus.transform, Vector3.up);
        }

        private void OnCameraZoom(InputValue value)
        {
            float zoomInput = value.Get<float>();

            if (zoomInput > float.Epsilon)
            {
                UpdateZoom(1f);
            }
            else if (zoomInput < -float.Epsilon)
            {
                UpdateZoom(-1f);
            }
        }

        private void OnMove(InputValue value)
        {
            _moveInput = value.Get<Vector2>();
        }

        private void UpdateZoom(float delta)
        {
            float workingDelta = _invertZoomDirection ? -delta : delta;
            _currentZoomLevel = Mathf.Clamp01(_currentZoomLevel + workingDelta * _zoomSensitivity * Time.deltaTime);
            UpdateCameraPosition();
        }

        public void FocusCameraOn(Vector3 location)
        {
            _desiredCameraFocusLocation = GetClampedFocusLocation(location);
        }
    }
}