using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Stats
{
    
    [System.Serializable]
    public class NPCCharacterClass: ICharacterData
    {
        public ClassTitle Title=> title;
  
        public int Level => UnityEngine.Random.Range(levelRange.x, levelRange.y);
        public int Strength => strength;
        public int Vitality=> vitality;
        public int Awareness=> awareness;
        public int Speed => speed;
        public int Skill=> skill;
        public int Resistance=> resistance;
        public int Concentration=> concentration;
        public int WillPower=> willPower;
        public int Charisma=> charisma;
        public int Luck=> luck;

        public float DifficultyMod { get => difficultyMod; set => difficultyMod = value;}
        public float LevelMod { get => levelMod; set => levelMod = value;}

        [SerializeField]private ClassTitle title;
     
        [SerializeField]private int2 levelRange;
        [SerializeField]private int strength;
        [SerializeField]private int vitality;
        [SerializeField]private int awareness;
        [SerializeField]private int speed;
        [SerializeField]private int skill;
        [SerializeField]private int resistance;
        [SerializeField]private int concentration;
        [SerializeField]private int willPower;
        [SerializeField]private int charisma;
        [SerializeField]private int luck;

        [SerializeField]private float difficultyMod;
        [SerializeField]private float levelMod;
    }
}