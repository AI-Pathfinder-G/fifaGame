using UnityEngine;
using SoccerGame.Core;
using SoccerGame.Data;

namespace SoccerGame.Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerEntity player;
        [SerializeField] private Transform cameraTransform;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float sprintSpeed = 9f;

        [Header("Stamina")]
        [SerializeField] private float staminaDrainRate = 0.15f;
        [SerializeField] private float staminaRegenRate = 0.1f;

        private void Update()
        {
            if (player == null || player.Rb == null)
                return;

            HandleMovement();
            HandleActions();
        }

        private void HandleMovement()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 forward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
            Vector3 right = cameraTransform != null ? cameraTransform.right : Vector3.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 moveDirection = forward * vertical + right * horizontal;
            if (moveDirection.sqrMagnitude > 1f)
                moveDirection.Normalize();

            bool isMoving = moveDirection.sqrMagnitude > 0.001f;
            player.IsSprinting = Input.GetKey(KeyCode.LeftShift) && isMoving && player.Stamina > 0f;

            float currentSpeed = player.IsSprinting ? sprintSpeed : moveSpeed;
            Vector3 velocity = moveDirection * currentSpeed;
            velocity.y = player.Rb.velocity.y;
            player.Rb.velocity = velocity;

            if (player.IsSprinting)
                player.Stamina -= staminaDrainRate * Time.deltaTime;
            else
                player.Stamina += staminaRegenRate * Time.deltaTime;
        }

        private void HandleActions()
        {
            if (Input.GetKeyDown(KeyCode.A)) FireAction("Pass");
            if (Input.GetKeyDown(KeyCode.S)) FireAction("Shoot");
            if (Input.GetKeyDown(KeyCode.Q)) FireAction("Through");
            if (Input.GetKeyDown(KeyCode.E)) FireAction("Cross");
            if (Input.GetKeyDown(KeyCode.D)) FireAction("Tackle");

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                Debug.Log("[PlayerController] Player switch requested.");
                GameEvents.RaisePlayerSwitched(player.FieldNumber);
            }
        }

        private void FireAction(string actionName)
        {
            Debug.Log($"[PlayerController] {player.name} performed action: {actionName}");
            GameEvents.RaisePlayerAction(actionName);
        }

        public void SetPlayer(PlayerEntity newPlayer)
        {
            if (player != null)
                player.IsUserControlled = false;

            player = newPlayer;

            if (player != null)
                player.IsUserControlled = true;
        }
    }
}
