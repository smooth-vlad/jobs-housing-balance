using ICities;
using UnityEngine;
using JobsHousingBalance.UI;
using JobsHousingBalance.UI.Panel;

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

            // Clean up icon button (this will also clean up its panel)
            if (iconButton != null)
            {
                UnityEngine.Object.Destroy(iconButton.gameObject);
                iconButton = null;
            }
        }
    }
}
