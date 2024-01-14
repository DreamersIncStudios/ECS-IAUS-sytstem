using System.Collections;
using System.Collections.Generic;
using Dreamers.InventorySystem;
using Dreamers.InventorySystem.Interfaces;
using UnityEngine;

using Sirenix.OdinInspector;
using Stats;
using Stats.Entities;

public class QuestSO : ItemBaseSO
{
    public new ItemType Type => ItemType.Quest;
    [SerializeField] Quality quality;
    public Quality Quality { get { return quality; } }
    public string Lore { get { return _lore; } }
    bool showLore => quality == Quality.Lengendary || quality == Quality.Exotic;
    [TextArea(3, 6)]
    [ShowIf(nameof(showLore))][SerializeField] private string _lore;
    [SerializeField] GameObject _model;
    public GameObject Model { get { return _model; } }
    public bool Collectable;
    [SerializeField] bool _questItem => true;

}
