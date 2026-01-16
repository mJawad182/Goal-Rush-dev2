using FStudio.Loaders;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace FStudio.Utilities {
    [Serializable]
    public class SceneEntry <Key> {
        public Key Id;
        public AssetReference Val;
    }

    [Serializable]
    public class SerializableSceneCollection <Key> {
        public SceneEntry<Key>[] Entries;

        public void Resize(Key[] keys) {
            var itemCount = keys.Length;

            if (Entries == null) {
                Entries = new SceneEntry<Key>[0];
            }

            if (Entries.Length != itemCount) {
                Array.Resize(ref Entries, itemCount);

                for (int i = 0; i < itemCount; i++) {
                    if (Entries[i] == null) {
                        Entries[i] = new SceneEntry<Key>();
                    }
                }
            }

            // fix enum
            for (int i = 0; i < itemCount; i++) {
                Entries[i].Id = keys[i];
            }
            //
        }

        public async void Unload() {
            await SceneLoader.LoadDefaultScene();
        }

        public async Task Load(Key key) {
            var scene = Find(key);

            if (scene == null) {
                Debug.LogWarning($"[SerializableSceneCollection] Scene with key '{key}' is not found. Falling back to first available scene.");
                if (Entries == null || Entries.Length == 0) {
                    Debug.LogError($"[SerializableSceneCollection] No scenes available in collection!");
                    return;
                }
                scene = Entries[0].Val;
            }

            try {
                var sceneInstance = await SceneLoader.LoadScene(scene);
                if (sceneInstance.Scene.IsValid()) {
                    Debug.Log($"[SerializableSceneCollection] Scene loaded successfully: {sceneInstance.Scene.name}");
                }
            } catch (System.Exception ex) {
                Debug.LogError($"[SerializableSceneCollection] Failed to load scene for key '{key}': {ex.Message}");
                // Try to load default scene as fallback
                Debug.Log($"[SerializableSceneCollection] Attempting to load default scene as fallback...");
                await SceneLoader.LoadDefaultScene();
            }
        }

        private AssetReference Find (Key key) {
            for (int i = 0, length = Entries.Length; i < length; i++) {
                if (Entries[i].Id.Equals(key)) {
                    return Entries[i].Val;
                }
            }

            return null;
        }
    }
}
