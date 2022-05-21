using UnityEngine;

namespace GameJam
{
    [CreateAssetMenu(fileName = "Faction", menuName = "Game/New Faction", order = 1)]
    public class Faction : ScriptableObject
    {
        public string Name;
        public Color Color = Color.white;
        public Faction[] EnemyFactions;
    }
}