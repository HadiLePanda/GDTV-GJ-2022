using UnityEngine;

namespace GameJam
{
    [RequireComponent(typeof(Corpse))]
    public class Enemy : Mob
    {
        [Header("Enemy Settings")]
        [SerializeField] private int experienceReward = 1;

        [Header("Enemy Corpse")]
        [SerializeField] private Corpse corpse;

        public int ExperienceReward => experienceReward;

        protected override void Start()
        {
            base.Start();

            // remember start position for brain distance logic
            startPosition = transform.position;
        }

        // combat ==============================================
        public override bool CanAttack(Entity entity)
        {
            return base.CanAttack(entity) &&
                   (entity is Player ||
                    (entity is Minion minion && minion.Owner != this));
        }

        // aggro ///////////////////////////////////////////////////////////////////
        // this function is called by entities that attack us and by AggroArea
        public override void OnAggroBy(Entity entity)
        {
            // call base function
            base.OnAggroBy(entity);

            // are we alive, and is the entity alive and of correct type?
            if (CanAttack(entity))
            {
                // no target yet(==self), or closer than current target?
                // => has to be at least 20% closer to be worth it, otherwise we
                //    may end up nervously switching between two targets
                // => we do NOT use Utils.ClosestDistance, because then we often
                //    also end up nervously switching between two animated targets,
                //    since their collides moves with the animation.
                //    => we don't even need closestdistance here because they are in
                //       the aggro area anyway. transform.position is perfectly fine
                if (Target == null)
                {
                    SetTarget(entity);
                }
                else if (entity != Target) // no need to check dist for same target
                {
                    float oldDistance = Vector3.Distance(transform.position, Target.transform.position);
                    float newDistance = Vector3.Distance(transform.position, entity.transform.position);
                    if (newDistance < oldDistance * 0.8)
                    {
                        SetTarget(entity);
                    }
                }
            }
        }
    }
}