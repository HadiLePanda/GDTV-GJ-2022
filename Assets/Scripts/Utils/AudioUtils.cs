using System.Collections.Generic;
using UnityEngine;

namespace GameJam
{
    public static partial class Utils
    {
        public static AudioClip GetRandomClip(IList<AudioClip> clips)
        {
            int randomClipIndex = Random.Range(0, clips.Count);
            AudioClip randomClip = clips[randomClipIndex];

            return randomClip;
        }
    }
}