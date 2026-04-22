using MAIGame.Echo;
using MAIGame.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MAIGame.EditorTools
{
    public static class Stage4SetupRunner
    {
        private const string TestPlaygroundPath = "Assets/Scenes/Levels/Test_Playground.unity";

        [MenuItem("MAI Game/Setup/Stage 4 Echo Selection")]
        public static void SetupStage4()
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

            PlayerInputHandler inputHandler = Object.FindObjectOfType<PlayerInputHandler>();

            var serializedSelection = new SerializedObject(selectionController);
            serializedSelection.FindProperty("echoManager").objectReferenceValue = echoManager;
            serializedSelection.FindProperty("inputHandler").objectReferenceValue = inputHandler;
            serializedSelection.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(selectionController);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}

