#region

using Gui;
using UnityEngine;
using UnityEngine.EventSystems;

#endregion

public class ObjRotator : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector2 _lastMousePos;
    private Vector3 _lerpRotation;

    private int _mouseDownCount;

    private Vector2 _mousePos;
    private Vector3 _rotation;
    public float Filter = 2f;
    public bool AllowX = true;

    public bool AllowY = true;

    public bool InvertX;
    public bool InvertY;
    public KeyCode KeyToHoldToRotate = KeyCode.None;
    public KeyCode KeyToHoldToPan = KeyCode.None;

    public PointerEventData.InputButton RotateButton;
    public PointerEventData.InputButton PanButton;
    private Vector3 _lastRaycast;
    private Vector3 _targetDrag;
    private Quaternion _targetRotation;

    public void Reset()
    {
        transform.rotation = Quaternion.identity;
    }

    private void Update()
    {
        var distanceFromTarget = (_targetDrag - transform.position).magnitude;
        if (_targetDrag.magnitude > 0.1f && distanceFromTarget > 0.1f)
        {
            var pos = Vector3.Slerp(transform.position, _targetDrag, 0.6f);
            transform.position = pos;
        }

        var distanceFromAngle = Quaternion.Angle(_targetRotation, transform.rotation);
        if (_targetRotation.eulerAngles.magnitude > 1f && distanceFromAngle > 1f)
        {
            var rot = Quaternion.Slerp(transform.rotation, _targetRotation, 0.7f);
            transform.rotation = rot;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _lastMousePos = eventData.position;
    }


    public void OnDrag(PointerEventData eventData)
    {
        var canRotate = (KeyToHoldToRotate == KeyCode.None) || (Input.GetKey(KeyToHoldToRotate));
        canRotate = canRotate && eventData.button == RotateButton;

        if (canRotate)
        {
            Rotate(eventData);
        }

        var canPan = (KeyToHoldToPan == KeyCode.None) || (Input.GetKey(KeyToHoldToPan));
        canPan = canPan && eventData.button == PanButton;

        if (canPan)
        {
            var pos = eventData.pointerCurrentRaycast.worldPosition;
            var position = transform.position;

            if (pos.magnitude < 0.001f) return;

            pos.z = position.z;
            _targetDrag = pos;
        }
    }


    private void Rotate(PointerEventData eventData)
    {
        var mousePos = eventData.position;
        var delta = mousePos - _lastMousePos;
        if (Mathf.Abs(delta.x) < Filter) delta.x = 0;
        if (Mathf.Abs(delta.y) < Filter) delta.y = 0;
        Debug.Log("delta : " + delta);
        if (delta.magnitude < 0.01f) return;
        Debug.Log("delta Pass");

        if (!AllowX)
        {
            delta.x = 0;
        }
        else if (InvertX)
        {
            delta.x = -delta.x;
        }

        if (!AllowY)
        {
            delta.y = 0;
        }
        else if (InvertY)
        {
            delta.y = -delta.y;
        }

        var axis = Vector3.Cross(delta, Vector3.forward).normalized;
        var position = transform.position;
        Debug.DrawRay(position, axis, Color.white, 1);

        var amount = Quaternion.AngleAxis(delta.magnitude, axis);
        _targetRotation = transform.rotation * amount;
//        transform.Rotate(axis, delta.magnitude, Space.World);
        _lastMousePos = mousePos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
    }
}