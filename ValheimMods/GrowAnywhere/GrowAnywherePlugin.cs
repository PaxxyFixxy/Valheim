using System;
using System.Collections.Generic;
using System.Linq;
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
    [HarmonyPatch(typeof(Plant), "UpdateHealth")]
    public static class ModifyPlantGrowBiom
    {
        private static void Prefix(ref Plant __instance)
        {
            Heightmap heightmap = Heightmap.FindHeightmap(__instance.transform.position);
            Heightmap.Biome biome = heightmap.GetBiome(__instance.transform.position);
            if (biome != (Heightmap.Biome.Mountain | Heightmap.Biome.Ocean))
            {
                __instance.m_biome = biome;
            }

        }
    }
    [HarmonyPatch(typeof(Plant), "HaveGrowSpace")]
    public static class ModifyPlantGrowSpace
    {
        private static void Postfix(ref Boolean __result)
        {
            __result = true;
        }
    }
}
