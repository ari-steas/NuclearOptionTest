using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace NuclearOptionTest.Patches
{
    [HarmonyPatch(typeof(AircraftSelectionMenu))]
    internal class AircraftSelectionMenuPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(AircraftSelectionMenu.Refresh))]
        public static void RefreshPostfix(
            AircraftSelectionMenu __instance,
            Airbase airbase,
            ref List<AircraftDefinition> ___aircraftSelection
        )
        {
            Plugin.Logger.LogInfo($"[AircraftSelectionMenuPatch] ({__instance.name}) Adding Aircraft to Selection");

            foreach (var aircraft in AircraftDefinitions.Definitions)
            {
                if (!___aircraftSelection.Contains(aircraft))
                    ___aircraftSelection.Add(aircraft);
            }
        } 
    }
}
