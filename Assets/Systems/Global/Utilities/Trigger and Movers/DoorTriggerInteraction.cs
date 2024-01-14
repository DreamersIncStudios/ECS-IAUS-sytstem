using UnityEngine;
namespace LS
{
    public class DoorTriggerInteraction : MonoBehaviour
    {
        [SerializeField]
        Door door;
        
        public void OnEvent(AdvancedTriggerArgs args)
        {
            switch (args.Event)
            {
                case EventType.EnterA:
                    door.OpenIn();
                    break;
                case EventType.EnterB:
                    door.OpenOut();
                    break;
                case EventType.ExitA:
                case EventType.ExitB:
                    door.Close();
                    break;
            }
        }
    }
}