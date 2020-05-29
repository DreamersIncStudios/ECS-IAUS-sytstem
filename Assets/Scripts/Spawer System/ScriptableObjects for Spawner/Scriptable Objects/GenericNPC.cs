using UnityEngine;
using UnityEngine.UIElements;

namespace SpawnerSystem.ScriptableObjects
{
    public class GenericNPC : ICharacterBase,IObject
    {
        [SerializeField] string _name;
        [SerializeField] Gender _gender;
        [SerializeField] GameObject _GO;
        [SerializeField] Vector3 _scale;

        public Color BaseColor;
        public string Name { get { return _name; } }

        public Gender gender { get { return _gender; } }

        public GameObject GO { get { return _GO; } }
        public Vector3 Scale { get { return _scale; } set { _scale = value; } }
    
        public GenericNPC(Gender gender)
        {
            _gender = gender;
            switch (gender) {
                case Gender.Female:
                _name = ((NPCNamesFemale)Random.Range(0, System.Enum.GetNames(typeof(NPCNamesFemale)).Length)).ToString();
                    break;
                case Gender.Male:
                    _name = ((NPCNamesMale)Random.Range(0, System.Enum.GetNames(typeof(NPCNamesMale)).Length)).ToString();
                    break;
                case Gender.Androgynous:
                    _name = ((NPCNamesMixed)Random.Range(0, System.Enum.GetNames(typeof(NPCNamesMixed)).Length)).ToString();
                    break;
            }
            BaseColor = new Color(Random.value, Random.value, Random.value);
            _GO = Resources.Load<GameObject>("NPC/BaseNPC");

        }

        public static GameObject SpawnGO(Gender gender, Vector3 position) 
        {
            GenericNPC test = new GenericNPC(gender);
            GameObject GOtest = Object.Instantiate(test.GO, position + new Vector3(0, 0, 0), Quaternion.identity);
            GOtest.GetComponent<Renderer>().material.color = test.BaseColor;
            GOtest.GetComponent<CharacterStats>().Name = test.Name;

            return GOtest;
        }

    }

}