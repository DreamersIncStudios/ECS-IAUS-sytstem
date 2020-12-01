using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamers.InventorySystem;
using Unity.Entities;
using Stats;
using Dreamers.InventorySystem.Base;
using Dreamers.InventorySystem.UISystem;

public class Testadder : MonoBehaviour,IConvertGameObjectToEntity
{
    public uint ChangeValue;
    public List<ItemBaseSO> addering;
   private InventoryBase Inventory;
    private EquipmentBase Equip;
    private PlayerCharacter PC;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new SubtractWalletValue() { Change = ChangeValue });

    }

    // Start is called before the first frame update
    DisplayMenu Menu;
    void Start()
    {
        PC = this.GetComponent<PlayerCharacter>();
        Inventory = this.GetComponent<CharacterInventory>().Inventory;
        Equip = this.GetComponent<CharacterInventory>().Equipment;
     SetupPC();

        addering[0].AddToInventory(Inventory);
        addering[0].AddToInventory(Inventory);
        addering[0].AddToInventory(Inventory);
        addering[1].AddToInventory(Inventory);
        addering[1].AddToInventory(Inventory);
        addering[1].AddToInventory(Inventory);
        addering[2].AddToInventory(Inventory);
        addering[2].AddToInventory(Inventory);
        addering[2].AddToInventory(Inventory);
        addering[2].AddToInventory(Inventory);

        Inventory.ItemsInInventory[0].Item.Use(Inventory, 0, PC);
       Menu = new  DisplayMenu(PC, Equip,Inventory);
        Menu.CloseInventory();
    }
    void SetupPC() {
        PC.Name = "Test";
        PC.Level = 5;
        PC.GetPrimaryAttribute((int)AttributeName.Level).BaseValue = 5;
        PC.GetPrimaryAttribute((int)AttributeName.Vitality).BaseValue = 50;
        PC.GetPrimaryAttribute((int)AttributeName.WillPower).BaseValue = 11;
        PC.GetPrimaryAttribute((int)AttributeName.Concentration).BaseValue = 10;
        PC.GetPrimaryAttribute((int)AttributeName.Strength).BaseValue = 14;
        PC.GetPrimaryAttribute((int)AttributeName.Charisma).BaseValue = 13;
        PC.GetPrimaryAttribute((int)AttributeName.Luck).BaseValue = 12;
        PC.GetPrimaryAttribute((int)AttributeName.Awareness).BaseValue = 12;
        PC.GetPrimaryAttribute((int)AttributeName.Speed).BaseValue = 17;
        PC.GetPrimaryAttribute((int)AttributeName.Skill).BaseValue = 20;
        PC.GetPrimaryAttribute((int)AttributeName.Resistance).BaseValue = 12;
        PC.GetVital((int)VitalName.Health).BaseValue = 360;
        PC.GetVital((int)VitalName.Mana).BaseValue = 40;
        PC.SetupStatsModifiers(); 
        PC.StatUpdate();

    }
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.I) && Menu.Displayed) { Menu.CloseInventory(); }
        if (Input.GetKeyUp(KeyCode.I) && !Menu.Displayed) { Menu = new DisplayMenu(PC, Equip, Inventory); }
    }
}
