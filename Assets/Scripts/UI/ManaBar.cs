namespace GameJam
{
    public class ManaBar : EnergyBar
    {
        protected override float GetFillPercentage()
        {
            return GetTarget().Mana.Percent();
        }

        protected override int GetCurrentEnergy()
        {
            return GetTarget().Mana.Current;
        }
    }
}