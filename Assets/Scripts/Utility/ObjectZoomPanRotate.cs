#region

using UnityEngine;
using UnityEngine.EventSystems;

#endregion

namespace Utility
{
    public class ObjectZoomPanRotate : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        private Camera _camera;
        private IHideGuiManager _hideGuiManager;
        private bool _hovering;
        private Vector2 _lastMousePos;

        private Vector3 _lastRaycast;
        private Vector3 _lerpRotation;

        private int _mouseDownCount;

        private Vector2 _mousePos;
        private Vector3 _offset;
        private Vector3 _rotation;
        private Vector3 _startPosition;
        private Quaternion _startRotation;
        private Vector3 _targetPosition;
        private Quaternion _targetRotation;

        public bool AllowHide = true;

        public bool AllowX = true;
        public bool AllowY = true;

        public float Filter = 2f;
        public GameObject HideGuiManager;

        public bool InvertX;
        public bool InvertY;
        public bool InvertZoom;
        public KeyCode KeyToHoldToPan = KeyCode.None;

        public KeyCode KeyToHoldToRotate = KeyCode.None;
        public PointerEventData.InputButton PanButton;

        public PointerEventData.InputButton RotateButton;
        public float ZoomSpeed = 10f;


        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!(eventData.button == PanButton || eventData.button == RotateButton)) return;
            _lastMousePos = eventData.position;
            _offset = transform.position - eventData.pointerCurrentRaycast.worldPosition;
            if (AllowHide) _hideGuiManager?.SaveHideStateAndHideAndLock(this);
        }


        public void OnDrag(PointerEventData eventData)
        {
            if (!(eventData.button == PanButton || eventData.button == RotateButton)) return;
            var canRotate = KeyToHoldToRotate == KeyCode.None || Input.GetKey(KeyToHoldToRotate);
            canRotate = canRotate && eventData.button == RotateButton;

            if (canRotate) Rotate(eventData);

            var canPan = KeyToHoldToPan == KeyCode.None || Input.GetKey(KeyToHoldToPan);
            canPan = canPan && eventData.button == PanButton;

            if (canPan) Pan(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!(eventData.button == PanButton || eventData.button == RotateButton)) return;
            _hideGuiManager?.HideGuiLocker.Unlock(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovering = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovering = false;
        }

        private void Start()
        {
            _camera = Camera.main;
            var transform1 = transform;
            _startPosition = transform1.position;
            _startRotation = transform1.rotation;
            Reset();
            if (HideGuiManager)
            {
                _hideGuiManager = HideGuiManager.GetComponent<IHideGuiManager>();
            }
        }

        public void Reset()
        {
            _targetPosition = _startPosition;
            _targetRotation = _startRotation;
        }

        private void Update()
        {
            const int targetFps = 60;
            var distanceFromTarget = (_targetPosition - transform.position).magnitude;
            if (distanceFromTarget > 0.1f)
            {
                var pos = Vector3.Lerp(transform.position, _targetPosition, 0.6f * targetFps * Time.deltaTime);
                transform.position = pos;
            }
            else
            {
                _targetPosition = transform.position;
            }

            var distanceFromAngle = Quaternion.Angle(_targetRotation, transform.rotation);
            if (Mathf.Abs(distanceFromAngle) > 1f)
            {
                var rot = Quaternion.Slerp(transform.rotation, _targetRotation, 0.7f * targetFps * Time.deltaTime);
                transform.rotation = rot;
            }
            else
            {
                _targetRotation = transform.rotation;
            }

            if (!_hovering) return;

            Zoom();
        }

        private void Zoom()
        {
            const float minDistanceFromCamera = 5.0f;
            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out var hit);
            var direction = (hit.point - _camera.transform.position).normalized;
            var scrollWheel = -Input.GetAxis("Mouse ScrollWheel");
            if (InvertZoom) scrollWheel = -scrollWheel;
            var target = transform.position + scrollWheel * ZoomSpeed * direction;
            if (target.z < _camera.transform.position.z + minDistanceFromCamera) return;
            _targetPosition = target;
        }

        private void Pan(PointerEventData eventData)
        {
            var pos = eventData.pointerCurrentRaycast.worldPosition;
            var position = transform.position;

            if (pos.magnitude < 0.001f) return;

            pos = pos + _offset;
            pos.z = position.z;
            _targetPosition = pos;
        }


        private void Rotate(PointerEventData eventData)
        {
            var mousePos = eventData.position;
            var delta = mousePos - _lastMousePos;
            if (Mathf.Abs(delta.x) < Filter) delta.x = 0;
            if (Mathf.Abs(delta.y) < Filter) delta.y = 0;
//        General.Logger.Log("delta : " + delta);
            if (delta.magnitude < 0.01f) return;
//        General.Logger.Log("delta Pass");

            if (!AllowX)
                delta.x = 0;
            else if (InvertX) delta.x = -delta.x;

            if (!AllowY)
                delta.y = 0;
            else if (InvertY) delta.y = -delta.y;

            var axis = Vector3.Cross(delta, Vector3.forward).normalized;

            var objTransform = transform;
            var position = objTransform.position;
            var originalRotation = objTransform.rotation;
            objTransform.RotateAround(position, axis, delta.magnitude);
            _targetRotation = objTransform.rotation;
            _targetRotation.z = 0;
            transform.rotation = originalRotation;
            _lastMousePos = mousePos;
        }
    }
}