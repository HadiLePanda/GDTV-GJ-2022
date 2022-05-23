using UnityEngine;

namespace GameJam
{
    [RequireComponent(typeof(Level))]
    [RequireComponent(typeof(MovementBase))]
    [DisallowMultipleComponent]
    public class PlayerSkills : Skills
    {
        [Header("Player References")]
        [SerializeField] private Experience experience;

        protected override void OnEnable()
        {
            base.OnEnable();
            experience.OnLevelUp += OnLevelUp;
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            experience.OnLevelUp -= OnLevelUp;
        }

        private void Start()
        {
            // load skills based on skill templates
            foreach (ScriptableSkill skillData in skillTemplates)
            {
                skills.Add(new Skill(skillData));
            }

            // spawn effects for any buffs that might still be active after loading
            // (OnStartServer is too early)
            // note: no need to do that in Entity.Start because we don't load them
            //       with previously casted skills
            for (int i = 0; i < buffs.Count; ++i)
            {
                if (buffs[i].BuffTimeRemaining() > 0)
                {
                    buffs[i].data.SpawnEffect(entity, entity);
                }
            }
        }

        public void CmdUse(int skillIndex)
        {
            // validate
            if (IsValidStateToUseSkill() &&
                0 <= skillIndex && skillIndex < skills.Count)
            {
                // skill learned and can be casted?
                if (skills[skillIndex].level > 0 && skills[skillIndex].IsReady())
                {
                    currentSkill = skillIndex;
                }
            }
        }

        // helper function: try to use a skill and walk into range if necessary
        public void TryUse(int skillIndex, bool ignoreState = false)
        {
            // only if not casting already
            // (might need to ignore that when coming from pending skill where
            //  CASTING is still true)
            if (entity.State != EntityState.CASTING.ToString() || ignoreState)
            {
                Skill skill = skills[skillIndex];
                if (CastCheckSelf(skill) && CastCheckTarget(skill))
                {
                    // check distance between self and target
                    Vector3 destination;
                    if (CastCheckDistance(skill, out destination))
                    {
                        // cast
                        CmdUse(skillIndex);
                    }
                }
            }
            else
            {
                ((Player)entity).pendingSkill = skillIndex;
            }
        }

        public bool HasLearned(string skillName)
        {
            // has this skill with at least level 1 (=learned)?
            return HasLearnedWithLevel(skillName, 1);
        }

        public bool HasLearnedWithLevel(string skillName, int skillLevel)
        {
            foreach (Skill skill in skills)
            {
                if (skill.level >= skillLevel && skill.name == skillName)
                {
                    return true;
                }
            }

            return false;
        }

        // helper function for command and UI
        // -> this is for learning and upgrading!
        public bool CanUpgrade(Skill skill)
        {
            return skill.level < skill.maxLevel &&
                   entity.Level.Current >= skill.upgradeRequiredLevel;
        }

        // -> this is for learning and upgrading!
        public void CmdUpgrade(int skillIndex)
        {
            // validate
            if (IsValidStateToUpgrade() &&
                0 <= skillIndex && skillIndex < skills.Count)
            {
                // can be upgraded?
                Skill skill = skills[skillIndex];
                if (CanUpgrade(skill))
                {
                    // upgrade
                    ++skill.level;
                    skills[skillIndex] = skill;
                }
            }
        }

        private bool IsValidStateToUseSkill()
        {
            return entity.State == EntityState.IDLE.ToString() || entity.State == EntityState.MOVING.ToString() || entity.State == EntityState.CASTING.ToString();
        }
        private bool IsValidStateToUpgrade()
        {
            return entity.State == EntityState.IDLE.ToString() || entity.State == EntityState.MOVING.ToString().ToString() || entity.State == EntityState.CASTING.ToString();
        }

        // events =======================================================================
        private void OnLevelUp()
        {
            // upgrade each skill that can be leveled up, now that we are at the new level
            for (int i = 0; i < skills.Count; i++)
            {
                if (CanUpgrade(skills[i]))
                {
                    CmdUpgrade(i);
                }
            }
        }

        private void OnValidate()
        {
            // auto-reference entity
            if (entity == null && TryGetComponent(out Player playerComponent))
            {
                entity = playerComponent;
            }
        }
    }
}