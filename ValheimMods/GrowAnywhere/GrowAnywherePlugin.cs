using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace GrowAnywhere
{
    [BepInPlugin("org.paxx.plugins.growanywhere", "Grow Anywhere Plug-In", "1.0.0.0")]
    [BepInProcess("valheim.exe")]
    public class GrowAnywherePlugin : BaseUnityPlugin
    {
        private ConfigEntry<bool> configEnableMod;
        private static readonly Harmony harmony = new Harmony("mod.growanywhere");

        void Awake()
        {
            configEnableMod = Config.Bind("General.Toggles",
                                                "enableMod",
                                                true,
                                                "Whether or not to enable the patch");
            if (configEnableMod.Value)
            {
                harmony.PatchAll();
            }
        }
        private void OnDestroy()
        {
            if (configEnableMod.Value)
            {
                harmony.UnpatchAll();
            }
        }
    }
    [HarmonyPatch(typeof(ZNetScene), "Awake")]
    public static class ModifyPlantGrow
    {
        private static void Prefix(ref ZNetScene __instance)
        {
            foreach (var prefab in __instance.m_prefabs)
            {
                if (prefab.name.ToLower().Contains("sapling"))
                {
                    var plant = prefab.GetComponent<Plant>();
                    plant.m_biome = (Heightmap.Biome)25;
                    plant.m_growRadius = 0.0f;
                }
            }
        }
    }
}
