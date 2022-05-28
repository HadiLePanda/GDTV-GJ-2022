// Group heal that heals all entities of same type in cast range
// => player heals players in cast range
// => monster heals monsters in cast range
using System.Collections.Generic;
using UnityEngine;

namespace GameJam
{
    [CreateAssetMenu(menuName = "Game/Skills/Heal AoE", order = 1)]
    public class HealAoeSkill : HealSkill
    {
        // OverlapSphereNonAlloc array to avoid allocations.
        // -> static so we don't create one per skill
        // -> this is worth it because skills are casted a lot!
        // -> should be big enough to work in just about all cases
        static Collider[] hitsBuffer = new Collider[10000];

        public override bool CheckTarget(Entity caster)
        {
            // no target necessary, but still set to self so that LookAt(target)
            // doesn't cause the player to look at a target that doesn't even matter
            //caster.SetTarget(caster);
            return true;
        }

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

        public override void Apply(Entity caster, int skillLevel)
        {
            // candidates hashset to be 100% sure that we don't apply an area skill
            // to a candidate twice. this could happen if the candidate has more
            // than one collider (which it often has).
            HashSet<Entity> candidates = new HashSet<Entity>();

            // find all entities of same type in castRange around the caster
            int hits = Physics.OverlapSphereNonAlloc(caster.transform.position, castRange.Get(skillLevel), hitsBuffer);
            for (int i = 0; i < hits; ++i)
            {
                Collider co = hitsBuffer[i];
                Entity candidate = co.GetComponentInParent<Entity>();
                if (candidate != null &&
                    candidate.IsAlive) // can't heal dead people
                {
                    // can heal ourself
                    if (canHealSelf &&
                        candidate == caster)
                    {
                        candidates.Add(candidate);
                    }

                    // can heal others
                    if (canHealOthers &&
                        caster.CanHeal(candidate))
                    {
                        // check case where can only heal same type
                        if (canHealOnlySameType &&
                            caster.GetType() != candidate.GetType())
                        { return; }

                        candidates.Add(candidate);
                    }
                }
            }

            // apply to all candidates
            foreach (Entity candidate in candidates)
            {
                caster.Combat.Heal(candidate, healsHealth.Get(skillLevel));
                caster.Combat.HealMana(candidate, healsMana.Get(skillLevel));

                // show effect on candidate
                SpawnEffect(caster, candidate);
            }
        }
    }
}