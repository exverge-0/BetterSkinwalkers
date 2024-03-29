using HarmonyLib;
using SkinwalkerMod;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

// ReSharper disable InconsistentNaming
namespace BetterSkinwalkers;

public class SkinwalkerBehavior
{
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
        if ((bool) (Object) __instance.ai && !__instance.ai.isEnemyDead && StartOfRound.Instance != null && StartOfRound.Instance.localPlayerController != null)
        {
            if (__instance.ai.gameObject.name == "DressGirl(Clone)" && Plugin.Instance.OnlyHauntedHearsGirl.Value)
            {
                DressGirlAI ai = (DressGirlAI) __instance.ai;
                if (ai.hauntingPlayer != StartOfRound.Instance.localPlayerController)
                {
                    SkinwalkerLogger.Log(__instance.name + " played voice line no (not haunted) EnemyAI: " + __instance.ai);
                    return false;
                }
                if (!ai.staringInHaunt && !ai.moveTowardsDestination)
                {
                    SkinwalkerLogger.Log(__instance.name + " played voice line no (not visible) EnemyAI: " + __instance.ai);
                    return false;
                }
            }
            Vector3 a = StartOfRound.Instance.localPlayerController.isPlayerDead ? StartOfRound.Instance.spectateCamera.transform.position : StartOfRound.Instance.localPlayerController.transform.position;
            if ((num = Vector3.Distance(a, __instance.transform.position)) < 100.0)
            {
                AudioClip sample = __instance.ai is MaskedPlayerEnemy masked ? SkinwalkerMod.GetPlayerSpecificSample(masked.mimickingPlayer.voicePlayerState.Name) : SkinwalkerModPersistent.Instance.GetSample();
                if ((bool) sample)
                {
                    SkinwalkerLogger.Log(__instance.name + " played voice line 1");
                    __instance.audioSource.PlayOneShot(sample);
                }
                else
                    SkinwalkerLogger.Log(__instance.name + " played voice line 0");
            }
            else if(__instance.ai is MaskedPlayerEnemy masked && StartOfRound.Instance.localPlayerController.holdingWalkieTalkie && Plugin.Random.Next(100)+1 <= Plugin.Instance.ChanceMimicUsesWalkie.Value)
            {
                AudioClip sample = SkinwalkerMod.GetPlayerSpecificSample(masked.mimickingPlayer.voicePlayerState.Name);
                if ((bool)sample)
                {
                    SkinwalkerLogger.Log(__instance.name + " played voice line 1 (walkie)");
                    bool played = false;
                    foreach (var walkie in WalkieTalkie.allWalkieTalkies)
                    {
                        if (walkie.playerHeldBy == StartOfRound.Instance.localPlayerController)
                        {
                            played = true;
                            walkie.thisAudio.PlayOneShot(sample);
                            break;
                        }
                    }
                    SkinwalkerLogger.Log(played ? __instance.name + " played voice line 1 (walkie)" : __instance.name + " played voice line 0 (walkie)");
                }
                else
                {
                    SkinwalkerLogger.Log(__instance.name + " played voice line 0");
                }
            }
            else
            {
                SkinwalkerLogger.Log(__instance.name + " played voice line no (too far away) " + num);
            }
        }
        else
            SkinwalkerLogger.Log(__instance.name + " played voice line no (dead) EnemyAI: " + __instance.ai);
        
        return false;
    }
}