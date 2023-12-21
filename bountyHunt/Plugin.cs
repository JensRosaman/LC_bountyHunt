using System.Reflection;
using BepInEx;
using bountyHunt.Patches;
using HarmonyLib;

namespace bountyHunt
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class BountyHuntBase : BaseUnityPlugin
    {
        private const string modGUID = "21stories.bounty";
        public const string NAME = "olivMod";
        public const string VERSION = "1.0";
        private static BountyHuntBase instance; 
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            
            Harmony harmony = new Harmony(modGUID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            /*harmony.PatchAll(typeof(BountyHuntBase));
            harmony.PatchAll(typeof(Patches.Patches.PaymentPatch));
            harmony.PatchAll(typeof(Patches.Patches.enemyHitPatch));
            harmony.PatchAll(typeof(bountyHunt.Patches.Patches.KillPlayerPatch));
            harmony.PatchAll(typeof(Akbar.ExplosionPatch));*/
           // harmony.PatchAll(typeof(Akbar.LandmineSkips));
            //harmony.PatchAll(typeof(Patches.Patches.KillPlayerPatch));
        }
        
    }
}