namespace SpawnerSystem.ScriptableObjects
{
    public interface ICharacterStat
    {
        CharacterStats Stats { get; }
    }

[System.Serializable]
    public struct CharacterStats {
    public int level, Str, vit, Awr, Spd, Skl, Res, Con, Will, Chars, Lck;
        public int BaseHealth, BaseMana;
    }

    public interface ICharacterBase {
        string Name { get; }
        Gender gender { get; }
    }

    public enum Gender{ Female, Male, Androgynous }
}