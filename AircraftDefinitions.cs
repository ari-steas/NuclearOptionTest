using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace NuclearOptionTest
{
    internal static class AircraftDefinitions
    {
        public static bool IsLoaded = false;
        public static List<AircraftDefinition> Definitions = new List<AircraftDefinition>();


        public static void LoadAssetBundle()
        {
            Definitions.Clear();

            var pluginsPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            foreach (var bundlePath in Directory.GetFiles(pluginsPath))
            {
                if (!Path.GetFileName(bundlePath).StartsWith("airprefab_"))
                    continue;

                Plugin.Logger.LogInfo($"[LoadAssetBundle] Loading asset bundle from {bundlePath}...");
                AssetBundle testBundle = null;
                try
                {
                    testBundle = AssetBundle.LoadFromFile(bundlePath);

                    var allAssets = testBundle.LoadAllAssets<AircraftDefinition>();
                    foreach (var asset in allAssets)
                    {
                        Plugin.Logger.LogInfo($"[LoadAssetBundle]    Loaded aircraft definition {asset.unitName}.");
                        Definitions.Add(asset);
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Logger.LogError(ex);
                }
                finally
                {
                    testBundle?.Unload(false);
                }
            }

            Plugin.Logger.LogInfo("[LoadAssetBundle] All definitions loaded.");
            IsLoaded = true;
        }
    }
}
