using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void OnPlayClicked()
    {
        SceneManager.LoadScene("Main"); // or your main game scene name
    }

    public void OnContinueClicked()
    {
        // Implement continue logic (e.g., load saved game)
        SceneManager.LoadScene("Main");
    }

    public void OnOptionsClicked()
    {
        // Show options UI or load options scene
        Debug.Log("Options clicked");
    }

    public void OnQuitClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}