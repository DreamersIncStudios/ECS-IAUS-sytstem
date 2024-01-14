using System.Dynamic;
using System.Threading.Tasks;
using Stats;
using Stats.Entities;
using UnityEngine;

namespace Dreamers.InventorySystem
{
    public interface IItemAction
    {
        public uint Amount { get;  }
        void Use(BaseCharacterComponent character);

    }
    [System.Serializable]
    public class HealthItem : IItemAction
    {
        public uint Amount => amount;
        [SerializeField] private uint amount;
        public RecoveryType Type => RecoveryType.Health;

        public void Use(BaseCharacterComponent character)
        {
            character.AdjustHealth((int)Amount);
        }
    }
    [System.Serializable]
    public class ManaItem : IItemAction
    {
        public uint Amount => amount;
        [SerializeField] private uint amount;
        public RecoveryType Type => RecoveryType.Mana;

        public void Use(BaseCharacterComponent character)
        {
            character.AdjustMana((int)Amount);
        }
    }

    public class StatBoost : IItemAction
    {
        public uint Amount => amount;
        [SerializeField] private uint amount;
        public AttributeName Attribute;
        [Tooltip("Delay in secs")]
        [Range(-1,999)]
        public int Duration;
        public async void Use(BaseCharacterComponent character)
        {
         // Increase stat Mod value
         await Task.Delay(Duration * 1000);
         // Decrease mod value

        }

    }

    public enum RecoveryType
    {
        Health, Mana, Durability, Status, Other
    }
}