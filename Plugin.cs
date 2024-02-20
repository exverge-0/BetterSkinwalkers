using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;

namespace BetterSkinwalkers
{
    [BepInPlugin("xyz.exverge.betterskinwalkers", "BetterSkinWalkers", "0.1.2")]
    [BepInDependency("RugbugRedfern.SkinwalkerMod")] // Who knew .RubbugRedfern was a domain? not me
    public class Plugin : BaseUnityPlugin
    {
        public readonly Harmony harmony = new("xyz.exverge.betterskinwalkers");

        // Skinwalkers: changed so that it can't remove all the audio clips if it exceeds 200 and must keep at least 75
        // Skinwalkers: Everyone can hear ghost girl
        // Skinwalkers: Mimics (masked) speak more often and only mimic the voice of the person they've controlled or are mimicking
        private void Awake()
        {
            Skinwalkers.player_clips_map = new Dictionary<String, List<int>>();
            harmony.PatchAll(typeof(Skinwalkers));
            // Plugin startup logic
            Logger.LogInfo("BetterSkinWalkers has loaded!");
        }
    }
}
