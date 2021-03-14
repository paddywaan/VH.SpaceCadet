using Mono.Cecil.Cil;
using MonoMod.Cil;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VH.SpaceCadet.EquipmentLevelGate
{
    public static class Hooks
    {
        private static Dictionary<string, ItemDrop> itemLookup = new Dictionary<string, ItemDrop>();
        internal static void Init()
        {
            On.Player.ToggleEquiped += Player_ToggleEquiped;
            On.FejdStartup.Start += FejdStartup_Start;
        }

        private static void FejdStartup_Start(On.FejdStartup.orig_Start orig, FejdStartup self)
        {
            orig(self);
            foreach (var i in ObjectDB.instance.m_items)
            {
                var iDrop = i.GetComponent<ItemDrop>();
                if (iDrop.m_itemData.IsEquipable() && iDrop.m_itemData.m_shared.m_name.Contains("$item"))
                {
                    itemLookup[iDrop.m_itemData.m_shared.m_name] = iDrop;
                    //Main.log.LogDebug($"{i.name},{idata.name},{idata.m_itemData.m_shared.m_name},{idata.m_itemData.m_shared.m_itemType},{idata.m_itemData.m_shared.m_skillType}");
                    //Main.log.LogDebug($"comparing {iDrop.m_itemData.m_shared.m_skillType} to {i.name}");
                    if (iDrop.m_itemData.m_shared.m_skillType == Skills.SkillType.Swords && !i.name.Contains($"Sword")) Config.GeneralSettings.Bind<int>($"{Skills.SkillType.Run}", $"{i.name}", 0, "The minimum level which is required to equip the item.");
                    else Config.GeneralSettings.Bind<int>($"{iDrop.m_itemData.m_shared.m_skillType}", $"{i.name}", 0, "The minimum level which is required to equip the item.");
                }
            }
        }

        private static bool Player_ToggleEquiped(On.Player.orig_ToggleEquiped orig, Player self, ItemDrop.ItemData item)
        {
            if (item.m_equiped) return orig(self, item);
            BepInEx.Configuration.ConfigEntry<int> conf = null;
            foreach (var i in Config.GeneralSettings.Keys)
            {
                if (i.Key.Equals(itemLookup[item.m_shared.m_name].name))
                {
                    Config.GeneralSettings.TryGetEntry(i, out conf);
                }
                
            }
            if (conf != null)
            {
                Skills.SkillType type;
                if (Enum.TryParse<Skills.SkillType>(conf.Definition.Section, out type))
                {
                    var skill = self.m_skills.m_skillData[type];
                    Main.log.LogDebug($"Found LevelGate of {conf.Value} for {itemLookup[item.m_shared.m_name].name} checked against player's {type} level: {skill.m_level}");
                    if (conf.Value > skill.m_level)
                    {
                        self.Message(MessageHud.MessageType.Center, $"Your {type} skill prevents you from equipping the {Localization.instance.Localize(item.m_shared.m_name)}. Min skill required: {conf.Value}");
                        return false;
                    }
                    
                }
            }
            return orig(self, item);
        }
    }
}
