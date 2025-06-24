using System.Collections.Generic;
using System.Linq;
using DV.CabControls.Spec;
using DV.Items;
using DV.Shops;
using DV.VFX;
using HarmonyLib;
using UnityEngine;
using UnityModManagerNet;
using VRTK;
using Random = UnityEngine.Random;

/*
 * Potential asset names
 * Prefabs:
 * CatSimpleBlack_rigged
 * CatSimpleGray_rigged
 * CatSimpleWhiteSpotted_rigged
 * CatSimpleYellow_rigged
 * Cat_1_LowPoly (model)
 */

namespace CatItems {

    public class CatObject : VRTK_InteractableObject
    {
        private void Reset()
        {
            // Ensure a collider exists
            if (GetComponent<Collider>() == null) {
                gameObject.AddComponent<BoxCollider>();
            }
            // Ensure a rigidbody exists
            if (GetComponent<Rigidbody>() == null) {
                interactableRigidbody = gameObject.AddComponent<Rigidbody>();
            }
        }

        protected virtual void Awake()
        {
            // Set up grabbable and not usable
            isGrabbable = true;
            isUsable = false;
            holdButtonToGrab = true;
            holdButtonToUse = false;
            useOnlyIfGrabbed = false;
            pointerActivatesUseAction = false;

            // Ensure collider and rigidbody exist
            if (GetComponent<Collider>() == null)
            {
                gameObject.AddComponent<BoxCollider>();
            }
            if (GetComponent<Rigidbody>() == null)
            {
                interactableRigidbody = gameObject.AddComponent<Rigidbody>();
            }

            base.Awake();
        }
    }
    
    public class CatReplacer : MonoBehaviour {
        
        public static UnityModManager.ModEntry mod;
        public static CattleZone aCattleZone;
        
        public static List<GameObject> catsToReplace = new List<GameObject>();
        
        public bool tryToSpawnCat = true;
        
        public Vector3 catPositionOffset = new Vector3(0, 0, 1);

        public void Update() {
                
            // If all else fails, find CattleZone in the scene
            if (!aCattleZone) {
                aCattleZone = FindObjectOfType<CattleZone>();
                if (aCattleZone) {
                    mod.Logger.Log("Found CattleZone in the scene.");
                } else {
                    mod.Logger.Error("No CattleZone found in the scene.");
                }
            }
            
            // replace cats with actual cats
            if (catsToReplace.Count > 0) {
                foreach (GameObject catModel in catsToReplace) {
                    var name = catModel.name;
                    var prefabName = "";
                    switch (name) {
                        case "replacethiswithacat-black":
                            prefabName = "CatSimpleBlack_rigged";
                            break;
                        case "replacethiswithacat-tabby":
                            prefabName = "CatSimpleGray_rigged";
                            break;
                        case "replacethiswithacat-spottedwhite":
                            prefabName = "CatSimpleWhiteSpotted_rigged";
                            break;
                        case "replacethiswithacat-orange":
                            prefabName = "CatSimpleYellow_rigged";
                            break;
                        default:
                            mod.Logger.Error("Unknown cat name: " + name + ", defaulting to CatSimpleGray_rigged");
                            prefabName = "CatSimpleGray_rigged";
                            break;
                    }
                    var prefab = aCattleZone.agentPrefabs.First(it => it.prefab.name == prefabName).prefab;
                    if (!prefab) {
                        mod.Logger.Error("Prefab " + prefabName + " not found in cattle zone.");
                        prefab = aCattleZone.agentPrefabs.First().prefab;
                    }
                    var catRoot = catModel.transform.parent;
                    catModel.name = "already-replaced-cat";
                    catModel.GetComponent<MeshRenderer>().enabled = false;
                    var catPrefab = Instantiate(prefab, catRoot.transform);
                    catPrefab.transform.localPosition = new Vector3(0, 0, 0.25f);
                    catPrefab.transform.localRotation = Quaternion.Euler(0, 180, 0);
                    mod.Logger.Log("Replaced " + name + " with " + prefabName);
                }
                catsToReplace.Clear();
            }
            
            
            
            // Toggle cat spawning with the 'M' key
            if (Input.GetKeyDown(KeyCode.M)) {
                tryToSpawnCat = !tryToSpawnCat;
                mod.Logger.Log("Cat spawning toggled: " + tryToSpawnCat);
            }
            
            if (tryToSpawnCat) {
                tryToSpawnCat = false;
                
                // Try to read prefab from resources
                GameObject catPrefab = Resources.Load<GameObject>("CatSimpleBlack_rigged");
                if (catPrefab != null) {
                    // Instantiate the cat prefab
                    GameObject catInstance = Instantiate(catPrefab);
                    catInstance.transform.position = PlayerManager.PlayerTransform.position + catPositionOffset;
                    catPositionOffset += new Vector3(0, 0, 1); // Update position offset for next cat
                    mod.Logger.Log("Cat spawned successfully, via prefab from resources.");
                } else {
                    mod.Logger.Error("Cat prefab not found in resources.");
                }
                
                // Try to read prefab from cattle zone
                if (aCattleZone) {
                    var catPrefabsFromZone = aCattleZone.agentPrefabs;
                    if (catPrefabsFromZone != null && catPrefabsFromZone.Length > 0) {
                        var prefabEntry = catPrefabsFromZone[Random.Range(0, catPrefabsFromZone.Length)];
                        //foreach (var prefabEntry in catPrefabsFromZone) {
                            if (prefabEntry.prefab != null) {
                                GameObject catInstance = Instantiate(prefabEntry.prefab);
                                catInstance.layer = 27; // World Item
                                catInstance.name = prefabEntry.prefab.name + " CatTest";
                                catInstance.transform.position = PlayerManager.PlayerTransform.position;
                                catPositionOffset += new Vector3(0, 0, 1); // Update position offset for next cat
                                mod.Logger.Log("Cat " + prefabEntry.prefab.name + " spawned successfully, via prefab from cattle zone.");

                                var box = catInstance.AddComponent<BoxCollider>();
                                box.center = new Vector3(0, 0.25f, 0);
                                box.size = new Vector3(0.25f, 0.5f, 0.5f);
                                catInstance.AddComponent<Rigidbody>();
                                
                                // these components are standard requirements for every item but cannot be added in Unity due to them being in Assembly and not a custom dll
                                var itemSpec = catInstance.AddComponent<Item>();
                                itemSpec.itemUseApproach = ItemUseApproach.Continuous;
                                catInstance.AddComponent<TrainItemActivityHandlerOverride>();
                                catInstance.AddComponent<ItemSaveData>();
                                catInstance.AddComponent<ShopRestocker>();

                                //TODO: Provide a way for item authors to declare which colliders are standard interaction colliders and which are not - needed for gadget support
                                itemSpec.colliderGameObjects = (from c in catInstance.GetComponentsInChildren<Collider>()
                                    select c.gameObject).ToArray();
                                
                                
                                // Get player car and put the cat in it
                                var playerCar = PlayerManager.Car;
                                /*if (playerCar) {
                                    var root = playerCar.carColliders.walkableRoot;
                                    // All colliders are children of the root
                                    // pick a random child collider
                                    var childColliders = root.GetComponentsInChildren<BoxCollider>();
                                    var randomCollider = childColliders[Random.Range(0, childColliders.Length)];
                                    // Pick a random position on top of the collider
                                    var randomPosition = randomCollider.center + new Vector3(0, randomCollider.size.y/2, 0);
                                    randomPosition.x = Random.Range(randomCollider.bounds.min.x, randomCollider.bounds.max.x);
                                    randomPosition.z = Random.Range(randomCollider.bounds.min.z, randomCollider.bounds.max.z);
                                    catInstance.transform.position = randomPosition;

                                    catInstance.transform.SetParent(root, false);
                                    catInstance.transform.localPosition = Vector3.zero;
                                }*/
                            }
                        //}
                    } else {
                        mod.Logger.Error("Cat prefab not found in cattle zone.");
                    }
                } else {
                    mod.Logger.Error("CattleZone instance is null.");
                }
                
            }
            
            if (Input.GetKeyDown(KeyCode.Comma)) {
                // Try to clone a cattle zone directly
                if (aCattleZone) {
                    GameObject clonedCattleZone = Instantiate(aCattleZone.gameObject);
                    clonedCattleZone.transform.position = PlayerManager.PlayerTransform.position;
                    clonedCattleZone.name = "ClonedCattleZone CatTest";
                    clonedCattleZone.GetComponent<BoxCollider>().size = new Vector3(5, 1, 5);
                    var zone = clonedCattleZone.GetComponent<CattleZone>();
                    mod.Logger.Log("Cloned CattleZone spawned successfully.");
                } else {
                    mod.Logger.Error("CattleZone.instance is null.");
                }
            }
        }
    }
    
