// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.LoveHate
{

    /// <summary>
    /// Add [FactionPopup] to an int to use a popup of corresponding factions
    /// using the faction database assigned to the current scene's FactionManager.
    /// </summary>
    public class FactionPopupAttribute : PropertyAttribute
    {

        public bool showReferenceDatabase = false;

        public FactionPopupAttribute(bool showReferenceDatabase = false)
        {
            this.showReferenceDatabase = showReferenceDatabase;
        }
    }
}
