using System;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameJam
{
    public enum DamageType { Normal, Crit, Block, Invincible }
    public enum HealType { Normal, Crit, Uncurable }

    // inventory, attributes etc. can influence values
    public interface ICombatBonus
    {
        int GetDamageBonus();
        int GetDefenseBonus();
        float GetCriticalChanceBonus();
        float GetBlockChanceBonus();
    }

    [DisallowMultipleComponent]
    public class Combat : MonoBehaviour
    {
        public static float popupCritFontSizeIncrease = 0.5f;

        [Header("Components")]
        [SerializeField] protected Entity entity;

        [Header("Stats")]
        public LinearInt baseDamage = new LinearInt { baseValue = 1 };
        public LinearInt baseDefense = new LinearInt { baseValue = 1 };
        public LinearFloat baseBlockChance;
        public LinearFloat baseCriticalChance;
        [Tooltip("Remove a percentage of health every attack (useful for monsters so they don't become weak and deal no damage when facing higher level players)")]
        [Range(0f, 1f)] public float bonusDamageHealthPercentage = 0f;

        [Header("States")]
        public bool invincible = false; // GMs, Npcs, ...
        public bool canBeStunned = true;

        [Header("Popups")]
        [SerializeField] protected GameObject damagePopupPrefab;
        [SerializeField] protected GameObject healPopupPrefab;

        [Header("Effects")]
        [SerializeField] protected GameObject criticalDamageEffectPrefab;

        // wrappers for easier access
        protected Level Level => entity.Level;

        // events
        public delegate void DealtDamageCallback(int damage, Entity target);
        public DealtDamageCallback OnDealtDamage;
        public delegate void DoneHealingCallback(int healing, Entity target);
        public DoneHealingCallback OnDoneHealing;

        public delegate void ReceivedDamageCallback(int damage, DamageType damageType, Entity damager);
        public ReceivedDamageCallback OnReceivedDamage;
        public delegate void ReceivedHealingCallback(int healing, HealType healType, Entity healer);
        public ReceivedHealingCallback OnReceivedHealing;

        public event Action<Entity> OnKilledEntity;

        // cache components that give a bonus (attributes, inventory, etc.)
        private ICombatBonus[] _bonusComponents;
        private ICombatBonus[] bonusComponents =>
            _bonusComponents ??= GetComponents<ICombatBonus>();

        // calculate damage
        public int damage
        {
            get
            {
                int bonus = 0;
                foreach (ICombatBonus bonusComponent in bonusComponents)
                {
                    bonus += bonusComponent.GetDamageBonus();
                }

                return baseDamage.Get(Level.Current) + bonus;
            }
        }

        // calculate defense
        public int defense
        {
            get
            {
                int bonus = 0;
                foreach (ICombatBonus bonusComponent in bonusComponents)
                {
                    bonus += bonusComponent.GetDefenseBonus();
                }

                return baseDefense.Get(Level.Current) + bonus;
            }
        }

        // calculate block
        public float blockChance
        {
            get
            {
                float bonus = 0;
                foreach (ICombatBonus bonusComponent in bonusComponents)
                {
                    bonus += bonusComponent.GetBlockChanceBonus();
                }

                return baseBlockChance.Get(Level.Current) + bonus;
            }
        }

        // calculate critical
        public float criticalChance
        {
            get
            {
                float bonus = 0;
                foreach (ICombatBonus bonusComponent in bonusComponents)
                {
                    bonus += bonusComponent.GetCriticalChanceBonus();
                }

                return baseCriticalChance.Get(Level.Current) + bonus;
            }
        }

        private NumberPopupManager numberPopupManager;

        //private void OnEnable()
        //{
        //    Entity.EntityHealthChanged += HandleEntityHealthChanged;
        //}
        //private void OnDisable()
        //{
        //    Entity.EntityHealthChanged -= HandleEntityHealthChanged;
        //}
        //
        //private void HandleEntityHealthChanged(Entity entity, int oldValue, int newValue)
        //{
        //    ShowDamagePopup(entity.transform.position);
        //}

        private void Start()
        {
            numberPopupManager = FindObjectOfType<NumberPopupManager>();
        }

        // combat ======================================================================================================
        // deal damage at another entity
        // (can be overwritten for players etc. that need custom functionality)
        public virtual void DealDamage(Entity target, int amount, Vector3 hitPoint, Vector3 hitNormal, float stunChance = 0, float stunTime = 0)
        {
            if (!target.IsAlive) { return; }

            Combat targetCombat = target.Combat;
            int damageDealt = 0;
            DamageType damageType = DamageType.Normal;
            bool isCrit = Random.value < criticalChance;

            // don't deal any damage if entity is invincible
            if (!targetCombat.invincible)
            {
                // block? (we use < not <= so that block rate 0 never blocks)
                if (Random.value < targetCombat.blockChance)
                {
                    damageType = DamageType.Block;
                }
                // deal damage
                else
                {
                    // subtract defense (but leave at least 1 damage, otherwise
                    // it may be frustrating for weaker players
                    damageDealt = Mathf.Max(amount - targetCombat.defense, 1);

                    // deal additional damage equal to a target's health percentage?
                    if (bonusDamageHealthPercentage > 0)
                    {
                        damageDealt *= Mathf.CeilToInt(1 + (bonusDamageHealthPercentage * target.Health.Max));
                    }

                    // critical hit?
                    if (isCrit)
                    {
                        damageDealt = Mathf.CeilToInt(damageDealt * 1.5f);
                        damageType = DamageType.Crit;
                    }

                    // deal the damage
                    target.Health.Current -= damageDealt;

                    // call OnReceivedDamage event on the target
                    // -> can be used for monsters to pull aggro
                    // -> can be used by equipment to decrease durability etc.
                    targetCombat.OnReceivedDamage?.Invoke(damageDealt, damageType, entity);

                    // stun?
                    if (targetCombat.canBeStunned &&
                        Random.value < stunChance)
                    {
                        //TODO Stun
                        //Stun(target, stunTime);
                    }

                    // call OnDealtDamage / OnKilledEntity events
                    OnDealtDamage?.Invoke(damage, target);

                    if (!target.IsAlive)
                    {
                        OnKilledEntity?.Invoke(target);
                    }
                }
            }
            else
            {
                damageType = DamageType.Invincible;
            }

            // let's make sure to pull aggro in any case so that archers
            // are still attacked if they are outside of the aggro range
            target.OnAggroBy(entity);

            // show damage effects
            targetCombat.PlayDamageReceivedEffects(damageDealt, damageType, hitPoint, hitNormal);

            // reset last combat time for both
            entity.lastCombatTime = Time.time;
            target.lastCombatTime = Time.time;
        }

        public virtual void Heal(Entity target, int amount)
        {
            if (!target.IsAlive) { return; }

            int healingDone = 0;
            HealType healType = HealType.Normal;
            Combat targetCombat = target.Combat;
            bool isCrit = UnityEngine.Random.value < criticalChance;

            // don't heal target if dead
            if (target.IsAlive)
            {
                // (leave at least 1 heal, otherwise it may be frustrating for weaker players)
                healingDone = Mathf.Max(amount, 1);

                // critical hit?
                if (isCrit)
                {
                    healingDone *= 2;
                    healType = HealType.Crit;
                }

                // do the healing
                target.Health.Current += healingDone;

                // call OnReceivedHealing event on the target
                // -> can be used for monsters to pull aggro
                // -> can be used by equipment to decrease durability etc.
                targetCombat.OnReceivedHealing?.Invoke(healingDone, healType, entity);

                OnDoneHealing?.Invoke(healingDone, target);
            }

            // show effects on clients
            targetCombat.PlayHealReceivedEffects(healingDone, healType);

            // reset last combat time for both
            entity.lastCombatTime = Time.time;
            target.lastCombatTime = Time.time;
        }
        
        /* TODO Add Stun
        private void Stun(Entity victim, float stunTime)
        {
            // dont allow a short stun to overwrite a long stun
            // => if a player is hit with a 10s stun, immediately
            //    followed by a 1s stun, we don't want it to end in 1s!
            double newStunEndTime = Time.time + stunTime;
            victim.stunTimeEnd = Math.Max(newStunEndTime, victim.stunTimeEnd);

            // if victim was casting a skill, put that skill on cd 
            // for a short amount of time
            if (victim.State == "CASTING" &&
                victim.Skills.currentSkill != -1 &&
                !((CommonBrain)victim.brain).EventSkillFinished(victim))
            {
                Skill currentSkill = victim.Skills.skills[victim.Skills.currentSkill];
                currentSkill.SetOnCooldown(2f);
            }
        }
        */

        // effects =====================================================================================================
        public void PlayDamageReceivedEffects(int amount, DamageType damageType, Vector3 hitPoint, Vector3 hitNormal)
        {
            ShowDamagePopup(amount, damageType);

            if (damageType == DamageType.Crit)
            {
                ShowCriticalDamageEffect(hitPoint, hitNormal);
            }
        }

        public void PlayHealReceivedEffects(int amount, HealType healType)
        {
            ShowHealPopup(amount, healType);
        }

        public void ShowDamagePopup(int amount, DamageType damageType)
        {
            //if (damagePopupPrefab == null) { return; }

            if (amount <= 0) { return; }

            // showing it above their head looks best, and we don't have to use
            // a custom shader to draw world space UI in front of the entity
            Bounds bounds = entity.Collider.bounds;
            float randomOffsetX = Random.Range(0f, 0.5f);
            float randomOffsetY = Random.Range(0f, 0.3f);
            Vector3 position = new Vector3(
                bounds.center.x + randomOffsetX,
                bounds.max.y + randomOffsetY,
                bounds.center.z);

            NumberPopup popup = numberPopupManager.SpawnDamagePopup(position);
            string damagedAmountText = $"-{amount}";

            if (damageType == DamageType.Normal)
            {
                popup.numberText.text = damagedAmountText;
            }
            else if (damageType == DamageType.Crit)
            {
                TextMeshPro popupText = popup.numberText;
                popupText.fontSize = popupText.fontSize + popupCritFontSizeIncrease;
                popupText.text = damagedAmountText + " Crit!";
            }
            else if (damageType == DamageType.Block)
            {
                popup.numberText.text = "<i>Block!</i>";
            }
            else if (damageType == DamageType.Invincible)
            {
                TextMeshPro popupText = popup.numberText;
                popupText.text = "<i>Invincible</i>";
                popupText.color = new Color(0.5f, 0.5f, 0.5f, 0.2f); // grey transparent text
            }
        }

        public void ShowHealPopup(int amount, HealType healType)
        {
            //if (healPopupPrefab == null) { return; }
            if (amount <= 0) { return; }

            // showing it above their head looks best, and we don't have to use
            // a custom shader to draw world space UI in front of the entity
            Bounds bounds = entity.Collider.bounds;
            float randomX = Random.Range(0f, 0.5f);
            float randomY = Random.Range(0f, 0.5f);
            Vector3 position = new Vector3(
                bounds.center.x + randomX,
                bounds.max.y + randomY,
                bounds.center.z);

            NumberPopup popup = numberPopupManager.SpawnHealPopup(position);
            string healedAmountText = $"+{amount}";

            if (healType == HealType.Normal)
            {
                popup.numberText.text = healedAmountText;
            }
            else if (healType == HealType.Crit)
            {
                TextMeshPro popupText = popup.numberText;
                popupText.fontSize = popupText.fontSize + popupCritFontSizeIncrease;
                popupText.text = healedAmountText + " Crit!";
            }
        }

        public void ShowCriticalDamageEffect(Vector3 hitPoint, Vector3 hitNormal)
        {
            // spawn the damage popup (if any) and set the text
            if (criticalDamageEffectPrefab == null) { return; }

            // show the effect at the hit point position
            Instantiate(criticalDamageEffectPrefab, hitPoint, Quaternion.LookRotation(-hitNormal));
        }
    }
}