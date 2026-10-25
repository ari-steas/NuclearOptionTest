using HarmonyLib;
using Mirage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace NuclearOptionTest
{
    internal static class AircraftDefinitions
    {
        public static bool IsLoaded = false;
        public static List<AircraftDefinition> Definitions = new();

        private static readonly HashSet<Type> RefAssetTypes =
        [
            typeof(Material),
            typeof(AudioClip),
            typeof(Texture2D),
            typeof(WeaponMount),
        ];

        public static void LoadAssetBundle()
        {
            Definitions.Clear();

            var pluginsPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            Plugin.Logger.LogInfo("Starting global reference search...");
            var sw = new Stopwatch();
            sw.Start();
            var globalRefs = GetGlobalRefDictionary(false);
            sw.Stop();
            Plugin.Logger.LogInfo($"Completed global reference search in {sw.ElapsedMilliseconds}ms.\nLocated objects per type:\n{string.Join("\n- ", globalRefs.Select(kvp => $"{kvp.Key.Name}: {kvp.Value.Count}"))}");

            foreach (var bundlePath in Directory.GetFiles(pluginsPath))
            {
                if (!Path.GetFileName(bundlePath).ToLowerInvariant().StartsWith("airprefab_"))
                    continue;

                Plugin.Logger.LogInfo($"[LoadAssetBundle] Loading asset bundle from {bundlePath}...");
                AssetBundle testBundle = null;

                try
                {
                    sw.Restart();
                    testBundle = AssetBundle.LoadFromFile(bundlePath);

                    var airDefs = GetBundleAssets(testBundle, globalRefs);

                    foreach (var definition in airDefs)
                    {
                        Plugin.Logger.LogInfo($"[LoadAssetBundle]    Loading aircraft definition {definition.unitName}...");
                        ProcessPrefab(definition, globalRefs);
                        Plugin.Logger.LogInfo($"[LoadAssetBundle]    Finished loading aircraft definition {definition.unitName}");
                    }
                    sw.Stop();
                }
                catch (Exception ex)
                {
                    Plugin.Logger.LogError(ex);
                }
                finally
                {
                    testBundle?.Unload(false);
                    sw.Stop();
                    Plugin.Logger.LogInfo($"[LoadAssetBundle] Finished loading asset bundle {bundlePath} ({sw.ElapsedMilliseconds}ms)");
                }
            }

            Plugin.Logger.LogInfo("[LoadAssetBundle] All definitions loaded.");
            IsLoaded = true;
        }

        private static Dictionary<Type, Dictionary<string, UnityEngine.Object>> GetGlobalRefDictionary(bool log)
        {
            var refs = new Dictionary<Type, Dictionary<string, UnityEngine.Object>>();
            foreach (var type in RefAssetTypes)
            {
                if (type == typeof(Object) || type.IsSubclassOf(typeof(Object)))
                    refs[type] = new Dictionary<string, UnityEngine.Object>();
            }

            var allComponents = new Stack<object>(250);
            foreach (var unitDef in Encyclopedia.i.aircraft)
                allComponents.Push(unitDef);

            foreach (var component in IterateData<UnityEngine.Object>(allComponents, log, (obj, typ) => typ == typeof(Faction) || typ.IsSubclassOf(typeof(Faction))))
            {
                if (RefAssetTypes.Contains(component.Item2))
                {
                    var cname = component.Item1.name.EndsWith("(Instance)") ? component.Item1.name[..^11] : component.Item1.name;

                    if (refs[component.Item2].TryAdd(cname, component.Item1) && log)
                        Plugin.Logger.LogInfo($"! {component.Item1.name}/{cname}");
                }
            }

            return refs;
        }

        private static List<AircraftDefinition> GetBundleAssets(AssetBundle bundle, Dictionary<Type, Dictionary<string, UnityEngine.Object>> assetRefs)
        {
            var airDefs = new List<AircraftDefinition>();

            foreach (var assetName in bundle.GetAllAssetNames())
            {
                if (assetName.EndsWith("prefab")) // Preloading prefabs causes issues in NO
                    continue;
                var asset = bundle.LoadAsset(assetName);
                var assetType = asset.GetType();
                Plugin.Logger.LogInfo($"Loaded bundle asset {assetType.Name}::{assetName}");

                if (asset is AircraftDefinition airDef)
                {
                    airDefs.Add(airDef);
                    continue;
                }

                if (RefAssetTypes.Contains(assetType) && assetRefs[assetType].TryAdd(asset.name, asset))
                    Plugin.Logger.LogInfo($"    Reference registered.");
            }

            return airDefs;
        }

        private static void ProcessPrefab(AircraftDefinition definition, Dictionary<Type, Dictionary<string, UnityEngine.Object>> assetRefs)
        {
            try
            {
                if (definition.unitPrefab == null)
                    throw new Exception("Null unit prefab");

                var allComponents = new Stack<object>(250);
                allComponents.Push(definition);

                // ignore factions, otherwise all units are captured
                foreach (var (thisComp, compType) in IterateData<Object>(allComponents, false, (obj, typ) => typ == typeof(Faction) || typ == typeof(WeaponMount)))
                {
                    // special handling is needed here
                    if (compType == typeof(MeshRenderer))
                    {
                        var meshRender = (MeshRenderer)thisComp;
                        var mats = new List<Material>();
                        foreach (var mat in meshRender.sharedMaterials)
                        {
                            Object newMat;
                            if (mat != null && assetRefs[typeof(Material)].TryGetValue(mat.name.Replace(" (Instance)", ""), out newMat))
                            {
                                mats.Add((Material)newMat);
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
                        continue;
                    }

                    foreach (var field in compType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                        TryReplaceValue(thisComp, field.GetValue, field.SetValue, field.FieldType, assetRefs);

                    foreach (var prop in compType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                        TryReplaceValue(thisComp, prop.GetValue, prop.SetValue, prop.PropertyType, assetRefs);
                }

                Definitions.Add(definition);
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError(ex);
            }
        }

        private static void TryReplaceValue(Object thisComp, Func<object, object> getValue, Action<object, object> setValue, Type fieldType,
            Dictionary<Type, Dictionary<string, Object>> assetRefs)
        {
            if (!assetRefs.ContainsKey(fieldType))
            {
                var elementType = fieldType.GetElementType();
                if (!fieldType.IsArray || elementType == null || !assetRefs.ContainsKey(elementType))
                    return;

                Plugin.Logger.LogInfo($"            Replacing {fieldType} {thisComp.name}...");
                var arrayVal = (Array)getValue(thisComp);
                var newArray = Array.CreateInstance(elementType, arrayVal.Length);
                for (int i = 0; i < arrayVal.Length; i++)
                {
                    Object iterVal = (Object) arrayVal.GetValue(i);
                    Object newVal;
                    if (assetRefs[elementType].TryGetValue(iterVal.name.Replace(" (Instance)", ""), out newVal))
                    {
                        newArray.SetValue(newVal, i);
                        Plugin.Logger.LogInfo($"                Replaced {elementType.Name}[{i}]/{iterVal.name}");
                    }
                    else
                    {
                        newArray.SetValue(iterVal, i);
                        Plugin.Logger.LogInfo($"                Could not find {elementType.Name}[{i}]/{iterVal.name ?? "NULL"}");
                    }
                }
                setValue(thisComp, newArray);
                return;
            }

            try
            {
                var fieldVal = (Object) getValue(thisComp);
                if (fieldVal == null)
                    return;

                Object newVal;
                if (assetRefs[fieldType].TryGetValue(fieldVal.name.Replace(" (Instance)", ""), out newVal))
                {
                    setValue(thisComp, newVal);
                    Plugin.Logger.LogInfo($"            Replaced {fieldType} {thisComp.name}/{fieldVal.name}");
                }
                else
                {
                    Plugin.Logger.LogInfo($"            Could not find {fieldType} {thisComp.name}/{fieldVal.name ?? "NULL"}");
                }
            }
            catch
            {
                // fail silently
            }
        }

        private static IEnumerable<(TData, Type)> IterateData<TData>(Stack<object> allComponents, bool log, Func<object, Type, bool> skipFunc = null) where TData : class
        {
            var usedHashes = new HashSet<int>(1000);

            while (allComponents.Count > 0)
            {
                var thisComp = allComponents.Pop();
                var compType = thisComp?.GetType();
                if (thisComp == null || !compType.IsClass || compType == typeof(string) || !usedHashes.Add(thisComp.GetHashCode()))
                    continue;

                if (log)
                {
                    string logName = $"> {compType.Name}";
                    if (thisComp is Array a)
                        logName = logName[..^1] + $"{a.Length}]";
                    if (compType.GetGenericArguments().Length > 0)
                        logName += $"<{string.Join(",", compType.GetGenericArguments().Select(t => t.Name))}>";
                    Plugin.Logger.LogInfo(logName);
                }

                if (skipFunc?.Invoke(thisComp, compType) ?? false)
                    continue;

                if (thisComp is TData compObject)
                    yield return (compObject, compType);

                // meshes throw errors when trying to access certain fields
                if (compType == typeof(Mesh) || compType == typeof(Shader) || compType == typeof(Material))
                    continue;

                var compGameObject = thisComp as GameObject;
                if (compGameObject != null)
                {
                    foreach (var comp in compGameObject.GetComponents(typeof(Component)))
                        allComponents.Push(comp);
                    foreach (var comp in compGameObject.GetComponentsInChildren(typeof(Component)))
                        allComponents.Push(comp);
                }

                if (thisComp is IEnumerable enumerable)
                {
                    foreach (var entry in enumerable)
                    {
                        var eType = entry?.GetType();
                        if (entry == null || !eType.IsClass || eType == typeof(string) || eType == typeof(Transform))
                        {
                            if (log && eType != typeof(byte))
                                Plugin.Logger.LogInfo($"-E     {eType?.Name ?? "NULL"}");
                            continue;
                        }
                        allComponents.Push(entry);
                        if (log)
                            Plugin.Logger.LogInfo($"+E     {eType.Name}");
                    }
                }

                foreach (var field in compType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (!field.FieldType.IsClass || field.FieldType == typeof(string) || field.FieldType == typeof(Transform))
                        continue;

                    try
                    {
                        var fieldVal = field.GetValue(thisComp);
                        if (fieldVal == null)
                            continue;
                        allComponents.Push(fieldVal);

                        if (log)
                            Plugin.Logger.LogInfo($"+F     {field.FieldType.Name}");
                    }
                    catch
                    {
                        // fail silently
                    }
                }

                foreach (var prop in compType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (!prop.PropertyType.IsClass || prop.PropertyType == typeof(string) || prop.PropertyType == typeof(Transform))
                        continue;

                    try
                    {
                        var fieldVal = prop.GetValue(thisComp);
                        if (fieldVal == null)
                            continue;
                        allComponents.Push(fieldVal);

                        if (log)
                            Plugin.Logger.LogInfo($"+P     {prop.PropertyType.Name}");
                    }
                    catch
                    {
                         // fail silently
                    }
                }
            }
        }
    }
}
