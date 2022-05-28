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
        public Level Level;
        public Health Health;
        public Mana Mana;
        public Combat Combat;
        public Skills Skills;
        public MovementBase Movement;

        [Header("Entity References")]
        public Transform entityRoot;
        public Transform modelRoot;

        // pet's destination should always be right next to player, not inside him
        // -> we use a helper property so we have a base reference point
        public Vector3 MinionDestination
        {
            get
            {
                Bounds bounds = Collider.bounds;
                return transform.position - transform.forward * bounds.size.x;
            }
        }

        [Header("Brain")]
        public ScriptableBrain brain;
        [ReadOnlyInspector] [SerializeField] protected Entity target = null;
        [ReadOnlyInspector][SerializeField] protected string state = "IDLE";
        [ReadOnlyInspector] public double stunTimeEnd;
        [ReadOnlyInspector] public double lastCombatTime;
        [ReadOnlyInspector] public float lastAmbientSoundTime;

        [Header("Skills")]
        [ReadOnlyInspector] public int pendingSkill;

        [Header("Faction")]
        public Faction Faction;

        [Header("Death")]
        [SerializeField] private float deathDecayTime = 10f;

        [Header("Entity Effects")]
        [SerializeField] protected AudioClip[] hurtSounds;
        [SerializeField] protected AudioClip[] deathSounds;
        [SerializeField] protected AudioClip[] decaySounds;
        [SerializeField] protected GameObject deathEffect;
        [SerializeField] protected GameObject decayEffect;

        public event Action<Entity> OnAggroByEntity;
        public event Action OnDied;

        public delegate void RecoveredEnergyCallback(Entity entity, int amount);
        public static RecoveredEnergyCallback OnEntityRecoveredMana;
        public static RecoveredEnergyCallback OnEntityRecoveredHealth;

        private Coroutine decayRoutine;

        public AudioClip[] GetHurtSounds() => hurtSounds;
        public AudioClip[] GetDeathSounds() => deathSounds;
        public AudioClip[] GetDecaySounds() => decaySounds;
        public GameObject GetDeathEffect() => deathEffect;
        public GameObject GetDecayEffect() => decayEffect;

        public bool IsAlive => Health.Current > 0;
        public bool IsStunned => state == "STUNNED";
        public float GetDecayTime() => deathDecayTime;

        public Entity Target => target;
        public void SetTarget(Entity entity) => target = entity;

        public string State => state;

        protected virtual void OnEnable()
        {
            Health.OnEmpty += OnDeath;
            Health.OnRecovered += OnRecoveredHealth;
            Mana.OnRecovered += OnRecoveredMana;
        }
        protected virtual void OnDisable()
        {
            Health.OnEmpty -= OnDeath;
            Health.OnRecovered -= OnRecoveredHealth;
            Mana.OnRecovered -= OnRecoveredMana;
        }

        protected virtual void Start()
        {
            state = "IDLE";

            if (!IsAlive)
            {
                state = "DEAD";
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

                if (target != null && (target.IsHidden() || !target.IsWorthUpdating()))
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

        // visibility ============================================================
        // hide an entity
        public void Hide() => entityRoot.gameObject.SetActive(false);

        public void Show() => entityRoot.gameObject.SetActive(true);

        // is the entity currently hidden?
        public bool IsHidden() => !entityRoot.gameObject.activeSelf;

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

        public virtual bool CanHeal(Entity entity)
        {
            return IsAlive &&
                   entity.IsAlive &&
                   entity != this &&
                   IsFactionAlly(entity) &&
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
        public virtual bool CanResurrectCorpse(Corpse corpse)
        {
            return IsAlive &&
                   (corpse is MobAliveCorpse aliveCorpse && IsFactionAlly(aliveCorpse.GetEntityInstance())) &&
                   !NavMesh.Raycast(transform.position, corpse.transform.position, out NavMeshHit hit, NavMesh.AllAreas);
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

        // damageable ============================================
        public void TakeDamage(int damage)
        {
            if (!IsAlive) { return; }

            Health.Remove(damage);
        }

        // recovery ==============================================
        private void OnRecoveredMana(int amount)
        {
            Combat.SpawnManaPopup(amount);
        }

        private void OnRecoveredHealth(int amount)
        {
            Combat.SpawnHealPopup(amount, HealType.Recovery);
        }

        // death =================================================
        protected virtual void OnDeath()
        {
            // the body starts to decay on death for a window of possibility to resurrect
            StartDecay();

            OnDied?.Invoke();
        }

        // death decay ===============================================
        protected virtual void StartDecay()
        {
            if (decayRoutine != null)
                StopCoroutine(decayRoutine);

            decayRoutine = StartCoroutine(RemoveCorpseAfterDecayTime());
        }

        IEnumerator RemoveCorpseAfterDecayTime()
        {
            yield return new WaitForSeconds(deathDecayTime);

            PlayDecayEffects();

            RemoveEntity();
        }

        public virtual void RemoveEntity()
        {
            Destroy(gameObject);
        }

        // effects ==============================================
        protected virtual void PlayDecayEffects()
        {
            if (GetDecayEffect() != null)
                Game.Vfx.SpawnParticle(GetDecayEffect(), transform.position);

            if (GetDecaySounds().Length > 0)
            {
                AudioClip randomClip = Utils.GetRandomClip(GetDecaySounds());
                Game.Audio.PlayWorldSfx(randomClip, transform.position);
            }
        }

        // events ===============================================
        public virtual void FootL()
        {

        }

        public virtual void FootR()
        {

        }

        public virtual void Hit()
        {

        }
    }
}