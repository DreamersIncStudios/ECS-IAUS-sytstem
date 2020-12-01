
namespace Stats
{
    public enum EffectStatus 
    {
        None,Poison, Blind, Silenced, Berzerk, Rage, Charged, Burn, Shock, Paralyzed, Confused,
        Slow, Cursed, Zombie, ALL
    }

    public struct StatusEffect
    {
        public EffectStatus Status;
        public int value;
        public uint Iterations;
        public float Frequency;
        public float Timer;

    }
}
