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
        [HarmonyPrefix]
        [HarmonyPatch(nameof(AircraftSelectionMenu.Initialize))]
        public static bool InitializePrefix(ref AircraftSelectionMenu __instance, Airbase airbase)
        {
            Plugin.Logger.LogInfo($"[AircraftSelectionMenuPatch] ({__instance.name}) Initialize prefix invoked! Button count: {__instance.listAircraftButtons.Count}");

            if (AircraftDefinitions.TestDef == null)
            {
                Plugin.Logger.LogInfo($"[AircraftSelectionMenuPatch] ({__instance.name}) TestDef is null, generating early.");

                foreach (var obj in GameObject.FindObjectsOfType<Hangar>())
                {
                    obj.GetAvailableAircraft();
                    if (AircraftDefinitions.TestDef != null)
                        break;
                }

                if (AircraftDefinitions.TestDef == null)
                {
                    Plugin.Logger.LogInfo($"[AircraftSelectionMenuPatch] ({__instance.name}) TestDef is still null - returning early!");
                    return true;
                }
            }

            if (__instance.listAircraftButtons.Count > 0)
                AircraftSelectionButtons.GenerateButtons();

            foreach (var button in AircraftSelectionButtons.Buttons)
                if (!__instance.listAircraftButtons.Contains(button))
                    __instance.listAircraftButtons.Add(button);

            return true; // exec original method
        } 
    }
}
