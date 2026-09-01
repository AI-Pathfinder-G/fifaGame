using UnityEngine;
using SoccerGame.Core;
using SoccerGame.Data;

namespace SoccerGame.Player
{
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        private void Awake()
        {
            if (animator == null)
                animator = GetComponent<Animator>();
        }

        public void UpdateAnim(PlayerEntity player)
        {
            if (animator == null || player == null || player.Rb == null)
                return;

            Vector3 velocity = player.Rb.velocity;
            Vector3 planarVelocity = new Vector3(velocity.x, 0f, velocity.z);
            float speed = planarVelocity.magnitude;

            float direction = 0f;
            if (speed > 0.01f)
            {
                Vector3 localDirection = player.transform.InverseTransformDirection(planarVelocity.normalized);
                direction = Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg;
            }

            animator.SetFloat("Speed", speed);
            animator.SetFloat("Direction", direction);
            animator.SetBool("HasBall", player.HasBall);
            animator.SetBool("IsSprinting", player.IsSprinting);
        }

        public void TriggerPass()
        {
            if (animator != null)
                animator.SetTrigger("Pass");
        }

        public void TriggerShoot()
        {
            if (animator != null)
                animator.SetTrigger("Shoot");
        }

        public void TriggerTackle()
        {
            if (animator != null)
                animator.SetTrigger("Tackle");
        }

        public void TriggerCelebrate()
        {
            if (animator != null)
                animator.SetTrigger("Celebrate");
        }
    }
}
