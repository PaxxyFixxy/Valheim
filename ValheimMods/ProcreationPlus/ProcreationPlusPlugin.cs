using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using System.Reflection;

namespace ProcreationPlus
{
    [BepInPlugin("org.paxx.plugins.procreationplus", "Procreation Plus Plug-In", "1.0.0.0")]
    [BepInProcess("valheim.exe")]
    public class ProcreationPlusPlugin : BaseUnityPlugin
    {
        private ConfigEntry<bool> configEnableMod;
        public static ConfigEntry<int> configMaxLox;
        public static ConfigEntry<int> configMaxBoars;
        private static readonly Harmony harmony = new Harmony("mod.procreationplus");

        void Awake()
        {
            configEnableMod = Config.Bind("General.Toggles",
                                                "enableMod",
                                                true,
                                                "Whether or not to enable the patch");
            configMaxLox = Config.Bind("General.Values",
                                    "maxLox",
                                    4,
                                    "The limit of lox within checking range");
            configMaxBoars = Config.Bind("General.Values",
                                    "maxBoars",
                                    5,
                                    "The limit of boars within checking range");
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
    public static class ModifyProcreation
    {
        private static void Prefix(ref Procreation __instance)
        {
            MethodInfo IsDue = typeof(Procreation).GetMethod("IsDue", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo ResetPregnancy = typeof(Procreation).GetMethod("ResetPregnancy", BindingFlags.NonPublic | BindingFlags.Instance);
            bool isDue = (bool)IsDue.Invoke(__instance, new object[] { });


            if (__instance.IsPregnant() && isDue)
            {
                ResetPregnancy.Invoke(__instance, new object[] { });
                var offspringPrefab = (GameObject)typeof(Procreation).GetField("m_offspringPrefab", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(__instance);
                var character = (Character)typeof(Character).GetField("m_character", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(__instance);
                GameObject gameObject = Object.Instantiate(offspringPrefab, __instance.transform.position - __instance.transform.forward * __instance.m_spawnOffset, Quaternion.LookRotation(-__instance.transform.forward, Vector3.up));
                
                if (gameObject.name.Contains("Lox"))
                {
                    Object.Destroy(gameObject.GetComponent<Procreation>());
                    gameObject.AddComponent<Growup>();
                    gameObject.GetComponent<Growup>().m_grownPrefab = offspringPrefab;
                    gameObject.GetComponent<Growup>().m_growTime = 3000f;
                    gameObject.transform.localScale -= new Vector3(0.7f, 0.7f, 0.7f);
                }
                Character component = gameObject.GetComponent<Character>();
                if ((bool)(Object)component)
                {
                    component.SetTamed(true);
                    component.SetLevel(Mathf.Max(__instance.m_minOffspringLevel, character.GetLevel()));
                }
                __instance.m_birthEffects.Create(gameObject.transform.position, Quaternion.identity, (Transform)null, 1f);
            }
        }
    }
    [HarmonyPatch(typeof(ZNetScene), "Awake")]
    public static class AddProcreationComponentPatch
    {
        private static void Prefix(ref ZNetScene __instance)
        {
            GameObject boarPrefab = null;
            GameObject loxPrefab = null;
            foreach (var prefab in __instance.m_prefabs)
            {
                if (prefab.name.ToLower().Equals("lox"))
                {
                    loxPrefab = prefab;
                }
                else if (prefab.name.ToLower().Equals("boar"))
                {
                    boarPrefab = prefab;
                    boarPrefab.GetComponent<Procreation>().m_maxCreatures = ProcreationPlusPlugin.configMaxBoars.Value;
                }
            }
            Procreation procreationToCopy = (Procreation)boarPrefab.GetComponent(typeof(Procreation));
            bool hasComp = (Procreation)loxPrefab.GetComponent(typeof(Procreation)) != null;
            if (!hasComp)
            {
                loxPrefab.AddComponent(typeof(Procreation));
            }
            loxPrefab.GetComponent<Procreation>().enabled = procreationToCopy.enabled;
            loxPrefab.GetComponent<Procreation>().hideFlags = procreationToCopy.hideFlags;
            loxPrefab.GetComponent<Procreation>().m_birthEffects = procreationToCopy.m_birthEffects;
            loxPrefab.GetComponent<Procreation>().m_loveEffects = procreationToCopy.m_loveEffects;
            loxPrefab.GetComponent<Procreation>().m_minOffspringLevel = procreationToCopy.m_minOffspringLevel;
            loxPrefab.GetComponent<Procreation>().m_offspring = loxPrefab;
            loxPrefab.GetComponent<Procreation>().m_maxCreatures = ProcreationPlusPlugin.configMaxLox.Value;
            loxPrefab.GetComponent<Procreation>().m_partnerCheckRange = 100;
            loxPrefab.GetComponent<Procreation>().m_pregnancyChance = procreationToCopy.m_pregnancyChance;
            loxPrefab.GetComponent<Procreation>().m_pregnancyDuration = procreationToCopy.m_pregnancyDuration;
            loxPrefab.GetComponent<Procreation>().m_requiredLovePoints = procreationToCopy.m_requiredLovePoints;
            loxPrefab.GetComponent<Procreation>().m_spawnOffset = procreationToCopy.m_spawnOffset;
            loxPrefab.GetComponent<Procreation>().m_totalCheckRange = procreationToCopy.m_totalCheckRange;
            loxPrefab.GetComponent<Procreation>().m_updateInterval = procreationToCopy.m_updateInterval;
            loxPrefab.GetComponent<Procreation>().tag = procreationToCopy.tag;
            loxPrefab.GetComponent<Procreation>().useGUILayout = procreationToCopy.useGUILayout;
        }
    }
}

