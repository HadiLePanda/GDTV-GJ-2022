using UnityEngine;

namespace GameJam
{
    [CreateAssetMenu(menuName = "Game/Skills/Target Damage", order = 1)]
    public class TargetDamageSkill : DamageSkill
    {
        // helper function to determine the target that the skill will be cast on
        // (e.g. cast on self if targeting a monster that isn't healable)
        Entity CorrectedTarget(Entity caster)
        {
            // targeting nothing?
            if (caster.Target == null)
            {
                return null;
            }

            // targeting self?
            if (caster.Target == caster)
            {
                return null;
            }

            // no valid target? try to cast on self or on our target
            return caster.Target;
        }

        public override bool CheckTarget(Entity caster)
        {
            Entity correctedTarget = CorrectedTarget(caster);

            // avoid setting ourselves as target
            // it feels better during combat, avoids having to switch target back and forth
            if (correctedTarget == caster)
            {
                return caster.IsAlive;
            }

            // target exists, alive, not self, ok type?
            return caster.Target != null && caster.CanAttack(caster.Target);
        }

        // (has corrected target already)
        public override bool CheckDistance(Entity caster, int skillLevel, out Vector3 destination)
        {
            // target still around?
            if (caster.Target != null)
            {
                destination = Utils.ClosestPoint(caster.Target, caster.transform.position);
                return Utils.ClosestDistance(caster, caster.Target) <= castRange.Get(skillLevel);
            }
            destination = caster.transform.position;
            return false;
        }

        // (has corrected target already)
        public override bool CheckFOV(Entity caster, int skillLevel)
        {
            Entity correctedTarget = CorrectedTarget(caster);

            // case where we apply on self without self targeting
            if (correctedTarget == caster)
                return true;

            return correctedTarget != null && caster.FieldOfView.IsInFOV(correctedTarget.transform);
        }

        // (has corrected target already)
        public override void Apply(Entity caster, int skillLevel)
        {
            Entity correctedTarget = CorrectedTarget(caster);

            // we didn't auto target anything, so apply on ourselves
            if (correctedTarget == caster && caster.IsAlive)
            {
                correctedTarget = caster;
            }
            // otherwise we apply on our target
            else if (caster.Target != null && caster.Target.IsAlive)
            {
                correctedTarget = caster.Target;
            }

            // get hit normal
            Quaternion hitRotation = Quaternion.FromToRotation(correctedTarget.transform.position, caster.transform.position);
            Vector3 hitNormal = hitRotation.eulerAngles;

            // deal damage directly with base damage + skill damage
            caster.Combat.DealDamage(correctedTarget,
                                       caster.Combat.damage + damage.Get(skillLevel),
                                       correctedTarget.transform.position, hitNormal,
                                       stunChance.Get(skillLevel),
                                       stunTime.Get(skillLevel));

            // apply drain
            int manaToDrain = manaDrain.Get(skillLevel);
            int healthToDrain = healthDrain.Get(skillLevel);
            if (manaToDrain > 0 || healthToDrain > 0)
            {
                Entity owner = drainToOwner && caster is Minion minion && minion.Owner != null
                                ? minion.Owner
                                : null;

                Entity correctedDrainReceiver = owner ? owner : caster;

                correctedDrainReceiver.Combat.Drain(correctedTarget,
                                                    manaToDrain,
                                                    healthToDrain);
            }
        }
    }
}