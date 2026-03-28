using UnityEngine;

public class ApplicationController : MonoBehaviour
{
    void Update()
    {
        QuitGame();
    }

    void QuitGame()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Quit!");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    /*
    (futuro) AudioController.cs → Volume, Mute
    (futuro) SceneController.cs → Load, Reload
    */
}
