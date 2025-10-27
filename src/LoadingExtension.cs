using ICities;
using UnityEngine;

namespace JobsHousingBalance
{
    public class LoadingExtension : LoadingExtensionBase
    {
        public override void OnLevelLoaded(LoadMode mode)
        {
            Debug.Log("JobsHousingBalance: Level loaded successfully");
        }
        
        public override void OnLevelUnloading()
        {
            Debug.Log("JobsHousingBalance: Level unloading");
        }
    }
}
