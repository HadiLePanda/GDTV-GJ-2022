using UnityEngine;

namespace GameJam
{
    public struct ProjectileData
    {
        public int damage;
        public int manaDrain;
        public int healthDrain;
        public float speed;
        public float lifetime;
        public float stunChance;
        public float stunTime;
    }

    [CreateAssetMenu(menuName = "Game/Skills/Projectile Skill", order = 1)]
    public class ProjectileSkill : ScriptableSkill
    {
        [Header("Projectile Settings")]
        public ProjectileSkillEffect projectile;
        public LinearInt damage;
        public LinearInt manaDrain;
        public LinearInt healthDrain;
        public LinearFloat speed;
        public LinearFloat lifetime;
        public LinearFloat stunChance;
        public LinearFloat stunTime;

        public override bool CheckDistance(Entity caster, int skillLevel, out Vector3 destination)
        {
            // can cast anywhere
            destination = caster.transform.position;
            return true;
        }

        public override bool CheckFOV(Entity caster, int skillLevel)
        {
            Entity correctedTarget = caster.Target;

            // case where we apply without target
            // can cast without visibility
            if (correctedTarget == null)
                return true;

            return correctedTarget != null && caster.FieldOfView.IsInFOV(correctedTarget.transform);
        }

        public override bool CheckTarget(Entity caster)
        {
            // no target necessary, but still set to self so that LookAt(target)
            // doesn't cause the player to look at a target that doesn't even matter
            //caster.SetTarget(caster);
            return true;
        }

        public override void Apply(Entity caster, int skillLevel)
        {
            // spawn projectile
            ProjectileSkillEffect projectileInstance = Instantiate(projectile, caster.Skills.effectMount.position, caster.transform.rotation);

            // setup projectile data
            ProjectileData projectileData = new ProjectileData()
            {
                damage = caster.Combat.damage + damage.Get(skillLevel),
                manaDrain = manaDrain.Get(skillLevel),
                healthDrain = healthDrain.Get(skillLevel),
                speed = speed.Get(skillLevel),
                lifetime = lifetime.Get(skillLevel),
                stunChance = stunChance.Get(skillLevel),
                stunTime = stunTime.Get(skillLevel),
            };
            projectileInstance.Setup(caster, projectileData);
        }
    }
}