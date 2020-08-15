using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Stats {
    [System.Serializable]
    public class Profession {
        //  bool PrimaryProffesion;
        public Professions Class;
        int[] _primaryStatLevelUp { get; set; }
        int[] _secondaryStatLevelUp { get; set; }

        public int[] PrimaryStatLevelUP { get {
               // _primaryStatLevelUp = new int[System.Enum.GetValues(typeof(AttributeName)).Length];
                return _primaryStatLevelUp; } set {
                _primaryStatLevelUp = value;
                //_primaryStatLevelUp = new int[System.Enum.GetValues(typeof(AttributeName)).Length];
                //switch (Class)
                //{
                //    case Professions.Dragoon:
                //        PrimaryMod(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1);
                //        break;
                //    case Professions.Medic:
                //        PrimaryMod(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1);
                //        break;
                //    case Professions.Aria:
                //        PrimaryMod(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1);
                //        break;
                //    case Professions.Knight:
                //        PrimaryMod(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1);
                //        break;
                //    case Professions.Summoner:
                //        PrimaryMod(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1);
                //        break;
                //}
            } }

        public int[] SecondaryStatLevelUP
        {
            get { return _secondaryStatLevelUp; }
            set
            {
                _secondaryStatLevelUp = value;
                //_primaryStatLevelUp = new int[System.Enum.GetValues(typeof(AttributeName)).Length];
                //switch (Class)
                //{
                //    case Professions.Dragoon:
                //        SecondaryMod(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1);
                //        break;
                //    case Professions.Medic:
                //        SecondaryMod(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1);
                //        break;
                //    case Professions.Aria:
                //        SecondaryMod(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1);
                //        break;
                //    case Professions.Knight:
                //        SecondaryMod(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1);
                //        break;
                //    case Professions.Summoner:
                //        SecondaryMod(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1);
                //        break;
                //}
            } }


        void PrimaryMod(int level, int Str, int vit,int Awr, int Spd, int Skl, int Res, int Con, int Will, int Chars, int Lck) {
            _primaryStatLevelUp[0] = level;
            _primaryStatLevelUp[1] = Str;
            _primaryStatLevelUp[2] = vit;
            _primaryStatLevelUp[3] = Awr;
            _primaryStatLevelUp[4] = Spd;
            _primaryStatLevelUp[5] = Skl;
            _primaryStatLevelUp[6] = Res;
            _primaryStatLevelUp[7] = Con;
            _primaryStatLevelUp[8] = Will;
            _primaryStatLevelUp[9] = Chars;
            _primaryStatLevelUp[10] = Lck;
        }
        void SecondaryMod(int level, int Str, int vit, int Awr, int Spd, int Skl, int Res, int Con, int Will, int Chars, int Lck)
        {
            _secondaryStatLevelUp[0] = level;
            _secondaryStatLevelUp[1] = Str;
            _secondaryStatLevelUp[2] = vit;
            _secondaryStatLevelUp[3] = Awr;
            _secondaryStatLevelUp[4] = Spd;
            _secondaryStatLevelUp[5] = Skl;
            _secondaryStatLevelUp[6] = Res;
            _secondaryStatLevelUp[7] = Con;
            _secondaryStatLevelUp[8] = Will;
            _secondaryStatLevelUp[9] = Chars;
            _secondaryStatLevelUp[10] = Lck;
        }
    }
    public enum Professions { Dragoon, Medic, Knight, Summoner, Aria, }

}