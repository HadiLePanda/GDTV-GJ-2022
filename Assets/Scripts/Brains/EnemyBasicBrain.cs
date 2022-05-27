using UnityEngine;

namespace GameJam
{
    [CreateAssetMenu(menuName = "Game/Brains/Enemy Basic Brain", order = 1)]
    public class EnemyBasicBrain : MobBrain
    {
        [Header("Movement")]
        [Range(0, 1)] public float moveProbability = 0.1f; // chance per second
        public float moveDistance = 10;
        // monsters should follow their targets even if they run out of the movement
        // radius. the follow dist should always be bigger than the biggest archer's
        // attack range, so that archers will always pull aggro, even when attacking
        // from far away.
        public float followDistance = 40;
        [Range(0.1f, 1)] public float attackToMoveRangeRatio = 0.8f; // move as close as 0.8 * attackRange to a target

        // events //////////////////////////////////////////////////////////////////
        public bool EventDeathTimeElapsed(Enemy enemy) => false;
            //enemy.State == "DEAD" && Time.time >= enemy.deathTimeEnd;

        public bool EventMoveRandomly(Enemy enemy) =>
            Random.value <= moveProbability * Time.deltaTime;

        public bool EventTargetTooFarToFollow(Enemy enemy)
        {
            return enemy.Target != null &&
                   Vector3.Distance(enemy.startPosition, Utils.ClosestPoint(enemy.Target, enemy.transform.position)) > followDistance;
        }

        // states //////////////////////////////////////////////////////////////////
        string UpdateServer_IDLE(Enemy enemy)
        {
            // events sorted by priority (e.g. target doesn't matter if we died)
            if (EventDied(enemy))
            {
                // we died.
                return "DEAD";
            }
            if (EventStunned(enemy))
            {
                enemy.Movement.Reset();
                return "STUNNED";
            }
            if (EventTargetDied(enemy))
            {
                // we had a target before, but it died now. clear it.
                enemy.SetTarget(null);
                enemy.Skills.CancelCast();
                return "IDLE";
            }
            if (EventTargetTooFarToFollow(enemy))
            {
                // we had a target before, but it's out of follow range now.
                // clear it and go back to start. don't stay here.
                enemy.SetTarget(null);
                enemy.Skills.CancelCast();
                enemy.Movement.Navigate(enemy.startPosition, 0);
                return "MOVING";
            }
            if (EventTargetTooFarToAttack(enemy))
            {
                // we had a target before, but it's out of attack range now.
                // follow it. (use collider point(s) to also work with big entities)
                float stoppingDistance = ((MobSkills)enemy.Skills).CurrentCastRange() * attackToMoveRangeRatio;
                Vector3 destination = Utils.ClosestPoint(enemy.Target, enemy.transform.position);
                enemy.Movement.Navigate(destination, stoppingDistance);
                return "MOVING";
            }
            if (EventSkillRequest(enemy))
            {
                // we had a target in attack range before and trying to cast a skill
                // on it. check self (alive, mana, weapon etc.) and target
                Skill skill = enemy.Skills.skills[enemy.Skills.currentSkill];
                if (enemy.Skills.CastCheckSelf(skill))
                {
                    if (enemy.Skills.CastCheckTarget(skill))
                    {
                        // start casting
                        enemy.Skills.StartCast(skill);
                        return "CASTING";
                    }
                    else
                    {
                        // invalid target. clear the attempted current skill.
                        //enemy.SetTarget(null);
                        enemy.Skills.currentSkill = -1;
                        return "IDLE";
                    }
                }
                else
                {
                    // we can't cast this skill at the moment (cooldown/low mana/...)
                    // -> clear the attempted current skill, but keep the target to
                    // continue later
                    enemy.Skills.currentSkill = -1;
                    return "IDLE";
                }
            }
            if (EventAggro(enemy))
            {
                // target in attack range. try to cast a first skill on it
                if (enemy.Skills.skills.Count > 0) enemy.Skills.currentSkill = ((MobSkills)enemy.Skills).NextSkill();
                else Debug.LogError(name + " has no skills to attack with.");
                return "IDLE";
            }
            if (EventMoveRandomly(enemy))
            {
                // walk to a random position in movement radius (from 'start')
                // note: circle y is 0 because we add it to start.y
                Vector2 circle2D = Random.insideUnitCircle * moveDistance;
                enemy.Movement.Navigate(enemy.startPosition + new Vector3(circle2D.x, 0, circle2D.y), 0);
                return "MOVING";
            }

            if (EventRandomAmbientSound(enemy))
            {
                PlayRandomLivingSound(enemy);
                enemy.lastAmbientSoundTime = Time.time;
            }

            if (EventDeathTimeElapsed(enemy)) { } // don't care
            if (EventMoveEnd(enemy)) { } // don't care
            if (EventSkillFinished(enemy)) { } // don't care
            if (EventTargetDisappeared(enemy)) { } // don't care

            return "IDLE"; // nothing interesting happened
        }

