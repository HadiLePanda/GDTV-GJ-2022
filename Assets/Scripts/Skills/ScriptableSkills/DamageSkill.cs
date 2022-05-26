// Base type for damage skill templates.
// => there may be target damage, targetless damage, aoe damage, etc.
using System.Text;
using UnityEngine;

namespace GameJam
{
    public abstract class DamageSkill : ScriptableSkill
    {
        [Header("Damage")]
        public LinearInt damage = new LinearInt { baseValue = 1 };
        public LinearFloat stunChance; // range [0,1]
        [Tooltip("In Seconds")]
        public LinearFloat stunTime;

        [Header("Damage Drain")]
        public LinearInt manaDrain;
        public LinearInt healthDrain;
        public bool drainToOwner;

        [Header("Damage Effect")]
        public OneTimeTargetSkillEffect effect;

        // helper function to spawn the skill effect on someone
        // (used by all the buff implementations and to load them after saving)
        public void SpawnEffect(Entity caster, Entity spawnTarget)
        {
            if (effect != null)
            {
                GameObject go = Instantiate(effect.gameObject, spawnTarget.transform.position, Quaternion.identity);
                OneTimeTargetSkillEffect effectComponent = go.GetComponent<OneTimeTargetSkillEffect>();
                effectComponent.caster = caster;
                effectComponent.target = spawnTarget;
            }
        }

        // tooltip
        public override string ToolTip(int skillLevel, bool showRequirements = false)
        {
            StringBuilder tip = new StringBuilder(base.ToolTip(skillLevel, showRequirements));
            tip.Replace("{DAMAGE}", damage.Get(skillLevel).ToString());
            tip.Replace("{STUNCHANCE}", Mathf.RoundToInt(stunChance.Get(skillLevel) * 100).ToString());
            tip.Replace("{STUNTIME}", stunTime.Get(skillLevel).ToString("F1"));
            return tip.ToString();
        }
    }
}