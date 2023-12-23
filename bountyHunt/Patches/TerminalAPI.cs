using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BepInEx.Logging;
using HarmonyLib;
using MonoMod.Cil;
using UnityEngine;
using bountyHunt;
namespace bountyHunt.misc;


public class TerminalAPI
{
    private static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("BountyHunt");
    private static TerminalAPI _instance;
    private static Terminal _terminal = GameObject.Find("TerminalScript").GetComponent<Terminal>();
    private static TerminalNode node;
    private static string[] words;
    public delegate TerminalNode ConditionCheck(string[] words);
    private static Dictionary<string, ConditionCheck> commandTable = new();
    public static int maxCharactersToType;
    public static TerminalAPI instance
    {
        get
        {
            // If the instance is null, create a new instance
            if (_instance == null)
            {
                _instance = new TerminalAPI();
            }
            return _instance;
        }
    }

    /*static TerminalAPI()
    {
       AddNewCommand("hej",Func<>);
    }

    public static TerminalNode fuck(string[] words)
    {
        static TerminalNode node = ScriptableObject.CreateInstance<TerminalNode>();
    }*/
   public static void AddNewCommand(string command,ConditionCheck func)
    {
      
        if (!commandTable.TryAdd(command,func))
        {
            logger.LogError("Command already added");
        }
        else
        {
            logger.LogInfo(commandTable.Count.ToString());
        }
    }

   public static void RemoveCommand(string command)
   {
       commandTable.Remove(command);
   }
   
   public static void AddCredits(int creditsToAdd)
   { 
        int newCredits = creditsToAdd + _terminal.groupCredits;
       _terminal.SyncGroupCreditsClientRpc(newCredits,_terminal.numberOfItemsInDropship);
       logger.LogInfo("Credits payed");
   }
    [HarmonyPatch(typeof(Terminal))]
    class ParsePatch
    {
        /*[HarmonyPrefix]
        [HarmonyPatch("ParsePlayerSentence")]
        private static void charLimitPatch(ref TerminalNode __instance)
        {
            __instance.maxCharactersToType = maxCharactersToType;
        }*/
        
        [HarmonyPrefix]
        [HarmonyPatch("ParsePlayerSentence")]
        private static bool CustomParser(ref Terminal __instance, ref TerminalNode __result)
        {
            logger.LogInfo(commandTable.Count.ToString());
            logger.LogInfo("patching parse");
            string text = __instance.screenText.text.Substring(__instance.screenText.text.Length - __instance.textAdded); // gets the substring of the text added textAdded is just the lenght
            words = text.Split(' ');
            if (words.Length == 0)
            {
                return true;
            }
            if (commandTable.ContainsKey(words[0].ToLower()))
            {
               node = commandTable[words[0]](words);
               node.clearPreviousText = true;
               logger.LogInfo("changing node");
               node.maxCharactersToType = maxCharactersToType;
               __result = node;
               return false;
            }

            return true;
        }
    }
}