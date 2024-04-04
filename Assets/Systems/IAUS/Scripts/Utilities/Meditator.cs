using Unity.Entities;
using UnityEngine;

namespace IAUS.Core.Utilities
{
    
    public interface IMeditator
    {
        public Entity Sender{get; set; }
    }
}