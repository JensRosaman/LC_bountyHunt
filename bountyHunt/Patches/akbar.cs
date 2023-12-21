
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using bountyHunt.misc;
using System.Reflection;
using UnityEngine;
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
        
       // [HarmonyPatch(typeof(GameController))]
        [HarmonyPatch("HandleButtonPress")]
        public static class CustomButtonPressPatch
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                if (Input.GetButtonDown("CustomButton"))
                {
                    // Your custom functionality here
                    Debug.Log("Custom button pressed!");
                }
            }
        }
    }
    

  
   /*
   [HarmonyPatch(typeof(Landmine))]
   internal class LandmineSkips
   {
       
       [HarmonyPrefix]
       [HarmonyPatch("Update")]
       static bool UpdateSkip()
       {
           if (ExplosionPatch.spawnExplosionTriggred)
           {
               // This statement will be called before the original method.
               // Returning false will skip the execution of the original method.
               return false;
           }
           
           return true;
       }
       
       [HarmonyPrefix]
       [HarmonyPatch(nameof(Landmine.SetOffMineAnimation))]
       static bool SetOffMineAnimationSkip(ref Landmine __instance)
       {
           if (ExplosionPatch.spawnExplosionTriggred)
           {
               __instance.hasExploded = true;
               // This statement will be called before the original method.
               // Returning false will skip the execution of the original method.
               return false;
           }
           
           return true;
       }
       [HarmonyPrefix]
       [HarmonyPatch("Start")]
       static bool startSkip()
       {
          
           if (ExplosionPatch.spawnExplosionTriggred)
           {
               // This statement will be called before the original method.
               // Returning false will skip the execution of the original method.
               return false;
           }
           
           return true;
       }
   }*/
  
}


