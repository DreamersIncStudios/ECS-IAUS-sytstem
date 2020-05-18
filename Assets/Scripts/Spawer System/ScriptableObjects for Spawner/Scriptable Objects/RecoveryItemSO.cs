using UnityEngine;
using System.Collections;

namespace SpawnerSystem.ScriptableObjects
{
    public class RecoveryItemSO : Droppable, iRecoverable
    {
        [SerializeField] RecoverType Type;
        [SerializeField] int recoveryAmount;


        public RecoverType recoverType { get { return Type; } }
    
        public int RecoverAmount { get { return recoveryAmount; } }

        public override void Spawn(Vector3 Pos) {
            GameObject temp = Instantiate(GO,Pos,Quaternion.identity);
            PickupRecoverItem PRI = temp.GetComponent<PickupRecoverItem>();
            PRI.RecoverWhat = recoverType;
            PRI.Amount = RecoverAmount;
        }

    }

}