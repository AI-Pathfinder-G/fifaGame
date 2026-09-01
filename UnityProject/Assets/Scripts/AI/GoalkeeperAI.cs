using UnityEngine;
using SoccerGame.Core;
using SoccerGame.Player;
using SoccerGame.Data;

namespace SoccerGame.AI
{
    public class GoalkeeperAI : MonoBehaviour
    {
        [SerializeField] private PlayerEntity goalkeeper;
        [SerializeField] private Transform goalCenter;
        [SerializeField] private float reactionSpeed = 3f;
        [SerializeField] private float diveDistance = 4f;

        [Header("Positioning")]
        [SerializeField] private float goalHalfWidth = 3.66f; // regulation goal is 7.32 m wide
        [SerializeField] private float lineOffset = 1.0f;     // max distance stepping off the line
        [SerializeField] private Transform ballTransform;

        private Animator animator;
        private bool isDiving;
        private Vector3 diveTarget;
        private float diveTimer;

        private void Awake()
        {
            if (goalkeeper == null)
                goalkeeper = GetComponent<PlayerEntity>();

            animator = GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            if (goalkeeper == null || goalCenter == null)
                return;

            if (isDiving)
            {
                UpdateDive();
                return;
            }

            PositionOnLine();
        }

        public void SetBallTransform(Transform ball)
        {
            ballTransform = ball;
        }

        private void PositionOnLine()
        {
            Vector3 gkPos = goalkeeper.transform.position;
            Vector3 goalPos = goalCenter.position;

            float targetX = goalPos.x;
            float targetZ = goalPos.z;

            if (ballTransform != null)
            {
                Vector3 ballPos = ballTransform.position;

                // Track the ball laterally, clamped between the posts.
                targetZ = Mathf.Clamp(ballPos.z, goalPos.z - goalHalfWidth, goalPos.z + goalHalfWidth);

                // Step off the line as the ball gets closer.
                float outDir = Mathf.Sign(ballPos.x - goalPos.x);
                float dist = Mathf.Abs(ballPos.x - goalPos.x);
                float advance = Mathf.Clamp01(1f - dist / 30f) * lineOffset;
                targetX = goalPos.x + outDir * advance;
            }

            Vector3 target = new Vector3(targetX, gkPos.y, targetZ);
            goalkeeper.transform.position = Vector3.Lerp(gkPos, target, reactionSpeed * Time.deltaTime);

            // Face the play.
            Vector3 look = ballTransform != null
                ? ballTransform.position - goalkeeper.transform.position
                : goalCenter.forward;
            look.y = 0f;
            if (look.sqrMagnitude > 0.001f)
            {
                goalkeeper.transform.rotation = Quaternion.Slerp(
                    goalkeeper.transform.rotation,
                    Quaternion.LookRotation(look),
                    reactionSpeed * 2f * Time.deltaTime);
            }
        }

        public void OnShotDetected(Vector3 ballPos, Vector3 ballVel)
        {
            if (isDiving || goalCenter == null)
                return;

            float goalX = goalCenter.position.x;
            float toGoal = goalX - ballPos.x;

            // Ball must be travelling toward this goal.
            if (Mathf.Abs(ballVel.x) < 0.01f || Mathf.Sign(ballVel.x) != Mathf.Sign(toGoal))
                return;

            float timeToLine = toGoal / ballVel.x;
            if (timeToLine < 0f || timeToLine > 1.5f)
                return;

            Vector3 intercept = ballPos + ballVel * timeToLine;

            // Only react to shots on target.
            if (Mathf.Abs(intercept.z - goalCenter.position.z) > goalHalfWidth + 0.5f)
                return;

            float clampedZ = Mathf.Clamp(intercept.z, goalCenter.position.z - goalHalfWidth, goalCenter.position.z + goalHalfWidth);
            float lateral = clampedZ - goalkeeper.transform.position.z;

            if (Mathf.Abs(lateral) > diveDistance)
                clampedZ = goalkeeper.transform.position.z + Mathf.Sign(lateral) * diveDistance;

            diveTarget = new Vector3(goalX, goalkeeper.transform.position.y, clampedZ);
            isDiving = true;
            diveTimer = 0f;

            if (animator != null)
            {
                animator.SetFloat("DiveDirection", Mathf.Sign(lateral));
                animator.SetTrigger("Dive");
            }
        }

        private void UpdateDive()
        {
            diveTimer += Time.deltaTime;
            float t = Mathf.Clamp01(diveTimer / 0.4f);
            goalkeeper.transform.position = Vector3.Lerp(goalkeeper.transform.position, diveTarget, t);

            // Recover after the dive has played out.
            if (t >= 1f && diveTimer > 1.2f)
                isDiving = false;
        }
    }
}
