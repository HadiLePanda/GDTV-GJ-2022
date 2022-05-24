using UnityEngine;

namespace GameJam
{
    [CreateAssetMenu(menuName = "Game/Skills/Passive Skill", order = 1)]
    public class PassiveSkill : BonusSkill
    {
        public override bool CheckTarget(Entity caster) { return false; }
        public override bool CheckDistance(Entity caster, int skillLevel, out Vector3 destination)
        {
            destination = caster.transform.position;
            return false;
        }
        public override bool CheckFOV(Entity caster, int skillLevel) { return false; }
        public override void Apply(Entity caster, int skillLevel) { }
    }
}