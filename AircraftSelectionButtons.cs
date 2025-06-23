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
            var cricketButtonClone = UnityEngine.Object.Instantiate(aircraftButtons.transform.Find("CI22Button"));
            cricketButtonClone.name = "TestButton";
            cricketButtonClone.transform.position -= new Vector3(100, 0, 0);
            cricketButtonClone.transform.parent = aircraftButtons.transform;
            cricketButtonClone.localScale = Vector3.one;

            var selectionButton = cricketButtonClone.transform.GetComponent<AircraftSelectionButton>();
            selectionButton.name = "TEST SELECT NAME";
            selectionButton.definition = AircraftDefinitions.Definitions[0]; // TODO

            Buttons = new[]
            {
                selectionButton
            };
            
            Plugin.Logger.LogInfo("Generated aircraft selection buttons.");
        }
    }
}
