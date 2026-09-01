using UnityEngine;

namespace SoccerGame.Camera
{
    public class BroadcastCameraController : MonoBehaviour
    {
        [SerializeField] private float height = 25f;
        [SerializeField] private float distance = 35f;
        [SerializeField] private float followSpeed = 3f;
        [SerializeField] private float rotationSpeed = 2f;
        [SerializeField] private float lookAhead = 5f;
        [SerializeField] private float minX = -40f;
        [SerializeField] private float maxX = 40f;
        [SerializeField] private float minZ = -60f;
        [SerializeField] private float maxZ = 60f;
        [SerializeField] private Transform target;

        private Vector3 _lastTargetPosition;
        private Vector3 _targetVelocity;

        private void Start()
        {
            if (target != null)
                _lastTargetPosition = target.position;
        }

        private void LateUpdate()
        {
            if (target == null) return;

            float dt = Mathf.Max(Time.deltaTime, 0.0001f);

            // Estimate target velocity for look-ahead
            _targetVelocity = (target.position - _lastTargetPosition) / dt;
            _lastTargetPosition = target.position;

            Vector3 lookAheadOffset = Vector3.ClampMagnitude(_targetVelocity, lookAhead);
            Vector3 focusPoint = target.position + lookAheadOffset;

            // Broadcast camera: elevated side view
            Vector3 desiredPosition = new Vector3(
                focusPoint.x,
                height,
                focusPoint.z - distance
            );

            // Clamp position to broadcast bounds
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
            desiredPosition.y = height;
            desiredPosition.z = Mathf.Clamp(desiredPosition.z, minZ, maxZ);

            // Frame-rate independent smoothing
            float followT = 1f - Mathf.Exp(-followSpeed * dt);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, followT);

            // Smoothly rotate to look at the focus point
            Vector3 lookDir = focusPoint - transform.position;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion desiredRotation = Quaternion.LookRotation(lookDir, Vector3.up);
                float rotT = 1f - Mathf.Exp(-rotationSpeed * dt);
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotT);
            }
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            if (target != null)
            {
                _lastTargetPosition = target.position;
                _targetVelocity = Vector3.zero;
            }
        }
    }
}
