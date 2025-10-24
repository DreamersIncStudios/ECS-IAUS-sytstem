namespace Stats
{
    public interface ICharacterData{
        public ClassTitle Title { get; }
        public int Level{ get; }
        public int Strength{ get; }
        public int Vitality{ get; }
        public int Awareness{ get; }
        public int Speed{ get; }
        public int Skill{ get; }
        public int Resistance{ get; }
        public int Concentration{ get; }
        public int WillPower{ get; }
        public int Charisma{ get; }
        public int Luck{ get; }

        //todo review if setters are needed
        public float DifficultyMod{ get; set; }
        public float LevelMod{ get; set; }
    }
}