        string UpdateServer_MOVING(Enemy enemy)
        {
            // events sorted by priority (e.g. target doesn't matter if we died)
            if (EventDied(enemy))
            {
                // we died.
                enemy.Movement.Reset();
                return "DEAD";
            }
            if (EventStunned(enemy))
            {
                enemy.Movement.Reset();
                return "STUNNED";
            }
            if (EventMoveEnd(enemy))
            {
                // we reached our destination.
                return "IDLE";
            }
            if (EventTargetDied(enemy))
            {
                // we had a target before, but it died now. clear it.
                enemy.SetTarget(null);
                enemy.Skills.CancelCast();
                enemy.Movement.Reset();
                return "IDLE";
            }
            if (EventTargetTooFarToFollow(enemy))
            {
                // we had a target before, but it's out of follow range now.
                // clear it and go back to start. don't stay here.
                enemy.SetTarget(null);
                enemy.Skills.CancelCast();
                enemy.Movement.Navigate(enemy.startPosition, 0);
                return "MOVING";
            }
            if (EventTargetTooFarToAttack(enemy))
            {
                // we had a target before, but it's out of attack range now.
                // follow it. (use collider point(s) to also work with big entities)
                float stoppingDistance = ((MobSkills)enemy.Skills).CurrentCastRange() * attackToMoveRangeRatio;
                Vector3 destination = Utils.ClosestPoint(enemy.Target, enemy.transform.position);
                enemy.Movement.Navigate(destination, stoppingDistance);
                return "MOVING";
            }
            if (EventAggro(enemy))
            {
                // target in attack range. try to cast a first skill on it
                // (we may get a target while randomly wandering around)
                if (enemy.Skills.skills.Count > 0) enemy.Skills.currentSkill = ((MobSkills)enemy.Skills).NextSkill();
                else Debug.LogError(name + " has no skills to attack with.");
                enemy.Movement.Reset();
                return "IDLE";
            }

            if (EventRandomAmbientSound(enemy))
            {
                PlayRandomLivingSound(enemy);
                enemy.lastAmbientSoundTime = Time.time;
            }

            if (EventDeathTimeElapsed(enemy)) { } // don't care
            if (EventSkillFinished(enemy)) { } // don't care
            if (EventTargetDisappeared(enemy)) { } // don't care
            if (EventSkillRequest(enemy)) { } // don't care, finish movement first
            if (EventMoveRandomly(enemy)) { } // don't care

            return "MOVING"; // nothing interesting happened
        }

        string UpdateServer_CASTING(Enemy enemy)
        {
            if (EventRandomAmbientSound(enemy)) { } // don't care

            // keep looking at the target (only Y rotation)
            if (enemy.Target)
            {
                enemy.Movement.LookAtY(enemy.Target.transform.position);
            }

            // events sorted by priority (e.g. target doesn't matter if we died)
            if (EventDied(enemy))
            {
                // we died.
                return "DEAD";
            }
            if (EventStunned(enemy))
            {
                enemy.Skills.CancelCast();
                enemy.Movement.Reset();
                return "STUNNED";
            }
            if (EventTargetDisappeared(enemy))
            {
                // cancel if the target matters for this skill
                if (enemy.Skills.skills[enemy.Skills.currentSkill].cancelCastIfTargetDied)
                {
                    enemy.Skills.CancelCast();
                    enemy.SetTarget(null);
                    return "IDLE";
                }
            }
            if (EventTargetDied(enemy))
            {
                // cancel if the target matters for this skill
                if (enemy.Skills.skills[enemy.Skills.currentSkill].cancelCastIfTargetDied)
                {
                    enemy.Skills.CancelCast();
                    enemy.SetTarget(null);
                    return "IDLE";
                }
            }
            if (EventSkillFinished(enemy))
            {
                // finished casting. apply the skill on the target.
                enemy.Skills.FinishCast(enemy.Skills.skills[enemy.Skills.currentSkill]);

                // did the target die? then clear it so that the enemy doesn't
                // run towards it if the target respawned
                // (target might be null if disappeared or targetless skill)
                if (enemy.Target != null && !enemy.Target.IsAlive)
                {
                    enemy.SetTarget(null);
                }

                // go back to IDLE, reset current skill
                ((MobSkills)enemy.Skills).lastSkill = enemy.Skills.currentSkill;
                enemy.Skills.currentSkill = -1;
                return "IDLE";
            }
            if (EventDeathTimeElapsed(enemy)) { } // don't care
            if (EventMoveEnd(enemy)) { } // don't care
            if (EventTargetTooFarToAttack(enemy)) { } // don't care, we were close enough when starting to cast
            if (EventTargetTooFarToFollow(enemy)) { } // don't care, we were close enough when starting to cast
            if (EventAggro(enemy)) { } // don't care, always have aggro while casting
            if (EventSkillRequest(enemy)) { } // don't care, that's why we are here
            if (EventMoveRandomly(enemy)) { } // don't care

            return "CASTING"; // nothing interesting happened
        }

