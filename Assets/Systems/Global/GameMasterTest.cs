
using UnityEngine;

using Unity.Jobs.LowLevel.Unsafe;

public class GameMasterTest : MonoBehaviour
{

    int ThreadCount => SMT ? SystemInfo.processorCount-1 : SystemInfo.processorCount/2;
    public bool SMT => SystemInfo.processorCount < 8;
    public bool SMTOverride;

    private void Awake()
    {
#if UNITY_WSA
        SMTOverride = true;
#endif
      
        if (SMTOverride)
        {
            JobsUtility.JobWorkerCount = SystemInfo.processorCount - 1;
        }
        else
        {
            JobsUtility.JobWorkerCount = ThreadCount;
        }
    }



}

