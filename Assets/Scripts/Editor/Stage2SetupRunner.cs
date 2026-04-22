using System.IO;
using MAIGame.Core;
using MAIGame.Data;
using MAIGame.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MAIGame.EditorTools
{
    public static class Stage2SetupRunner
    {
        private const string PlayerConfigPath = "Assets/ScriptableObjects/Configs/PlayerConfig.asset";
        private const string PlayerPrefabPath = "Assets/Prefabs/Characters/Player.prefab";
        private const string TestPlaygroundPath = "Assets/Scenes/Levels/Test_Playground.unity";

        [MenuItem("MAI Game/Setup/Stage 2 Player")]
        public static void SetupStage2()
        {
            EnsureDirectories();
            PlayerConfig playerConfig = EnsurePlayerConfig();
            GameObject playerPrefab = EnsurePlayerPrefab(playerConfig);
            SetupTestPlayground(playerPrefab);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsureDirectories()
        {
            Directory.CreateDirectory("Assets/ScriptableObjects/Configs");
            Directory.CreateDirectory("Assets/Prefabs/Characters");
            Directory.CreateDirectory("Assets/Scenes/Levels");
        }

        private static PlayerConfig EnsurePlayerConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<PlayerConfig>(PlayerConfigPath);
            if (config != null)
            {
                return config;
            }

            config = ScriptableObject.CreateInstance<PlayerConfig>();
            AssetDatabase.CreateAsset(config, PlayerConfigPath);
            return config;
        }

        private static GameObject EnsurePlayerPrefab(PlayerConfig playerConfig)
        {
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (existingPrefab != null)
            {
                return existingPrefab;
            }

            var root = new GameObject("Player");
            root.transform.position = new Vector3(0f, 1.05f, 0f);

            var characterController = root.AddComponent<CharacterController>();
            characterController.height = 2f;
            characterController.radius = 0.35f;
            characterController.center = new Vector3(0f, 1f, 0f);
            characterController.stepOffset = 0.35f;
            characterController.slopeLimit = 45f;

            root.AddComponent<PlayerInputHandler>();
            root.AddComponent<GroundChecker>();
            var playerController = root.AddComponent<PlayerController>();

            var cameraTarget = new GameObject("CameraTarget");
            cameraTarget.transform.SetParent(root.transform);
            cameraTarget.transform.localPosition = new Vector3(0f, 1.5f, 0f);

            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Visual";
            visual.transform.SetParent(root.transform);
            visual.transform.localPosition = new Vector3(0f, 1f, 0f);
            Object.DestroyImmediate(visual.GetComponent<Collider>());

            var serializedController = new SerializedObject(playerController);
            serializedController.FindProperty("playerConfig").objectReferenceValue = playerConfig;
            serializedController.FindProperty("cameraTarget").objectReferenceValue = cameraTarget.transform;
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static void SetupTestPlayground(GameObject playerPrefab)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var bootstrapObject = new GameObject("GameBootstrap");
            bootstrapObject.AddComponent<GameBootstrap>();

            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.transform.position = new Vector3(0f, -0.05f, 0f);
            ground.transform.localScale = new Vector3(20f, 0.1f, 20f);

            var ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ramp.name = "Test Ramp";
            ramp.transform.position = new Vector3(3f, 0.35f, 4f);
            ramp.transform.rotation = Quaternion.Euler(0f, 0f, -12f);
            ramp.transform.localScale = new Vector3(3f, 0.25f, 2f);

            var player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, scene);
            player.transform.position = new Vector3(0f, 1.05f, 0f);

            var cameraObject = new GameObject("Main Camera");
            var camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            camera.tag = "MainCamera";

            var controller = player.GetComponent<PlayerController>();
            var serializedController = new SerializedObject(controller);
            serializedController.FindProperty("followCamera").objectReferenceValue = camera;
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            var lightObject = new GameObject("Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            EditorSceneManager.SaveScene(scene, TestPlaygroundPath);
        }
    }
}

