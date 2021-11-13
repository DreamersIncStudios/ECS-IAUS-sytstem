using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;
namespace DreamersInc.FactionSystem
{
    [Serializable]
    public class Relationship {

        public const string AffinityTraitName = "Affinity";

        public const int AffinityTraitIndex = 0;
        [Range(-100, 100)] public int2[] threshold ={ -50,50};

        public static Pool<Relationship> pool = new Pool<Relationship>();

#if UNITY_2019_3_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void InitStaticVariables()
        {
            pool = new Pool<Relationship>();
        }
#endif


        public int factionID = 0;
        public bool inheritable = true;
        public float[] traits = new float[0];

        public float affinity
        {
            get
            {
                return ((traits != null) && (traits.Length > 0)) ? traits[0] : 0;
            }
            set
            {
                if ((traits != null) && (traits.Length > 0))
                {
                    traits[0] = value;
                }
            }
        }

        public Relationship() { }

        public Relationship(int factionID, float[] traits)
        {
            Assign(factionID, traits);
        }

        public Relationship(int factionID, bool inheritable, float[] traits)
        {
            Assign(factionID, inheritable, traits);
        }

        public void Assign(int factionID, float[] traits)
        {
            Assign(factionID, true, traits);
        }

        public void Assign(int factionID, bool inheritable, float[] traits)
        {
            this.factionID = factionID;
            this.inheritable = inheritable;    
            Traits.Copy(traits, ref this.traits);

        }

        public float GetTrait(int index)
        {
            return (0 <= index && index < traits.Length) ? traits[index] : 0;
        }
        public int2 GetThreshold(int index)
        { 
            return (0 <= index && index < traits.Length) ? threshold[index] : 0;
        }

        public bool2 CheckThreshold(int index) {
            return (0 <= index && index < traits.Length) ? new bool2(traits[index] > threshold[index].x, traits[index] < threshold[index].y): false;
        }

        public void SetTrait(int index, float value)
        {
            if (0 <= index && index < traits.Length)
            {
                traits[index] = value;
            }
        }

        public static float GetDefaultValue(int judgeFactionID, int subjectFactionID, int traitID)
        {
            var isSelfAffinity = (judgeFactionID == subjectFactionID) && (traitID == Relationship.AffinityTraitIndex);
            return isSelfAffinity ? 100 : 0;
        }


        public static Relationship GetNew(int factionID, float[] traits)
        {
            return GetNew(factionID, true, traits);
        }

        public static Relationship GetNew(int factionID, bool inheritable, float[] traits)
        {
            var relationship = pool.Get();
            relationship.Assign(factionID, inheritable, traits);
            return relationship;
        }

        public static void Release(Relationship relationship)
        {
            pool.Release(relationship);
        }

    }
}