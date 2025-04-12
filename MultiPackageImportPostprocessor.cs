using UnityEditor;
using UnityEngine;

public class MultiPackageImportPostprocessor : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        if (MultiPackageImportManager.IsImporting())
        {
            Debug.Log("[MultiPackageImporter] Detected import finished. Proceeding to next...");
            MultiPackageImportManager.NotifyImportCompleted();
        }
    }
}
