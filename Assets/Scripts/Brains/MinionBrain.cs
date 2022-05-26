using UnityEngine;

namespace GameJam
{
    [CreateAssetMenu(menuName = "Game/Brains/Minion Brain", order = 1)]
    public class MinionBrain : MobBrain
    {
        [Header("Movement")]
        [Range(0, 1)] public float moveProbability = 0.1f; // chance per second
        public float moveWanderDistance = 10;
        public float returnDistance = 25; // return to player if dist > ...
        // pets should follow their targets even if they run out of the movement radius.
        // the follow dist should always be bigger than the biggest archer's attack range,
        // so that archers will always pull aggro, even when attacking from far away.
        public float followDistance = 20;
        // minion should teleport if the owner gets too far away for whatever reason
        public float teleportDistance = 30;
        [Range(0.1f, 1)] public float attackToMoveRangeRatio = 0.8f; // move as close as 0.8 * attackRange to a target

        // events //////////////////////////////////////////////////////////////////
        public bool EventOwnerDisappeared(Minion minion) =>
            minion.Owner == null;

        public bool EventMoveRandomly(Minion minion) =>
            Random.value <= moveProbability * Time.deltaTime;

        public bool EventDeathTimeElapsed(Minion minion) => false;
            //minion.State == "DEAD" && Time.time >= minion.deathTimeEnd;

        public bool EventNeedReturnToOwner(Minion minion) =>
            minion.Target == null &&
            Vector3.Distance(minion.Owner.MinionDestination, minion.transform.position) > returnDistance;

        public bool EventNeedTeleportToOwner(Minion minion) =>
            Vector3.Distance(minion.Owner.MinionDestination, minion.transform.position) > teleportDistance;

        public bool EventTargetTooFarToFollow(Minion minion) =>
            minion.Target != null &&
            Vector3.Distance(minion.Owner.MinionDestination, Utils.ClosestPoint(minion.Target, minion.transform.position)) > followDistance;

        // states //////////////////////////////////////////////////////////////////
        string UpdateServer_IDLE(Minion minion)
        {
            if (EventRandomAmbientSound(minion))
            {
                PlayRandomLivingSound(minion);
            }

            // events sorted by priority (e.g. target doesn't matter if we died)
            if (EventOwnerDisappeared(minion))
            {
                // owner might get destroyed for some reason
                minion.Health.Deplete();
                return "DEAD";
            }
            if (EventDied(minion))
            {
                // we died.
                return "DEAD";
            }
            if (EventStunned(minion))
            {
                minion.Movement.Reset();
                return "STUNNED";
            }
            if (EventTargetDied(minion))
            {
                // we had a target before, but it died now. clear it.
                minion.SetTarget(null);
                minion.Skills.CancelCast();
                return "IDLE";
            }
            if (EventNeedTeleportToOwner(minion))
            {
                minion.Movement.Warp(minion.Owner.MinionDestination);
                return "IDLE";
            }
            if (EventNeedReturnToOwner(minion))
            {
                // return to owner only while IDLE
                minion.SetTarget(null);
                minion.Skills.CancelCast();
                minion.Movement.Navigate(minion.Owner.MinionDestination, 0);
                return "MOVING";
            }
            if (EventTargetTooFarToFollow(minion))
            {
                // we had a target before, but it's out of follow range now.
                // clear it and go back to start. don't stay here.
                minion.SetTarget(null);
                minion.Skills.CancelCast();
                minion.Movement.Navigate(minion.Owner.MinionDestination, 0);
                return "MOVING";
            }
            if (EventTargetTooFarToAttack(minion))
            {
                // we had a target before, but it's out of attack range now.
                // follow it. (use collider point(s) to also work with big entities)
                float stoppingDistance = ((MobSkills)minion.Skills).CurrentCastRange() * attackToMoveRangeRatio;
                Vector3 destination = Utils.ClosestPoint(minion.Target, minion.transform.position);
                minion.Movement.Navigate(destination, stoppingDistance);
                return "MOVING";
            }
            if (EventSkillRequest(minion))
            {
                // we had a target in attack range before and trying to cast a skill
                // on it. check self (alive, mana, weapon etc.) and target
                Skill skill = minion.Skills.skills[minion.Skills.currentSkill];
                if (minion.Skills.CastCheckSelf(skill))
                {
                    if (minion.Skills.CastCheckTarget(skill))
                    {
                        // start casting
                        minion.Skills.StartCast(skill);
                        return "CASTING";
                    }
                    else
                    {
                        // invalid target. clear the attempted current skill.
                        minion.SetTarget(null);
                        minion.Skills.currentSkill = -1;
                        return "IDLE";
                    }
                   
                }
                else
                {
                    // we can't cast this skill at the moment (cooldown/low mana/...)
                    // -> clear the attempted current skill, but keep the target to
                    // continue later
                    minion.SetTarget(null);
                    minion.Skills.currentSkill = -1;
                    return "IDLE";
                }
            }
            if (EventAggro(minion))
            {
                // target in attack range. try to cast a first skill on it
                if (minion.Skills.skills.Count > 0) minion.Skills.currentSkill = ((MobSkills)minion.Skills).NextSkill();
                else Debug.LogError(name + " has no skills to attack with.");
                return "IDLE";
            }
            if (EventMoveRandomly(minion))
            {
                // walk to a random position in movement radius (from 'start')
                // note: circle y is 0 because we add it to start.y
                Vector2 circle2D = Random.insideUnitCircle * moveWanderDistance;
                minion.Movement.Navigate(minion.startPosition + new Vector3(circle2D.x, 0, circle2D.y), 0);
                return "MOVING";
            }
            if (EventMoveEnd(minion)) { } // don't care
            if (EventDeathTimeElapsed(minion)) { } // don't care
            if (EventSkillFinished(minion)) { } // don't care
            if (EventTargetDisappeared(minion)) { } // don't care

            return "IDLE"; // nothing interesting happened
        }

