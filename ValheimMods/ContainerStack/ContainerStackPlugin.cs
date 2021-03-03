using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace ContainerStack
{
    [BepInPlugin("org.paxx.plugins.containerstack", "Container Stack Plug-In", "1.0.0.0")]
    [BepInProcess("valheim.exe")]
    public class ContainerStackPlugin : BaseUnityPlugin
    {
        private ConfigEntry<bool> configEnableMod;
        static public ConfigEntry<int> configContainerWidth;
        static public ConfigEntry<int> configContainerHeight;
        private static readonly Harmony harmony = new Harmony("mod.containerstack");

        void Awake()
        {
            configEnableMod = Config.Bind("General.Toggles",
                                                "enableMod",
                                                true,
                                                "Whether or not to enable the patch");
            configContainerWidth = Config.Bind("General.Values",
                                    "containerWidth",
                                    3,
                                    "The container width for wood stack and stone pile");
            configContainerHeight = Config.Bind("General.Values",
                                    "containerHeight",
                                    3,
                                    "The container height for wood stack and stone pile");
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
    public static class AddContainerComponentPatch
    {
        private static void Prefix(ref ZNetScene __instance)
        {
            UnityEngine.GameObject stackPrefab = null;
            UnityEngine.GameObject pilePrefab = null;
            UnityEngine.GameObject chestPrefab = null;

            foreach (var prefab in __instance.m_prefabs)
            {
                if (prefab.name.Contains("wood_stack"))
                {
                    stackPrefab = prefab;
                }
                if (prefab.name.Contains("stone_pile"))
                {
                    pilePrefab = prefab;
                }
                if (prefab.name.Contains("piece_chest_wood"))
                {
                    chestPrefab = prefab;
                }
            }

            var conToCopy = (Container)chestPrefab.GetComponent(typeof(Container));
            FieldInfo widthField = typeof(Container).GetField("m_width", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo heightField = typeof(Container).GetField("m_height", BindingFlags.NonPublic | BindingFlags.Instance);

            Container stackContainer = (Container)stackPrefab.GetComponent(typeof(Container));
            if (stackContainer == null)
            {
                stackContainer = (Container)stackPrefab.AddComponent(typeof(Container));
                stackContainer.m_width = 5;
                stackContainer.m_height = 3;
                stackContainer.m_name = "Wood Stack";
                stackContainer.m_privacy = conToCopy.m_privacy;
                stackContainer.m_checkGuardStone = conToCopy.m_checkGuardStone;
                stackContainer.m_autoDestroyEmpty = conToCopy.m_autoDestroyEmpty;
                stackContainer.m_defaultItems = conToCopy.m_defaultItems;
                stackContainer.m_open = conToCopy.m_open;
                stackContainer.m_closed = conToCopy.m_closed;
                stackContainer.m_openEffects = conToCopy.m_openEffects;
                stackContainer.m_closeEffects = conToCopy.m_closeEffects;
                
            }
            else
            {
                stackContainer.m_width = ContainerStackPlugin.configContainerWidth.Value;
                stackContainer.m_height = ContainerStackPlugin.configContainerHeight.Value;
            }
            Container pileContainer = (Container)pilePrefab.GetComponent(typeof(Container));

            if (pileContainer == null)
            {
                pileContainer = (Container)pilePrefab.AddComponent(typeof(Container));
                pileContainer.m_width = 5;
                pileContainer.m_height = 3;
                pileContainer.m_name = "Stone Pile";
                pileContainer.m_privacy = conToCopy.m_privacy;
                pileContainer.m_checkGuardStone = conToCopy.m_checkGuardStone;
                pileContainer.m_autoDestroyEmpty = conToCopy.m_autoDestroyEmpty;
                pileContainer.m_defaultItems = conToCopy.m_defaultItems;
                pileContainer.m_open = conToCopy.m_open;
                pileContainer.m_closed = conToCopy.m_closed;
                pileContainer.m_openEffects = conToCopy.m_openEffects;
                pileContainer.m_closeEffects = conToCopy.m_closeEffects;
            }
            else
            {
                stackContainer.m_width = ContainerStackPlugin.configContainerWidth.Value;
                stackContainer.m_height = ContainerStackPlugin.configContainerHeight.Value;
            }
        }
    }
}
