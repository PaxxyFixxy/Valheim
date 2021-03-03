using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace UniversalItemStand
{
    [BepInPlugin("org.paxx.plugins.universalitemstand", "Universal Item Stand Plug-In", "1.0.0.0")]
    [BepInProcess("valheim.exe")]
    public class UniversalItemStandPlugin : BaseUnityPlugin
    {
        private ConfigEntry<bool> configEnableMod;
        private static readonly Harmony harmony = new Harmony("mod.universalitemstand");

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
    [HarmonyPatch(typeof(ItemStand), "CanAttach")]
    public static class ModifyAttachmentCondition
    {
        private static void Postfix(ref ItemStand __instance, ref ItemDrop.ItemData item, ref Boolean __result)
        {
            __result = true;
        }
    }
    [HarmonyPatch(typeof(ItemStand), "GetAttachPrefab")]
    public static class ModifyAttachmentPrefab
    {
        private static void Postfix(ref ItemStand __instance, ref GameObject item, ref GameObject __result)
        {
            GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(item.name);
            Transform transform = itemPrefab.transform.Find("attach");
            if ((bool)(UnityEngine.Object)transform)
            {
                __result = transform.gameObject;
            }
            else
            {
                List<Transform> transforms = new List<Transform>();
                foreach (Transform child in itemPrefab.transform)
                {
                    transforms.Add(child);

                }
                var foundTransform = transforms.FirstOrDefault<Transform>(t => t.gameObject != null);
                __result = foundTransform.gameObject;
            }

        }
    }
}