        string UpdateServer_MOVING(Minion minion)
        {
            if (EventRandomAmbientSound(minion))
            {
                PlayRandomLivingSound(minion);
            }

            // events sorted by priority (e.g. target doesn't matter if we died)
            if (EventOwnerDisappeared(minion))
            {
                // owner might get destroyed for some reason
                minion.Health.Deplete();
                return "DEAD";
            }
            if (EventDied(minion))
            {
                // we died.
                minion.Movement.Reset();
                return "DEAD";
            }
            if (EventStunned(minion))
            {
                minion.Movement.Reset();
                return "STUNNED";
            }
            if (EventMoveEnd(minion))
            {
                // we reached our destination.
                return "IDLE";
            }
            if (EventTargetDied(minion))
            {
                // we had a target before, but it died now. clear it.
                minion.SetTarget(null);
                minion.Skills.CancelCast();
                minion.Movement.Reset();
                return "IDLE";
            }
            if (EventNeedTeleportToOwner(minion))
            {
                minion.Movement.Warp(minion.Owner.MinionDestination);
                return "IDLE";
            }
            if (EventTargetTooFarToFollow(minion))
            {
                // we had a target before, but it's out of follow range now.
                // clear it and go back to owner. don't stay here.
                minion.SetTarget(null);
                minion.Skills.CancelCast();
                minion.Movement.Navigate(minion.Owner.MinionDestination, 0);
                return "MOVING";
            }
            if (EventTargetTooFarToAttack(minion))
            {
                // we had a target before, but it's out of attack range now.
                // follow it. (use collider point(s) to also work with big entities)
                float stoppingDistance = ((MobSkills)minion.Skills).CurrentCastRange() * attackToMoveRangeRatio;
                Vector3 destination = Utils.ClosestPoint(minion.Target, minion.transform.position);
                minion.Movement.Navigate(destination, stoppingDistance);
                return "MOVING";
            }
            if (EventAggro(minion))
            {
                // target in attack range. try to cast a first skill on it
                // (we may get a target while randomly wandering around)
                if (minion.Skills.skills.Count > 0) minion.Skills.currentSkill = ((MobSkills)minion.Skills).NextSkill();
                else Debug.LogError(name + " has no skills to attack with.");
                minion.Movement.Reset();
                return "IDLE";
            }
            if (EventNeedReturnToOwner(minion))
            {
                //todo destination too far from owner, recalculate return position
                //if ()
                // return to owner only while IDLE
                //minion.SetTarget(null);
                minion.Skills.CancelCast();
                minion.Movement.Navigate(minion.Owner.MinionDestination, 0);
                return "MOVING";
            }
            if (EventDeathTimeElapsed(minion)) { } // don't care
            if (EventSkillFinished(minion)) { } // don't care
            if (EventTargetDisappeared(minion)) { } // don't care
            if (EventSkillRequest(minion)) { } // don't care, finish movement first

            return "MOVING"; // nothing interesting happened
        }

