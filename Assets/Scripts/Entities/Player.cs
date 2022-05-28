using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace GameJam
{
    [RequireComponent(typeof(PlayerSkills))]
    [RequireComponent(typeof(PlayerSkillbar))]
    public class Player : Entity
    {
        public new PlayerControllerMovement Movement => (PlayerControllerMovement)base.Movement;
        public new PlayerSkills Skills => (PlayerSkills)base.Skills;

        [Header("Player Components")]
        public Experience Experience;
        public PlayerLook Look;
        public PlayerSkillbar Skillbar;
        [SerializeField] private MinionStorage minionStorage;

        [Header("Player Game Settings")]
        [SerializeField] private float visRange = 300f;

        // when moving into attack range of a target, we always want to move a
        // little bit closer than necessary to tolerate for latency and other
        // situations where the target might have moved away a little bit already.
        [Header("Player Movement Settings")]
        [Range(0.1f, 1)] public float attackToMoveRangeRatio = 0.8f;

        [HideInInspector] public Entity nextTarget;
        //TODO get input from new input asset
        [HideInInspector] public KeyCode cancelActionKey;
        [HideInInspector] public bool cancelActionRequested;

        public void CmdCancelAction() { cancelActionRequested = true; }

        public static Player localPlayer;

        private void Awake()
        {
            localPlayer = this;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Combat.OnKilledEntity += OnKilledEnemy;
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            Combat.OnKilledEntity -= OnKilledEnemy;
        }

        public override bool CanResurrectCorpse(Corpse corpse)
        {
            return IsAlive &&
                   (corpse is MobAliveCorpse || corpse is GroundCorpse) &&
                   !NavMesh.Raycast(transform.position, corpse.transform.position, out NavMeshHit hit, NavMesh.AllAreas);
        }

        // events //////////////////////////////////////////////////////////////////
        private void OnKilledEnemy(Entity victim)
        {
            // killed a monster that gives exp
            if (victim is Enemy enemy && enemy.ExperienceReward > 0)
            {
                long xpReward = Experience.BalanceExperienceReward(enemy.ExperienceReward, Level.Current, enemy.Level.Current);

                Experience.Current += xpReward;

                Combat.SpawnExperiencePopup((int)xpReward);
            }
        }

        // visibility ==========================================
        // the range at which enemies can be updated
        public float VisRange() => visRange;

        // movement ////////////////////////////////////////////////////////////////
        // check if movement is currently allowed
        // -> not in Movement.cs because we would have to add it to each player
        //    movement system. (can't use an abstract PlayerMovement.cs because
        //    PlayerNavMeshMovement needs to inherit from NavMeshMovement already)
        public bool IsMovementAllowed()
        {
            // some skills allow movement while casting
            bool castingAndAllowed = State == "CASTING" &&
                                     Skills.currentSkill != -1 &&
                                     Skills.skills[Skills.currentSkill].allowMovement;

            // in a state where movement is allowed?
            // and if local player: not typing in an input?
            // (fix: only check for local player. checking in all cases means that
            //       no player could move if host types anything in an input)
            return (State == "IDLE" || State == "MOVING" || castingAndAllowed);
        }

        // death ===============================================
        protected override void OnDeath()
        {
            base.OnDeath();

            Movement.Reset();
            Mana.Deplete();

            KillAllSummonedMinions();
        }

        public override void RemoveEntity()
        {
            // hide player model
            modelRoot.gameObject.SetActive(false);
        }

        private void KillAllSummonedMinions()
        {
            // kill all released minions
            Minion[] releasedMinions = FindObjectsOfType<Minion>().Where(x => x.Owner == this).ToArray();

            foreach (Minion releasedMinion in releasedMinions)
            {
                releasedMinion.Health.Deplete();
            }

            // empty the minion storage just in case
            minionStorage.Empty();
        }

        private void OnValidate()
        {
            // auto-reference
            if (Experience == null && TryGetComponent(out Experience experienceComponent))
            {
                Experience = experienceComponent;
            }
        }
    }
}