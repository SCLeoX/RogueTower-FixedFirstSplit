using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RogueTower_FixedFirstSplit
{
    [BepInPlugin("me.tepis.roguetower.fixedfirstsplit", "Fixed First Split", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private ConfigEntry<int> firstSplitLevelConfig;
        private ConfigEntry<bool> forceFirstSplitAtConfiguredLevelConfig;
        internal static ManualLogSource logger;
        public static Plugin instance;
        private bool shouldForceSpawnSplitOnceAllowed;
        private static GameObject[] boostedLTtiles = null;
        private static GameObject[] boostedLRtiles = null;
        private static GameObject[] boostedTRtiles = null;
        private static GameObject[] boostedLTRtiles = null;
        private static void EnsureInitializeBoostedTileSets(GameObject[] LTtiles, GameObject[] LRtiles, GameObject[] TRtiles, GameObject[] LTRtiles)
        {
            if (boostedLTtiles != null)
            {
                return;
            }
            boostedLTtiles = BoostTileSet(LTtiles);
            boostedLRtiles = BoostTileSet(LRtiles);
            boostedTRtiles = BoostTileSet(TRtiles);
            boostedLTRtiles = BoostTileSet(LTRtiles);
        }
        private static GameObject[] BoostTileSet(GameObject[] original)
        {
            const int boostTimes = 1000;
            GameObject[] boosted = new GameObject[original.Length * boostTimes];
            int i = 0;
            foreach (GameObject tile in original)
            {
                for (int j = 0; j < boostTimes; j++)
                {
                    boosted[i] = tile;
                    i++;
                }
            }
            return boosted;
        }
        private void WaveStarted(int previousPathCount)
        {
            StartCoroutine(WaveStartedCo(previousPathCount));
        }
        private IEnumerator WaveStartedCo(int previousPathCount)
        {
            yield return new WaitForSeconds(0.1f);
            if (previousPathCount != SpawnManager.instance.tileSpawnUis.Count)
            {
                logger.LogInfo($"Observed path count change from {previousPathCount} to {SpawnManager.instance.tileSpawnUis.Count}.");
                instance.shouldForceSpawnSplitOnceAllowed = false;
            }
        }
        private void Awake()
        {
            logger = Logger;
            instance = this;

            firstSplitLevelConfig = Config.Bind(
                "General",
                "First_Split_Level",
                20,
                "The level for first split to occur. Due to game restrictions, this value cannot be lower than 4.");

            forceFirstSplitAtConfiguredLevelConfig = Config.Bind(
                "General",
                "Force_First_Split_At_Configured_Level",
                true,
                "Whether to force a split the level configfured in `First_Split_Level`. If set to false, the level configured in `First_Split_Level` will be the first level that a split may spawn.");

            new Harmony("me.tepis.roguetower.lesssplitsrng").PatchAll();
            logger.LogInfo($"Plugin wt.tepis.roguetower.lesssplitsrng is loaded!");
        }
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        private void OnDisable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "GameScene")
            {
                return;
            }
            shouldForceSpawnSplitOnceAllowed = forceFirstSplitAtConfiguredLevelConfig.Value;
        }

        public enum ShouldSplit
        {
            FORCE_YES,
            FORCE_NO,
            DO_NOT_FORCE,
        }

        public ShouldSplit ShouldSpawnSplit()
        {
            int actualLevel = SpawnManager.instance.level + 1;
            if (actualLevel < firstSplitLevelConfig.Value)
            {
                return ShouldSplit.FORCE_NO;
            }
            if (shouldForceSpawnSplitOnceAllowed)
            {
                return ShouldSplit.FORCE_YES;
            }
            return ShouldSplit.DO_NOT_FORCE;
        }

        [HarmonyPatch(typeof(TileManager), nameof(TileManager.SpawnNewTile))]
        class TileManager_SpawnNewTile
        {
            class State
            {
                public int pathCount;
                public GameObject[] Ltiles;
                public GameObject[] Ttiles;
                public GameObject[] Rtiles;
                public GameObject[] LTtiles;
                public GameObject[] LRtiles;
                public GameObject[] TRtiles;
                public GameObject[] LTRtiles;
            }

            static AccessTools.FieldRef<TileManager, GameObject[]> LtilesRef = AccessTools.FieldRefAccess<TileManager, GameObject[]>("Ltiles");
            static AccessTools.FieldRef<TileManager, GameObject[]> TtilesRef = AccessTools.FieldRefAccess<TileManager, GameObject[]>("Ttiles");
            static AccessTools.FieldRef<TileManager, GameObject[]> RtilesRef = AccessTools.FieldRefAccess<TileManager, GameObject[]>("Rtiles");
            static AccessTools.FieldRef<TileManager, GameObject[]> LTtilesRef = AccessTools.FieldRefAccess<TileManager, GameObject[]>("LTtiles");
            static AccessTools.FieldRef<TileManager, GameObject[]> LRtilesRef = AccessTools.FieldRefAccess<TileManager, GameObject[]>("LRtiles");
            static AccessTools.FieldRef<TileManager, GameObject[]> TRtilesRef = AccessTools.FieldRefAccess<TileManager, GameObject[]>("TRtiles");
            static AccessTools.FieldRef<TileManager, GameObject[]> LTRtilesRef = AccessTools.FieldRefAccess<TileManager, GameObject[]>("LTRtiles");

            static void Prefix(TileManager __instance, out State __state)
            {
                ShouldSplit shouldSplit = instance.ShouldSpawnSplit();
                logger.LogInfo($"Using shouldSplit strategy {Enum.GetName(typeof(ShouldSplit), shouldSplit)}.");
                if (shouldSplit == ShouldSplit.DO_NOT_FORCE)
                {
                    __state = null;
                    return;
                }
                State state = new();
                state.pathCount = SpawnManager.instance.tileSpawnUis.Count;
                state.Ltiles = LtilesRef(__instance);
                state.Ttiles = TtilesRef(__instance);
                state.Rtiles = RtilesRef(__instance);
                state.LTtiles = LTtilesRef(__instance);
                state.LRtiles = LRtilesRef(__instance);
                state.TRtiles = TRtilesRef(__instance);
                state.LTRtiles = LTRtilesRef(__instance);
                __state = state;
                if (shouldSplit == ShouldSplit.FORCE_YES)
                {
                    LtilesRef(__instance) = new GameObject[] { state.Ltiles[UnityEngine.Random.Range(0, state.Ltiles.Length)] };
                    TtilesRef(__instance) = new GameObject[] { state.Ttiles[UnityEngine.Random.Range(0, state.Ttiles.Length)] };
                    RtilesRef(__instance) = new GameObject[] { state.Rtiles[UnityEngine.Random.Range(0, state.Rtiles.Length)] };
                    EnsureInitializeBoostedTileSets(state.LTtiles, state.LRtiles, state.TRtiles, state.LTRtiles);
                    LTtilesRef(__instance) = boostedLTtiles;
                    LRtilesRef(__instance) = boostedLRtiles;
                    TRtilesRef(__instance) = boostedTRtiles;
                    LTRtilesRef(__instance) = boostedLTRtiles;
                }
                else
                {
                    LTtilesRef(__instance) = new GameObject[0];
                    LRtilesRef(__instance) = new GameObject[0];
                    TRtilesRef(__instance) = new GameObject[0];
                    LTRtilesRef(__instance) = new GameObject[0];
                }
            }

            static void Postfix(TileManager __instance, State __state)
            {
                if (__state == null)
                {
                    return;
                }
                instance.WaveStarted(__state.pathCount);
                LtilesRef(__instance) = __state.Ltiles;
                TtilesRef(__instance) = __state.Ttiles;
                RtilesRef(__instance) = __state.Rtiles;
                LTtilesRef(__instance) = __state.LTtiles;
                LRtilesRef(__instance) = __state.LRtiles;
                TRtilesRef(__instance) = __state.TRtiles;
                LTRtilesRef(__instance) = __state.LTRtiles;
            }
        }
    }
}
