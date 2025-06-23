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

            var currentMountKeys = new HashSet<string>(__instance.weaponMounts.Select(wm => wm.jsonKey));

            foreach (var def in AircraftDefinitions.Definitions)
            {
                if (__instance.aircraft.Contains(def))
                    continue;
                __instance.aircraft.Add(def);
                Plugin.Logger.LogInfo($"[EncyclopediaPatch]     Added aircraft definition {def.unitName}.");

                foreach (var wepMan in def.unitPrefab.GetComponentsInChildren<WeaponManager>())
                {
                    foreach (var mount in wepMan.hardpointSets.SelectMany(hSet => hSet.weaponOptions))
                    {
                        if (mount == null || instance.weaponMounts.Contains(mount))
                            continue;

                        while (!currentMountKeys.Add(mount.jsonKey))
                        {
                            mount.jsonKey += "_NEW";
                        }

                        __instance.weaponMounts.Add(mount);
                        Plugin.Logger.LogInfo($"[EncyclopediaPatch]         Added weapon definitions {mount.jsonKey}.");
                    }
                }

                def.unitPrefab.GetComponent<NetworkIdentity>().PrefabHash = def.unitName.GetHashCode(); // TODO make this safer/cleaner
            }

            foreach (var def in __instance.aircraft)
            {
                Plugin.Logger.LogInfo($"Definition {def.unitPrefab?.name} ({def.unitName}) network hash: {def.unitPrefab?.GetNetworkIdentity()?.PrefabHash.ToString() ?? "NULL"}");
            }

            return true;
        }
    }
}
