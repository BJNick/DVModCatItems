using System.Collections.Generic;
using System.Linq;
using DV.Shops;
using DV.VFX;
using HarmonyLib;
using UnityEngine;
using UnityModManagerNet;

/*
 * DV Prefab Names:
 * CatSimpleBlack_rigged
 * CatSimpleGray_rigged
 * CatSimpleWhiteSpotted_rigged
 * CatSimpleYellow_rigged
 */
namespace CatItems {
    public class CatReplacer : MonoBehaviour {
        public static UnityModManager.ModEntry mod;

        public static List<GameObject> catsToReplace = new();

        public CattleZone aCattleZone;
        public float lastCheckTime = -1f;
        public int failedAttempts;

        public void Update() {
            if (!aCattleZone && catsToReplace.Count > 0) {
                SearchForCattleZone();
                return;
            }

            if (lastCheckTime + 1f > Time.time) {
                return; // only check every second
            }

            lastCheckTime = Time.time;

            // Replace fake cat models with animated cats
            if (catsToReplace.Count > 0) {
                foreach (GameObject catModel in catsToReplace) {
                    var name = catModel.name;
                    string prefabName;
                    switch (name) {
                        case "already-replaced-cat":
                            mod.Logger.Log("Cat model " + name + " already replaced, skipping.");
                            continue; // already replaced, skip
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
                        mod.Logger.Error("Prefab " + prefabName + " not found in the cattle zone.");
                        prefab = aCattleZone.agentPrefabs.First().prefab;
                    }

                    var catRoot = catModel.transform.parent;
                    catModel.name = "already-replaced-cat";
                    catModel.GetComponent<MeshRenderer>().enabled = false;
                    var catPrefab = Instantiate(prefab, catRoot.transform);
                    catPrefab.transform.localPosition = new Vector3(0, 0, 0.25f);
                    catPrefab.transform.localRotation = Quaternion.Euler(0, 180, 0);
                    StabilizeRigidbody(catRoot.gameObject);
                }

                catsToReplace.Clear();
            }
        }

        public void StabilizeRigidbody(GameObject gameObject) {
            var rb = gameObject.GetComponentInChildren<Rigidbody>();
            if (!rb) {
                return; // A store display item, no need to stabilize
            }
            // Put center of mass lower to reduce toppling over
            rb.centerOfMass = new Vector3(0, -0.1f, 0.25f);
            // Increase angular drag to prevent spinning
            rb.angularDrag = 10f;
            // Reduce mass to make it easier to move along with a train
            rb.mass = 0.1f;
            // Add friction materials to reduce sliding
            var material = new PhysicMaterial {
                dynamicFriction = 1f,
                staticFriction = 1f,
                bounciness = 0f,
                frictionCombine = PhysicMaterialCombine.Maximum
            };
            var colliders = gameObject.GetComponentsInChildren<Collider>();
            foreach (var collider in colliders) {
                collider.material = material;
            }
        }
        
        public void SearchForCattleZone() {
            if (aCattleZone) {
                return; // already found
            }

            var checkDelay = Mathf.Clamp(failedAttempts * failedAttempts, 1f, 60f);
            if (Time.time - lastCheckTime < checkDelay) {
                return; // only check every 1-60 seconds depending on failed attempts
            }

            lastCheckTime = Time.time;
            aCattleZone = FindCattleZone();
            if (!aCattleZone) {
                failedAttempts++;
                mod.Logger.Log("Failed to find a CattleZone, retrying in " + checkDelay + " seconds.");
            } else {
                failedAttempts = 0; // reset on success
            }
        }

        public CattleZone FindCattleZone(string withPrefabName = "CatSimpleBlack_rigged") {
            var cattleZones = FindObjectsOfType<CattleZone>();
            foreach (var cattleZone in cattleZones) {
                if (cattleZone.agentPrefabs.Any(it => it.prefab.name == withPrefabName)) {
                    mod.Logger.Log("Found CattleZone with prefab " + withPrefabName);
                    return cattleZone;
                }
            }

            return null;
        }
    }

    // Patch ShopRestocker
    [HarmonyPatch(typeof(ShopRestocker))]
    internal static class ShopRestocker_Awake_Patch {
        [HarmonyPatch(nameof(ShopRestocker.Awake))]
        [HarmonyPostfix]
        private static void AfterAwake(ShopRestocker __instance) {
            // iterate over all children of the ShopRestocker and see if any of them match
            foreach (Transform child in __instance.transform) {
                if (child.name.StartsWith("replacethiswithacat-")) {
                    CatReplacer.catsToReplace.Add(child.gameObject);
                    //CatReplacer.mod.Logger.Log("ShopRestocker_Awake_Patch.AfterAwake replace cat called for " + child.name);
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
                    //CatReplacer.mod.Logger.Log("ShelfItem_Awake_Patch.AfterAwake replace cat called for " + child.name);
                    return;
                }
            }
        }
    }
}