// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System;

namespace PixelCrushers.LoveHate
{

    /// <summary>
    /// Adds personality traits to a GameObject. This isn't necessary if the 
    /// GameObject has a FactionMember but may be useful for other GameObjects 
    /// such as items and locations.
    /// 
    /// This class also provides utility methods to work with trait arrays.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class Traits : MonoBehaviour
    {

        /// <summary>
        /// An editor-time-only reference to the FactionDatabase, to be used only 
        /// by the editor to get the presets list for the Traits custom editor.
        /// </summary>
        [Tooltip("An editor-time-only reference to a Faction Database, used only by the editor to get the presets list.")]
        public FactionDatabase factionDatabase;

        /// <summary>
        /// The personality traits.
        /// </summary>
        public float[] traits = new float[0];

        /// <summary>
        /// Finds a faction database if it's unassigned, first looking on a 
        /// FactionManager and failing that the first FactionDatabase it finds.
        /// </summary>
        public void FindResources()
        {
            if (factionDatabase == null)
            {
                var factionManager = FindObjectOfType<FactionManager>();
                if (factionManager != null && factionManager.factionDatabase != null)
                {
                    factionDatabase = factionManager.factionDatabase;
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

        /// <summary>
        /// Gets a trait value by name.
        /// </summary>
        /// <returns>The trait value.</returns>
        /// <param name="traitName">The trait name.</param>
        public float GetTrait(string traitName)
        {
            FindResources();
            if (factionDatabase != null)
            {
                for (int i = 0; i < factionDatabase.personalityTraitDefinitions.Length; i++)
                {
                    if (string.Equals(traitName, factionDatabase.personalityTraitDefinitions[i].name))
                    {
                        return traits[i];
                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// Deep copies a source array into a destination array.
        /// </summary>
        /// <param name="source">Source.</param>
        /// <param name="dest">Destination.</param>
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

        /// <summary>
        /// Allocates an array of a specified size.
        /// </summary>
        /// <param name="count">Count.</param>
        public static float[] Allocate(int count)
        {
            return new float[count];
        }

        /// <summary>
        /// Allocates an array of a specified size.
        /// </summary>
        /// <param name="count">Count.</param>
        /// /// <param name="initialize">Set `true` to initialize the array to zero.</param>
        public static float[] Allocate(int count, bool initialize)
        {
            var traits = Allocate(count);
            if (initialize)
            {
                Initialize(traits);
            }
            return traits;
        }

        /// <summary>
        /// Initializes an array to zero.
        /// </summary>
        /// <param name="traits">Traits</param>
        public static void Initialize(float[] traits)
        {
            if (traits == null) return;
            Array.Clear(traits, 0, traits.Length);
        }

        /// <summary>
        /// Computes the "alignment" of two sets of values, which is how well their extremes
        /// match, not how close their values are. If both sets are all zero, they're close
        /// in value but they have no extremes so the alignment is zero.
        /// </summary>
        /// <returns>A value in the range -1 to +1 indicating how well the values align.</returns>
        /// <param name="a">The first set of values.</param>
        /// <param name="b">The second set of values.</param>
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

        public void OnDrawGizmos()
        {
            Gizmos.DrawIcon(transform.position, Faction.GizmoIconNames[0], true);
        }

    }

}
