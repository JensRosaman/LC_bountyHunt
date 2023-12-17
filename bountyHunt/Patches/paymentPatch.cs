

using HarmonyLib;
using System.Collections;
using System.Linq;
using GameNetcodeStuff;
using System.Collections.Generic;
using GameNetcodeStuff;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using BepInEx.Logging;
using Unity.Netcode;


namespace bountyHunt.Patches
{
    class Patches
    {
        private static Dictionary<PlayerControllerB, int> playerDict = new Dictionary<PlayerControllerB, int>();
        private static int hitCount { get; set; }
        private static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("YourPatchClass");
        [HarmonyPatch(typeof(EnemyAI))]
        internal class enemyHitPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch("HitEnemy")]
            private static void PrefixHitEnemy(ref int force,ref PlayerControllerB playerWhoHit, ref bool playHitSFX)
            {
                hitCount += 1;
                logger.LogInfo("Hitcount updated");
                if (playerDict.ContainsKey(playerWhoHit))
                {
                    playerDict[playerWhoHit] += 1;
                }
                else
                {
                    playerDict.Add(playerWhoHit, 1);
                }
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void SyncCreditsServerRpc(int newCredits)
        {
            SyncCreditsClientRpc(newCredits);
        }
        
        [ClientRpc]
        private void SyncCreditsClientRpc(int newCredits)
        {
            Terminal terminal = GameObject.Find("TerminalScript").GetComponent<Terminal>();
            terminal.groupCredits = newCredits;
        }

        //TEST
        /*
        [HarmonyPatch(typeof(PlayerControllerB))]
        internal class KillPlayerPatch
        {
            
            [HarmonyPatch(nameof(PlayerControllerB.KillPlayer))]
            [HarmonyPostfix]
            static void on_kill(ref PlayerControllerB __instance)
            {
                if (__instance.isCrouching)
                {
                    hitCount += 1;
                    logger.LogInfo("Hitcount updated");
                }
            }
        }
        */
        [HarmonyPatch(typeof(Terminal))]
        internal class PaymentPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch("ParsePlayerSentence")]
            private static void CustomParser(ref Terminal __instance, ref TerminalNode __result)
            {
                string text =
                    __instance.screenText.text.Substring(__instance.screenText.text.Length - __instance.textAdded);
                if (text.ToLower() == "bounty" || text.ToLower() == "bög")
                {
                    TerminalNode node = new TerminalNode();
                    node.displayText = "Nothing in the list";
                    node.clearPreviousText = true;
                    if (hitCount > 0)//playerDict.Count > 0
                    {
                        
                        node.displayText = "credits succesfully payed out"; 
                         (__instance.groupCredits += (hitCount * 10);
                        __instance.SyncGroupCreditsServerRpc();
                        logger.LogInfo("paying out creds");
                    }
                    __result = node;
                }
            }
        }
    }
}