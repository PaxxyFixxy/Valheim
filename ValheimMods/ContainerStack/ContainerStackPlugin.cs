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
        private static readonly Harmony harmony = new Harmony("mod.containerstack");

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
    [HarmonyPatch(typeof(Piece), "Awake")]
    public static class AddContainerComponentPatch
    {
        private static void Prefix(ref Piece __instance)
        {
            var go = __instance.gameObject;
            var player = Player.m_localPlayer;
            PieceTable hammerTable = null;
            var tables = new List<PieceTable>();
            player.GetInventory().GetAllPieceTables(tables);
            foreach (var table in tables)
            {
                if (table.name.StartsWith("_Hammer"))
                {
                    hammerTable = table;
                }
            }
            var piece = hammerTable.m_pieces.First(p => p.name.StartsWith("piece_chest_wood"));
            if (go.name.Contains("wood_stack"))
            {
                var conToCopy = (Container)piece.GetComponent(typeof(Container));
                Container container = (Container)go.AddComponent(typeof(Container));
                container.m_name = "Wood Stack";
                container.m_privacy = conToCopy.m_privacy;
                container.m_checkGuardStone = conToCopy.m_checkGuardStone;
                container.m_autoDestroyEmpty = conToCopy.m_autoDestroyEmpty;
                container.m_defaultItems = conToCopy.m_defaultItems;
                container.m_open = conToCopy.m_open;
                container.m_closed = conToCopy.m_closed;
                container.m_openEffects = conToCopy.m_openEffects;
                container.m_closeEffects = conToCopy.m_closeEffects;
                FieldInfo inventoryField = typeof(Container).GetField("m_inventory", BindingFlags.NonPublic | BindingFlags.Instance);
                inventoryField.SetValue(container, new Inventory(container.m_name, conToCopy.m_bkg, 5, 3));

            }
            if (go.name.Contains("stone_pile"))
            {
                var conToCopy = (Container)piece.GetComponent(typeof(Container));
                Container container = (Container)go.AddComponent(typeof(Container));
                container.m_name = "Stone Pile";
                container.m_privacy = conToCopy.m_privacy;
                container.m_checkGuardStone = conToCopy.m_checkGuardStone;
                container.m_autoDestroyEmpty = conToCopy.m_autoDestroyEmpty;
                container.m_defaultItems = conToCopy.m_defaultItems;
                container.m_open = conToCopy.m_open;
                container.m_closed = conToCopy.m_closed;
                container.m_openEffects = conToCopy.m_openEffects;
                container.m_closeEffects = conToCopy.m_closeEffects;
                FieldInfo inventoryField = typeof(Container).GetField("m_inventory", BindingFlags.NonPublic | BindingFlags.Instance);
                inventoryField.SetValue(container, new Inventory(container.m_name, conToCopy.m_bkg, 5, 3));

            }
        }
    }
}
