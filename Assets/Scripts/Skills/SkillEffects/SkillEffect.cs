// Base component for skill effects.
using UnityEngine;

namespace GameJam
{
    public abstract class SkillEffect : MonoBehaviour
    {
        [HideInInspector] public Entity target;
        [HideInInspector] public Entity caster;
    }
}