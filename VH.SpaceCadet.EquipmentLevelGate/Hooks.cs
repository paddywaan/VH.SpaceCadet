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
        private readonly static Dictionary<string, ItemDrop> itemLookup = new Dictionary<string, ItemDrop>();
        internal static void Init()
        {
            On.Humanoid.EquipItem += Humanoid_EquipItem;
            On.FejdStartup.Start += FejdStartup_Start;
            
        }

        private static bool Humanoid_EquipItem(On.Humanoid.orig_EquipItem orig, Humanoid self, ItemDrop.ItemData item, bool triggerEquipEffects)
        {
            try
            {
                if (!self.IsPlayer() || item.m_equiped) return orig(self, item, triggerEquipEffects);
                BepInEx.Configuration.ConfigEntry<int> conf = null;
                foreach (var i in Config.GeneralSettings.Keys)
                {
                    if (itemLookup.ContainsKey(item.m_shared.m_name) && i.Key.Equals(itemLookup[item.m_shared.m_name].name))
                    {
                        Config.GeneralSettings.TryGetEntry(i, out conf);
                    }
                }
                if (conf != null)
                {
                    if (Enum.TryParse<Skills.SkillType>(conf.Definition.Section, out Skills.SkillType type))
                    {
                        var skill = self.GetSkills().m_skillData[type];
                        Main.log.LogDebug($"Found LevelGate of {conf.Value} for {itemLookup[item.m_shared.m_name].name} checked against player's {type} level: {skill.m_level}");
                        if (conf.Value > skill.m_level)
                        {
                            self.Message(MessageHud.MessageType.Center, $"Your {type} skill prevents you from equipping the {Localization.instance.Localize(item.m_shared.m_name)}. Min skill required: {conf.Value}");
                            return true;
                        }

                    }
                }
                return orig(self, item, triggerEquipEffects);
            } catch (Exception ex)
            {
                Main.log.LogError(ex);
                return orig(self, item, triggerEquipEffects);
            }
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
                    var isSwordNotContainsSword = iDrop.m_itemData.m_shared.m_skillType == Skills.SkillType.Swords && !i.name.Contains($"Sword"); //Filter out non swords (armors) from the default type (sword)
                    Config.GeneralSettings.Bind<int>($"{(isSwordNotContainsSword ? Skills.SkillType.Run : iDrop.m_itemData.m_shared.m_skillType)}", $"{i.name}", 0, "The minimum level which is required to equip the item.");
                }
            }
        }
    }
}
