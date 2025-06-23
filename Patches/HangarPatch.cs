using HarmonyLib;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NuclearOptionTest.Patches
{
    [HarmonyPatch(typeof(Hangar))]
    public static class HangarPatch
    {
        
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Hangar.GetAvailableAircraft))]
        public static bool GetAvailableAircraftPrefix(ref Hangar __instance, ref AircraftDefinition[] __result)
        {
            Plugin.Logger.LogInfo("Start invoke GetAvailableHangarPrefix");
            var availableAircraft = (AircraftDefinition[]) typeof(Hangar).GetField("availableAircraft", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(__instance) ?? throw new Exception();
            AircraftDefinition[] newAvailableAircraft;

            newAvailableAircraft = new AircraftDefinition[availableAircraft.Length + AircraftDefinitions.Definitions.Count - 1];
            for (int i = 0; i < availableAircraft.Length; i++)
                newAvailableAircraft[i] = availableAircraft[i];
            for (int i = 0; i < AircraftDefinitions.Definitions.Count; i++)
                newAvailableAircraft[AircraftDefinitions.Definitions.Count - 1 + i] = AircraftDefinitions.Definitions[i];

            __result = newAvailableAircraft;
            Plugin.Logger.LogInfo($"    Hangar @ {__instance.airbase?.SavedAirbase.UniqueName ?? "NULL"} available aircraft:\n" +
                                  $"{string.Join(", ", newAvailableAircraft.Select(a => a?.unitName ?? "NULL"))}"
                                  );

            return false; // Skip the original method
        }
        

        //[HarmonyPrefix]
        //[HarmonyPatch(nameof(Hangar.GetAvailableAircraft))]
        //public static bool GetAvailableAircraftPrefix(ref Hangar __instance, ref AircraftDefinition[] __result)
        //{
        //    Plugin.Logger.LogInfo("Start invoke GetAvailableHangarPrefix");
        //    var availableAircraft = (AircraftDefinition[]) typeof(Hangar).GetField("availableAircraft", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(__instance) ?? throw new Exception();
        //
        //    foreach (var def in availableAircraft)
        //    {
        //        if (def.unitName == "KR-67 Ifrit")
        //        {
        //            def.unitName = "TEST AIRCRAFT UNIT NAME 2";
        //            def.aircraftParameters.aircraftName = "TEST AIRCRAFT NAME 2";
        //            def.aircraftParameters.aircraftDescription = "TEST AIRCRAFT DESCRIPTION";
        //        }
        //    }
        //
        //    __result = availableAircraft;
        //    Plugin.Logger.LogInfo($"Hangar @ {__instance.airbase?.SavedAirbase.UniqueName ?? "NULL"} available aircraft: {string.Join(", ", availableAircraft.Select(a => a.unitName))}");
        //
        //    return false; // Skip the original method
        //}


        
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Hangar.CanSpawnAircraft))]
        public static bool CanSpawnAircraftPrefix(ref Hangar __instance, ref bool __result, AircraftDefinition definition)
        {
            Plugin.Logger.LogInfo($"Hangar @ {__instance.airbase?.SavedAirbase.UniqueName ?? "NULL"} try spawn {definition.unitName}");

            if (!__instance.Available)
            {
                __result = false;
                return false; // skip original
            }

            if (AircraftDefinitions.Definitions.Contains(definition))
            {
                __result = true;
                return false; // skip original
            }

            return true; // Execute the original method
        }
        
    }
}
