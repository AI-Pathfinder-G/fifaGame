using System.Collections.Generic;
using UnityEngine;
using SoccerGame.Core;
using SoccerGame.Player;
using SoccerGame.Data;

namespace SoccerGame.AI
{
    public static class TacticalEvaluator
    {
        private const float HalfLength = 52.5f;
        private const float TransitionZone = 12f;
        private const float MaxShotRange = 35f;
        private const float PassLaneWidth = 1.5f;
        private const float OpennessRadius = 8f;

        public static GamePhase EvaluatePhase(Transform ball, TeamSide team)
        {
            if (ball == null)
                return GamePhase.TransitionToAttack;

            // Home attacks +X, Away attacks -X.
            float attackSign = team == TeamSide.Home ? 1f : -1f;
            float progress = ball.position.x * attackSign;

            if (progress > TransitionZone)
                return GamePhase.Attack;
            if (progress < -TransitionZone)
                return GamePhase.Defense;
            return GamePhase.TransitionToAttack;
        }

        public static float EvaluateShotChance(Vector3 pos, Vector3 goal)
        {
            float distance = Vector3.Distance(pos, goal);
            if (distance >= MaxShotRange)
                return 0f;

            float distanceScore = 1f - distance / MaxShotRange;

            // Central positions are more dangerous than wide ones.
            float angleScore = 1f - Mathf.Clamp01(Mathf.Abs(pos.z - goal.z) / 34f);

            float chance = distanceScore * (0.7f + 0.3f * angleScore);
            return Mathf.Clamp01(chance);
        }

        public static bool IsPassLaneBlocked(Vector3 from, Vector3 to, List<PlayerEntity> opponents)
        {
            if (opponents == null || opponents.Count == 0)
                return false;

            Vector3 lane = to - from;
            float laneLength = lane.magnitude;
            if (laneLength < 0.01f)
                return false;

            Vector3 laneDir = lane / laneLength;

            foreach (PlayerEntity opp in opponents)
            {
                if (opp == null)
                    continue;

                Vector3 toOpp = opp.transform.position - from;
                float projection = Vector3.Dot(toOpp, laneDir);

                // Ignore opponents behind the passer or beyond the receiver.
                if (projection < 0f || projection > laneLength)
                    continue;

                float distanceToLane = (toOpp - laneDir * projection).magnitude;
                if (distanceToLane <= PassLaneWidth)
                    return true;
            }

            return false;
        }

        public static PlayerEntity FindNearestToBall(List<PlayerEntity> team, Vector3 ballPos)
        {
            if (team == null || team.Count == 0)
                return null;

            PlayerEntity nearest = null;
            float bestDist = float.MaxValue;

            foreach (PlayerEntity p in team)
            {
                if (p == null)
                    continue;

                float dist = (p.transform.position - ballPos).sqrMagnitude;
                if (dist < bestDist)
                {
                    bestDist = dist;
                    nearest = p;
                }
            }

            return nearest;
        }

        public static float OpennessScore(Vector3 pos, List<PlayerEntity> opponents)
        {
            if (opponents == null || opponents.Count == 0)
                return 1f;

            float nearest = float.MaxValue;
            foreach (PlayerEntity opp in opponents)
            {
                if (opp == null)
                    continue;

                float dist = Vector3.Distance(pos, opp.transform.position);
                if (dist < nearest)
                    nearest = dist;
            }

            if (nearest == float.MaxValue)
                return 1f;

            return Mathf.Clamp01(nearest / OpennessRadius);
        }

        public static bool IsOffside(Vector3 playerPos, Vector3 ballPos, List<PlayerEntity> defenders, TeamSide attacking)
        {
            float attackSign = attacking == TeamSide.Home ? 1f : -1f;
            float playerProgress = playerPos.x * attackSign;
            float ballProgress = ballPos.x * attackSign;

            // Cannot be offside in your own half.
            if (playerProgress <= 0f)
                return false;

            // Cannot be offside if level with or behind the ball.
            if (playerProgress <= ballProgress)
                return false;

            // Find the second-last defender (deepest is usually the goalkeeper).
            float deepest = float.MinValue;
            float secondDeepest = float.MinValue;

            if (defenders != null)
            {
                foreach (PlayerEntity d in defenders)
                {
                    if (d == null)
                        continue;

                    float depth = d.transform.position.x * attackSign;
                    if (depth > deepest)
                    {
                        secondDeepest = deepest;
                        deepest = depth;
                    }
                    else if (depth > secondDeepest)
                    {
                        secondDeepest = depth;
                    }
                }
            }

            if (secondDeepest == float.MinValue)
                secondDeepest = deepest == float.MinValue ? -HalfLength : deepest;

            // The offside line is the ball or the second-last defender, whichever is further forward.
            float offsideLine = Mathf.Max(ballProgress, secondDeepest);
            return playerProgress > offsideLine;
        }
    }
}
