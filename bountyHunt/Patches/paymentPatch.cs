

using System;
using HarmonyLib;
using System.Collections;
using System.Linq;
using GameNetcodeStuff;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using BepInEx.Logging;
using Unity.Netcode;
using bountyHunt.misc;
using LethalLib;


namespace bountyHunt.Patches
{
    class NetworkSync : NetworkBehaviour
    {
        public static NetworkSync network;
        private void Start()
        {

            NetworkSync.network = this;

        }
              
        [ServerRpc(RequireOwnership = false)]
        public static void SyncCreditsServerRpc(int newCredits)
        {
            SyncCreditsClientRpc(newCredits);
        }
        [ClientRpc]
        private static void SyncCreditsClientRpc(int newCredits)
        {
            
            Terminal terminal = GameObject.Find("TerminalScript").GetComponent<Terminal>();
            terminal.groupCredits = newCredits;
        }
    }
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
        
        //TEST
       
        [HarmonyPatch(typeof(PlayerControllerB))]
        internal class KillPlayerPatch
        {
            
            [HarmonyPatch(nameof(PlayerControllerB.DamagePlayer))]
            [HarmonyPostfix]
            static void on_kill(ref PlayerControllerB __instance)
            {
                /*if (__instance.isCrouching)
                {
                    hitCount += 1;
                    logger.LogInfo("Hitcount updated");
                }*/
                hitCount += 1;
                logger.LogInfo("Hitcount updated");
            }
        }
        
        [HarmonyPatch(typeof(Terminal))]
        internal class PaymentPatch : NetworkBehaviour
        {
            
            [HarmonyPostfix]
            [HarmonyPatch("ParsePlayerSentence")]
            private static void CustomParser(ref Terminal __instance, ref TerminalNode __result)
            {
                string text = __instance.screenText.text.Substring(__instance.screenText.text.Length - __instance.textAdded);
                string[] words = text.Split(' ');
                TerminalNode node = ScriptableObject.CreateInstance<TerminalNode>();
                if (text.ToLower() == "bounty" || text.ToLower() == "bög")
                {
                    
                    node.displayText = "No hits done";
                    node.clearPreviousText = true;
                    if (hitCount > 0)//playerDict.Count > 0
                    {
                        NetworkSync network = new NetworkSync();
                        node.displayText = "credits succesfully payed out"; 
                         //(__instance.groupCredits += (hitCount * 10);
                         
                        NetworkSync.SyncCreditsServerRpc(hitCount * 10);
                        logger.LogInfo("paying out creds");
                    }
                    __result = node;
                }
                
                else if (words.Length > 0 && ( words[0].ToLower() == "calcDet" || words[0].ToLower() == "det"))
                {
                    string path;
                    if (words.Length >= 2)
                    {
                        path = words[1];
                        Matrix matrix = new Matrix(path: path);
                        node.displayText = matrix.calc_det().ToString();
                        node.clearPreviousText = true;
                        __result = node;
                    }
                    else
                    {
                        path = @"C:\Users\olive\Desktop\";


                        string displayText;

                        try
                        {
                            Matrix matrix = new Matrix(path: path);
                            displayText = matrix.calc_det().ToString();
                        }
                        catch (Exception ex)
                        {
                            displayText =  $"Error when calculating det: {ex.Message}";;
                        }


                        node.displayText = displayText;
                        node.clearPreviousText = true;
                        __result = node;
                    }
                }
                
            }
        }
    }
}