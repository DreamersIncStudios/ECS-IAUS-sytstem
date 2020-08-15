namespace Stats
{
    [System.Serializable]
    public class MagicClass : BaseStat
    {
        public MagicClass() {
            ExpToLevel = 1000;
            BaseValue = 1;
            LevelModifier = 1.2f;
        }
    
    }
    public enum MagicClasses {
        Acceleration,
        Convergence,
        Dispersion,
        Absorption,
        Release,
        Movement,
        Oscillation,
        Weight,
        Sensory,
        Spirit,
        Ancient,
        }
}