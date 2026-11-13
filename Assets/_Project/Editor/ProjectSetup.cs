using System;
using System.IO;
using ExtensaoZero.Multiplayer;
using ExtensaoZero.Player;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace ExtensaoZero.EditorTools
{
    /// <summary>
    /// Setup reproduzível do projeto: pastas, PlayerSettings, URP, cena Bootstrap,
    /// prefabs de rede. Executável pelo menu ou em batch mode.
    /// </summary>
    public static class ProjectSetup
    {
        private const string ScenePath = "Assets/_Project/Scenes/Bootstrap.unity";
        private const string UrpPath = "Assets/_Project/Settings/EZ_URP_Asset.asset";
        private const string RendererPath = "Assets/_Project/Settings/EZ_UniversalRenderer.asset";
        private const string VolumePath = "Assets/_Project/Settings/EZ_VolumeProfile.asset";
        private const string PlayerPrefabPath = "Assets/_Project/Resources/NetworkPlayer.prefab";
        private const string StatePrefabPath = "Assets/_Project/Resources/NetworkGameState.prefab";
        private const string DefaultMaterialPath = "Assets/_Project/Resources/EZ_Default.mat";

        [MenuItem("Extensao Zero/Setup Project")]
        public static void Setup()
        {
            EnsureFolders();
            ConfigurePlayerSettings();
            ConfigureUrp();
            CreateDefaultMaterial();
            CreateVolumeProfile();
            CreateScene();
            CreateNetworkPrefabs();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[EZ] Setup concluído: URP, cena Bootstrap e prefabs de rede.");
        }

        private static void EnsureFolders()
        {
            var folders = new[]
            {
                "Assets/_Project/Art",
                "Assets/_Project/Audio",
                "Assets/_Project/Characters",
                "Assets/_Project/Environment",
                "Assets/_Project/Materials",
                "Assets/_Project/Models",
                "Assets/_Project/Prefabs",
                "Assets/_Project/Resources",
                "Assets/_Project/Scenes",
                "Assets/_Project/Settings",
                "Assets/_Project/ScriptableObjects",
                "Builds/Linux"
            };

            foreach (var folder in folders)
            {
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            }

            AssetDatabase.Refresh();
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.companyName = "Estudio Linha Zero";
            PlayerSettings.productName = "Extensao Zero";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.runInBackground = true;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.usePlayerLog = true;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, "com.linhazero.extensaozero");

            QualitySettings.vSyncCount = 0;
            QualitySettings.antiAliasing = 0;

            // activeInputHandler: 0 = legacy, 1 = Input System, 2 = ambos.
            // A UI usa StandaloneInputModule (legacy) e o gameplay usa Input System.
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
            if (assets != null && assets.Length > 0)
            {
                var serialized = new SerializedObject(assets[0]);
                var handler = serialized.FindProperty("activeInputHandler");
                if (handler != null)
                {
                    handler.intValue = 2;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }
                else
                {
                    Debug.LogWarning("[EZ] activeInputHandler não encontrado; ajuste manualmente para Both.");
                }
            }
        }

        private static void ConfigureUrp()
        {
            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
            if (renderer == null)
            {
                renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                renderer.name = "EZ_UniversalRenderer";
                AssetDatabase.CreateAsset(renderer, RendererPath);
            }

            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(UrpPath);
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                pipeline.name = "EZ_URP_Asset";
                AssetDatabase.CreateAsset(pipeline, UrpPath);
            }

            pipeline.msaaSampleCount = 2;
            pipeline.renderScale = 1f;
            pipeline.shadowDistance = 26f;
            pipeline.supportsHDR = false;
            pipeline.supportsCameraDepthTexture = false;
            pipeline.supportsCameraOpaqueTexture = false;

            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;
            EditorUtility.SetDirty(pipeline);
            EditorUtility.SetDirty(renderer);
        }

        private static void CreateDefaultMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(DefaultMaterialPath);
            if (material != null) return;
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            material = new Material(shader) { name = "EZ_Default" };
            AssetDatabase.CreateAsset(material, DefaultMaterialPath);
        }

        private static void CreateVolumeProfile()
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                profile.name = "EZ_VolumeProfile";
                AssetDatabase.CreateAsset(profile, VolumePath);
            }

            if (profile.TryGet(out Bloom bloom))
            {
                bloom.intensity.Override(0.3f);
                bloom.threshold.Override(1.05f);
            }
            else
            {
                bloom = profile.Add<Bloom>(true);
                bloom.intensity.Override(0.3f);
                bloom.threshold.Override(1.05f);
            }

            if (profile.TryGet(out Vignette vignette))
            {
                vignette.intensity.Override(0.3f);
            }
            else
            {
                vignette = profile.Add<Vignette>(true);
                vignette.intensity.Override(0.3f);
            }

            if (!profile.TryGet(out FilmGrain grain))
            {
                grain = profile.Add<FilmGrain>(true);
            }

            grain.intensity.Override(0.14f);
            EditorUtility.SetDirty(profile);
        }

        private static void CreateScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
        }

        private static void CreateNetworkPrefabs()
        {
            if (File.Exists(PlayerPrefabPath)) AssetDatabase.DeleteAsset(PlayerPrefabPath);
            if (File.Exists(StatePrefabPath)) AssetDatabase.DeleteAsset(StatePrefabPath);

            var player = new GameObject("NetworkPlayer");
            player.AddComponent<CharacterController>();
            player.AddComponent<FirstPersonController>();
            player.AddComponent<PlayerInteractor>();
            player.AddComponent<PlayerEquipment>();
            player.AddComponent<NetworkObject>();
            player.AddComponent<NetworkPlayer>();
            PrefabUtility.SaveAsPrefabAsset(player, PlayerPrefabPath);
            UnityEngine.Object.DestroyImmediate(player);

            var state = new GameObject("NetworkGameState");
            state.AddComponent<NetworkObject>();
            state.AddComponent<NetworkGameState>();
            PrefabUtility.SaveAsPrefabAsset(state, StatePrefabPath);
            UnityEngine.Object.DestroyImmediate(state);

            AssetDatabase.Refresh();
        }
    }
}
