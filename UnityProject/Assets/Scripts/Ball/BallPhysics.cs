using UnityEngine;
using SoccerGame.Core;
using SoccerGame.Player;

namespace SoccerGame.Ball
{
    /// <summary>
    /// Custom ball flight model: quadratic air drag, Magnus lift, rolling
    /// friction, ground bounce and spin decay. Runs in FixedUpdate.
    /// </summary>
    [RequireComponent(typeof(BallEntity))]
    public class BallPhysics : MonoBehaviour
    {
        // Regulation ball: 0.43 kg, 11 cm radius.
        private const float Mass = 0.43f;
        private const float Radius = 0.11f;
        private const float AirDensity = 1.225f;
        private const float DragCoeff = 0.47f;
        private const float SpinCoeff = 0.01f;
        private const float RollingFriction = 0.02f;
        private const float Restitution = 0.35f;

        private const float MinSpeed = 0.01f;
        private const float RollStopSpeed = 0.05f;

        [SerializeField] private BallEntity ball;
        [SerializeField] private Rigidbody rb;

        private void Awake()
        {
            if (ball == null) ball = GetComponent<BallEntity>();
            if (rb == null) rb = GetComponent<Rigidbody>();

            rb.mass = Mass;
            rb.useGravity = false; // Gravity is applied manually below.
        }

        private void FixedUpdate()
        {
            if (ball.State == BallState.InPossession) return;
            if (ball.State == BallState.Dead) return;

            Vector3 velocity = rb.linearVelocity;
            Vector3 position = rb.position;

            // Gravity (acceleration, mass independent).
            rb.AddForce(Physics.gravity, ForceMode.Acceleration);

            if (position.y > Radius)
            {
                ApplyAerodynamics(velocity);
            }
            else
            {
                ApplyGroundForces(velocity, position);
            }

            // Spin decay.
            ball.Spin *= 1f - 0.5f * Time.fixedDeltaTime;
        }

        private void ApplyAerodynamics(Vector3 velocity)
        {
            float speed = velocity.magnitude;
            if (speed < MinSpeed) return;

            float area = Mathf.PI * Radius * Radius;

            // Quadratic drag: a = -(Cd * rho * PI * r^2 * |v| * v) / m
            Vector3 drag = -(DragCoeff * AirDensity * area * speed * velocity) / Mass;
            rb.AddForce(drag, ForceMode.Acceleration);

            // Magnus lift: a = spinCoeff * (Spin x v) / m
            Vector3 magnus = SpinCoeff * Vector3.Cross(ball.Spin, velocity) / Mass;
            rb.AddForce(magnus, ForceMode.Acceleration);
        }

        private void ApplyGroundForces(Vector3 velocity, Vector3 position)
        {
            Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);
            float horizontalSpeed = horizontal.magnitude;

            // Rolling friction: a = -mu * g * dir(v)
            if (horizontalSpeed > MinSpeed)
            {
                Vector3 friction = -(horizontal / horizontalSpeed) * (RollingFriction * Physics.gravity.magnitude);
                rb.AddForce(friction, ForceMode.Acceleration);
            }
            else if (horizontalSpeed > 0f && horizontalSpeed < RollStopSpeed)
            {
                // Kill residual creep so the ball actually comes to rest.
                rb.linearVelocity = new Vector3(0f, velocity.y, 0f);
            }

            // Bounce.
            if (position.y < Radius && velocity.y < 0f)
            {
                Vector3 v = rb.linearVelocity;
                v.y = -v.y * Restitution;
                v.x *= 0.85f;
                v.z *= 0.85f;
                rb.linearVelocity = v;

                // Depenetrate so the ball rests exactly on the surface.
                rb.position = new Vector3(position.x, Radius, position.z);
            }
        }
    }
}
