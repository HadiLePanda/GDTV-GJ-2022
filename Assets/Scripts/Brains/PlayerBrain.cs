using UnityEngine;

namespace GameJam
{
    [CreateAssetMenu(menuName = "Game/Brains/Player Brain", order = 1)]
    public class PlayerBrain : CommonBrain
    {
        [Tooltip("Being stunned interrupts the cast. Enable this option to continue the cast afterwards.")]
        public bool continueCastAfterStunned = true;

        // events //////////////////////////////////////////////////////////////////
        public bool EventCancelAction(Player player)
        {
            bool result = player.cancelActionRequested;
            player.cancelActionRequested = false; // reset
            return result;
        }

        // states //////////////////////////////////////////////////////////////////
        private bool CanCancelSkillInCurrentState(Player player)
        {
            return player.Movement.State != MoveState.CROUCHING && player.Movement.State != MoveState.CLIMBING && player.Movement.State != MoveState.CRAWLING;
        }

        string UpdateServer_IDLE(Player player)
        {
            // events sorted by priority (e.g. target doesn't matter if we died)
            if (EventDied(player))
            {
                // we died.
                return "DEAD";
            }
            if (EventStunned(player))
            {
                player.Movement.Reset();
                return "STUNNED";
            }
            if (EventCancelAction(player))
            {
                // the only thing that we can cancel is the target
                if (player.Target != null)
                {
                    player.SetTarget(null);
                }
                return "IDLE";
            }
            if (EventMoveStart(player))
            {
                // cancel casting (if any)
                player.Skills.CancelCast();
                return "MOVING";
            }
            if (EventSkillRequest(player))
            {
                // don't cast in a different movement state
                if (CanCancelSkillInCurrentState(player))
                {
                    // user wants to cast a skill.
                    // check self (alive, mana, weapon etc.) and target and distance
                    Skill skill = player.Skills.skills[player.Skills.currentSkill];
                    player.nextTarget = player.Target; // return to this one after any corrections by skills.CastCheckTarget
                    if (player.Skills.CastCheckSelf(skill) &&
                        player.Skills.CastCheckTarget(skill) &&
                        player.Skills.CastCheckDistance(skill, out Vector3 destination) &&
                        player.Skills.CastCheckFOV(skill))
                    {
                        // start casting and cancel movement in any case
                        // (player might move into attack range * 0.8 but as soon as we
                        //  are close enough to cast, we fully commit to the cast.)
                        player.Movement.Reset();
                        player.Skills.StartCast(skill);
                        return "CASTING";
                    }
                    else
                    {
                        // checks failed. reset the attempted current skill
                        player.Skills.currentSkill = -1;
                        player.nextTarget = null; // nevermind, clear again (otherwise it's shown in UITarget)
                        return "IDLE";
                    }
                }
            }
            if (EventSkillFinished(player)) { } // don't care
            if (EventMoveEnd(player)) { } // don't care
            if (EventTargetDied(player)) { } // don't care
            if (EventTargetDisappeared(player)) { } // don't care

            return "IDLE"; // nothing interesting happened
        }

