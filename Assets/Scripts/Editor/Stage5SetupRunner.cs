using MAIGame.Data;
using MAIGame.Echo;
using MAIGame.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MAIGame.EditorTools
{
    public static class Stage5SetupRunner
    {
        private const string EchoConfigPath = "Assets/ScriptableObjects/Configs/EchoConfig.asset";
        private const string TestPlaygroundPath = "Assets/Scenes/Levels/Test_Playground.unity";

        [MenuItem("MAI Game/Setup/Stage 5 Echo Swap")]
        public static void SetupStage5()
        {
            Scene scene = EditorSceneManager.OpenScene(TestPlaygroundPath, OpenSceneMode.Single);

            EchoManager echoManager = Object.FindObjectOfType<EchoManager>();
            if (echoManager == null)
            {
                var managerObject = new GameObject("EchoManager");
                SceneManager.MoveGameObjectToScene(managerObject, scene);
                echoManager = managerObject.AddComponent<EchoManager>();
            }

            EchoSelectionController selectionController = echoManager.GetComponent<EchoSelectionController>();
            if (selectionController == null)
            {
                selectionController = echoManager.gameObject.AddComponent<EchoSelectionController>();
            }

            EchoSwapController swapController = echoManager.GetComponent<EchoSwapController>();
            if (swapController == null)
            {
                swapController = echoManager.gameObject.AddComponent<EchoSwapController>();
            }

            PlayerInputHandler inputHandler = Object.FindObjectOfType<PlayerInputHandler>();
            PlayerController playerController = inputHandler != null ? inputHandler.GetComponent<PlayerController>() : null;
            CharacterController characterController = inputHandler != null ? inputHandler.GetComponent<CharacterController>() : null;
            GroundChecker groundChecker = inputHandler != null ? inputHandler.GetComponent<GroundChecker>() : null;
            EchoConfig echoConfig = AssetDatabase.LoadAssetAtPath<EchoConfig>(EchoConfigPath);

            var serializedSwap = new SerializedObject(swapController);
            serializedSwap.FindProperty("echoConfig").objectReferenceValue = echoConfig;
            serializedSwap.FindProperty("selectionController").objectReferenceValue = selectionController;
            serializedSwap.FindProperty("inputHandler").objectReferenceValue = inputHandler;
            serializedSwap.FindProperty("playerController").objectReferenceValue = playerController;
            serializedSwap.FindProperty("playerTransform").objectReferenceValue = inputHandler != null ? inputHandler.transform : null;
            serializedSwap.FindProperty("playerCharacterController").objectReferenceValue = characterController;
            serializedSwap.FindProperty("groundChecker").objectReferenceValue = groundChecker;
            serializedSwap.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(swapController);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
