using UnityEngine;
using System.Collections;
using System;
using Unity.Burst;
namespace Stats
{
    public class CharaterGenerator : MonoBehaviour
    {

        private PlayerCharacter _toon;
        private const int STARGING_POINTS = 350;
        private const int MIN_STATING_ATTRIBUTE_VALUE = 10;
        private const int STARTING_VALUE = 50;
        private int leftPoints = 0;
        // Use this for initialization
        void Start()
        {
            _toon = new PlayerCharacter();
            _toon.Init();
            leftPoints = STARGING_POINTS;
            for (int i = 0; i < Enum.GetValues(typeof(AttributeName)).Length; i++)
            {
                _toon.GetPrimaryAttribute(i).BaseValue = STARTING_VALUE;
                leftPoints -= (STARTING_VALUE - MIN_STATING_ATTRIBUTE_VALUE);
            }

            _toon.StatUpdate();
        }

        void OnGUI()
        {
            DisplayName();
            DisplayAttributes();
            DisplayVitals();
            DisplaySkills();
            DisplayLeftPoints();
        }

        private void DisplayName()
        {
            GUI.Label(new Rect(10, 10, 50, 25), "name");
            _toon.Name = GUI.TextField(new Rect(65, 10, 100, 25), _toon.Name);
        }

        private void DisplayAttributes()
        {
            for (int i = 0; i < Enum.GetValues(typeof(AttributeName)).Length; i++)
            {
                GUI.Label(new Rect(10, 40 + (i * 25), 100, 25), ((AttributeName)i).ToString());
                GUI.Label(new Rect(115, 40 + (i * 25), 30, 25), _toon.GetPrimaryAttribute(i).AdjustBaseValue.ToString());

                if (GUI.Button(new Rect(150, 40 + (i * 25), 30, 25), "-"))
                {
                    if (_toon.GetPrimaryAttribute(i).BaseValue > MIN_STATING_ATTRIBUTE_VALUE)
                    {
                        _toon.GetPrimaryAttribute(i).BaseValue--;
                        leftPoints++;
                        _toon.StatUpdate();
                    }
                };

                if (GUI.Button(new Rect(180, 40 + (i * 25), 30, 25), "+"))
                {
                    if (leftPoints > 0)
                    {
                        _toon.GetPrimaryAttribute(i).BaseValue++;
                        leftPoints--;
                        _toon.StatUpdate();
                    }
                };
            }
        }

        private void DisplayVitals()
        {
            for (int i = 0; i < Enum.GetValues(typeof(VitalName)).Length; i++)
            {
                GUI.Label(new Rect(10, 40 + ((i + 7) * 25), 100, 25), ((VitalName)i).ToString());
                GUI.Label(new Rect(115, 40 + ((i + 7) * 25), 30, 25), _toon.GetVital(i).AdjustBaseValue.ToString());
            }
        }

        private void DisplaySkills()
        {
            for (int i = 0; i < Enum.GetValues(typeof(StatName)).Length; i++)
            {
                GUI.Label(new Rect(250, 40 + (i * 25), 100, 25), ((StatName)i).ToString());
                GUI.Label(new Rect(355, 40 + (i * 25), 30, 25), _toon.GetStat(i).AdjustBaseValue.ToString());
            }
        }

        private void DisplayLeftPoints()
        {
            GUI.Label(new Rect(250, 10, 100, 25), "Left points : " + leftPoints.ToString());
        }
    }
}