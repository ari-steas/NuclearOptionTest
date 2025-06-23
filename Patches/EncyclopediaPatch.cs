using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mirage;

namespace NuclearOptionTest.Patches
{
    [HarmonyPatch(typeof(Encyclopedia))]
    internal static class EncyclopediaPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("AfterLoad")]
        public static bool AfterLoadPrefix(ref Encyclopedia __instance)
        {
            if (!AircraftDefinitions.IsLoaded)
                AircraftDefinitions.LoadAssetBundle();

            Plugin.Logger.LogInfo("[EncyclopediaPatch] Inserting definitions...");
            var instance = __instance; // needed to avoid ref issues in lambdas

            var currentAircraftKeys = new HashSet<string>(__instance.aircraft.Select(wm => wm.jsonKey));
            var currentMountKeys = new HashSet<string>(__instance.weaponMounts.Select(wm => wm.jsonKey));

            foreach (var aircraftDef in AircraftDefinitions.Definitions)
            {
                if (__instance.aircraft.Contains(aircraftDef))
                    continue;

                if (aircraftDef.unitPrefab == null)
                {
                    Plugin.Logger.LogError($"Missing unit prefab on aircraft definition {aircraftDef.name}!");
                    continue;
                }

                // ensure jsonkey is unique, otherwise !!Fun!! occurs
                while (!currentAircraftKeys.Add(aircraftDef.jsonKey))
                    aircraftDef.jsonKey += "_NEW";
                __instance.aircraft.Add(aircraftDef);

                Plugin.Logger.LogInfo($"[EncyclopediaPatch]     Added aircraft definition {aircraftDef.unitName}.");

                foreach (var wepMan in aircraftDef.unitPrefab.GetComponentsInChildren<WeaponManager>())
                {
                    foreach (var mount in wepMan.hardpointSets.SelectMany(hSet => hSet.weaponOptions))
                    {
                        if (mount == null || instance.weaponMounts.Contains(mount))
                            continue;

                        // ensure jsonkey is unique, otherwise !!Fun!! occurs
                        while (!currentMountKeys.Add(mount.jsonKey))
                            mount.jsonKey += "_NEW";

                        __instance.weaponMounts.Add(mount);
                        Plugin.Logger.LogInfo($"[EncyclopediaPatch]         Added weapon definition {mount.jsonKey}.");
                    }
                }

                aircraftDef.unitPrefab.GetComponent<NetworkIdentity>().PrefabHash = aircraftDef.jsonKey.GetHashCode(); // TODO make this safer/cleaner
            }

            foreach (var def in __instance.aircraft)
            {
                Plugin.Logger.LogInfo($"Definition {def.unitPrefab?.name} ({def.unitName}) network hash: {def.unitPrefab?.GetNetworkIdentity()?.PrefabHash.ToString() ?? "NULL"}");
            }

            return true;
        }
    }
}
