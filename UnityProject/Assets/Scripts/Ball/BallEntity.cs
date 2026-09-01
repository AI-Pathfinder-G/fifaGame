using UnityEngine;
using SoccerGame.Core;
using SoccerGame.Player;

namespace SoccerGame.Ball
{
    /// <summary>
    /// Core ball state container. Owns the ball's physical references and
    /// exposes high-level possession / release operations to gameplay code.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public class BallEntity : MonoBehaviour
    {
        private const float FlyingVerticalThreshold = 0.1f;

        [SerializeField] private Rigidbody rb;
        [SerializeField] private SphereCollider collider;

        public BallState State { get; private set; }
        public PlayerEntity Owner { get; private set; }

        /// <summary>Angular velocity (rad/s) driving Magnus force and roll behaviour.</summary>
        public Vector3 Spin { get; set; }

        public Rigidbody Rb => rb;
        public SphereCollider Collider => collider;

        private void Awake()
        {
            if (rb == null) rb = GetComponent<Rigidbody>();
            if (collider == null) collider = GetComponent<SphereCollider>();
        }

        public void SetOwner(PlayerEntity owner)
        {
            State = BallState.InPossession;
            Owner = owner;
            Spin = Vector3.zero;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        public void Release(Vector3 velocity, Vector3 spin)
        {
            Owner = null;
            State = Mathf.Abs(velocity.y) > FlyingVerticalThreshold ? BallState.Flying : BallState.Rolling;
            rb.linearVelocity = velocity;
            rb.angularVelocity = spin;
            Spin = spin;
        }

        public void ResetPosition(Vector3 pos)
        {
            transform.position = pos;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            Spin = Vector3.zero;
            Owner = null;
            State = BallState.Dead;
        }
    }
}
