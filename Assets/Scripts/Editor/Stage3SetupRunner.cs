using System.IO;
using MAIGame.Echo;
using MAIGame.Data;
using MAIGame.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MAIGame.EditorTools
{
    public static class Stage3SetupRunner
    {
        private const string EchoConfigPath = "Assets/ScriptableObjects/Configs/EchoConfig.asset";
        private const string EchoMaterialPath = "Assets/Art/Materials/EchoGhost.mat";
        private const string EchoPrefabPath = "Assets/Prefabs/Echo/Echo.prefab";
        private const string TestPlaygroundPath = "Assets/Scenes/Levels/Test_Playground.unity";

        [MenuItem("MAI Game/Setup/Stage 3 Echo")]
        public static void SetupStage3()
        {
            EnsureDirectories();
            EchoConfig echoConfig = EnsureEchoConfig();
            Material echoMaterial = EnsureEchoMaterial();
            GameObject echoPrefab = EnsureEchoPrefab(echoMaterial);
            SetupEchoManager(echoConfig, echoPrefab);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsureDirectories()
        {
            Directory.CreateDirectory("Assets/Art/Materials");
            Directory.CreateDirectory("Assets/Prefabs/Echo");
            Directory.CreateDirectory("Assets/ScriptableObjects/Configs");
        }

        private static EchoConfig EnsureEchoConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<EchoConfig>(EchoConfigPath);
            if (config != null)
            {
                return config;
            }

            config = ScriptableObject.CreateInstance<EchoConfig>();
            AssetDatabase.CreateAsset(config, EchoConfigPath);
            return config;
        }

        private static Material EnsureEchoMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(EchoMaterialPath);
            if (material != null)
            {
                return material;
            }

            material = new Material(Shader.Find("Standard"))
            {
                name = "EchoGhost",
                color = new Color(0.25f, 0.8f, 1f, 0.35f)
            };

            material.SetFloat("_Mode", 3f);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

            AssetDatabase.CreateAsset(material, EchoMaterialPath);
            return material;
        }

        private static GameObject EnsureEchoPrefab(Material echoMaterial)
        {
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(EchoPrefabPath);
            if (existingPrefab != null)
            {
                return existingPrefab;
            }

            var root = new GameObject("Echo");
            root.AddComponent<EchoInstance>();
            root.AddComponent<EchoVisual>();

            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "GhostVisual";
            visual.transform.SetParent(root.transform);
            visual.transform.localPosition = new Vector3(0f, 1f, 0f);
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;
            Object.DestroyImmediate(visual.GetComponent<Collider>());

            Renderer renderer = visual.GetComponent<Renderer>();
            renderer.sharedMaterial = echoMaterial;

            var echoVisual = root.GetComponent<EchoVisual>();
            var serializedVisual = new SerializedObject(echoVisual);
            SerializedProperty renderersProperty = serializedVisual.FindProperty("targetRenderers");
            renderersProperty.arraySize = 1;
            renderersProperty.GetArrayElementAtIndex(0).objectReferenceValue = renderer;
            serializedVisual.ApplyModifiedPropertiesWithoutUndo();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, EchoPrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static void SetupEchoManager(EchoConfig echoConfig, GameObject echoPrefab)
        {
            Scene scene = EditorSceneManager.OpenScene(TestPlaygroundPath, OpenSceneMode.Single);

            EchoManager echoManager = Object.FindObjectOfType<EchoManager>();
            if (echoManager == null)
            {
                var managerObject = new GameObject("EchoManager");
                SceneManager.MoveGameObjectToScene(managerObject, scene);
                echoManager = managerObject.AddComponent<EchoManager>();
            }

            PlayerInputHandler inputHandler = Object.FindObjectOfType<PlayerInputHandler>();
            GroundChecker groundChecker = inputHandler != null ? inputHandler.GetComponent<GroundChecker>() : null;

            var serializedManager = new SerializedObject(echoManager);
            serializedManager.FindProperty("echoConfig").objectReferenceValue = echoConfig;
            serializedManager.FindProperty("echoPrefab").objectReferenceValue = echoPrefab;
            serializedManager.FindProperty("playerTransform").objectReferenceValue = inputHandler != null ? inputHandler.transform : null;
            serializedManager.FindProperty("inputHandler").objectReferenceValue = inputHandler;
            serializedManager.FindProperty("groundChecker").objectReferenceValue = groundChecker;
            serializedManager.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(echoManager);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }
}
