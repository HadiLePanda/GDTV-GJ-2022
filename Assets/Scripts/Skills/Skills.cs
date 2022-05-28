// skill system for all entities (players, monsters, npcs, towers, ...)
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GameJam
{
    // serializable events
    [Serializable] public class UnityEventSkill : UnityEvent<Skill> { }

    [DisallowMultipleComponent]
    public abstract class Skills : MonoBehaviour, IHealthBonus, IManaBonus, ICombatBonus
    {
        [Header("References")]
        [SerializeField] protected Entity entity = null;
        public AudioSource SkillAudio;

        // 'skillTemplates' are the available skills (first one is default attack)
        // 'skills' are the loaded skills with cooldowns etc.
        [Header("Skills & Buffs")]
        public ScriptableSkill[] skillTemplates;
        public readonly List<Skill> skills = new List<Skill>();
        public readonly List<Buff> buffs = new List<Buff>(); // active buffs

        // effect mount is where the arrows/fireballs/etc. are spawned
        // -> can be overwritten, e.g. for mages to set it to the weapon's effect
        //    mount
        // -> assign to right hand if in doubt!
#pragma warning disable CS0649 // Field is never assigned to
        [Tooltip("effect mount is where the arrows/fireballs/etc. are spawned. -> can be overwritten, e.g. for mages to set it to the weapon's effect mount -> assign to right hand if in doubt!")]
        [SerializeField] private Transform _effectMount;
        [SerializeField] private Transform _auraMount;
#pragma warning restore CS0649 // Field is never assigned to
        public virtual Transform effectMount
        {
            get => _effectMount;
            private set => _effectMount = value;
        }
        public virtual Transform auraMount
        {
            get => _auraMount;
            private set => _auraMount = value;
        }

        public event Action<Skill> onSkillCastStarted;
        public event Action<Skill> onSkillCastFinished;
        public event Action<Skill> onSkillCastCanceled;

        // current skill (synced because we need it as an animation parameter)
        [ReadOnlyInspector] public int currentSkill = -1;

        protected virtual void OnEnable()
        {
            entity.Health.OnEmpty += OnDeath;
        }
        protected virtual void OnDisable()
        {
            entity.Health.OnEmpty -= OnDeath;
        }

        // boni ////////////////////////////////////////////////////////////////////
        public int GetHealthBonus(int baseHealth)
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            int passiveBonus = 0;
            foreach (Skill skill in skills)
            {
                if (skill.level > 0 && skill.data is PassiveSkill passiveSkill)
                {
                    passiveBonus += passiveSkill.healthMaxBonus.Get(skill.level);
                }
            }

            int buffBonus = 0;
            foreach (Buff buff in buffs)
            {
                buffBonus += buff.healthMaxBonus;
            }

            return passiveBonus + buffBonus;
        }

        public int GetHealthRecoveryBonus()
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            float passivePercent = 0;
            foreach (Skill skill in skills)
            {
                if (skill.level > 0 && skill.data is PassiveSkill passiveSkill)
                {
                    passivePercent += passiveSkill.healthPercentPerSecondBonus.Get(skill.level);
                }
            }

            float buffPercent = 0;
            foreach (Buff buff in buffs)
            {
                buffPercent += buff.healthPercentPerSecondBonus;
            }

            // value is lifted by ceiling, which means if we get less than 1pt/s in total, we automatically get 1pt/s as a minimum (feels more rewarding than getting nothing even with passives that give a small bonus)
            return Mathf.CeilToInt(Convert.ToInt32(passivePercent * entity.Health.Max) + Convert.ToInt32(buffPercent * entity.Health.Max));
        }

        public int GetManaBonus(int baseMana)
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            int passiveBonus = 0;
            foreach (Skill skill in skills)
            {
                if (skill.level > 0 && skill.data is PassiveSkill passiveSkill)
                {
                    passiveBonus += passiveSkill.manaMaxBonus.Get(skill.level);
                }
            }

            int buffBonus = 0;
            foreach (Buff buff in buffs)
            {
                buffBonus += buff.manaMaxBonus;
            }

            return passiveBonus + buffBonus;
        }

        public int GetManaRecoveryBonus()
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            float passivePercent = 0;
            foreach (Skill skill in skills)
            {
                if (skill.level > 0 && skill.data is PassiveSkill passiveSkill)
                {
                    passivePercent += passiveSkill.manaPercentPerSecondBonus.Get(skill.level);
                }
            }

            float buffPercent = 0;
            foreach (Buff buff in buffs)
            {
                buffPercent += buff.manaPercentPerSecondBonus;
            }

            // value is lifted by ceiling, which means if we get less than 1pt/s in total, we automatically get 1pt/s as a minimum (feels more rewarding than getting nothing even with passives that give a small bonus)
            return Mathf.CeilToInt(Convert.ToInt32(passivePercent * entity.Mana.Max) + Convert.ToInt32(buffPercent * entity.Mana.Max));
        }

        public int GetDamageBonus()
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            int passiveBonus = 0;
            foreach (Skill skill in skills)
            {
                if (skill.level > 0 && skill.data is PassiveSkill passiveSkill)
                {
                    passiveBonus += passiveSkill.damageBonus.Get(skill.level);
                }
            }

            int buffBonus = 0;
            foreach (Buff buff in buffs)
            {
                buffBonus += buff.damageBonus;
            }

            return passiveBonus + buffBonus;
        }

        public int GetDefenseBonus()
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            int passiveBonus = 0;
            foreach (Skill skill in skills)
            {
                if (skill.level > 0 && skill.data is PassiveSkill passiveSkill)
                {
                    passiveBonus += passiveSkill.defenseBonus.Get(skill.level);
                }
            }

            int buffBonus = 0;
            foreach (Buff buff in buffs)
            {
                buffBonus += buff.defenseBonus;
            }

            return passiveBonus + buffBonus;
        }

        public float GetCriticalChanceBonus()
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            float passiveBonus = 0;
            foreach (Skill skill in skills)
            {
                if (skill.level > 0 && skill.data is PassiveSkill passiveSkill)
                {
                    passiveBonus += passiveSkill.criticalChanceBonus.Get(skill.level);
                }
            }

            float buffBonus = 0;
            foreach (Buff buff in buffs)
            {
                buffBonus += buff.criticalChanceBonus;
            }

            return passiveBonus + buffBonus;
        }

        public float GetBlockChanceBonus()
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            float passiveBonus = 0;
            foreach (Skill skill in skills)
            {
                if (skill.level > 0 && skill.data is PassiveSkill passiveSkill)
                {
                    passiveBonus += passiveSkill.blockChanceBonus.Get(skill.level);
                }
            }

            float buffBonus = 0;
            foreach (Buff buff in buffs)
            {
                buffBonus += buff.blockChanceBonus;
            }

            return passiveBonus + buffBonus;
        }

        ////////////////////////////////////////////////////////////////////////////
        private void Update()
        {
            // only update if it's worth updating (see IsWorthUpdating comments)
            if (entity.IsWorthUpdating())
            {
                CleanupBuffs();
            }
        }

        // helper function to find a skill index
        public int GetSkillIndexByName(string skillName)
        {
            // (avoid FindIndex to minimize allocations)
            for (int i = 0; i < skills.Count; ++i)
            {
                if (skills[i].name == skillName)
                {
                    return i;
                }
            }

            return -1;
        }

        // helper function to find a buff index
        public int GetBuffIndexByName(string buffName)
        {
            // (avoid FindIndex to minimize allocations)
            for (int i = 0; i < buffs.Count; ++i)
            {
                if (buffs[i].name == buffName)
                {
                    return i;
                }
            }

            return -1;
        }

        // the first check validates the caster
        // (the skill won't be ready if we check self while casting it. so the
        //  checkSkillReady variable can be used to ignore that if needed)
        // has a weapon (important for projectiles etc.), no cooldown, hp, mp?
        public bool CastCheckSelf(Skill skill, bool checkSkillReady = true) =>
            skill.CheckSelf(entity, checkSkillReady);

        // the second check validates the target and corrects it for the skill if
        // necessary (e.g. when trying to heal an npc, it sets target to self first)
        // (skill shots that don't need a target will just return true if the user
        //  wants to cast them at a valid position)
        public bool CastCheckTarget(Skill skill) =>
            skill.CheckTarget(entity);

        // the third check validates the distance between the caster and the target
        // (target entity or target position in case of skill shots)
        // note: castchecktarget already corrected the target (if any), so we don't
        //       have to worry about that anymore here
        public bool CastCheckDistance(Skill skill, out Vector3 destination) =>
            skill.CheckDistance(entity, out destination);

        // the third check validates the distance between the caster and the target
        // (target entity or target position in case of skill shots)
        // note: castchecktarget already corrected the target (if any), so we don't
        //       have to worry about that anymore here
        public bool CastCheckFOV(Skill skill) =>
            skill.CheckFOV(entity);

        // starts casting
        public void StartCast(Skill skill)
        {
            // start casting and set the casting end time
            skill.castTimeEnd = Time.time + skill.castTime;

            // save modifications
            skills[currentSkill] = skill;

            // rpc for client sided effects
            RpcCastStarted(skill);
        }

        // cancel a skill cast properly
        public void CancelCast(bool resetCurrentSkill = true)
        {
            // reset cast time, otherwise if a buff has a 10s cast time and we
            // cancel the cast after 1s, then we would have to wait 9 more seconds
            // before we can attempt to cast it again.
            // -> we cancel it in any case. players will have to wait for 'casttime'
            //    when attempting another cast anyway.
            if (currentSkill != -1)
            {
                Skill skill = skills[currentSkill];
                skill.castTimeEnd = Time.time - skill.castTime;
                skills[currentSkill] = skill;

                // cancel any looping sound
                skill.data.OnCastCanceled(entity);

                // reset current skill
                if (resetCurrentSkill)
                {
                    currentSkill = -1;
                }
            }
        }

        // finishes casting. casting and waiting has to be done in the state machine
        public void FinishCast(Skill skill)
        {
            // * check if we can currently cast a skill (enough entity.Mana etc.)
            // * check if we can cast THAT skill on THAT target
            // note: we don't check the distance again. the skill will be cast even
            //   if the target walked a bit while we casted it (it's simply better
            //   gameplay and less frustrating)
            if (CastCheckSelf(skill, false) && CastCheckTarget(skill))
            {
                // let the skill template handle the action
                skill.Apply(entity);

                // rpc for client sided effects
                // -> pass that skill because skillIndex might be reset in the mean
                //    time, we never know
                RpcCastFinished(skill);

                // decrease entity.Mana in any case
                entity.Mana.Current -= skill.manaCosts;

                // start the cooldown (and save it in the struct)
                skill.SetOnCooldown(skill.cooldown);

                // save any skill modifications in any case
                skills[currentSkill] = skill;
            }
            else
            {
                // not all requirements met. no need to cast the same skill again
                currentSkill = -1;
            }
        }

        // check if we can cancel the skill we're currently casting
        public bool CanCancelCurrentCast()
        {
            return currentSkill != -1 &&
                   skills[currentSkill].canCancelCast;
        }

        // skill cast started rpc for client sided effects
        // note: pass Skill to avoid sync race conditions with indices etc.
        public void RpcCastStarted(Skill skill)
        {
            // validate: still alive?
            if (entity.IsAlive)
            {
                // call scriptableskill event
                skill.data.OnCastStarted(entity);

                // call event
                onSkillCastStarted?.Invoke(skill);
            }
        }

        // skill cast finished rpc for client sided effects
        // note: pass Skill to avoid sync race conditions with indices etc.
        public void RpcCastFinished(Skill skill)
        {
            // validate: still alive?
            if (entity.IsAlive)
            {
                // call scriptableskill event
                skill.data.OnCastFinished(entity);

                // call event
                onSkillCastFinished?.Invoke(skill);
            }
        }

        public void RpcCastCanceled(Skill skill)
        {
            // validate: still alive?
            if (entity.IsAlive)
            {
                // call scriptableskill event
                skill.data.OnCastCanceled(entity);

                // call event
                onSkillCastCanceled?.Invoke(skill);
            }
        }

        // helper function to add or refresh a buff
        public void AddOrRefreshBuff(Buff buff)
        {
            // reset if already in buffs list, otherwise add
            int index = GetBuffIndexByName(buff.name);
            if (index != -1) { buffs[index] = buff; }
            else { buffs.Add(buff); }
        }

        // helper function to remove all buffs that ended
        public void CleanupBuffs()
        {
            for (int i = 0; i < buffs.Count; ++i)
            {
                if (buffs[i].BuffTimeRemaining() == 0)
                {
                    buffs.RemoveAt(i);
                    --i;
                }
            }
        }

        // events /////////////////////////////////////////////////////////////
        public void OnDeath()
        {
            // clear buffs that shouldn't remain after death
            for (int i = 0; i < buffs.Count; ++i)
            {
                if (!buffs[i].remainAfterDeath)
                {
                    buffs.RemoveAt(i);
                    --i;
                }
            }

            // reset currently casted skill
            CancelCast();
        }

        private void OnValidate()
        {
            // auto-reference entity
            if (entity == null && TryGetComponent(out Entity entityComponent))
            {
                entity = entityComponent;
            }
        }
    }
}