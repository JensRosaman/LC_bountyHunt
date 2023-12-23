
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using bountyHunt.misc;
using System.Reflection;
namespace bountyHunt.Patches;

public class Akbar : MonoBehaviour
{
    private static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("YourPatchClass");
    private static bool vestActive = true;
    
    
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class ExplosionPatch
    {
        
        private static Explosion bomb;
       // private static readonly FieldInfo mineActivated = AccessTools.Field(typeof(Landmine), "mineActivated");
        [HarmonyPatch(nameof(PlayerControllerB.DamagePlayer))]
        //[HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void spawn_explosion_on_death(ref PlayerControllerB __instance)
        {
            /*if (__instance.isCrouching)
            {
                vestActive = true;
            }
            else
            {
                vestActive = false;
            }*/
            if (vestActive)
            {
                try
                {
                    
                    if (__instance == null)
                    {
                        logger.LogError("playercontroller is null");
                        return;
                    }

                    if (bomb == null)
                    {
                        // Initialize _landmine if not already initialized
                        //_landmine = __instance.gameObject.AddComponent<Landmine>();
                        bomb = __instance.gameObject.AddComponent<Explosion>();
                    }
                    
                    if (bomb != null)
                    {
                        bomb.TriggerMineOnLocalClientByExiting();
                        bomb.Detonate(__instance.gameObject.transform.position + Vector3.up, spawnExplosionEffect:true);
                       // spawnExplosionTriggred = true;
                        /*mineActivated.SetValue(obj: _landmine, value: true);
                        MethodInfo TriggerMineOnLocalClientByExiting = typeof(Landmine).GetMethod("TriggerMineOnLocalClientByExiting", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (TriggerMineOnLocalClientByExiting != null)
                        {
                            _landmine.hasExploded = false;
                            TriggerMineOnLocalClientByExiting.Invoke(_landmine, null);
                            logger.LogInfo("spawining explosion");
                           
                        }*/
                    }

                }
                catch (TargetInvocationException e)
                {
                    logger.LogError(e.InnerException);
                    
                }

                
            }

        }
        
    }
}


