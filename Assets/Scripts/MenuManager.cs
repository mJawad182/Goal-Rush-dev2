using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{

    public void shop()
    {
        SceneManager.LoadScene("Shop");
    }

    public void QuitGame()
    {
        Application.Quit();
    }


    public void back()
    {
        SceneManager.LoadScene("MainMenu");
    }


}
