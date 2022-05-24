using UnityEngine;

namespace GameJam
{
    [RequireComponent(typeof(Corpse))]
    public class Enemy : Mob
    {
        // combat ==============================================
        public override bool CanAttack(Entity entity)
        {
            return base.CanAttack(entity) &&
                   (entity is Player ||
                    (entity is Minion minion && minion.Owner != this));
        }
    }
}