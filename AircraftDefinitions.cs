using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NuclearOptionTest
{
    internal static class AircraftDefinitions
    {
        public static bool IsLoaded = false;
        public static List<AircraftDefinition> Definitions = new();

        public static void LoadAssetBundle()
        {
            Definitions.Clear();

            var pluginsPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var materials = GetGlobalMaterials();
            var audioClips = GetGlobalAudioClips();

            foreach (var bundlePath in Directory.GetFiles(pluginsPath))
            {
                if (!Path.GetFileName(bundlePath).StartsWith("airprefab_"))
                    continue;

                Plugin.Logger.LogInfo($"[LoadAssetBundle] Loading asset bundle from {bundlePath}...");
                AssetBundle testBundle = null;
                try
                {
                    testBundle = AssetBundle.LoadFromFile(bundlePath);

                    var allDefs = testBundle.LoadAllAssets<AircraftDefinition>();
                    foreach (var definition in allDefs)
                    {
                        try
                        {
                            Plugin.Logger.LogInfo($"[LoadAssetBundle]    Loading aircraft definition {definition.unitName}...");
                            if (definition.unitPrefab == null)
                                throw new Exception("Null unit prefab");

                            foreach (var meshRender in definition.unitPrefab.GetComponentsInChildren<MeshRenderer>())
                            {
                                var mats = new List<Material>();
                                foreach (var mat in meshRender.sharedMaterials)
                                {
                                    Material newMat;
                                    if (mat != null && materials.TryGetValue(
                                            mat.name.Replace(" (Instance)", ""), out newMat))
                                    {
                                        mats.Add(newMat);
                                        Plugin.Logger.LogInfo($"            Replaced material {meshRender.name}/{mat.name}");
                                    }
                                    else
                                    {
                                        mats.Add(mat);
                                        Plugin.Logger.LogInfo($"            Could not find material {meshRender.name}/{mat?.name ?? "NULL"}");
                                    }
                                }

                                meshRender.SetMaterials(mats);
                                meshRender.SetSharedMaterials(mats);
                            }
                            Plugin.Logger.LogInfo($"[LoadAssetBundle]        Completed material postprocessing.");

                            foreach (var audioSource in definition.unitPrefab.GetComponentsInChildren<AudioSource>())
                            {
                                AudioClip newClip;
                                if (audioSource.clip != null && audioClips.TryGetValue(audioSource.clip.name, out newClip))
                                    audioSource.clip = newClip;
                                else
                                    Plugin.Logger.LogInfo(
                                        $"            Could not find audio clip {audioSource.name}/{audioSource.clip?.name ?? "NULL"}");
                            }
                            Plugin.Logger.LogInfo($"[LoadAssetBundle]        Completed audio postprocessing.");

                            Definitions.Add(definition);
                            Plugin.Logger.LogInfo($"[LoadAssetBundle]    Finished loading aircraft definition {definition.unitName}.");
                        }
                        catch (Exception ex)
                        {
                            Plugin.Logger.LogError(ex);
                        }
                    }

                    Plugin.Logger.LogInfo($"Bundle contents: {string.Join(", ", testBundle.GetAllAssetNames())}");

                    Plugin.Logger.LogInfo($"Loaded {testBundle.LoadAllAssets<Material>().Length} materials.");
                    Plugin.Logger.LogInfo($"Loaded {testBundle.LoadAllAssets<AudioClip>().Length} sounds.");
                    Plugin.Logger.LogInfo($"Loaded {testBundle.LoadAllAssets<Texture2D>().Length} textures.");
                    Plugin.Logger.LogInfo($"Loaded {testBundle.LoadAllAssets<Shader>().Length} shaders.");
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

        private static Dictionary<string, Material> GetGlobalMaterials()
        {
            var mats = new Dictionary<string, Material>();

            foreach (var airDef in Encyclopedia.i.GetAircraftAndVehicles())
            {
                if (airDef?.unitPrefab == null)
                    continue;

                GetRendererMaterials(airDef.unitPrefab, mats);
                foreach (var wepPrefab in airDef.unitPrefab.GetComponentsInChildren<WeaponManager>().SelectMany(man =>
                             man.hardpointSets.SelectMany(hp => hp.weaponOptions.Select(wo => wo?.prefab))))
                    GetRendererMaterials(wepPrefab, mats);
            }

            return mats;
        }

        private static Dictionary<string, AudioClip> GetGlobalAudioClips()
        {
            var clips = new Dictionary<string, AudioClip>();

            foreach (var airDef in Encyclopedia.i.GetAircraftAndVehicles())
            {
                if (airDef?.unitPrefab == null)
                    continue;

                GetObjectClips(airDef.unitPrefab, clips);
                foreach (var wepPrefab in airDef.unitPrefab.GetComponentsInChildren<WeaponManager>().SelectMany(man =>
                             man.hardpointSets.SelectMany(hp => hp.weaponOptions.Select(wo => wo?.prefab))))
                    GetObjectClips(wepPrefab, clips);
            }

            return clips;
        }

        private static void GetRendererMaterials(GameObject obj, Dictionary<string, Material> mats)
        {
            if (obj == null)
                return;

            var renderers = obj.GetComponentsInChildren<MeshRenderer>();
            foreach (var renderer in renderers)
            {
                if (renderer?.sharedMaterials == null)
                    continue;
                foreach (var mat in renderer.sharedMaterials)
                {
                    if (mat == null)
                        continue;
                    if (mats.TryAdd(mat.name, mat))
                    {
                        Plugin.Logger.LogInfo($"Found material {obj.name}::{mat.name}");
                    }
                }
            }
        }

        private static void GetObjectClips(GameObject obj, Dictionary<string, AudioClip> clips)
        {
            if (obj == null)
                return;

            var sources = obj.GetComponentsInChildren<AudioSource>();
            foreach (var audioSource in sources)
            {
                if (audioSource?.clip == null)
                    continue;
                if (clips.TryAdd(audioSource.clip.name, audioSource.clip))
                {
                    Plugin.Logger.LogInfo($"Found audio clip {obj.name}::{audioSource.clip.name}");
                }
            }
        }
    }
}
