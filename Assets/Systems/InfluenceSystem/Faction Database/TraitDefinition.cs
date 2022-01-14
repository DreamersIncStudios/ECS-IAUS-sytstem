// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System;

namespace PixelCrushers.LoveHate
{

    /// <summary>
    /// A trait definition defines a single personality value (such as Charity) or 
    /// relationship value (such as Rivalry).
    /// </summary>
    [Serializable]
    public class TraitDefinition
    {

        /// <summary>
        /// The name of the trait.
        /// </summary>
        public string name = string.Empty;

        /// <summary>
        /// An optional description.
        /// </summary>
        public string description = string.Empty;

        public TraitDefinition() { }

        public TraitDefinition(string name, string description)
        {
            this.name = name;
            this.description = description;
        }

    }

}
