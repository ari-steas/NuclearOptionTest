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
        public static AircraftDefinition TestDef = null;

        public static void CopyDef(AircraftDefinition def)
        {
            Plugin.Logger.LogInfo("Start copy testdef");
            //var copy = def.Copy();
            //
            //copy.unitName = "TEST AIRCRAFT UNIT NAME";
            //copy.name = "TEST NAME";
            //copy.aircraftParameters.aircraftName = "TEST AIRCRAFT NAME";
            //copy.aircraftParameters.aircraftDescription = "TEST AIRCRAFT DESCRIPTION";
            //copy.code = "TST-D";
            //copy.jsonKey = "TEST NAME";
            //
            //TestDef = copy;

            

            TestDef = ScriptableObject.CreateInstance<AircraftDefinition>();

            {
                TestDef.name = "TEST NAME";
                TestDef.aircraftInfo = new AircraftInfo
                {
                    emptyWeight = 10700,
                    maneuverability = 9,
                    maxSpeed = 1500,
                    maxWeight = 30000,
                    stallSpeed = 240,
                };
                var aParams = ScriptableObject.CreateInstance<AircraftParameters>();
                {
                    aParams.name = "TestAircraftParams";
                    aParams.aircraftDescription = "TEST AIRCRAFT DESCRIPTION";
                    aParams.aircraftGLimit = 9;
                    aParams.aircraftName = "TEST AIRCRAFT NAME";
                    aParams.aircraftSize = 8;
                    aParams.aircraftType = "JET";
                    aParams.airfoils = def.aircraftParameters.airfoils;
                    aParams.approachSpeed = 60;
                    aParams.collectivePID = Vector3.zero;
                    aParams.cornerSpeed = 180;
                    aParams.cruiseThrottle = 0.99f;
                    aParams.DefaultFuelLevel = 1;
                    aParams.fixedGear = false;
                    aParams.groundTurningRadius = 15;
                    aParams.hoverPID = Vector3.zero;
                    aParams.hoverTiltFactor = 1;
                    aParams.HUDExtras = def.aircraftParameters.HUDExtras;
                    aParams.landingSpeed = 75;
                    aParams.levelBias = 0.15f;
                    aParams.liveries = def.aircraftParameters.liveries;
                    aParams.loadouts = def.aircraftParameters.loadouts;
                    aParams.maxSpeed = 600;
                    aParams.minimumRadarAlt = 50;
                    aParams.PIDReferenceAirspeed = 180;
                    aParams.pitchPID = def.aircraftParameters.pitchPID;
                    aParams.rankRequired = 4;
                    aParams.rollPID = def.aircraftParameters.rollPID;
                    aParams.shortLandingSpeed = 75;
                    aParams.StandardLoadouts = def.aircraftParameters.StandardLoadouts;
                    aParams.StatusDisplay = def.aircraftParameters.StatusDisplay;
                    aParams.takeoffDistance = 1200;
                    aParams.takeoffMusic = def.aircraftParameters.takeoffMusic;
                    aParams.takeoffSpeed = 75;
                    aParams.tiltPID = Vector3.zero;
                    aParams.turningRadius = 1500;
                    aParams.verticalLanding = false;
                    aParams.yawPID = def.aircraftParameters.yawPID;
                }
                TestDef.aircraftParameters = aParams;
                TestDef.armorTier = 1;
                TestDef.bogeyName = "";
                TestDef.captureCapacity = 0;
                TestDef.captureDefense = 0;
                TestDef.captureStrength = 0;
                TestDef.code = "TST-D";
                TestDef.damageTolerance = 1;
                TestDef.description = "TEST DESCRIPTION";
                TestDef.disabled = false;
                TestDef.dontAutomaticallyAddToEncyclopedia = false;
                TestDef.friendlyIcon = def.friendlyIcon;
                TestDef.height = 4.6f;
                TestDef.hostileIcon = def.hostileIcon;
                TestDef.iconRange = 100000;
                TestDef.iconSize = 1;
                TestDef.IsObstacle = true;
                TestDef.jsonKey = "TEST NAME";
                TestDef.length = 19;
                TestDef.manpower = 1;
                TestDef.mapIcon = def.mapIcon;
                TestDef.mapIconSize = 1.2f;
                TestDef.mapOrient = true;
                TestDef.mass = 16040;
                TestDef.maxEditorHeight = 10000;
                TestDef.minEditorHeight = 0;
                TestDef.radarSize = 0.0015f;
                TestDef.roleIdentity = def.roleIdentity;
                TestDef.spawnOffset = new Vector3(0, 2.3f, -1.4f);
                TestDef.typeIdentity = def.typeIdentity;
                TestDef.unitName = "TEST AIRCRAFT UNIT NAME";

                TestDef.unitPrefab = LoadAssetBundle();
                //TestDef.unitPrefab = def.unitPrefab;

                TestDef.value = 126;
                TestDef.visibleRange = 3500;
                TestDef.width = 14.3f;
            };
        }

        public static GameObject LoadAssetBundle()
        {
            // TODO actually make the asset bundle lol lmao
            string assetPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "multiroletest");
            Plugin.Logger.LogInfo($"[LoadAssetBundle] Loading prefab from {assetPath}...");
            var testBundle = AssetBundle.LoadFromFile(assetPath);

            var myPrefab = testBundle.LoadAsset<GameObject>("Multirole1") ?? throw new Exception($"Failed to locate object! Present assets: {string.Join(", ", testBundle.LoadAllAssets().Select(a => a.name))}");
            
            ((Unit)myPrefab.GetComponent<Aircraft>()).definition = TestDef;


            Plugin.Logger.LogInfo("[LoadAssetBundle] All prefabs loaded.");
            testBundle.Unload(false);
            return myPrefab;
        }
    }
}
