using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupRecoverItem : MonoBehaviour
{
    public RecoverType RecoverWhat;
    public int Amount;

    public void Awake()
    {
        this.GetComponent<SphereCollider>().isTrigger = true;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            CharacterStats PC = other.gameObject.GetComponent<CharacterStats>();
            switch (RecoverWhat) {
                case RecoverType.health:
                    PC.AdjustHP(Amount);
                    Destroy(gameObject);
                    break;

                case RecoverType.mana:
                    PC.AdjustMana(Amount);
                    Destroy(gameObject);
                    break;
            }
        }
    }
}

public enum RecoverType {
    health,
    mana

}
