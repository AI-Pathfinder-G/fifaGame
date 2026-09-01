using UnityEngine;
using SoccerGame.Core;
using SoccerGame.Player;
using SoccerGame.Ball;
using SoccerGame.Data;

namespace SoccerGame.Match
{
    /// <summary>
    /// Handles player swaps and enforces the substitution limit.
    /// </summary>
    public class SubstitutionManager : MonoBehaviour
    {
        [SerializeField] private int maxSubs = 5;

        public int SubsUsed { get; private set; }
        public int MaxSubs
        {
            get => maxSubs;
            set => maxSubs = Mathf.Max(0, value);
        }
        public bool CanSubstitute => SubsUsed < maxSubs;

        public bool Substitute(PlayerEntity outPlayer, PlayerEntity inPlayer)
        {
            if (!CanSubstitute || outPlayer == null || inPlayer == null)
                return false;

            Vector3 position = outPlayer.transform.position;
            Quaternion rotation = outPlayer.transform.rotation;
            Transform parent = outPlayer.transform.parent;

            outPlayer.gameObject.SetActive(false);

            inPlayer.transform.SetParent(parent);
            inPlayer.transform.SetPositionAndRotation(position, rotation);
            inPlayer.gameObject.SetActive(true);

            SubsUsed++;
            Debug.Log($"[SubstitutionManager] {inPlayer.Data?.PlayerName} replaced {outPlayer.Data?.PlayerName}");
            return true;
        }

        public void ResetSubstitutions()
        {
            SubsUsed = 0;
        }
    }
}
