using UnityEngine;

namespace GameJam
{
    public class HealthBar : EnergyBar
    {
        static Color enemyColor = Color.red;
        static Color allyColor = Color.green;

        private void Start()
        {
            Player player = Player.localPlayer;

            if (entity != null)
            {
                Color desiredColor = player.IsFactionAlly(entity) ? allyColor : enemyColor;

                fill.color = desiredColor;
            }
        }

        protected override float GetFillPercentage()
        {
            return GetTarget().Health.Percent();
        }

        protected override int GetCurrentEnergy()
        {
            return GetTarget().Health.Current;
        }
    }
}