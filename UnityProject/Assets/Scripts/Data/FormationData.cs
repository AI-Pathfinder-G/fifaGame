using UnityEngine;

namespace SoccerGame.Data
{
    [CreateAssetMenu(fileName = "Formation", menuName = "Soccer/Formation")]
    public class FormationData : ScriptableObject
    {
        public string Name;
        public FormationSlot[] Slots;

        public Vector3 GetWorldPosition(int index, float fieldLength, float fieldWidth, SoccerGame.Core.TeamSide side)
        {
            if (Slots == null || index < 0 || index >= Slots.Length)
                return Vector3.zero;

            Vector2 normalized = Slots[index].BasePosition;

            // BasePosition is normalized: x in [0,1] along field length (own goal -> opponent goal),
            // y in [0,1] along field width (left touchline -> right touchline).
            float x = (normalized.x - 0.5f) * fieldLength;
            float z = (normalized.y - 0.5f) * fieldWidth;

            // Mirror (rotate 180 degrees) so the Away team defends the opposite goal.
            if (side == SoccerGame.Core.TeamSide.Away)
            {
                x = -x;
                z = -z;
            }

            return new Vector3(x, 0f, z);
        }
    }
}
