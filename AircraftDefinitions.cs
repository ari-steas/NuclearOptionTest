using Mirage;
using NuclearOption.SavedMission;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NuclearOptionTest
{
    internal static class AircraftDefinitions
    {
        public static bool IsLoaded = false;
        public static List<AircraftDefinition> Definitions = new List<AircraftDefinition>();


        public static void LoadAssetBundle()
        {
            Definitions.Clear();

            // TODO allow loading multiple bundles
            string bundlePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "multiroletest");
            Plugin.Logger.LogInfo($"[LoadAssetBundle] Loading asset bundle from {bundlePath}...");
            var testBundle = AssetBundle.LoadFromFile(bundlePath);

            var allAssets = testBundle.LoadAllAssets<AircraftDefinition>();
            foreach (var asset in allAssets)
            {
                Plugin.Logger.LogInfo($"[LoadAssetBundle]    Loaded aircraft definition {asset.unitName}.");
                Definitions.Add(asset);
            }

            Plugin.Logger.LogInfo("[LoadAssetBundle] All definitions loaded.");
            testBundle.Unload(false);
            IsLoaded = true;
        }
    }
}