        string UpdateServer_MOVING(Player player)
        {
            // events sorted by priority (e.g. target doesn't matter if we died)
            if (EventDied(player))
            {
                // we died.
                return "DEAD";
            }
            if (EventStunned(player))
            {
                player.Movement.Reset();
                return "STUNNED";
            }
            if (EventMoveEnd(player))
            {
                // finished moving. do whatever we did before.
                return "IDLE";
            }
            if (EventCancelAction(player))
            {
                // if we're casting, cancel casting (if possible) and stop moving
                if (player.Skills.currentSkill != -1)
                {
                    if (player.Skills.CanCancelCurrentCast())
                    {
                        player.Skills.CancelCast();
                        return "IDLE";
                    }
                    else
                    {
                        return "CASTING";
                    }

                }
                //  if we're not casting, remove current target
                else if (player.Skills.currentSkill == -1 &&
                         player.Target != null)
                {
                    player.SetTarget(null);

                    return "IDLE";
                }

                //player.Movement.Reset(); <- done locally. doing it here would reset localplayer to the slightly behind server position otherwise
                //return "CASTING";
            }
            // SPECIAL CASE: Skill Request while doing rubberband movement
            // -> we don't really need to react to it
            // -> we could just wait for move to end, then react to request in IDLE
            // -> BUT player position on server always lags behind in rubberband movement
            // -> SO there would be a noticeable delay before we start to cast
            //
            // SOLUTION:
            // -> start casting as soon as we are in range
            // -> BUT don't ResetMovement. instead let it slide to the final position
            //    while already starting to cast
            // -> NavMeshAgentRubberbanding won't accept new positions while casting
            //    anyway, so this is fine
            if (EventSkillRequest(player))
            {
                // don't cast while mounted or in a different movement state
                // (no MOUNTED state because we'd need MOUNTED_STUNNED, etc. too)
                if (CanCancelSkillInCurrentState(player))
                {
                    Skill skill = player.Skills.skills[player.Skills.currentSkill];
                    if (player.Skills.CastCheckSelf(skill) &&
                        player.Skills.CastCheckTarget(skill) &&
                        player.Skills.CastCheckDistance(skill, out Vector3 destination) &&
                        player.Skills.CastCheckFOV(skill))
                    {
                        //Debug.Log("MOVING->EventSkillRequest: early cast started while sliding to destination...");
                        // player.rubberbanding.ResetMovement(); <- DO NOT DO THIS.
                        player.Skills.StartCast(skill);
                        return "CASTING";
                    }
                }
            }
            if (EventMoveStart(player)) { } // don't care
            if (EventSkillFinished(player)) { } // don't care
            if (EventTargetDied(player)) { } // don't care
            if (EventTargetDisappeared(player)) { } // don't care

            return "MOVING"; // nothing interesting happened
        }

        void UseNextTargetIfAny(Player player)
        {
            // use next target if the user tried to target another while casting
            // (target is locked while casting so skill isn't applied to an invalid
            //  target accidentally)
            if (player.nextTarget != null)
            {
                player.SetTarget(player.nextTarget);
                player.nextTarget = null;
            }
        }

        string UpdateServer_CASTING(Player player)
        {
            // keep looking at the target for server & clients (only Y rotation)
            if (player.Target && player.Movement.DoCombatLookAt())
                player.Movement.LookAtY(player.Target.transform.position);

            // events sorted by priority (e.g. target doesn't matter if we died)
            //
            // IMPORTANT: nextTarget might have been set while casting, so make sure
            // to handle it in any case here. it should definitely be null again
            // after casting was finished.
            // => this way we can reliably display nextTarget on the client if it's
            //    != null, so that UITarget always shows nextTarget>target
            //    (this just feels better)
            if (EventDied(player))
            {
                // we died.
                UseNextTargetIfAny(player); // if user selected a new target while casting
                return "DEAD";
            }
            if (EventStunned(player))
            {
                // cancel cast & movement
                // (only clear current skill if we don't continue cast after stunned)
                player.Skills.CancelCast(!continueCastAfterStunned);
                player.Movement.Reset();
                return "STUNNED";
            }
            if (EventMoveStart(player))
            {
                // we do NOT cancel the cast if the player moved, and here is why:
                // * local player might move into cast range and then try to cast.
                // * server then receives the Cmd, goes to CASTING state, then
                //   receives one of the last movement updates from the local player
                //   which would cause EventMoveStart and cancel the cast.
                // * this is the price for rubberband movement.
                // => if the player wants to cast and got close enough, then we have
                //    to fully commit to it. there is no more way out except via
                //    cancel action. any movement in here is to be rejected.
                //    (many popular MMOs have the same behaviour too)
                //

                // we do NOT reset movement either. allow sliding to final position.
                // (NavMeshAgentRubberbanding doesn't accept new ones while CASTING)
                //player.Movement.Reset(); <- DO NOT DO THIS

                // we do NOT return "CASTING". EventMoveStart would constantly fire
                // while moving for skills that allow movement. hence we would
                // always return "CASTING" here and never get to the castfinished
                // code below.
                //return "CASTING";
            }
            if (EventCancelAction(player))
            {
                // cancel casting if possible
                if (player.Skills.CanCancelCurrentCast())
                {
                    player.Skills.CancelCast();

                    UseNextTargetIfAny(player); // if user selected a new target while casting

                    return "IDLE";
                }
            }
            if (EventTargetDisappeared(player))
            {
                // cancel if the target matters for this skill
                if (player.Skills.currentSkill != -1 &&
                    player.Skills.skills[player.Skills.currentSkill].cancelCastIfTargetDied)
                {
                    player.Skills.CancelCast();
                    UseNextTargetIfAny(player); // if user selected a new target while casting
                    return "IDLE";
                }
            }
            if (EventTargetDied(player))
            {
                // cancel if the target matters for this skill
                if (player.Skills.skills[player.Skills.currentSkill].cancelCastIfTargetDied)
                {
                    player.Skills.CancelCast();
                    UseNextTargetIfAny(player); // if user selected a new target while casting
                    return "IDLE";
                }
            }
            if (EventSkillFinished(player))
            {
                // apply the skill after casting is finished
                // note: we don't check the distance again. it's more fun if players
                //       still cast the skill if the target ran a few steps away
                Skill skill = player.Skills.skills[player.Skills.currentSkill];

                // apply the skill on the target
                player.Skills.FinishCast(skill);

                // clear current skill for now
                player.Skills.currentSkill = -1;

                // use next target if the user tried to target another while casting
                UseNextTargetIfAny(player);

                // go back to IDLE
                return "IDLE";
            }
            if (EventMoveEnd(player)) { } // don't care
            if (EventSkillRequest(player)) { } // don't care

            return "CASTING"; // nothing interesting happened
        }