        string UpdateServer_CASTING(Minion minion)
        {
            if (EventRandomAmbientSound(minion)) { } // don't care

            // keep looking at the target for server & clients (only Y rotation)
            if (minion.Target)
                minion.Movement.LookAtY(minion.Target.transform.position);

            // events sorted by priority (e.g. target doesn't matter if we died)
            if (EventOwnerDisappeared(minion))
            {
                // owner might get destroyed for some reason
                minion.Health.Deplete();
                return "DEAD";
            }
            if (EventDied(minion))
            {
                // we died.
                return "DEAD";
            }
            if (EventStunned(minion))
            {
                minion.Skills.CancelCast();
                minion.Movement.Reset();
                return "STUNNED";
            }
            if (EventTargetDisappeared(minion))
            {
                // cancel if the target matters for this skill
                if (minion.Skills.skills[minion.Skills.currentSkill].cancelCastIfTargetDied)
                {
                    minion.Skills.CancelCast();
                    minion.SetTarget(null);
                    return "IDLE";
                }
            }
            if (EventTargetDied(minion))
            {
                // cancel if the target matters for this skill
                if (minion.Skills.skills[minion.Skills.currentSkill].cancelCastIfTargetDied)
                {
                    minion.Skills.CancelCast();
                    minion.SetTarget(null);
                    return "IDLE";
                }
            }
            if (EventSkillFinished(minion))
            {
                // finished casting. apply the skill on the target.
                minion.Skills.FinishCast(minion.Skills.skills[minion.Skills.currentSkill]);

                // did the target die? then clear it so that the monster doesn't
                // run towards it if the target respawned
                if (minion.Target != null && !minion.Target.IsAlive)
                    minion.SetTarget(null);

                // go back to IDLE. reset current skill.
                ((MobSkills)minion.Skills).lastSkill = minion.Skills.currentSkill;
                minion.Skills.currentSkill = -1;
                return "IDLE";
            }
            if (EventMoveEnd(minion)) { } // don't care
            if (EventDeathTimeElapsed(minion)) { } // don't care
            if (EventNeedTeleportToOwner(minion)) { } // don't care
            if (EventNeedReturnToOwner(minion)) { } // don't care
            if (EventTargetTooFarToAttack(minion)) { } // don't care, we were close enough when starting to cast
            if (EventTargetTooFarToFollow(minion)) { } // don't care, we were close enough when starting to cast
            if (EventAggro(minion)) { } // don't care, always have aggro while casting
            if (EventSkillRequest(minion)) { } // don't care, that's why we are here

            return "CASTING"; // nothing interesting happened
        }

        string UpdateServer_STUNNED(Minion minion)
        {
            if (EventRandomAmbientSound(minion)) { } // don't care

            // events sorted by priority (e.g. target doesn't matter if we died)
            if (EventOwnerDisappeared(minion))
            {
                // owner might get destroyed for some reason
                minion.Health.Deplete();
                return "DEAD";
            }
            if (EventDied(minion))
            {
                // we died.
                minion.Skills.CancelCast(); // in case we died while trying to cast
                return "DEAD";
            }
            if (EventStunned(minion))
            {
                return "STUNNED";
            }

            // go back to idle if we aren't stunned anymore and process all new
            // events there too
            return "IDLE";
        }

        string UpdateServer_DEAD(Minion minion)
        {
            if (EventRandomAmbientSound(minion)) { } // don't care, can't make a sound while dead

            // events sorted by priority (e.g. target doesn't matter if we died)
            if (EventOwnerDisappeared(minion))
            {
                // owner might get destroyed for some reason
                minion.Health.Deplete();
                return "DEAD";
            }
            //if (EventDeathTimeElapsed(minion))
            //{
            //    // we were lying around dead for long enough now.
            //    // hide while respawning, or disappear forever
            //    Destroy(minion.gameObject);
            //    return "DEAD";
            //}
            if (EventSkillRequest(minion)) { } // don't care
            if (EventSkillFinished(minion)) { } // don't care
            if (EventMoveEnd(minion)) { } // don't care
            if (EventNeedTeleportToOwner(minion)) { } // don't care
            if (EventNeedReturnToOwner(minion)) { } // don't care
            if (EventTargetDisappeared(minion)) { } // don't care
            if (EventTargetDied(minion)) { } // don't care
            if (EventTargetTooFarToFollow(minion)) { } // don't care
            if (EventTargetTooFarToAttack(minion)) { } // don't care
            if (EventAggro(minion)) { } // don't care
            if (EventDied(minion)) { } // don't care, of course we are dead

            return "DEAD"; // nothing interesting happened
        }

        public override string UpdateBrain(Entity entity)
        {
            Minion minion = (Minion)entity;

            string stateResult = string.Empty;
            if (minion.State == "IDLE") stateResult = UpdateServer_IDLE(minion);
            else if (minion.State == "MOVING") stateResult = UpdateServer_MOVING(minion);
            else if (minion.State == "CASTING") stateResult = UpdateServer_CASTING(minion);
            else if (minion.State == "STUNNED") stateResult = UpdateServer_STUNNED(minion);
            else if (minion.State == "DEAD") stateResult = UpdateServer_DEAD(minion);
            else
            {
                Debug.LogError("invalid state:" + minion.State);
                stateResult = "IDLE";
            }

            UpdateClient(entity);

            return stateResult;
        }

        public void UpdateClient(Entity entity)
        {
            Minion minion = (Minion)entity;
            if (minion.State == "CASTING")
            {
                // keep looking at the target for server & clients (only Y rotation)
                if (minion.Target)
                {
                    minion.Movement.LookAtY(minion.Target.transform.position);
                }
            }
        }

        // DrawGizmos can be used for debug info
        public override void DrawGizmos(Entity entity)
        {
            Minion minion = (Minion)entity;

            // draw the movement area (around 'start' if game running,
            // or around current position if still editing)
            Vector3 startHelp = Application.isPlaying ? minion.Owner.MinionDestination : minion.transform.position;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(startHelp, returnDistance);

            // draw the follow dist
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(startHelp, followDistance);
        }
    }
}