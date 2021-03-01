using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ProcreationPlus
{
    [BepInPlugin("org.paxx.plugins.procreationplus", "Procreation Plus Plug-In", "1.0.0.0")]
    [BepInProcess("valheim.exe")]
    public class GrowAnywherePlugin : BaseUnityPlugin
    {
        private ConfigEntry<bool> configEnableMod;
        private static readonly Harmony harmony = new Harmony("mod.procreationplus");

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
    [HarmonyPatch(typeof(Procreation), "Procreate")]
    public static class ModifyMaxCreatures
    {
        private static void Prefix(ref Procreation __instance)
        {
            __instance.m_maxCreatures = 99;
        }
    }
}


