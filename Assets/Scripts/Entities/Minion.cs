using UnityEngine;

namespace GameJam
{
    public class Minion : Mob
    {
        private Entity owner;
        public Entity Owner => owner;

        public Vector3 OwnerDestinationOffset { get; private set; } = Vector3.zero;

        public Vector3 GetOwnerDestination() => owner.MinionDestination + OwnerDestinationOffset;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (owner != null)
            {
                owner.Combat.OnDealtDamage += HandleOwnerDealtDamage;
            }
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            if (owner != null)
            {
                owner.Combat.OnDealtDamage -= HandleOwnerDealtDamage;
            }
        }

        private void HandleOwnerDealtDamage(int damage, Entity target)
        {
            if (Target == null)
            {
                OnAggroBy(target);
            }
        }

        protected override void Start()
        {
            base.Start();

            // randomize follow position offset
            OwnerDestinationOffset = new Vector3(Random.Range(0f, 5f), 0, Random.Range(0f, 3f));
        }

        //public float GetLifetime() => lifetime;
        //public void AddLifetime(float time) => RemainingLifetime += time;

        //private void Awake()
        //{
        //    RemainingLifetime = GetLifetime();
        //}

        //protected override void Update()
        //{
        //    base.Update();
        //
        //    //ProcessLifetime();
        //}

        //private void ProcessLifetime()
        //{
        //    RemainingLifetime -= Time.deltaTime;
        //
        //    // lifetime ran out, minion dies
        //    if (RemainingLifetime <= 0)
        //    {
        //        Health.Deplete();
        //    }
        //}

        public void Setup(Entity owner, int level, Entity corpseEntity = null)
        {
            this.owner = owner;

            // change faction to owner's
            Faction = owner.Faction;
            
            // copy entity skills based on skill templates
            if (corpseEntity != null && corpseEntity.Skills.skills.Count > 0)
            {
                Skills.skills.Clear();
                foreach (ScriptableSkill skillData in corpseEntity.Skills.skillTemplates)
                {
                    Skills.skills.Add(new Skill(skillData));
                }
            }

            // setup runtime stats
            Level.SetLevel(level);
        }

        // combat =====================================
        public override bool CanAttack(Entity entity)
        {
            return base.CanAttack(entity) &&
                    entity != owner &&
                    (owner != null && owner.CanAttack(entity));
        }

        // minions can't resurrect other corpses
        public override bool CanResurrect(Entity entity) => false;

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

        // death ======================================
        protected override void OnDeath()
        {
            base.OnDeath();

            Movement.Reset();
            Mana.Deplete();
        }
    }
}