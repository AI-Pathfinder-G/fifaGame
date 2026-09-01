using UnityEngine;
using SoccerGame.Core;
using SoccerGame.Data;

namespace SoccerGame.Player
{
    public class PlayerEntity : MonoBehaviour
    {
        public PlayerData Data;
        public TeamSide Team;
        public PositionRole Role;
        public int FieldNumber;

        public Rigidbody Rb { get; private set; }
        public Animator Anim { get; private set; }
        public bool IsUserControlled { get; set; }
        public bool HasBall { get; set; }

        private float stamina = 1f;
        public float Stamina
        {
            get => stamina;
            set => stamina = Mathf.Clamp01(value);
        }

        public bool IsSprinting { get; set; }
        public Vector3 FormationPosition { get; set; }
        public Vector3 TargetPosition { get; set; }

        private void Awake()
        {
            Rb = GetComponent<Rigidbody>();
            Anim = GetComponent<Animator>();
        }
    }
}
