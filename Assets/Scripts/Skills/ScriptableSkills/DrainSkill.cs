using System.Collections.Generic;
using UnityEngine;

namespace GameJam
{
    [CreateAssetMenu(menuName = "Game/Skills/Drain AoE", order = 1)]
    public class DrainSkill : ScriptableSkill
    {
        // OverlapSphereNonAlloc array to avoid allocations.
        // -> static so we don't create one per skill
        // -> this is worth it because skills are casted a lot!
        // -> should be big enough to work in just about all cases
        static Collider[] hitsBuffer = new Collider[10000];

        [Header("Drain")]
        public ParticleAttractorSkillEffect effect;
        public LinearInt manaDrain;
        public LinearInt healthDrain;

        [Header("Stun")]
        public LinearFloat stunChance; // range [0,1]
        [Tooltip("In Seconds")]
        public LinearFloat stunTime;

        public override bool CheckDistance(Entity caster, int skillLevel, out Vector3 destination)
        {
            // can cast anywhere
            destination = caster.transform.position;
            return true;
        }

        public override bool CheckFOV(Entity caster, int skillLevel)
        {
            // can cast without visibility
            return true;
        }

        public override bool CheckTarget(Entity caster)
        {
            return true;
        }

        public override void Apply(Entity caster, int skillLevel)
        {
            // candidates hashset to be 100% sure that we don't apply an area skill
            // to a candidate twice. this could happen if the candidate has more
            // than one collider (which it often has).
            HashSet<Entity> candidates = new HashSet<Entity>();

            // for the generic case, cast the skill from the entity location
            // for the player, cast the skill around the look target
            Vector3 castPosition = caster.transform.position;
            if (caster is Player player)
            {
                castPosition = player.Movement.look.lookTarget.transform.position
                                .ChangeY(player.transform.position.y);
            }

            // find all entities of same type in castRange around the caster
            int hits = Physics.OverlapSphereNonAlloc(castPosition, castRange.Get(skillLevel), hitsBuffer);
            for (int i = 0; i < hits; ++i)
            {
                Collider co = hitsBuffer[i];
                Entity candidate = co.GetComponentInParent<Entity>();
                if (candidate != null  &&
                    caster.CanAttack(candidate)) // can drain that candidate (based on if we can attack it)
                {
                    candidates.Add(candidate);
                    break;
                }
            }

            // apply to all candidates
            foreach (Entity candidate in candidates)
            {
                // apply drain to only the first candidate (avoids )
                if (manaDrain.Get(skillLevel) > 0 || healthDrain.Get(skillLevel) > 0)
                {
                    caster.Combat.Drain(candidate,
                                        manaDrain.Get(skillLevel),
                                        healthDrain.Get(skillLevel));

                    // show effect on candidate
                    SpawnEffect(caster, candidate, candidate.transform);
                }
            }
        }

        // helper function to spawn the skill effect on someone
        // (used by all the buff implementations and to load them after saving)
        public void SpawnEffect(Entity caster, Entity target, Transform targetTransform)
        {
            if (effect != null)
            {
                GameObject go = Instantiate(effect.gameObject, target.transform.position, Quaternion.identity);
                ParticleAttractorSkillEffect effectComponent = go.GetComponent<ParticleAttractorSkillEffect>();
                effectComponent.caster = caster;
                effectComponent.target = target;
                effectComponent.Setup(caster, targetTransform);
            }
        }
    }
}