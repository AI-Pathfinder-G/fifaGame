using UnityEngine;
using SoccerGame.Core;
using SoccerGame.Player;
using SoccerGame.Data;

namespace SoccerGame.AI
{
    public class FormationController : MonoBehaviour
    {
        [SerializeField] private FormationData formation;
        [SerializeField] private float fieldLength = 105f;
        [SerializeField] private float fieldWidth = 68f;
        [SerializeField, Range(0f, 1f)] private float ballInfluence = 0.25f;
        [SerializeField] private float phaseShift = 12f;

        private Transform ballTransform;

        public FormationData Formation => formation;
        public float FieldLength => fieldLength;
        public float FieldWidth => fieldWidth;

        public void SetFormation(FormationData newFormation)
        {
            formation = newFormation;
        }

        public void SetBallTransform(Transform ball)
        {
            ballTransform = ball;
        }

        public Vector3 GetTargetPosition(PlayerEntity player, int slotIndex, TeamSide side, GamePhase phase)
        {
            if (formation == null)
                return player != null ? player.transform.position : Vector3.zero;

            // Base slot position (use Home orientation; Away mirroring handled below).
            Vector3 target = formation.GetWorldPosition(slotIndex, fieldLength, fieldWidth, TeamSide.Home);

            // Phase bias shifts the whole block up or down the pitch.
            float bias = 0f;
            switch (phase)
            {
                case GamePhase.Attack:
                    bias = phaseShift;
                    break;
                case GamePhase.Defense:
                    bias = -phaseShift;
                    break;
                case GamePhase.TransitionToAttack:
                    bias = 0f;
                    break;
            }
            target.x += bias;

            // Ball influence drags the slot toward the ball without abandoning shape.
            if (ballTransform != null)
            {
                Vector3 ballPos = ballTransform.position;
                Vector3 influenced = new Vector3(ballPos.x, target.y, ballPos.z);
                target = Vector3.Lerp(target, influenced, ballInfluence);
            }

            // Keep the slot inside the pitch.
            float halfL = fieldLength * 0.5f;
            float halfW = fieldWidth * 0.5f;
            target.x = Mathf.Clamp(target.x, -halfL, halfL);
            target.z = Mathf.Clamp(target.z, -halfW, halfW);

            // Mirror for the away team.
            if (side == TeamSide.Away)
            {
                target.x = -target.x;
                target.z = -target.z;
            }

            return target;
        }
    }
}
