## Continuous Integration
A common use case for Addressables is having a Continuous Integration (CI) system perform the player content build and player builds.  While each team and project setup is unique, here are some general guidelines for building Addressables with CI systems.

### Selecting a Content Builder
One of the main choices when building Addressables content is selecting a Content Builder.  By default, if you call `AddressableAssetSettings.BuildPlayerContent()` the `BuildScriptPackedMode` is the Data Builder that gets used.  This is because `BuildPlayerContent()` checks the `ActivePlayerDataBuilder` and calls into that script's `BuildDataImplementation(..)`  

If you've implemented your own custom `IDataBuilder` and want to use it for your CI builds, you'll need to set the `ActivePlayerDataBuilderIndex`.  This is not a static property so you'll need to go through an instance of the `AddressableAssetSettings`.  By default you can access this through `AddressableAssetSettingsDefaultObject.Settings`.  This index refers to the position of the `IDataBuilder` in the list of `AddressableAssetSettings.DataBuilders`  Here's a code sample to set a custom `IDataBuilder`:

```
public static void SetCustomDataBuilder(IDataBuilder builder)
{ 
    AddressableAssetSettings settings 
        = AddressableAssetSettingsDefaultObject.Settings;
    
    int index = settings.DataBuilders.IndexOf((ScriptableObject) builder);
    if (index > 0)
        settings.ActivePlayerDataBuilderIndex = index;
    else if (AddressableAssetSettingsDefaultObject.Settings.AddDataBuilder(builder))
        settings.ActivePlayerDataBuilderIndex = AddressableAssetSettingsDefaultObject.Settings.DataBuilders.Count - 1;
    else
        Debug.LogWarning($"{builder} could not be found or added to the list of DataBuilders");
}
```
If your custom `IDataBuilder` has already been added to the list of `DataBuilders` in the project prior, all that needs to happen is for you to be sure the `ActivePlayerDataBuilderIndex` is set to the correct index.

### Cleaning the Addressables Content Builder Cache
`IDataBuilder` implementations define a `ClearCachedData()` which is responsible for cleaning up the files created by that data builder.  For example, the built in `BuildScriptPackedMode` deletes the following:
- The content catalog
- The serialized settings file
- The built AssetBundles
- Any link.xml files created

calling `IDataBuilder.ClearCachedData()` as part of your CI process makes sure no pre-generated content is used, if that is desired.
### Cleaning the Scriptable Build Pipeline Cache
Cleaning the Scriptable Build Pipeline (SBP) cache cleans the `BuildCache` folder from the `Library` directory along with all the hash maps generated by the build and the Type Database.  The `Library/BuildCache` folder contains `.info` files created by SBP during the build which speeds up subsequent builds by reading data from these `.info` files instead of re-generating data that hasn't changed.

How to clear the SBP cache in a script without prompting: `BuildCache.PurgeCache(false);`