        string UpdateServer_STUNNED(Player player)
        {
            // events sorted by priority (e.g. target doesn't matter if we died)
            if (EventDied(player))
            {
                // we died.
                return "DEAD";
            }
            if (EventCancelAction(player))
            {
                if (player.Target != null)
                {
                    player.SetTarget(null);
                }
                return "STUNNED";
            }
            if (EventStunned(player))
            {
                return "STUNNED";
            }

            // go back to idle if we aren't stunned anymore and process all new
            // events there too
            return "IDLE";
        }

        string UpdateServer_DEAD(Player player)
        {
            if (EventMoveStart(player))
            {
                // if a player gets killed while sliding down a slope or while in
                // the air then he might continue to move after dead. it's fine as
                // long as we don't allow client input to control the move.
                return "DEAD";
            }
            if (EventMoveEnd(player)) { } // don't care
            if (EventSkillFinished(player)) { } // don't care
            if (EventDied(player)) { } // don't care
            if (EventCancelAction(player)) { } // don't care
            if (EventTargetDisappeared(player)) { } // don't care
            if (EventTargetDied(player)) { } // don't care
            if (EventSkillRequest(player)) { } // don't care

            return "DEAD"; // nothing interesting happened
        }

        public override string UpdateBrain(Entity entity)
        {
            Player player = (Player)entity;

            string stateResult = string.Empty;
            if (player.State == "IDLE") stateResult = UpdateServer_IDLE(player);
            else if (player.State == "MOVING") stateResult = UpdateServer_MOVING(player);
            else if (player.State == "CASTING") stateResult = UpdateServer_CASTING(player);
            else if (player.State == "STUNNED") stateResult = UpdateServer_STUNNED(player);
            else if (player.State == "DEAD") stateResult = UpdateServer_DEAD(player);
            else Debug.LogError("invalid state:" + player.State);

            UpdateClient(entity);
           
            return stateResult;
        }

        public void UpdateClient(Entity entity)
        {
            Player player = (Player)entity;

            if (player.State == "IDLE" || player.State == "MOVING")
            {
                // cancel action if escape key was pressed
                if (Input.GetKeyDown(player.cancelActionKey))
                {
                    player.Movement.Reset(); // reset locally because we use rubberband movement
                    player.CmdCancelAction();
                }
            }
            else if (player.State == "CASTING")
            {
                // keep looking at the target for server & clients (only Y rotation)
                if (player.Target && player.Movement.DoCombatLookAt())
                {
                    player.Movement.LookAtY(player.Target.transform.position);
                }

                // reset any movement
                player.Movement.Reset();

                // cancel action if escape key was pressed
                if (Input.GetKeyDown(player.cancelActionKey)) player.CmdCancelAction();
            }
            else if (player.State == "STUNNED")
            {
                // reset any movement
                player.Movement.Reset();

                // cancel action if escape key was pressed
                if (Input.GetKeyDown(player.cancelActionKey)) player.CmdCancelAction();
            }
            else if (player.State == "DEAD") { }
            else Debug.LogError("invalid state:" + player.State);
        }
    }
}