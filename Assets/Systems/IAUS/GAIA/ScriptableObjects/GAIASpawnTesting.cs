using UnityEngine;

namespace DreamersIncStudio.GAIACollective
{
    public class GAIASpawnTesting : MonoBehaviour
    {
        public GAIASpawnSO SpawnSO;

        public void Start()
        {
            Invoke(nameof(RunThis),2);
        }
        
        public void RunThis()
        {
            SpawnSO.LoadSpawnData();
        }
    }
}