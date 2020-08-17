using Dreamers.InventorySystem;
using Dreamers.InventorySystem.Base;
using Dreamers.InventorySystem.UISystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vender : MonoBehaviour
{
    public StoreBase Base;
    DisplayStore Store;
    public GameObject player;

    private void Start()
    {
        Base.CharacterInventory= player.GetComponent<CharacterInventory>();
        Store = new DisplayStore(Base, player.GetComponent<CharacterInventory>().Inventory);

    }
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.V) && Store.Displayed) { Store.CloseStore(); }
        if (Input.GetKeyUp(KeyCode.V) && !Store.Displayed) {
            Base.CharacterInventory = player.GetComponent<CharacterInventory>();
            Store = new DisplayStore(Base, player.GetComponent<CharacterInventory>().Inventory); }

    }


}
