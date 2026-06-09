using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public string electricCarSceneName = "Araba"; 

    public string dcEngineSceneName = "BasicScene"; 

    public void LoadElectricCarScene()
    {
        if (!string.IsNullOrEmpty(electricCarSceneName))
        {
            Debug.Log(electricCarSceneName + " sahnesi yükleniyor...");
            SceneManager.LoadScene(electricCarSceneName);
        }
        else
        {
            Debug.LogError("Electric Car Sahne adı girilmemiş!");
        }
    }

    public void LoadDCEngineScene()
    {
        if (!string.IsNullOrEmpty(dcEngineSceneName))
        {
            Debug.Log(dcEngineSceneName + " sahnesi yükleniyor...");
            SceneManager.LoadScene(dcEngineSceneName);
        }
        else
        {
            Debug.LogError("DC Engine Sahne adı girilmemiş!");
        }
    }

    public void QuitApplication()
    {
        Debug.Log("Uygulamadan çıkılıyor...");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
