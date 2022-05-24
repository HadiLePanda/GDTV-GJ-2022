// Base component for skill effects.
using UnityEngine;

namespace GameJam
{
    public abstract class SkillEffect : MonoBehaviour
    {
        [ReadOnlyInspector] public Entity target;
        [ReadOnlyInspector] public Entity caster;
    }
}