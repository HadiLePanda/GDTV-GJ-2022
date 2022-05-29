using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GameJam
{
    public abstract class ScriptableSkill : ScriptableObject
    {
        [Header("Info")]
        public Sprite icon;

        [Header("Settings")]
        [Tooltip("Learnt by default like Normal attack etc.")]
        public bool learnDefault;
        public bool showCastBar;
        public bool cancelCastIfTargetDied; // direct hit may want to cancel if target died. buffs doesn't care. etc.
        public bool canCancelCast;
        [Tooltip("Can we move while casting this skill?")]
        public bool allowMovement = true;

        [Header("Requirements")]
        public LinearInt requiredLevel; // required player level

        [Header("Properties")]
        public int maxLevel = 1;
        public LinearInt manaCosts;
        public LinearFloat castTime;
        public LinearFloat cooldown;
        public LinearFloat castRange;

        [Header("Sound")]
        public AudioClip castStartSound;
        public AudioClip castEndSound;
        [Tooltip("Loop the casting sound while channeling. The loop stops when casting ends.")]
        public bool loopCastStartSound = false;

        // the skill casting process ///////////////////////////////////////////////

        // 1. self check: alive, enough mana, cooldown ready etc.?
        // (most skills can only be cast while alive. some maybe while dead or only
        //  if we have ammo, etc.)
        public virtual bool CheckSelf(Entity caster, int skillLevel)
        {
            // has a weapon (important for projectiles etc.), no cooldown, hp, mp?
            // note: only require equipment if required weapon category != ""

            // alive
            if (!caster.IsAlive)
            {
                return false;
            }
            // enough mana to cast
            if (caster.Mana.Current < manaCosts.Get(skillLevel))
            {
                return false;
            }

            return true;
        }

        // 2. target check: can we cast this skill 'here' or on this 'target'?
        // => e.g. sword hit checks if target can be attacked
        //         skill shot checks if the position under the mouse is valid etc.
        //         buff checks if it's a friendly player, etc.
        // ===> IMPORTANT: this function HAS TO correct the target if necessary,
        //      e.g. for a buff that is cast on 'self' even though we target a NPC
        //      while casting it
        public abstract bool CheckTarget(Entity caster);

        // 3. distance check: do we need to walk somewhere to cast it?
        //    e.g. on a monster that's far away
        //    => returns 'true' if distance is fine, 'false' if we need to move
        // (has corrected target already)
        public abstract bool CheckDistance(Entity caster, int skillLevel, out Vector3 destination);

        // 4. fov check: is the target within field of view?
        //    e.g. on a monster that's behind us
        //    => returns 'true' if within fov, 'false' if not
        // (has corrected target already)
        public abstract bool CheckFOV(Entity caster, int skillLevel);

        // 5. apply skill: deal damage, heal, launch projectiles, etc.
        // (has corrected target already)
        public abstract void Apply(Entity caster, int skillLevel);

        // events for effects /////////////////////////////////////////
        public virtual void OnCastStarted(Entity caster)
        {
            // cancel any looping cast sound just to make sure
            if (caster.Skills.SkillAudio != null && castStartSound != null)
            {
                CancelAnyCastLoopSound(caster);
                StartCastSound(caster, loopCastStartSound);
            }
        }

        public virtual void OnCastFinished(Entity caster)
        {
            // cancel any looping cast sound, play cast end sound
            if (caster.Skills.SkillAudio != null && castEndSound != null)
            {
                CancelAnyCastLoopSound(caster);
                PlayCastEndSound(caster);
            }
        }

        public virtual void OnCastCanceled(Entity caster)
        {
            // make sure to cancel any looping cast sound
            CancelAnyCastLoopSound(caster);
        }

        //TODO Do we not need this rpc to call effects like sounds and vfx etc.. logic?
        // OnCastCanceled doesn't seem worth the Rpc bandwidth, since skill effects
        // can check if caster.currentSkill == -1

        // audio helpers /////////////////////////////////////////////////////////////////
        private void StartCastSound(Entity caster, bool loopCastSound)
        {
            if (castStartSound == null) return;

            // loop casting sound if enabled
            // => gets stopped at the cast finish
            if (loopCastSound)
            {
                caster.Skills.SkillAudio.loop = true;
                caster.Skills.SkillAudio.clip = castStartSound;
                caster.Skills.SkillAudio.Play();
            }
            else
            {
                caster.Skills.SkillAudio.loop = false;
                caster.Skills.SkillAudio.PlayOneShot(castStartSound);
            }
        }
        private void CancelAnyCastLoopSound(Entity caster)
        {
            if (castStartSound == null && caster.Skills.SkillAudio.clip == null) return;

            // stop the casting loop sound if any
            if (loopCastStartSound)
            {
                caster.Skills.SkillAudio.clip = null;
                caster.Skills.SkillAudio.loop = false;
                //caster.Skills.SkillAudio.Stop();
            }
        }
        private void PlayCastEndSound(Entity caster)
        {
            if (castEndSound == null) return;

            caster.Skills.SkillAudio.PlayOneShot(castEndSound);
        }

        // tooltip /////////////////////////////////////////////////////////////////
        // (dynamic ones are filled in Skill.cs)
        // -> note: each tooltip can have any variables, or none if needed
        public virtual string ToolTip(int level, bool showRequirements = false)
        {
            // note: caching StringBuilder is worse for GC because .Clear frees the internal array and reallocates.
            string castTimeText = castTime.Get(level) > 0 ? Utils.PrettySeconds(castTime.Get(level)) : "Instant";
            string cooldownText = castTime.Get(level) > 0 ? Utils.PrettySeconds(cooldown.Get(level)) : "Instant";

            StringBuilder tip = new StringBuilder();
            tip.AppendLine($"{name} - Lv. {level}");
            tip.AppendLine($"Mana cost: {manaCosts.Get(level)}");
            tip.AppendLine($"Cast time: {castTimeText}");
            tip.AppendLine($"Cooldown: {cooldownText}");
            tip.AppendLine($"Range: {castRange.Get(level)}");

            return tip.ToString();
        }

        //TODO OnValidate to fill default tooltip

        // caching /////////////////////////////////////////////////////////////////
        static Dictionary<int, ScriptableSkill> cache;
        public static Dictionary<int, ScriptableSkill> All
        {
            get
            {
                // not loaded yet?
                if (cache == null)
                {
                    // get all ScriptableSkills in resources
                    ScriptableSkill[] skills = Resources.LoadAll<ScriptableSkill>("");

                    // check for duplicates, then add to cache
                    List<string> duplicates = skills.ToList().FindDuplicates(skill => skill.name);
                    if (duplicates.Count == 0)
                    {
                        cache = skills.ToDictionary(skill => skill.name.GetStableHashCode(), skill => skill);
                    }
                    else
                    {
                        foreach (string duplicate in duplicates)
                        {
                            Debug.LogError("Resources folder contains multiple ScriptableSkills with the name " + duplicate + ". If you are using subfolders like 'Warrior/NormalAttack' and 'Archer/NormalAttack', then rename them to 'Warrior/(Warrior)NormalAttack' and 'Archer/(Archer)NormalAttack' instead.");
                        }
                    }
                }
                return cache;
            }
        }
    }
}