        string UpdateServer_STUNNED(Enemy enemy)
        {
            if (EventRandomAmbientSound(enemy)) { } // don't care

            // events sorted by priority (e.g. target doesn't matter if we died)
            if (EventDied(enemy))
            {
                // we died.
                return "DEAD";
            }
            if (EventStunned(enemy))
            {
                return "STUNNED";
            }

            // go back to idle if we aren't stunned anymore and process all new
            // events there too
            return "IDLE";
        }

        string UpdateServer_DEAD(Enemy enemy)
        {
            if (EventRandomAmbientSound(enemy)) { } // don't care, can't make a sound while dead

            // disabled death respawning in state machine, death handled in entity
            //if (EventDeathTimeElapsed(enemy))
            //{
            //    // we were lying around dead for long enough now.
            //    // hide while respawning, or disappear forever
            //    if (enemy.respawn) enemy.Hide();
            //    else Destroy(enemy.gameObject);
            //    return "DEAD";
            //}
            if (EventSkillRequest(enemy)) { } // don't care
            if (EventSkillFinished(enemy)) { } // don't care
            if (EventMoveEnd(enemy)) { } // don't care
            if (EventTargetDisappeared(enemy)) { } // don't care
            if (EventTargetDied(enemy)) { } // don't care
            if (EventTargetTooFarToFollow(enemy)) { } // don't care
            if (EventTargetTooFarToAttack(enemy)) { } // don't care
            if (EventAggro(enemy)) { } // don't care
            if (EventMoveRandomly(enemy)) { } // don't care
            if (EventStunned(enemy)) { } // don't care
            if (EventDied(enemy)) { } // don't care, of course we are dead

            return "DEAD"; // nothing interesting happened
        }

        public override string UpdateBrain(Entity entity)
        {
            Enemy enemy = (Enemy)entity;

            string stateResult = string.Empty;
            if (enemy.State == "IDLE") stateResult = UpdateServer_IDLE(enemy);
            else if (enemy.State == "MOVING") stateResult = UpdateServer_MOVING(enemy);
            else if (enemy.State == "CASTING") stateResult = UpdateServer_CASTING(enemy);
            else if (enemy.State == "STUNNED") stateResult = UpdateServer_STUNNED(enemy);
            else if (enemy.State == "DEAD") stateResult = UpdateServer_DEAD(enemy);
            else
            {
                Debug.LogError("invalid state:" + enemy.State);
                stateResult = "IDLE";
            }

            UpdateClient(entity);

            return stateResult;
        }

        public void UpdateClient(Entity entity)
        {
            Enemy enemy = (Enemy)entity;
            if (enemy.State == "CASTING")
            {
                // keep looking at the target (only Y rotation)
                if (enemy.Target)
                {
                    enemy.Movement.LookAtY(enemy.Target.transform.position);
                }
            }
        }

        // DrawGizmos can be used for debug info
        public override void DrawGizmos(Entity entity)
        {
            Enemy enemy = (Enemy)entity;

            // draw the movement area (around 'start' if game running,
            // or around current position if still editing)
            Vector3 startHelp = Application.isPlaying ? enemy.startPosition : enemy.transform.position;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(startHelp, moveDistance);

            // draw the follow dist
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(startHelp, followDistance);
        }
    }
}