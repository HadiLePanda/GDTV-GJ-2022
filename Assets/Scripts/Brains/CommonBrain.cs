// some common events shared amongst all our brain implementations.
// -> it's not necessary to inherit from CommonBrain for your custom brains.
//    this is just to save redundancies.
using UnityEngine;

namespace GameJam
{
    public abstract class CommonBrain : ScriptableBrain
    {
        public bool EventAggro(Entity entity) =>
            entity.Target != null && entity.Target.IsAlive;

        public bool EventDied(Entity entity) =>
            !entity.IsAlive;

        // only fire when stopped moving
        public bool EventMoveEnd(Entity entity) =>
            entity.State == "MOVING" && !entity.Movement.IsMoving();

        // only fire when started moving
        public bool EventMoveStart(Entity entity) =>
            entity.State != "MOVING" && entity.Movement.IsMoving();

        public bool EventSkillFinished(Entity entity) =>
            0 <= entity.Skills.currentSkill && entity.Skills.currentSkill < entity.Skills.skills.Count &&
            entity.Skills.skills[entity.Skills.currentSkill].CastTimeRemaining() <= 0;

        public bool EventSkillRequest(Entity entity) =>
            0 <= entity.Skills.currentSkill && entity.Skills.currentSkill < entity.Skills.skills.Count;

        public bool EventStunned(Entity entity) =>
            Time.time <= entity.stunTimeEnd;

        public bool EventTargetDied(Entity entity) =>
            entity.Target != null && !entity.Target.IsAlive;

        public bool EventTargetDisappeared(Entity entity) =>
            entity.Target == null;

        public bool EventTargetTooFarToAttack(Entity entity) =>
            entity.Target != null &&
            0 <= entity.Skills.currentSkill && entity.Skills.currentSkill < entity.Skills.skills.Count &&
            !entity.Skills.CastCheckDistance(entity.Skills.skills[entity.Skills.currentSkill], out Vector3 destination);

        public bool EventTargetNotInFOV(Entity entity) =>
            entity.Target != null &&
            !entity.Skills.CastCheckFOV(entity.Skills.skills[entity.Skills.currentSkill]);
    }
}