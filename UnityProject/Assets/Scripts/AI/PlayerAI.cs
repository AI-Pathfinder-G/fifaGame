using System.Collections.Generic;
using UnityEngine;
using SoccerGame.Core;
using SoccerGame.Player;
using SoccerGame.Data;

namespace SoccerGame.AI
{
    public class PlayerAI : MonoBehaviour
    {
        [SerializeField] private PlayerEntity player;
        [SerializeField] private FormationController formation;
        [SerializeField] private float aiUpdateInterval = 0.05f;
        [SerializeField] private float aiSpeed = 5f;

        [Header("Decision Tuning")]
        [SerializeField] private float shootingRange = 25f;
        [SerializeField] private float pressureRadius = 3f;
        [SerializeField] private float supportDistance = 12f;
        [SerializeField] private float pressDistance = 20f;
        [SerializeField] private float dribbleStep = 4f;

        private Transform ballTransform;
        private float nextDecisionTime;
        private Vector3 targetPosition;
        private int slotIndex;

        private readonly List<PlayerEntity> teammates = new List<PlayerEntity>();
        private readonly List<PlayerEntity> opponents = new List<PlayerEntity>();

        private void Start()
        {
            if (player == null)
                player = GetComponent<PlayerEntity>();

            CachePlayers();

            if (player != null)
                targetPosition = player.transform.position;
        }

        private void Update()
        {
            if (player == null)
                return;

            if (Time.time >= nextDecisionTime)
            {
                nextDecisionTime = Time.time + aiUpdateInterval;
                MakeDecision();
            }

            MoveTowardTarget();
        }

        public void SetFormationPosition(Vector3 position)
        {
            targetPosition = position;
        }

        public void SetBallTransform(Transform ball)
        {
            ballTransform = ball;
        }

        public void SetSlotIndex(int index)
        {
            slotIndex = index;
        }

        public GamePhase EvaluatePhase()
        {
            if (ballTransform == null || player == null)
                return GamePhase.TransitionToAttack;

            return TacticalEvaluator.EvaluatePhase(ballTransform, player.Team);
        }

        private void CachePlayers()
        {
            teammates.Clear();
            opponents.Clear();

            PlayerEntity[] found = Object.FindObjectsByType<PlayerEntity>(FindObjectsSortMode.None);
            foreach (PlayerEntity p in found)
            {
                if (p == null)
                    continue;

                if (p.Team == player.Team)
                    teammates.Add(p); // includes this player
                else
                    opponents.Add(p);
            }
        }

        private void MakeDecision()
        {
            if (ballTransform == null)
            {
                HoldFormation(GamePhase.TransitionToAttack);
                return;
            }

            if (player.HasBall)
            {
                DecideWithBall();
            }
            else if (TeamHasBall())
            {
                MoveToSupport();
            }
            else if (OpponentHasBall())
            {
                Defend();
            }
            else
            {
                // Loose ball: nearest player chases it, everyone else holds shape.
                PlayerEntity nearest = TacticalEvaluator.FindNearestToBall(teammates, ballTransform.position);
                if (nearest == player)
                    targetPosition = ballTransform.position;
                else
                    HoldFormation(EvaluatePhase());
            }
        }

        private void DecideWithBall()
        {
            Vector3 pos = player.transform.position;
            Vector3 goal = GetOpponentGoalPosition();
            float distToGoal = Vector3.Distance(pos, goal);
            float shotChance = TacticalEvaluator.EvaluateShotChance(pos, goal);

            if (distToGoal <= shootingRange && shotChance >= 0.55f)
            {
                Shoot(goal);
                return;
            }

            if (IsUnderPressure())
            {
                PlayerEntity option = FindBestPassOption();
                if (option != null)
                    Pass(option);
                else
                    HoldBall();
                return;
            }

            if (HasSpaceAhead(goal))
            {
                Dribble(goal);
                return;
            }

            PlayerEntity safeOption = FindBestPassOption();
            if (safeOption != null)
                Pass(safeOption);
            else
                HoldBall();
        }

        private void MoveToSupport()
        {
            Vector3 basePos = formation != null
                ? formation.GetTargetPosition(player, slotIndex, player.Team, GamePhase.Attack)
                : player.transform.position;

            // Offer a passing lane at a useful distance from the carrier.
            Vector3 ballPos = ballTransform.position;
            Vector3 offset = basePos - ballPos;
            offset.y = 0f;
            if (offset.sqrMagnitude < 0.01f)
                offset = player.transform.forward;
            offset.Normalize();

            Vector3 supportPos = ballPos + offset * supportDistance;
            supportPos.x = Mathf.Clamp(supportPos.x, -52.5f, 52.5f);
            supportPos.z = Mathf.Clamp(supportPos.z, -34f, 34f);
            supportPos.y = player.transform.position.y;

            targetPosition = supportPos;
        }

