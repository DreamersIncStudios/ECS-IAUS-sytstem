using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Generic Player Character Class
 * Will be using BurgZergArcade Derived Player Character System  that is already in main project file
 */
namespace Stats
{
    public class PlayerCharacter : MonoBehaviour
    {
        [Range(0, 999)]
        public int CurHealth;
        [Range(0, 999)]
        public int CurMana;
        [Range(0, 999)]
        public int MaxHealth;
        [Range(0, 999)]
        public int MaxMana;


        private void Start()
        {
            CurHealth = MaxHealth;
            CurMana = MaxMana;
        }
    }
}