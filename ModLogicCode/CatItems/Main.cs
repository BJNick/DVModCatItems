using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityModManagerNet;
using Object = UnityEngine.Object;

namespace CatItems
{
    [EnableReloading]
    static class Main
    {
        public static bool enabled;

        public static CatReplacer replacer;
        
        static bool Load(UnityModManager.ModEntry modEntry) {
            var harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            CatReplacer.mod = modEntry;
            modEntry.OnSessionStart += OnSessionStart;
            
            return true;
        }
        
        static bool Unload(UnityModManager.ModEntry modEntry) {
            var harmony = new Harmony(modEntry.Info.Id);
            harmony.UnpatchAll(modEntry.Info.Id);
            
            modEntry.OnSessionStart -= OnSessionStart;
            if (replacer != null) {
                Object.Destroy(replacer.gameObject);
                replacer = null;
            }
            return true;
        }
        
        static void OnSessionStart(UnityModManager.ModEntry modEntry) {
            if (replacer == null) {
                GameObject catItemsReplacer = new GameObject("CatItemsReplacer");
                replacer = catItemsReplacer.AddComponent<CatReplacer>();
            }
        }
        
    }
}