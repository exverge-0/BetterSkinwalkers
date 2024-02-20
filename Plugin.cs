using System;
using System.Collections.Generic;
using BepInEx;
using Dissonance.Config;
using HarmonyLib;

namespace BetterSkinwalkers
{
    [BepInPlugin("xyz.exverge.betterskinwalkers", "BetterSkinWalkers", "0.2.0")]
    [BepInDependency("RugbugRedfern.SkinwalkerMod")] // Who knew .RubbugRedfern was a domain?
    public class Plugin : BaseUnityPlugin
    {
        public readonly Harmony harmony = new("xyz.exverge.betterskinwalkers");
        
        private void Awake()
        {
            SkinwalkerMod.player_clips_map = new Dictionary<String, List<int>>();
            harmony.PatchAll(typeof(SkinwalkerBehavior));
            harmony.PatchAll(typeof(SkinwalkerMod));
            Logger.LogInfo("BetterSkinWalkers has loaded!");
        }
    }
}
