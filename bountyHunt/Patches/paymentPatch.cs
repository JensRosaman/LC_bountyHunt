

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
    /*class NetworkSync : NetworkManager
    {
        public static NetworkSync network;
        private void Start()
        {

            network = this;

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
    }*/
    
   internal class Patches
    {
        private static int hitCount { get; set; }
        private static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("BountyHunt");
        private static Terminal _terminal;
        private static int creditMultiplier = 10;


        static Patches()
        {
            TerminalAPI.AddNewCommand("calcdet",calcDet);
            TerminalAPI.AddNewCommand("payCreds",payCreds);
        }

        public static TerminalNode calcDet(string[] words)
        {
            TerminalNode node = ScriptableObject.CreateInstance<TerminalNode>();
            string path;
            if (words.Length >= 2)
            {
                path = words[1];
                Matrix matrix = new Matrix(path: path);
                node.displayText = matrix.calc_det().ToString();
                node.clearPreviousText = true;

                return node;
            }
        
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
            return node;
            
        }

        public static TerminalNode payCreds(string[] words)
        {
            TerminalNode node = ScriptableObject.CreateInstance<TerminalNode>();
            node.displayText = "No hits done";
            node.clearPreviousText = true;
            if (hitCount > 0)//playerDict.Count > 0
            {
                TerminalAPI.AddCredits(creditMultiplier * hitCount);
                node.displayText = "credits succesfully payed out"; 
            }
            return node;
        }
        
        
        [HarmonyPatch(typeof(EnemyAI))]
        internal class EnemyHitPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch("HitEnemy")]
            private static void PrefixHitEnemy()
            {
                
                TerminalAPI.AddCredits(creditMultiplier * hitCount);
                
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
    }
}