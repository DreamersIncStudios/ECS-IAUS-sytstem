// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System;
using System.Collections.Generic;

namespace PixelCrushers.LoveHate
{

    /// <summary>
    /// A preset is a predefined set of traits, useful as a shortcut to 
    /// apply traits to factions and deeds.
    /// </summary>
    [Serializable]
    public class Preset
    {

        /// <summary>
        /// The preset's name.
        /// </summary>
        public string name = string.Empty;

        /// <summary>
        /// An optional description. Used only for the designer's benefit in the editor.
        /// </summary>
        public string description = string.Empty;

        /// <summary>
        /// The personality traits associated with this preset.
        /// </summary>
        public float[] traits = new float[0];

        public Preset()
        {
            name = string.Empty;
            traits = new float[0];
        }

        public Preset(string name, float[] traits)
        {
            this.name = name;
            Traits.Copy(traits, ref this.traits);
        }

        public Preset(string name, TraitDefinition[] traitDefinitions)
        {
            this.name = name;
            this.traits = Traits.Allocate(traitDefinitions.Length);
        }

    }

}
