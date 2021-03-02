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
            UnityEngine.Debug.Log("PROCREATING!");
        }
        private static void Postfix(ref Procreation __instance)
        {
            
            var baseAI = (BaseAI)typeof(Procreation).GetField("m_baseAI", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var tameable = (Tameable)typeof(Procreation).GetField("m_tameable", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var myPrefab = (GameObject)typeof(Procreation).GetField("m_myPrefab", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var offspringPrefab = (GameObject)typeof(Procreation).GetField("m_offspringPrefab", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            UnityEngine.Debug.Log("Who?: " + myPrefab.name);
            UnityEngine.Debug.Log("Is Alerted?: " + baseAI.IsAlerted());
            UnityEngine.Debug.Log("Is Hungry?: " + tameable.IsHungry());
            UnityEngine.Debug.Log("Too Many Creatures?: " + (SpawnSystem.GetNrOfInstances(myPrefab, __instance.transform.position, __instance.m_totalCheckRange) + SpawnSystem.GetNrOfInstances(offspringPrefab, __instance.transform.position, __instance.m_totalCheckRange) >= __instance.m_maxCreatures));
            UnityEngine.Debug.Log("Partner Check Range?: " + __instance.m_partnerCheckRange);
            UnityEngine.Debug.Log("Partner in Range?: " + (SpawnSystem.GetNrOfInstances(myPrefab, __instance.transform.position, __instance.m_partnerCheckRange, procreationOnly: true) < 2));
            if (baseAI.IsAlerted() || (tameable.IsHungry() || SpawnSystem.GetNrOfInstances(myPrefab, __instance.transform.position, __instance.m_totalCheckRange) + SpawnSystem.GetNrOfInstances(offspringPrefab, __instance.transform.position, __instance.m_totalCheckRange) >= __instance.m_maxCreatures) || SpawnSystem.GetNrOfInstances(myPrefab, __instance.transform.position, __instance.m_partnerCheckRange, procreationOnly: true) < 2)
            {
                UnityEngine.Debug.Log("Pregnancy Condition met!");
            }
            else
            {
                UnityEngine.Debug.Log("Pregnancy Condition not met!");
            }
            if (__instance.IsPregnant())
            {
                UnityEngine.Debug.Log("Is Pregnant!");
            }
            if (!__instance.IsPregnant())
            {
                UnityEngine.Debug.Log("Is Not Pregnant!");
            }

        }
    }
    [HarmonyPatch(typeof(ZNetScene), "Awake")]
    public static class AddProcreationComponentPatch
    {
        private static void Prefix(ref ZNetScene __instance)
        {
            UnityEngine.GameObject boarPrefab = null;
            UnityEngine.GameObject loxPrefab = null;
            foreach (var prefab in __instance.m_prefabs)
            {

                if (prefab.name.ToLower().Equals("lox"))
                {
                    UnityEngine.Debug.Log("Found Lox Prefab");
                    loxPrefab = prefab;
                    UnityEngine.Debug.Log(loxPrefab.name);
                }
                if (prefab.name.ToLower().Equals("boar"))
                {
                    UnityEngine.Debug.Log("Found Boar Prefab");
                    boarPrefab = prefab;
                    UnityEngine.Debug.Log(boarPrefab.name);
                }
            }

            Procreation procreationToCopy = (Procreation)boarPrefab.GetComponent(typeof(Procreation));
            UnityEngine.Debug.Log("ToCopy: " + procreationToCopy.ToString());
            if (procreationToCopy != null)
            {
                UnityEngine.Debug.Log("Boar Procreation Ready");
            }
            bool hasComp = (Procreation)loxPrefab.GetComponent(typeof(Procreation)) != null;
            UnityEngine.Debug.Log("Has Component: " + hasComp);
            if (!hasComp)
            {
                UnityEngine.Debug.Log("Procreation was null, would fill");
                loxPrefab.AddComponent(typeof(Procreation));
            }
            loxPrefab.GetComponent<Procreation>().enabled = procreationToCopy.enabled;
            loxPrefab.GetComponent<Procreation>().hideFlags = procreationToCopy.hideFlags;
            loxPrefab.GetComponent<Procreation>().m_birthEffects = procreationToCopy.m_birthEffects;
            loxPrefab.GetComponent<Procreation>().m_loveEffects = procreationToCopy.m_loveEffects;
            loxPrefab.GetComponent<Procreation>().m_minOffspringLevel = procreationToCopy.m_minOffspringLevel;
            loxPrefab.GetComponent<Procreation>().m_offspring = loxPrefab;
            loxPrefab.GetComponent<Procreation>().m_maxCreatures = procreationToCopy.m_maxCreatures;
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


