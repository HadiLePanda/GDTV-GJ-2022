using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace GameJam
{
    public enum EntityState : byte { IDLE, MOVING, CASTING, STUNNED, DEAD }

    [SelectionBase]
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(Level))]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(Mana))]
    [DisallowMultipleComponent]
    public abstract class Entity : MonoBehaviour, IDamageable
    {
        [Header("Entity Components")]
        public Animator Animator;
        public Collider Collider;
        public AudioSource VoiceAudio;
        public AudioSource SkillAudio;
        public Level Level;
        public Health Health;
        public Mana Mana;
        public Combat Combat;
        public Skills Skills;
        public MovementBase Movement;

        [Header("Entity References")]
        public Transform entityRoot;
        public Transform modelRoot;

        [Header("Brain")]
        public ScriptableBrain brain;
        [ReadOnlyInspector] [SerializeField] protected Entity target = null;
        [ReadOnlyInspector][SerializeField] protected string state = EntityState.IDLE.ToString();
        [ReadOnlyInspector] public double stunTimeEnd;
        [ReadOnlyInspector] public double lastCombatTime;

        [Header("Skills")]
        [ReadOnlyInspector] public int pendingSkill;

        [Header("Faction")]
        public Faction Faction;

        [Header("Death")]
        [SerializeField] private float corpseDecayTime = 5f;

        [Header("Entity Sounds")]
        [SerializeField] protected AudioClip[] hurtSounds;
        [SerializeField] protected AudioClip[] deathSounds;
        [SerializeField] protected AudioClip[] decaySounds;

        [Header("Entity Effects")]
        [SerializeField] protected GameObject deathEffect;
        [SerializeField] protected GameObject decayEffect;

        public event Action<Entity> OnAggroByEntity;
        public event Action OnDied;

        private Coroutine decayRoutine;

        public AudioClip[] GetHurtSounds() => hurtSounds;
        public AudioClip[] GetDeathSounds() => deathSounds;
        public AudioClip[] GetDecaySounds() => decaySounds;
        public GameObject GetDeathEffect() => deathEffect;
        public GameObject GetDecayEffect() => decayEffect;

        public bool IsAlive => Health.Current > 0;
        public bool IsStunned => stunTimeEnd > 0;

        public Entity Target => target;
        public void SetTarget(Entity entity) => target = entity;

        public string State => state;

        protected virtual void OnEnable()
        {
            Health.OnEmpty += OnDeath;
        }
        protected virtual void OnDisable()
        {
            Health.OnEmpty -= OnDeath;
        }

        protected void Start()
        {
            state = EntityState.IDLE.ToString();

            if (!IsAlive)
            {
                state = EntityState.DEAD.ToString();
            }
        }

        protected virtual void Update()
        {
            if (IsWorthUpdating())
            {
                if (brain != null)
                {
                    state = brain.UpdateBrain(this);
                }

                if (target != null && (target.IsHidden() || target.IsWorthUpdating()))
                {
                    target = null;
                }
            }
        }

        // function to check which entities need to be updated.
        // monsters, npcs etc. don't have to be updated if no player is around
        // -> can be overwritten if necessary (e.g. pets might be too far but should still be updated to run to owner)
        // -> update only if:
        //    - inside player's visibility range
        //    - if the entity is hidden, otherwise it would never be updated again because it would never get new observers
        public virtual bool IsWorthUpdating()
        {
            // distance to player inside visibility range
            Player player = Player.localPlayer;
            if (player == null) { return false; }

            float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);
            return distanceToPlayer <= player.VisRange(); // || IsHidden()
        }

        // visibility //////////////////////////////////////////////////////////////
        // hide an entity
        public void Hide() => entityRoot.gameObject.SetActive(false);

        public void Show() => entityRoot.gameObject.SetActive(true);

        // is the entity currently hidden?
        public bool IsHidden() => entityRoot.gameObject.activeSelf;

        // combat =====================================
        // we need a function to check if an entity can attack another.
        // => overwrite to add more cases like 'monsters can only attack players'
        //    or 'player can attack minions but not own minions' etc.
        // => raycast NavMesh to prevent attacks through walls, while allowing
        //    attacks through steep hills etc. (unlike Physics.Raycast). this is
        //    very important to prevent exploits where someone might try to attack an
        //    enemy through a dungeon wall, etc.
        public virtual bool CanAttack(Entity entity)
        {
            return IsAlive &&
                   entity.IsAlive &&
                   entity != this &&
                   !IsFactionAlly(entity) &&
                   !NavMesh.Raycast(transform.position, entity.transform.position, out NavMeshHit hit, NavMesh.AllAreas);
        }

        public virtual bool CanResurrect(Entity entity)
        {
            return IsAlive &&
                   !entity.IsAlive &&
                   entity != this &&
                   IsFactionAlly(entity) &&
                   !NavMesh.Raycast(transform.position, entity.transform.position, out NavMeshHit hit, NavMesh.AllAreas);
        }

        public virtual void OnAggroBy(Entity entity)
        {
            // addon system hooks
            OnAggroByEntity?.Invoke(entity);
        }

        public bool IsFactionAlly(Entity entity)
        {
            foreach (Faction enemyFaction in Faction.EnemyFactions)
            {
                if (entity.Faction == enemyFaction)
                {
                    return false;
                }
            }

            return true;
        }

        // damageable =================================
        public void TakeDamage(int damage)
        {
            if (!IsAlive) { return; }

            Health.Remove(damage);
        }

        // death ======================================
        protected virtual void OnDeath()
        {
            // the body starts to decay on death for a window of possibility to resurrect
            StartDecay();

            //TODO reset movement and navigation
            //Movement.Reset();

            OnDied?.Invoke();
        }

        // decay ===============================================
        protected virtual void StartDecay()
        {
            if (decayRoutine != null)
                StopCoroutine(decayRoutine);

            decayRoutine = StartCoroutine(RemoveCorpseAfterDecayTime());
        }

        IEnumerator RemoveCorpseAfterDecayTime()
        {
            yield return new WaitForSeconds(corpseDecayTime);

            PlayDecayEffects();

            RemoveCorpse();
        }

        protected virtual void PlayDecayEffects()
        {
            if (GetDecayEffect() != null)
                Game.Vfx.SpawnParticle(GetDecayEffect(), transform.position);

            if (GetDecaySounds().Length > 0)
            {
                AudioClip randomClip = Utils.GetRandomClip(GetDecaySounds());
                VoiceAudio.PlayOneShot(randomClip);
            }
        }

        public virtual void RemoveCorpse()
        {
            Destroy(gameObject);
        }
    }
}