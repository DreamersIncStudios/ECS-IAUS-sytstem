using UnityEngine;
using UnityEngine.UI;
using Dreamers.Global;
using Stats;
using Dreamers.InventorySystem.Base;

namespace Dreamers.InventorySystem.UISystem
{
    public class DisplayStore
    {
        readonly UIManager Manager;
        public bool Displayed { get { return (bool)MenuPanelParent; } }
        bool Buying=true;
        GameObject MenuPanelParent { get; set; }
        private InventoryBase Inventory;
        private StoreBase Store;
        public DisplayStore(StoreBase storeBase, InventoryBase inventoryBase) {
            Manager = UIManager.instance;
            Store = storeBase;
            Inventory = inventoryBase;
            MenuPanelParent = CreateStoreUI(new Vector2(0, 0),
                new Vector2(0, 0));
        }


        GameObject CreateStoreUI(Vector2 Size, Vector2 Position)
        {
            if (MenuPanelParent)
                Object.Destroy(MenuPanelParent);

            GameObject Parent = Manager.UICanvas();
            GameObject MainPanel = Manager.Panel(Parent.transform, Size, Position);
            MainPanel.name = Store.StoreName;
            RectTransform PanelRect = MainPanel.GetComponent<RectTransform>();
            PanelRect.pivot = new Vector2(0.5f, .5f);
            PanelRect.anchorMax = new Vector2(1, 1);
            PanelRect.anchorMin = new Vector2(.0f, .0f);

            VerticalLayoutGroup VLG = MainPanel.AddComponent<VerticalLayoutGroup>();
            VLG.padding = new RectOffset() { bottom = 20, top = 20, left = 20, right = 20 };
            VLG.childAlignment = TextAnchor.UpperCenter;
            VLG.childControlHeight = true; VLG.childControlWidth = false;
            VLG.childForceExpandHeight = false; VLG.childForceExpandWidth = true;

            Text titleGO = Manager.TextBox(MainPanel.transform, new Vector2(400, 50)).GetComponent<Text>();
            titleGO.alignment = TextAnchor.MiddleCenter;
            titleGO.text = Store.StoreName;
            titleGO.fontSize = 24;
            HorizontalLayoutGroup BuySell = Manager.Panel(MainPanel.transform, new Vector2(1920, 60), Position).AddComponent<HorizontalLayoutGroup>();
            BuySell.name = "BuySell Header";
            BuySell.childControlHeight = false;
            BuySell.childForceExpandHeight = false;

            Manager.UIButton(BuySell.transform, "Buy Items")
                .onClick.AddListener(() =>
                {
                    Buying = true;
                      ItemPanel = DisplayItems(ItemType.None, MainPanel.transform);
                });

            Manager.UIButton(BuySell.transform, "Sell Items")
                .onClick.AddListener(() =>
                {
                    Buying = false;

                      ItemPanel = DisplayItems(ItemType.None, MainPanel.transform);
                });
            #region header
            HorizontalLayoutGroup ButtonHeader = Manager.Panel(MainPanel.transform, new Vector2(1920, 60), Position).AddComponent<HorizontalLayoutGroup>();
            ButtonHeader.name = "Button Header";
            ButtonHeader.childControlHeight = false;
            ButtonHeader.childForceExpandHeight = false;
            Manager.UIButton(ButtonHeader.transform, "All")
                .onClick.AddListener(() =>
                {
                    ItemPanel = DisplayItems(ItemType.None, MainPanel.transform);
                });
            

            for (int i = 1; i < 6; i++)
            {
                int test = i;
                Button Temp = Manager.UIButton(ButtonHeader.transform, ((ItemType)i).ToString());
                Temp.onClick.AddListener(() =>
                {
                    ItemPanel = DisplayItems((ItemType)test, MainPanel.transform);
                });
                ;
                Temp.name = ((ItemType)i).ToString();

            }
            #endregion

            ItemPanel = DisplayItems(ItemType.None, MainPanel.transform);
            return MainPanel;
        }

        public void CloseStore() { 
             Object.Destroy(MenuPanelParent);

        }
        GameObject ItemPanel { get; set; }
        GameObject DisplayItems(ItemType Filter, Transform Parent) 
        {
            if (ItemPanel)
                Object.Destroy(ItemPanel);

           GridLayoutGroup basePanel = Manager.Panel(Parent.transform, new Vector2(1920,0), new Vector2(0, 0))
                .AddComponent<GridLayoutGroup>();
            basePanel.name = "Items Display";
            basePanel.padding = new RectOffset() { bottom = 20, top = 20, left = 20, right = 20 };
            basePanel.spacing = new Vector2(20, 20);
            InventoryBase inventory;
            if (Buying)
            {
                inventory = Store.StoreInventory;
            }
            else {
                inventory = Inventory;
            }

            for (int i = 0; i < inventory.ItemsInInventory.Count - 1; i++)
            {
                ItemSlot Slot = inventory.ItemsInInventory[i];
                int IndexOf = i;
                if (Slot.Item.Type == Filter)
                {
                    Button temp = ItemButton(basePanel.transform, Slot);
                    temp.onClick.AddListener(() =>
                    {
                      
                            GameObject pop = PopUpItemPanel(temp.GetComponent<RectTransform>().anchoredPosition
                                 + new Vector2(575, -175)
                                 , Slot, IndexOf);
                            pop.AddComponent<PopUpMouseControl>();
                        
                    });
                }
                if (Filter == ItemType.None) 
                {
                    Button temp = ItemButton(basePanel.transform, Slot);
                    temp.onClick.AddListener(() =>
                    {
                       
                        GameObject pop = PopUpItemPanel(temp.GetComponent<RectTransform>().anchoredPosition
                             + new Vector2(100, -175)
                             , Slot, IndexOf);
                    
                    });
                }


            }
                return basePanel.gameObject;
        }

