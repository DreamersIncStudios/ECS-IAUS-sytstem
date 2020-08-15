namespace SpawnerSystem.ScriptableObjects
{
    public interface ICharacterStat
    {

        uint Level { get;  }
        int BaseHealth { get; }
        int BaseMana { get; }

    }

    public interface ICharacterBase {
        string Name { get; }
        Gender gender { get; }
    }

    public enum Gender{ Female, Male, Androgynous }
}