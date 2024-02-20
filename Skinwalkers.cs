using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using HarmonyLib;
using SkinwalkerMod;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace BetterSkinwalkers;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class Skinwalkers
{
    public static Dictionary<String, List<int>> player_clips_map;
    
    [HarmonyPatch(typeof(SkinwalkerBehaviour), "SetNextTime")]
    [HarmonyPrefix]
    public static bool SetNextTime(ref SkinwalkerBehaviour __instance)
    {
        if (SkinwalkerNetworkManager.Instance.VoiceLineFrequency.Value <= 0f)
        {
            __instance.nextTimeToPlayAudio = float.MaxValue;
        }
        else
        {
            if (__instance.ai is MaskedPlayerEnemy)
            {
                __instance.nextTimeToPlayAudio = Time.time +
                                                 Random.Range(3f, 10f) / SkinwalkerNetworkManager.Instance
                                                     .VoiceLineFrequency.Value;
            }
            else
            {
                __instance.nextTimeToPlayAudio = Time.time +
                                                 Random.Range(15f, 40f) / SkinwalkerNetworkManager.Instance
                                                     .VoiceLineFrequency.Value;
            }
        }
        return false;
    }
    [HarmonyPatch(typeof(SkinwalkerBehaviour), "AttemptPlaySound")]
    [HarmonyPrefix]
    public static bool AttemptPlaySound(ref SkinwalkerBehaviour __instance)
    {
        float num;
        if ((bool) (Object) __instance.ai && !__instance.ai.isEnemyDead)
        {
            Vector3 a = StartOfRound.Instance.localPlayerController.isPlayerDead ? StartOfRound.Instance.spectateCamera.transform.position : StartOfRound.Instance.localPlayerController.transform.position;
            if (StartOfRound.Instance == null || StartOfRound.Instance.localPlayerController == null || (num = Vector3.Distance(a, __instance.transform.position)) < 100.0)
            {
                AudioClip sample = __instance.ai is MaskedPlayerEnemy masked ? GetPlayerSpecificSample(masked.mimickingPlayer.voicePlayerState.Name) : SkinwalkerModPersistent.Instance.GetSample();
                if ((bool) sample)
                {
                    SkinwalkerLogger.Log(__instance.name + " played voice line 1");
                    __instance.audioSource.PlayOneShot(sample);
                }
                else
                    SkinwalkerLogger.Log(__instance.name + " played voice line 0");
            }
            else
                SkinwalkerLogger.Log(__instance.name + " played voice line no (too far away) " + num);
        }
        else
            SkinwalkerLogger.Log(__instance.name + " played voice line no (dead) EnemyAI: " + __instance.ai);
        
        return false;
    }
    
    // SkinwalkerModPersistent
    
    [HarmonyPatch(typeof(SkinwalkerModPersistent), "GetSample")]
    [HarmonyPrefix]
    public static bool GetSample(ref SkinwalkerModPersistent __instance, ref AudioClip __result)
    {
        while (__instance.cachedAudio.Count > 200)
            __instance.cachedAudio.RemoveAt(Random.Range(0, __instance.cachedAudio.Count - 125));
        if (__instance.cachedAudio.Count == 0)
        {
            __result = null;
            return false;
        }
        int index = Random.Range(0, __instance.cachedAudio.Count - 1);
        AudioClip sample = __instance.cachedAudio[index];
        __result = sample;
        __instance.cachedAudio.RemoveAt(index);
        foreach (var list in player_clips_map.Values)
        {
            if (list.Remove(sample.GetInstanceID()))
            {
                return false;
            }
        }
        return false;
    }
    
    internal static AudioClip GetPlayerSpecificSample(string player)
    {
        var instance = SkinwalkerModPersistent.Instance;
        while (instance.cachedAudio.Count > 200)
            instance.cachedAudio.RemoveAt(Random.Range(0, instance.cachedAudio.Count - 125));
        if (instance.cachedAudio.Count == 0 || !player_clips_map.ContainsKey(player))
            return null;
        List<int> cachedIds = player_clips_map.GetValueSafe(player);
        int index = Random.Range(0, cachedIds.Count - 1);
        int id = cachedIds[index];
        AudioClip sample = null;
        foreach (var a in instance.cachedAudio)
        {
            if (a.GetInstanceID() == id)
            {
                sample = a;
                break;
            }
        }
        instance.cachedAudio.Remove(sample);
        cachedIds.RemoveAt(index);
        return sample;
    }
    
    [HarmonyPatch(typeof(SkinwalkerModPersistent), "Update")]
    [HarmonyPrefix]
    internal static bool Update(ref SkinwalkerModPersistent __instance)
    {
        if (Time.realtimeSinceStartup > (double) __instance.nextTimeToCheckFolder)
        {
            __instance.nextTimeToCheckFolder = Time.realtimeSinceStartup + 8f;
            if (!Directory.Exists(__instance.audioFolder))
            {
                SkinwalkerLogger.Log("Audio folder not present. Don't worry about it, it will be created automatically when you play with friends. (" + __instance.audioFolder + ")");
                return false;
            }
            string[] files = Directory.GetFiles(__instance.audioFolder);
            SkinwalkerLogger.Log(string.Format("Got audio file paths ({0})", files.Length));
            foreach (string path in files)
                __instance.StartCoroutine(__instance.LoadWavFile(path, audioClip =>
                {
                    SkinwalkerModPersistent.Instance.cachedAudio.Add(audioClip);
                    var player = path.Split(["Dissonance_Diagnostics\\Output_"], StringSplitOptions.None)[1].Split('_')[0];
                    if (!player_clips_map.ContainsKey(player))
                        player_clips_map.Add(player, new List<int>());
                    player_clips_map.GetValueSafe(player).Add(audioClip.GetInstanceID());
                }));
        }
        if (Time.realtimeSinceStartup <= (double) __instance.nextTimeToCheckEnemies)
            return false;
        __instance.nextTimeToCheckEnemies = Time.realtimeSinceStartup + 5f;
        foreach (EnemyAI enemyAi in Object.FindObjectsOfType<EnemyAI>(true))
        {
            SkinwalkerLogger.Log("IsEnemyEnabled " + enemyAi.name + " " + __instance.IsEnemyEnabled(enemyAi));
            if (__instance.IsEnemyEnabled(enemyAi) && !enemyAi.TryGetComponent(out SkinwalkerBehaviour _))
                enemyAi.gameObject.AddComponent<SkinwalkerBehaviour>().Initialize(enemyAi);
        }
        return false;
    }
}