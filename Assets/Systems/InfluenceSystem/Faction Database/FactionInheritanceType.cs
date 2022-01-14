// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.LoveHate
{

    /// <summary>
    /// Defines how inheritance should be handled.
    /// </summary>
    public enum FactionInheritanceType
    { 
        /// <summary>
        /// Average the parents' values.
        /// </summary>
        Average,

        /// <summary>
        /// Sum the parents' values.
        /// </summary>
        Sum
	}

}
