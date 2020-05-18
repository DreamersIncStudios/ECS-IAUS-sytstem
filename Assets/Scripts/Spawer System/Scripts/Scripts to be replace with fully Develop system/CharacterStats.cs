using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public string Name;
    [Range(0,2000)]
    public int Health;
    [Range(0, 2000)]
    public int Mana;

    public bool dead { get { return Health == 0; } }

    // Start is called before the first frame update
    private void Awake()
    {
        Health = 150;
        Mana = 150;
    }

    public void AdjustHP(int HP) {
   
            Health += HP;
        if (Health < 0)
            Health = 0;
        if (Health > 2000)
            Health = 2000;
    }

    public void AdjustMana(int HP)
    {

        Mana += HP;
        if (Mana< 0)
            Mana = 0;
        if (Mana > 2000)
            Mana = 2000;
    }
}
