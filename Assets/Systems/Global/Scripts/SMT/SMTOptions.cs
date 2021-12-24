
using UnityEngine;

using Unity.Jobs.LowLevel.Unsafe;

namespace Core
{
    [System.Serializable]
    public class SMTOptions
    {

        int ThreadCount => SMT ? SystemInfo.processorCount - 1 : SystemInfo.processorCount / 2;
        public bool SMT => SystemInfo.processorCount < 8;
        public bool SMTOverride;

        public SMTOptions(bool SMT)
        {
#if UNITY_WSA
        SMTOverride = true;
#endif
            SMTOverride = SMT;
            Debug.Log(SystemInfo.processorCount);
            if (SMTOverride)
            {
#if !UNITY_WSA
                JobsUtility.JobWorkerCount = SystemInfo.processorCount - 1;
#endif
#if UNITY_WSA
            JobsUtility.JobWorkerCount = 6;
#endif
            }
            else
            {
                JobsUtility.JobWorkerCount = ThreadCount;
            }
        }
    }
}

