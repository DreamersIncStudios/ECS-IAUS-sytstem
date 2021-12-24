// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System;

namespace PixelCrushers.LoveHate
{

    /// <summary>
    /// Defines a relationship to another faction, which is a set of traits that
    /// always includes affinity as the first trait.
    /// </summary>
    [Serializable]
    public class Relationship
    {

        public const string AffinityTraitName = "Affinity";

        public const int AffinityTraitIndex = 0;

        /// <summary>
        /// A static pool of objects, to prevent garbage collection stutter.
        /// </summary>
        public static Pool<Relationship> pool = new Pool<Relationship>();

#if UNITY_2019_3_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void InitStaticVariables()
        {
            pool = new Pool<Relationship>();
        }
#endif

        /// <summary>
        /// The ID of the faction this relationship is directed to.
        /// </summary>
        public int factionID = 0;

        /// <summary>
        /// Do child factions inherit this relationship?
        /// </summary>
        public bool inheritable = true;

        /// <summary>
        /// Relationship traits. The first trait is always affinity.
        /// </summary>
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

        /// <summary>
        /// Gets a new relationship from the pool.
        /// </summary>
        /// <returns>The new relationship.</returns>
        /// <param name="factionID">Faction ID.</param>
        /// <param name="traits">Traits.</param>
        public static Relationship GetNew(int factionID, float[] traits)
        {
            return GetNew(factionID, true, traits);
        }

        /// <summary>
        /// Gets a new relationship from the pool.
        /// </summary>
        /// <returns>The new relationship.</returns>
        /// <param name="factionID">Faction ID.</param>
        /// <param name="traits">Traits.</param>
        public static Relationship GetNew(int factionID, bool inheritable, float[] traits)
        {
            var relationship = pool.Get();
            relationship.Assign(factionID, inheritable, traits);
            return relationship;
        }

        /// <summary>
        /// Releases a relationship object back to the pool.
        /// </summary>
        /// <param name="deed">Deed.</param>
        public static void Release(Relationship relationship)
        {
            pool.Release(relationship);
        }

    }

}
