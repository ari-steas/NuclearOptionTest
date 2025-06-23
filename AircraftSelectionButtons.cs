using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace NuclearOptionTest
{
    internal static class AircraftSelectionButtons
    {
        public static AircraftSelectionButton[] Buttons = Array.Empty<AircraftSelectionButton>();

        public static void GenerateButtons()
        {
            var aircraftButtons = GameObject.Find("AircraftButtons");
            var cricketButton = aircraftButtons.transform.Find("CI22Button");

            var buttonList = new List<AircraftSelectionButton>();

            foreach (var definition in AircraftDefinitions.Definitions)
            {
                var cricketButtonClone = UnityEngine.Object.Instantiate(cricketButton);
                cricketButtonClone.name = "SelectButton_" + definition.jsonKey;
                cricketButtonClone.transform.position -= new Vector3(100, 0, 0);
                cricketButtonClone.transform.parent = aircraftButtons.transform;
                cricketButtonClone.localScale = Vector3.one;

                var selectionButton = cricketButtonClone.transform.GetComponent<AircraftSelectionButton>();
                selectionButton.name = "AirSelectButton_" + definition.jsonKey;
                selectionButton.definition = definition;
                buttonList.Add(selectionButton);
            }

            Buttons = buttonList.ToArray();
            
            Plugin.Logger.LogInfo("Generated aircraft selection buttons.");
        }
    }
}