            Button ItemButton(Transform Parent, ItemSlot Slot)
            {
                Button temp = Manager.UIButton(Parent, Slot.Item.ItemName);
                temp.name = Slot.Item.ItemName;
                Text texttemp = temp.GetComponentInChildren<Text>();
                texttemp.alignment = TextAnchor.LowerCenter;
                if (Slot.Item.Stackable)
                    texttemp.text += Slot.Count;
                temp.GetComponentInChildren<Text>().alignment = TextAnchor.LowerCenter;
                temp.GetComponent<Image>().sprite = Slot.Item.Icon;


                return temp;
            }
        GameObject PopUpItemPanel(Vector2 Pos, ItemSlot Slot, int IndexOf)
        {
            GameObject PopUp = Manager.Panel(Manager.UICanvas().transform, new Vector2(400, 400), Pos);
            Image temp = PopUp.GetComponent<Image>();
            Color color = temp.color; color.a = 1.0f;
            temp.color = color;

            HorizontalLayoutGroup group = PopUp.AddComponent<HorizontalLayoutGroup>();
            PopUp.AddComponent<PopUpMouseControl>();

            group.childControlWidth = false;

            Text info = Manager.TextBox(PopUp.transform, new Vector2(250, 300));
            info.text = Slot.Item.ItemName + "\n";
            info.text += Slot.Item.Description + "\n";
            info.fontSize = 20;

            info.text += "Cost: " + Mathf.RoundToInt(Slot.Item.Value * Store.Sell) + " gil";

            VerticalLayoutGroup ButtonPanel = Manager.Panel(PopUp.transform, new Vector2(150, 300), Pos).AddComponent<VerticalLayoutGroup>();
            if (Buying)
            {
                switch (Slot.Item.Type)
                {
                    case ItemType.General:
                    case ItemType.Crafting_Materials:

                        Button Buy1 = Manager.UIButton(ButtonPanel.transform, "Buy 1 ");
                        Button Buy5 = Manager.UIButton(ButtonPanel.transform, "Buy 5 ");
                        Buy1.onClick.AddListener(() =>
                        {
                            Store.BuyItemFrom(Slot);
                            Object.Destroy(PopUp);

                        });
                        Buy5.onClick.AddListener(() =>
                        {
                            Store.BuyXItemsFrom(Slot, 5);

                            Object.Destroy(PopUp);

                        });
                        break;
                    case ItemType.Armor:
                    case ItemType.Weapon:
                    case ItemType.Blueprint_Recipes:
                        Button Buy = Manager.UIButton(ButtonPanel.transform, "Buy");
                        Buy.onClick.AddListener(() =>
                        {
                            Store.BuyItemFrom(Slot);
                            Object.Destroy(PopUp);
                        });

                        break;
                }
            }
            else
            {
                switch (Slot.Item.Type)
                {
                    case ItemType.General:
                    case ItemType.Crafting_Materials:

                        Button Sell1 = Manager.UIButton(ButtonPanel.transform, "Sell 1 ");
                        Button Sell5 = Manager.UIButton(ButtonPanel.transform, "Sell 5 ");
                        Sell1.onClick.AddListener(() =>
                        {
                            Store.SellItemTo(Slot,IndexOf);
                            Object.Destroy(PopUp);

                        });
                        Sell5.onClick.AddListener(() =>
                        {
                            Store.SellxItemsTo(Slot,IndexOf,5);
                            ItemPanel = DisplayItems(ItemType.None, MenuPanelParent.transform);

                            Object.Destroy(PopUp);

                        });
                        break;
                    case ItemType.Armor:
                    case ItemType.Weapon:
                    case ItemType.Blueprint_Recipes:
                        Button Sell= Manager.UIButton(ButtonPanel.transform, "sell");
                       Sell.onClick.AddListener(() =>
                        {
                            Store.SellItemTo(Slot,IndexOf);
                            ItemPanel = DisplayItems(ItemType.None, MenuPanelParent.transform);
                            Object.Destroy(PopUp);
                        });

                        break;
                }
            }
            return PopUp;
        }
    }

}
