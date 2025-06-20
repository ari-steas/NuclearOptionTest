using System.IO;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using System.Xml.Linq;
using UnityEngine;

namespace NuclearOptionTest
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;

        public Harmony Harmony;

        private void Awake()
        {
            // Plugin startup logic

            Harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            Harmony.PatchAll(Assembly.GetExecutingAssembly());

            Logger = base.Logger;
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}
