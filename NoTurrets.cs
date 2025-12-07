using System.Linq;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LobbyCompatibility.Attributes;
using LobbyCompatibility.Enums;

namespace NoTurrets;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("BMX.LobbyCompatibility", BepInDependency.DependencyFlags.HardDependency)]
[LobbyCompatibility(CompatibilityLevel.Everyone, VersionStrictness.None)]
public class NoTurrets : BaseUnityPlugin
{
    public static NoTurrets Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static Harmony? Harmony { get; set; }

    private void Awake()
    {
        Logger = base.Logger;
        Instance = this;

        Patch();

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }

    private static void PluginLogInfoWithPrefix(string content)
    {
        Logger.LogInfo($"[EVIL] {content}");
    }

    [HarmonyPatch(typeof(RoundManager))]
    internal class TurretPatch
    {
        [HarmonyPatch("SpawnMapObjects")]
        [HarmonyPrefix]
        static void RemoveAllTurrets(RoundManager __instance)
        {
            if (__instance.currentLevel.spawnableMapObjects.Length <= 0)
            {
                PluginLogInfoWithPrefix("No enemies to spawn!");
                return;
            }

            PluginLogInfoWithPrefix("Trying to remove turrets from the game...");
            var spawnableEnemies = __instance.currentLevel.spawnableMapObjects.ToList();
            int originalListLength = spawnableEnemies.Count;

            spawnableEnemies.RemoveAll(x => x.prefabToSpawn.name.Contains("Turret"));
            __instance.currentLevel.spawnableMapObjects = spawnableEnemies.ToArray();

            if (spawnableEnemies.Count != originalListLength)
            {
                PluginLogInfoWithPrefix($"Successfully removed turrets from the game yay! {originalListLength} != {spawnableEnemies.Count}");
            }
            else
            {
                PluginLogInfoWithPrefix("No turrets were needed to be removed!");
            }
        }
    }

    internal static void Patch()
    {
        Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

        Logger.LogDebug("Patching...");

        Harmony.PatchAll();

        Logger.LogDebug("Finished patching!");
    }

    internal static void Unpatch()
    {
        Logger.LogDebug("Unpatching...");

        Harmony?.UnpatchSelf();

        Logger.LogDebug("Finished unpatching!");
    }
}
