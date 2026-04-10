using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class XRSimulatorCollision : MonoBehaviour
{
    public bool applyGravity = true;
    public float gravityStrength = -9.81f;

    private CharacterController _cc;
    private Vector3 _previousPosition;
    private float _verticalVelocity = 0f;

    void OnEnable()
    {
        _cc = GetComponent<CharacterController>();
        _cc.stepOffset = 0.3f;
        _cc.slopeLimit = 45f;
        _cc.minMoveDistance = 0.001f;
        _previousPosition = transform.position;
    }

    void Update()
    {
        Vector3 currentPos = transform.position;
        Vector3 horizontalDelta = currentPos - _previousPosition;
        horizontalDelta.y = 0f;
        transform.position = _previousPosition;

        if (applyGravity)
        {
            if (_cc.isGrounded)
            {
                _verticalVelocity = -1f;
            }
            else
            {
                _verticalVelocity += gravityStrength * Time.deltaTime;
            }
        }
        else
        {
            _verticalVelocity = 0f;
        }

        _cc.Move(horizontalDelta + Vector3.up * _verticalVelocity * Time.deltaTime);
        _previousPosition = transform.position;
    }
}
