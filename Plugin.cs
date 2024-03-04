using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace BetterSkinwalkers
{
    [BepInPlugin("xyz.exverge.betterskinwalkers", "BetterSkinWalkers", "0.2.1")]
    [BepInDependency("RugbugRedfern.SkinwalkerMod")] // Who knew .RubbugRedfern was a domain?
    public class Plugin : BaseUnityPlugin
    {
        private readonly Harmony harmony = new("xyz.exverge.betterskinwalkers");

        internal static Plugin Instance;
        
        internal ConfigEntry<bool> OnlyHauntedHearsGirl;
        
        private void Awake()
        {
            Instance = this;
            OnlyHauntedHearsGirl = Config.Bind(
                "General",
                  "OnlyHauntedHearsGirl",
                false,
                "Changes whether or not everyone can hear ghost girl or if only the haunted person can.");
            SkinwalkerMod.player_clips_map = new Dictionary<String, List<int>>();
            harmony.PatchAll(typeof(SkinwalkerBehavior));
            harmony.PatchAll(typeof(SkinwalkerMod));
            Logger.LogInfo("BetterSkinWalkers has loaded!");
        }
    }
}
