
#if(UNITY_EDITOR)

using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;

public class UserEditor : MonoBehaviour
{
    public static void OpenScene(string sceneName)
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene("Assets/_MhAsset/Scenes/" + sceneName + ".unity");
        }
    }

    [MenuItem("[Open Scene]/1_MH_Lobby")]
    public static void OpenScene_Lobby()
    {
        OpenScene("Lobby");
    }

    [MenuItem("[Open Scene]/2_MH_GamePlay")]
    public static void OpenScene_GamePlay()
    {
        OpenScene("GamePlay_Online");
    }


    
}

#endif
