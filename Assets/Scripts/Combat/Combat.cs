using System;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameJam
{
    public enum DamageType { Normal, Crit, Block, Invincible }
    public enum HealType { Normal, Crit, Recovery, Uncurable }

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

        [Header("Effects")]
        [SerializeField] protected GameObject criticalDamageEffect;
        [SerializeField] protected GameObject drainedEffect;
        [SerializeField] protected AudioClip drainedSound;

        // wrappers for easier access
        protected Level Level => entity.Level;

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
        private float nextHurtSoundTime;

        // events
        public delegate void DealtDamageCallback(int damage, Entity target);
        public DealtDamageCallback OnDealtDamage;
        public delegate void DoneHealingCallback(int healing, Entity target);
        public DoneHealingCallback OnDoneHealing;
        public DoneHealingCallback OnDoneManaHealing;
        public delegate void DrainedEntityCallback(int healthDrain, int manaDrain, Entity target);
        public DrainedEntityCallback OnDrainedEntity;

        public delegate void ReceivedDamageCallback(int damage, DamageType damageType, Entity damager);
        public ReceivedDamageCallback OnReceivedDamage;
        public delegate void ReceivedHealingCallback(int healing, HealType healType, Entity healer);
        public ReceivedHealingCallback OnReceivedHealing;
        public ReceivedHealingCallback OnReceivedManaHealing;

        public event Action<Entity> OnKilledEntity;

        public GameObject GetDrainedEffect() => drainedEffect;
        public AudioClip GetDrainedSound() => drainedSound;

        private void Start()
        {
            numberPopupManager = FindObjectOfType<NumberPopupManager>();
        }

        // combat ======================================================================================================
        // deal damage at another entity
        // (can be overwritten for players etc. that need custom functionality)
        public virtual int DealDamage(Entity target, int amount, Vector3 hitPoint, Vector3 hitNormal, float stunChance = 0, float stunTime = 0)
        {
            if (!target.IsAlive) { return 0; }

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
                    damageDealt = Mathf.Max(1, amount - targetCombat.defense);

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

                    // stun?
                    if (targetCombat.canBeStunned &&
                        Random.value < stunChance)
                    {
                        Stun(target, stunTime);
                    }

                    // deal the damage
                    target.Health.Remove(damageDealt);
                    //TODO target.TakeDamage() using IDamageable

                    // call OnReceivedDamage event on the target
                    // -> can be used for monsters to pull aggro
                    // -> can be used by equipment to decrease durability etc.
                    targetCombat.OnReceivedDamage?.Invoke(damageDealt, damageType, entity);

                    // call OnDealtDamage / OnKilledEntity events
                    OnDealtDamage?.Invoke(damage, target);

                    if (!target.IsAlive)
                    {
                        OnKilledEntity?.Invoke(target);

                        targetCombat.PlayDeathEffects();
                    }
                    else
                    {
                        targetCombat.PlayHurtEffects(damageType, hitPoint, hitNormal);
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

            targetCombat.SpawnDamagePopup(damageDealt, damageType);

            // reset last combat time for both
            entity.lastCombatTime = Time.time;
            target.lastCombatTime = Time.time;

            return damageDealt;
        }

        public virtual int Heal(Entity target, int amount)
        {
            // can't heal a dead target
            if (!target.IsAlive) { return 0; }

            HealType healType = HealType.Normal;
            Combat targetCombat = target.Combat;
            bool isCrit = Random.value < criticalChance;

            // (leave at least 1 heal, otherwise it may be frustrating for weaker players)
            int healingDone = Mathf.Max(1, amount);

            // critical hit?
            if (isCrit)
            {
                healingDone = Mathf.CeilToInt(healingDone * 1.5f);
                healType = HealType.Crit;
            }

            // do the healing
            target.Health.Add(healingDone);

            // call OnReceivedHealing event on the target
            // -> can be used for monsters to pull aggro
            // -> can be used by equipment to decrease durability etc.
            targetCombat.OnReceivedHealing?.Invoke(healingDone, healType, entity);

            OnDoneHealing?.Invoke(healingDone, target);

            // show effects
            targetCombat.SpawnHealPopup(healingDone, healType);

            // reset last combat time for both
            entity.lastCombatTime = Time.time;
            target.lastCombatTime = Time.time;

            return healingDone;
        }

        public virtual int HealMana(Entity target, int amount)
        {
            // can't heal a dead target
            if (!target.IsAlive) { return 0; }

            HealType healType = HealType.Normal;
            Combat targetCombat = target.Combat;
            bool isCrit = Random.value < criticalChance;

            // (leave at least 1 heal, otherwise it may be frustrating for weaker players)
            int healingDone = Mathf.Max(1, amount);

            // critical hit?
            if (isCrit)
            {
                healingDone = Mathf.CeilToInt(healingDone * 1.5f);
                healType = HealType.Crit;
            }

            // do the healing
            target.Mana.Add(healingDone);

            // call OnReceivedHealing event on the target
            // -> can be used for monsters to pull aggro
            // -> can be used by equipment to decrease durability etc.
            targetCombat.OnReceivedManaHealing?.Invoke(healingDone, healType, entity);

            OnDoneManaHealing?.Invoke(healingDone, target);

            // show effects
            targetCombat.SpawnManaPopup(healingDone);

            // reset last combat time for both
            entity.lastCombatTime = Time.time;
            target.lastCombatTime = Time.time;

            return healingDone;
        }
        
        public virtual void Drain(Entity target, int manaAmount, int healthAmount)
        {
            if (manaAmount < 1 && healthAmount < 1) return;

            Combat targetCombat = target.Combat;

            // drain mana
            if (manaAmount > 0)
            {
                target.Mana.Remove(manaAmount);
                entity.Mana.Add(manaAmount);

                entity.Combat.SpawnManaPopup(manaAmount);
            }
            // drain health
            if (healthAmount > 0)
            {
                target.Health.Remove(healthAmount);
                target.Combat.SpawnDamagePopup(healthAmount, DamageType.Normal);

                entity.Health.Add(healthAmount);
                entity.Combat.SpawnHealPopup(healthAmount, HealType.Normal);
            }

            target.Combat.PlayDrainedEffects();

            OnDrainedEntity?.Invoke(healthAmount, manaAmount, target);

            // reset last combat time for both
            entity.lastCombatTime = Time.time;
            target.lastCombatTime = Time.time;
        }

        public virtual void ResurrectCorpse(Corpse corpse)
        {
            if (!corpse.CanBeResurrected()) { return; }

            Entity corpseEntityInstance = corpse.GetEntityInstance();

            corpse.ResurrectAsMinion(entity);

            // reset last combat time
            entity.lastCombatTime = Time.time;
        }

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

        // effects =====================================================================================================
        private void PlayHurtEffects(DamageType damageType, Vector3 hitPoint, Vector3 hitNormal)
        {
            // hurt sound
            if (entity.GetHurtSounds().Length > 0 &&
                Time.time > nextHurtSoundTime)
            {
                nextHurtSoundTime = Time.time;

                AudioClip randomClip = Utils.GetRandomClip(entity.GetHurtSounds());
                entity.VoiceAudio.PlayOneShot(randomClip);
            }

            // crit particle
            if (damageType == DamageType.Crit)
            {
                SpawnCriticalDamageEffect(hitPoint, hitNormal);
            }
        }

        private void PlayDeathEffects()
        {
            if (entity.GetDeathSounds().Length > 0)
            {
                AudioClip randomClip = Utils.GetRandomClip(entity.GetDeathSounds());
                Game.Audio.PlayWorldSfx(randomClip, transform.position);
            }
            if (entity.GetDeathEffect() != null)
            {
                Game.Vfx.SpawnParticle(entity.GetDeathEffect(), transform.position);
            }
        }

        public void PlayDrainedEffects()
        {
            if (GetDrainedSound() != null)
            {
                Game.Audio.PlayWorldSfx(GetDrainedSound(), transform.position);
            }
            if (GetDrainedEffect() != null)
            {
                Game.Vfx.SpawnParticle(GetDrainedEffect(), transform);
            }
        }

        //PlayHealEffects()

        public void SpawnCriticalDamageEffect(Vector3 hitPoint, Vector3 hitNormal)
        {
            if (criticalDamageEffect == null) { return; }

            // show the effect at the hit point position
            Game.Vfx.SpawnParticle(criticalDamageEffect, hitPoint, Quaternion.LookRotation(-hitNormal));
        }

        // popups ======================================================================
        public void SpawnDamagePopup(int amount, DamageType damageType)
        {
            if (amount <= 0) { return; }

            Vector3 position = GetDamagePopupSpawnPosition();
            NumberPopup popup = numberPopupManager.SpawnDamagePopup(position);
            string damagedAmountText = $"-{amount}";

            if (damageType == DamageType.Crit)
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
            else
            {
                popup.numberText.text = damagedAmountText;
            }
        }

        public void SpawnHealPopup(int amount, HealType healType)
        {
            if (amount <= 0) { return; }

            Vector3 position = GetHealPopupSpawnPosition();
            NumberPopup popup = numberPopupManager.SpawnHealPopup(position);
            string healedAmountText = $"+{amount}";

            if (healType == HealType.Crit)
            {
                TextMeshPro popupText = popup.numberText;
                popupText.fontSize = popupText.fontSize + popupCritFontSizeIncrease;
                popupText.text = healedAmountText + " Crit!";
            }
            else
            {
                popup.numberText.text = healedAmountText;
            }
        }

        public void SpawnManaPopup(int amount)
        {
            if (amount <= 0) { return; }

            Vector3 position = GetHealPopupSpawnPosition();
            NumberPopup popup = numberPopupManager.SpawnManaPopup(position);
            string manaAmountText = $"+{amount}";
            
            popup.numberText.text = manaAmountText;
        }

        public void SpawnExperiencePopup(int amount)
        {
            if (amount <= 0) { return; }

            Vector3 position = GetDamagePopupSpawnPosition();
            NumberPopup popup = numberPopupManager.SpawnExperiencePopup(position);
            string expAmountText = $"+{amount} EXP";

            popup.numberText.text = expAmountText;
        }

        private Vector3 GetDamagePopupSpawnPosition()
        {
            // showing it above their head looks best, and we don't have to use
            // a custom shader to draw world space UI in front of the entity
            Bounds bounds = entity.Collider.bounds;
            float randomOffsetX = Random.Range(0f, 0.5f);
            float randomOffsetY = Random.Range(0f, 0.3f);
            Vector3 position = new Vector3(
                bounds.center.x + randomOffsetX,
                bounds.max.y + randomOffsetY,
                bounds.center.z);

            return position;
        }
        private Vector3 GetHealPopupSpawnPosition()
        {
            // showing it above their head looks best, and we don't have to use
            // a custom shader to draw world space UI in front of the entity
            Bounds bounds = entity.Collider.bounds;
            float randomOffsetX = Random.Range(-1.0f, 0f);
            float randomOffsetY = Random.Range(0f, 1f);
            Vector3 position = new Vector3(
                bounds.center.x + randomOffsetX,
                bounds.max.y + randomOffsetY,
                bounds.center.z);

            return position;
        }
    }
}