        private void Defend()
        {
            PlayerEntity nearest = TacticalEvaluator.FindNearestToBall(teammates, ballTransform.position);
            float distToBall = Vector3.Distance(player.transform.position, ballTransform.position);

            if (nearest == player && distToBall <= pressDistance)
            {
                // Closest defender presses the ball carrier.
                targetPosition = ballTransform.position;
            }
            else
            {
                HoldFormation(GamePhase.Defense);
            }
        }

        private void HoldFormation(GamePhase phase)
        {
            if (formation != null)
                targetPosition = formation.GetTargetPosition(player, slotIndex, player.Team, phase);
        }

        private void Shoot(Vector3 goal)
        {
            Vector3 dir = goal - ballTransform.position;
            dir.y = 0f;
            KickBall(dir.normalized, 25f);
            targetPosition = player.transform.position;
        }

        private void Pass(PlayerEntity target)
        {
            Vector3 dir = target.transform.position - ballTransform.position;
            dir.y = 0f;
            float dist = dir.magnitude;
            KickBall(dir.normalized, Mathf.Clamp(dist * 1.5f, 8f, 22f));
            targetPosition = player.transform.position;
        }

        private void Dribble(Vector3 goal)
        {
            Vector3 dir = goal - player.transform.position;
            dir.y = 0f;
            dir.Normalize();

            targetPosition = player.transform.position + dir * dribbleStep;
            KickBall(dir, 3f);
        }

        private void HoldBall()
        {
            targetPosition = player.transform.position;
        }

        private void KickBall(Vector3 direction, float power)
        {
            if (ballTransform == null)
                return;

            Rigidbody rb = ballTransform.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.AddForce(direction * power, ForceMode.VelocityChange);
            }
        }

        private bool TeamHasBall()
        {
            foreach (PlayerEntity p in teammates)
                if (p != null && p != player && p.HasBall)
                    return true;
            return false;
        }

        private bool OpponentHasBall()
        {
            foreach (PlayerEntity p in opponents)
                if (p != null && p.HasBall)
                    return true;
            return false;
        }

        private bool IsUnderPressure()
        {
            Vector3 pos = player.transform.position;
            foreach (PlayerEntity opp in opponents)
                if (opp != null && Vector3.Distance(opp.transform.position, pos) <= pressureRadius)
                    return true;
            return false;
        }

        private bool HasSpaceAhead(Vector3 goal)
        {
            Vector3 dir = goal - player.transform.position;
            dir.y = 0f;
            Vector3 checkPoint = player.transform.position + dir.normalized * 5f;
            return TacticalEvaluator.OpennessScore(checkPoint, opponents) > 0.5f;
        }

        private PlayerEntity FindBestPassOption()
        {
            PlayerEntity best = null;
            float bestScore = float.MinValue;
            Vector3 pos = player.transform.position;
            float attackSign = player.Team == TeamSide.Home ? 1f : -1f;

            foreach (PlayerEntity mate in teammates)
            {
                if (mate == null || mate == player)
                    continue;

                Vector3 matePos = mate.transform.position;
                float dist = Vector3.Distance(pos, matePos);
                if (dist < 2f || dist > 40f)
                    continue;

                if (TacticalEvaluator.IsPassLaneBlocked(pos, matePos, opponents))
                    continue;

                float openness = TacticalEvaluator.OpennessScore(matePos, opponents);
                float forwardBonus = (matePos.x - pos.x) * attackSign * 0.02f;
                float score = openness + forwardBonus - dist * 0.005f;

                if (score > bestScore)
                {
                    bestScore = score;
                    best = mate;
                }
            }

            return best;
        }

        private Vector3 GetOpponentGoalPosition()
        {
            float sign = player.Team == TeamSide.Home ? 1f : -1f;
            return new Vector3(52.5f * sign, 0f, 0f);
        }

        private void MoveTowardTarget()
        {
            Transform t = player.transform;
            Vector3 target = new Vector3(targetPosition.x, t.position.y, targetPosition.z);
            t.position = Vector3.MoveTowards(t.position, target, aiSpeed * Time.deltaTime);

            Vector3 look = target - t.position;
            look.y = 0f;
            if (look.sqrMagnitude > 0.001f)
                t.rotation = Quaternion.Slerp(t.rotation, Quaternion.LookRotation(look), 10f * Time.deltaTime);
        }
    }
}
