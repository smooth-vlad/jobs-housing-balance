using ICities;
using UnityEngine;
using JobsHousingBalance.UI;

namespace JobsHousingBalance
{
    public class LoadingExtension : LoadingExtensionBase
    {
        private IconButton iconButton;

        public override void OnLevelLoaded(LoadMode mode)
        {
            Debug.Log("JobsHousingBalance: Level loaded successfully");

            // Create icon button
            try
            {
                iconButton = IconButton.Create();
                if (iconButton != null)
                {
                    Debug.Log("JobsHousingBalance: Icon button created successfully");
                }
                else
                {
                    Debug.LogWarning("JobsHousingBalance: Failed to create icon button");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("JobsHousingBalance: Error creating icon button: " + ex.Message);
            }
        }
        
        public override void OnLevelUnloading()
        {
            Debug.Log("JobsHousingBalance: Level unloading");

            // Clean up icon button
            if (iconButton != null)
            {
                UnityEngine.Object.Destroy(iconButton.gameObject);
                iconButton = null;
            }
        }
    }
}
