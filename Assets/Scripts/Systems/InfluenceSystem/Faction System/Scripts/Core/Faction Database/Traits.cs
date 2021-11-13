using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;

namespace DreamersInc.FactionSystem
{
    [Serializable]
    public class Traits : MonoBehaviour
    {
        [Tooltip("An editor-time-only reference to a Faction Database, used only by the editor to get the presets list.")]
        public FactionDatabase factionDatabase;

        public float[] traits = new float[0];
        public int2[] threshold = new int2[0];

        public void FindResources()
        {
            if (factionDatabase == null)
            {
                var factionManager = FindObjectOfType<FactionManager>();
                if (factionManager != null && FactionManager.Database != null)
                {
                    factionDatabase = FactionManager.Database;
                }
                else
                {
                    var databases = Resources.FindObjectsOfTypeAll<FactionDatabase>();
                    if (databases.Length > 0)
                    {
                        factionDatabase = databases[0];
                    }
                }
            }
        }

        public float GetTrait(string traitName) {
            FindResources();
            if (factionDatabase != null) {
                for (int i = 0; i < factionDatabase.personalityTraitDefinitions.Length; i++)
                {
                    if (string.Equals(traitName, factionDatabase.personalityTraitDefinitions[i].name))
                        return traits[i];
                }
            
            }
            return 0;
        }
        public int2 GetThreshold(string traitName)
        {
            FindResources();
            if (factionDatabase != null)
            {
                for (int i = 0; i < factionDatabase.personalityTraitDefinitions.Length; i++)
                {
                    if (string.Equals(traitName, factionDatabase.personalityTraitDefinitions[i].name))
                        return threshold[i];
                }

            }
            return 0;
        }

        public bool2 CheckThreshold(string traitName) {
            FindResources();
            if (factionDatabase != null)
            {
                for (int i = 0; i < factionDatabase.personalityTraitDefinitions.Length; i++)
                {
                    if (string.Equals(traitName, factionDatabase.personalityTraitDefinitions[i].name))
                    {
                        float value = GetTrait(traitName);
                        return new bool2(value > threshold[i].x, value < threshold[i].y);
                    }
                }
            }

            return false;
        }

        public static void Copy(float[] source, ref float[] dest)
        {
            if (source == null)
            {
                dest = null;
            }
            else if ((dest == null) || (dest.Length != source.Length))
            {
                dest = (float[])source.Clone();
            }
            else
            {
                Array.Copy(source, dest, source.Length);
            }
        }

        public static float[] Allocate(int count)
        {
            return new float[count];
        }

        public static float[] Allocate(int count, bool initialize)
        {
            var traits = Allocate(count);
            if (initialize)
            {
                Initialize(traits);
            }
            return traits;
        }
        public static float Alignment(float[] a, float[] b)
        {
            var length = Mathf.Min(a.Length, b.Length);
            if (length == 0) return 0;
            float overlap = 0;
            for (int i = 0; i < length; i++)
            {
                var d = 1 - (Mathf.Abs(a[i] - b[i]) / 200);
                overlap += d;
            }
            return overlap / length;
        }
        public static void Initialize(float[] traits)
        {
            if (traits == null) return;
            Array.Clear(traits, 0, traits.Length);
        }

    }

    [Serializable]
    public class TraitDefinition {
        public string name = string.Empty;
        public string description = string.Empty;
        public TraitDefinition() { }
        public TraitDefinition(string name, string description) {
            this.name = name;
            this.description = description;
        }
    
    }
}