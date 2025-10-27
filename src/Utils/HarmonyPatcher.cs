using System;
using UnityEngine;
using HarmonyLib;

namespace JobsHousingBalance.Utils
{
    /// <summary>
    /// Manages Harmony patches for the Jobs Housing Balance mod
    /// </summary>
    public static class HarmonyPatcher
    {
        private static Harmony instance = null;
        private const string harmonyId = "com.vladislav.jobshousingbalance";

        /// <summary>
        /// Applies Harmony patches
        /// </summary>
        public static void ApplyPatches()
        {
            try
            {
                if (instance == null)
                {
                    instance = new Harmony(harmonyId);
                    instance.PatchAll();
                    Debug.Log("JobsHousingBalance: Harmony patches applied successfully");
                }
                else
                {
                    Debug.LogWarning("JobsHousingBalance: Harmony patches already applied");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"JobsHousingBalance: Failed to apply Harmony patches: {ex.Message}");
            }
        }

        /// <summary>
        /// Removes Harmony patches
        /// </summary>
        public static void RemovePatches()
        {
            try
            {
                if (instance != null)
                {
                    instance.UnpatchAll(harmonyId);
                    instance = null;
                    Debug.Log("JobsHousingBalance: Harmony patches removed successfully");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"JobsHousingBalance: Failed to remove Harmony patches: {ex.Message}");
            }
        }
    }
}

