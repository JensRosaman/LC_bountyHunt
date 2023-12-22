using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Logging;
using bountyHunt.misc;
using HarmonyLib;
using Microsoft.CodeAnalysis;
using Unity.Audio;
using UnityEngine;
namespace bountyHunt.Patches;


class EnemySpawnPatch
{
    private static Dictionary<string, Type> EnemyHash = new Dictionary<string, Type>
    {
        {"nutcracker",typeof(NutcrackerEnemyAI)},
        {"bracken",typeof(FlowermanAI)},
        {"hoard",typeof(HoarderBugAI)}
    };
    private static Type EnemyToSpawn;

    static EnemySpawnPatch()
    {
        TerminalAPI.AddNewCommand("Spawn",setEnemySpawn);

    }
    private static TerminalNode setEnemySpawn(string[] words)
    {
        TerminalNode node = ScriptableObject.CreateInstance<TerminalNode>();
        node.displayText = "wow";
        return node;
    }
    [HarmonyPrefix]
    [HarmonyPatch(typeof(RoundManager), "LoadNewLevel")]
    private static bool SpecifyEnemySpawn(ref SelectableLevel newLevel)
    {
        foreach (SpawnableEnemyWithRarity enemy in newLevel.Enemies)
        {
            if ((UnityEngine.Object)(object)enemy.enemyType.enemyPrefab.GetComponent<HoarderBugAI>() != (UnityEngine.Object)null)
            {
                enemy.rarity = 999;
                continue;
            }
            enemy.rarity = 0;
        }
        return true;
    }
}
  