    // patch CattleZone
    [HarmonyPatch(typeof(CattleZone))]
    internal static class CattleZone_Start_Patch {
        [HarmonyPatch(nameof(CattleZone.Start))]
        [HarmonyPostfix]
        private static void AfterStart(CattleZone __instance) {
            CatReplacer.aCattleZone = __instance;
            CatReplacer.mod.Logger.Log("CattleZone_Start_Patch.AfterStart called");
        }
    }
    
    // patch ShopRestocker
    [HarmonyPatch(typeof(ShopRestocker))]
    internal static class ShopRestocker_Awake_Patch {
        [HarmonyPatch(nameof(ShopRestocker.Awake))]
        [HarmonyPostfix]
        private static void AfterAwake(ShopRestocker __instance) {
            // iterate over all children of the ShopRestocker and see if any of them match
            foreach (Transform child in __instance.transform) {
                if (child.name.StartsWith("replacethiswithacat-")) {
                    CatReplacer.catsToReplace.Add(child.gameObject);
                    CatReplacer.mod.Logger.Log("ShopRestocker_Awake_Patch.AfterAwake replace cat called for " + child.name);
                    return;
                }
            }
        }
    }
    
    // Also patch ShelfItem
    [HarmonyPatch(typeof(ShelfItem))]
    internal static class ShelfItem_Awake_Patch {
        [HarmonyPatch(nameof(ShelfItem.Awake))]
        [HarmonyPostfix]
        private static void AfterAwake(ShelfItem __instance) {
            if (__instance.transform.GetChild(0) == null) {
                return;
            }
            // iterate over all children of the ShelfItem and see if any of them match
            foreach (Transform child in __instance.transform.GetChild(0)) {
                if (child.name.StartsWith("replacethiswithacat-")) {
                    CatReplacer.catsToReplace.Add(child.gameObject);
                    CatReplacer.mod.Logger.Log("ShelfItem_Awake_Patch.AfterAwake replace cat called for " + child.name);
                    return;
                }
            }
        }
    }
    
}