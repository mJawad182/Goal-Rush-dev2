using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace FStudio.Loaders {
    public static class SceneLoader {
        private const string DEFAULT_SCENE_NAME = "DefaultScene";

        public static async Task LoadDefaultScene () {
            var loader = SceneManager.LoadSceneAsync(DEFAULT_SCENE_NAME, LoadSceneMode.Single);
            
            while (!loader.isDone) {
                await Task.Yield();
            }
        }

        public static async Task<SceneInstance> LoadScene (AssetReference sceneAsset) {
            Debug.Log($"[SceneLoader] Loading scene {sceneAsset}");

            try {
                var loader = await Addressables.LoadSceneAsync(sceneAsset, UnityEngine.SceneManagement.LoadSceneMode.Single).Task;
                
                if (loader.Scene.IsValid()) {
                    var activator = loader.ActivateAsync();

                    while (!activator.isDone) {
                        await Task.Yield();
                    }

                    Debug.Log($"[SceneLoader] Scene loaded successfully: {loader.Scene.name}");
                    return loader;
                } else {
                    Debug.LogError($"[SceneLoader] Failed to load scene: Invalid scene instance returned");
                    throw new System.Exception($"Scene load failed: Invalid scene instance");
                }
            } catch (InvalidKeyException ex) {
                Debug.LogError($"[SceneLoader] InvalidKeyException: The scene asset with GUID '{sceneAsset.AssetGUID}' is not marked as Addressable. " +
                              $"Please add the scene to an Addressables group in Unity Editor. Error: {ex.Message}");
                throw;
            } catch (System.Exception ex) {
                Debug.LogError($"[SceneLoader] Failed to load scene {sceneAsset}: {ex.GetType().Name} - {ex.Message}");
                throw;
            }
        }
    }
}
