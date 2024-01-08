using BepInEx;
using BepInEx.Configuration;
using GameNetcodeStuff;
using HarmonyLib;
using Jotunn.Utils;
using System.Reflection;
using UnityEngine;
using BepInEx.Bootstrap;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using DunGen;
using UnityEngine.UIElements;

namespace CustomHUD
{
    [BepInPlugin("me.eladnlg.customhud", "Elads HUD", "1.1.0")]
    [BepInDependency("com.zduniusz.lethalcompany.lbtokg", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin instance;
        public AssetBundle assets;
        public GameObject HUD;
        public static bool shouldDoKGConversion = false;

        internal static ConfigEntry<PocketFlashlightOptions> pocketedFlashlightDisplayMode;
        internal static ConfigEntry<StaminaTextOptions> detailedStamina;
        internal static ConfigEntry<bool> displayTimeLeft;
        internal static ConfigEntry<float> hudScale;
        internal static ConfigEntry<bool> autoHideHealthbar;
        internal static ConfigEntry<float> healthbarHideDelay;

        private void Awake()
        {
            if (instance != null) {
                throw new System.Exception("what the cuck??? more than 1 plugin instance.");
            }
            instance = this;
            // Plugin startup logic
            hudScale = Config.Bind("General", "HUDScale", 1f, "The size of the HUD.");
            autoHideHealthbar = Config.Bind("General", "HideHealthbarAutomatically", true, "Should the healthbar be hidden after not taking damage for a while.");
            healthbarHideDelay = Config.Bind("General", "HealthbarHideDelay", 4f, "The amount of time before the healthbar starts fading away.");
            pocketedFlashlightDisplayMode = Config.Bind("General", "FlashlightBattery", PocketFlashlightOptions.Separate,
@"How the flashlight battery is displayed whilst unequipped.
Disabled - Flashlight battery will not be displayed.
Vanilla - Flashlight battery will be displayed when you don't have a battery-using item equipped.
Separate - Flashlight battery will be displayed using a dedicated panel. (recommended)");
            detailedStamina = Config.Bind("General", "DetailedStamina", StaminaTextOptions.PercentageOnly, 
@"What the stamina text should display.
Disabled - The stamina text will be hidden.
PercentageOnly - Only the percentage will be displayed. (recommended)
Full - Both percentage and rate of gain/loss will be displayed.");
            displayTimeLeft = Config.Bind("General", "DisplayTimeLeft", true, "Should the uses/time left for a battery-using item be displayed.");

            Logger.LogInfo($"Plugin Elad's HUD is loaded!");

            // load hud
            assets = AssetUtils.LoadAssetBundleFromResources("customhud", typeof(PlayerPatches).Assembly);
            HUD = assets.LoadAsset<GameObject>("PlayerInfo");

            // patch game
            var harmony = new Harmony("me.eladnlg.customhud");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private void Start()
        {
            Logger.LogInfo(Chainloader.PluginInfos.Count + " plugins loaded");
            foreach (var chain in Chainloader.PluginInfos)
            {
                Logger.LogInfo("Plugin GUID: " + chain.Value.Metadata.GUID);
            }
            shouldDoKGConversion = Chainloader.PluginInfos.Any(pair => pair.Value.Metadata.GUID == "com.zduniusz.lethalcompany.lbtokg");
        }
    }

    [HarmonyPatch(typeof(HUDManager))]
    public class HUDPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch("Awake")]
        static void Awake_Postfix(HUDManager __instance)
        {
            HUDElement[] elements = __instance.GetPrivateField<HUDElement[]>("HUDElements");
            elements[2].canvasGroup.alpha = 0;
            GameObject HUD = Object.Instantiate(Plugin.instance.HUD, elements[2].canvasGroup.transform.parent);
            HUD.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f) * Plugin.hudScale.Value;
            elements[2].canvasGroup = HUD.GetComponent<CanvasGroup>();
        }
    }

    [HarmonyPatch(typeof(PlayerControllerB))]
    public static class PlayerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch("LateUpdate")]
        static void LateUpdate_Prefix(PlayerControllerB __instance)
        {
            if (!__instance.IsOwner || (__instance.IsServer && !__instance.isHostPlayerObject))
                return;

            if (CustomHUD_Mono.instance == null)
                return;

            CustomHUD_Mono.instance.UpdateFromPlayer(__instance);
        }
    }
}
