using UnityEngine;

namespace GameJam
{
    public partial class Utils
    {
        public static bool AnimatorParameterExists(string paramName, Animator anim)
        {
            if (anim.parameterCount == 0)
                return false;

            bool exists = false;

            AnimatorControllerParameter[] animParams = anim.parameters;
            for (int i = 0; i < animParams.Length; i++)
            {
                if (animParams[i].name == paramName)
                    exists = true;
            }

            return exists;
        }
    }
}