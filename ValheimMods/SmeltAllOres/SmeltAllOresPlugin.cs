using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace SmeltAllOres
{
    [BepInPlugin("org.paxx.plugins.smeltalloresplugin", "Smelt All Ores Plug-In", "1.0.0.0")]
    [BepInProcess("valheim.exe")]
    public class SmeltAllOresPlugin : BaseUnityPlugin
    {
        private ConfigEntry<bool> configEnableMod;
        private static readonly Harmony harmony = new Harmony("mod.smeltallores");

        void Awake()
        {
            configEnableMod = Config.Bind("General.Toggles",
                                                "enableMod",
                                                true,
                                                "Whether or not to enable the patch");
            harmony.PatchAll();
        }
        private void OnDestroy()
        {
            harmony.UnpatchAll();
        }
    }

    [HarmonyPatch(typeof(Smelter), "Awake")]
    public static class ModifyAllowedOres
    {
        [HarmonyPatch(typeof(Smelter), "Awake")]
        static void Postfix(ref Smelter __instance)
        {
            if (__instance.m_name != "$piece_blastfurnace")
            {
                return;
            }

            ObjectDB instance = ObjectDB.instance;
            List<ItemDrop> materials = instance.GetAllItems(ItemDrop.ItemData.ItemType.Material, "");
            Dictionary<string, ItemDrop> metals = new Dictionary<string, ItemDrop>
            {
                { "$item_copperore", null },
                { "$item_copper", null },
                { "$item_ironscrap", null },
                { "$item_iron", null },
                { "$item_tinore", null },
                { "$item_tin", null },
                { "$item_silverore", null },
                { "$item_silver", null }
            };
            foreach (ItemDrop material in materials)
            {
                if (metals.Keys.Contains(material.m_itemData.m_shared.m_name))
                {
                    metals[material.m_itemData.m_shared.m_name] = material;
                }
            }

            List<Smelter.ItemConversion> conversions = new List<Smelter.ItemConversion>()
            {
                new Smelter.ItemConversion{ m_from = metals["$item_copperore"], m_to = metals["$item_copper"]},
                new Smelter.ItemConversion{ m_from = metals["$item_tinore"], m_to = metals["$item_tin"]},
                new Smelter.ItemConversion{ m_from = metals["$item_ironscrap"], m_to = metals["$item_iron"]},
                new Smelter.ItemConversion{ m_from = metals["$item_silverore"], m_to = metals["$item_silver"]}
            };

            foreach (Smelter.ItemConversion conversion in conversions)
            {
                __instance.m_conversion.Add(conversion);
            }
        }
    }
}
