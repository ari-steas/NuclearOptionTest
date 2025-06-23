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
            Plugin.Logger.LogInfo($"[AircraftSelectionMenuPatch] ({__instance.name}) Generating custom buttons.");

            if (__instance.listAircraftButtons.Count > 0)
                AircraftSelectionButtons.GenerateButtons();

            foreach (var button in AircraftSelectionButtons.Buttons)
                if (!__instance.listAircraftButtons.Contains(button))
                    __instance.listAircraftButtons.Add(button);

            return true; // exec original method
        } 
    }
}
