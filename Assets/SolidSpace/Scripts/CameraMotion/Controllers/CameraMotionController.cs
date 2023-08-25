using SolidSpace.GameCycle;
using SolidSpace.UI.Core;
using UnityEngine;

namespace SolidSpace.CameraMotion
{
    public class CameraMotionController : IInitializable, IUpdatable
    {
        private readonly Camera _camera;
        private readonly IUIManager _uiManager;
        private readonly Transform _cameraTransform;

        private int _zoom;
        private bool _isMoving;
        private Vector2 _startMousePosition;
        private Vector2 _startCameraPosition;

        public CameraMotionController(Camera camera, IUIManager uiManager)
        {
            _camera = camera;
            _uiManager = uiManager;
            _cameraTransform = camera.transform;
        }
        
        public void OnInitialize()
        {
            SetCameraPosition(Vector2.zero);
            _camera.orthographicSize = GetScreenSize().y / 2;
        }
        
        public void OnUpdate()
        {
            var mousePosition = GetMousePosition();
            var screenSize = GetScreenSize();
            var scrollDelta = (int) Input.mouseScrollDelta.y;
            var isCursorInsideScreen = (mousePosition.x > 0) && (mousePosition.x < screenSize.x) &&
                                       (mousePosition.y > 0) && (mousePosition.y < screenSize.y);
            var newZoom = Mathf.Clamp(_zoom + scrollDelta, 0, 3);
            
            if (isCursorInsideScreen && newZoom != _zoom && !_uiManager.IsMouseOver)
            {
                var cursor = mousePosition / screenSize - new Vector2(0.5f, 0.5f);
                var offset = cursor * screenSize * (1f / (1 << _zoom) - 1f / (1 << newZoom));
                SetCameraPosition(GetCameraPosition() + offset);
                
                _zoom = newZoom;
                _camera.orthographicSize = screenSize.y / (1 << (_zoom + 1));
                _isMoving = false;
            }

            if (_isMoving)
            {
                var mouseDelta = mousePosition / screenSize - _startMousePosition;
                SetCameraPosition(_startCameraPosition - mouseDelta * screenSize / (1 << _zoom));

                if (Input.GetMouseButton(1))
                {
                    return;
                }

                _isMoving = false;
            }

            if (Input.GetMouseButtonDown(1) && !_uiManager.IsMouseOver)
            {
                _isMoving = true;
                _startMousePosition = mousePosition / screenSize;
                _startCameraPosition = GetCameraPosition();
            }
        }

        private void SetCameraPosition(Vector2 position)
        {
            _cameraTransform.position = new Vector3(position.x, position.y, -1);
        }

        private Vector2 GetCameraPosition()
        {
            return _cameraTransform.position;
        }

        private Vector2 GetMousePosition()
        {
            return Input.mousePosition;
        }

        private Vector2 GetScreenSize()
        {
            return new Vector2(Screen.width, Screen.height);
        }

        public void OnFinalize()
        {
            
        }
    }
}