using UnityEngine;
using UnityEngine.Rendering;

namespace GameJam
{
    public class PostProcessingController : MonoBehaviour
    {
        public Volume healthVolume;

        private void Update()
        {
            Player player = Player.localPlayer;
            if (player != null)
            {
                healthVolume.weight = player.Health.Current < 0.5f ? 1 - player.Health.Percent() : 0f;
            }
            else
            {
                healthVolume.weight = 0f;
            }
        }
    }
}