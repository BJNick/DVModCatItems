# Blåhaj

This mod adds [Blåhaj](https://knowyourmeme.com/editorials/guides/what-does-blahaj-mean-the-seemingly-inexplicable-popularity-of-ikeas-iconic-shark-plushie-explained) to Derail Valley.

![screenshot.png](screenshot.png)

## How to install

1. install Unity Mod Manager(UMM)
2. install [Custom Item Mod](https://github.com/t0stiman/dv_custom_item_mod)
3. download this mods zip from the [Releases](https://git.tostiman.com/tostiman/dv_blahaj/releases) tab and manually extract it into DV's mod folder

After installing, you can buy the item in the shops.

## How to create your own custom item

You can use this project as an example. In a nutshell, these are the steps you need to take:

1. Download this repository.
2. The `ItemBundleBuilder` folder is a Unity project. Open it with Unity Hub.
3. Replace the assets with your own.
4. Open `Assets/DontBuildThis/SampleScene`.
5. Click on the `Script` object.
6. In the inspector -> Bundle Builder, fill in a bundle name and set the export folder to `<your mod folder>/items/Blahaj/`.
7. Click the button to build the asset bundle. Note: files in the `DontBuildThis` folder will not be included in the bundle.
8. Edit `<your mod folder>/items/Blahaj/item_info.json` to your liking.

Name = the name of your custom item  
Description = description of the item (shown in the shops)  
Amount = How many of this item the player can buy. If they buy this amount, the item will be out of stock.  
Price = price in DV dollars  
AssetBundleName = name of the asset bundle (from step 6)  
PrefabPath = path to the prefab of the item  
IconStandardPath = Path to the standard icon. This will be the icon of the item in the players inventory.  
IconDroppedPath = Path to the dropped icon. This icon will be shown in your inventory when you've dropped the item. For vanilla game items, this is a blue (#72A2B3) silhouette of the item.  

9.  Go to `<your mod folder>/items` and rename the `Blahaj` folder to the name of your item (doesn't have to be an exact match).
10. To copy the required files to DV's mod folder, you can use the `package.ps1` PowerShell script. Before you execute it, set your DV installation folder in the first line of the script.  
    If you don't know how to use PowerShell, you can also manually copy the files over. Just replicate the file structure of this mods zip.

## Credits

"Blahaj" (https://skfb.ly/oGwQt) by Kaine_G is licensed under Creative Commons Attribution (http://creativecommons.org/licenses/by/4.0/).