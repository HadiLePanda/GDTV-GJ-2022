namespace GameJam
{
    public class HealthBar : EnergyBar
    {
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