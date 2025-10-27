using ICities;
using UnityEngine;
using JobsHousingBalance.Utils;

namespace JobsHousingBalance
{
    public class JobsHousingBalanceMod : IUserMod
    {
        public string Name => "Jobs Housing Balance";
        
        public string Description => "Shows visual overlay of jobs vs housing balance in your city";

        public void OnEnabled()
        {
            Debug.Log("JobsHousingBalance: Mod enabled");
            HarmonyPatcher.ApplyPatches();
        }

        public void OnDisabled()
        {
            Debug.Log("JobsHousingBalance: Mod disabled");
            HarmonyPatcher.RemovePatches();
        }
    }
}
