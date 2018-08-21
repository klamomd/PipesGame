using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Advertisements;

public class ButtonNextLevel : MonoBehaviour
{
    private int nextScene = 0;
    public AudioSource snapSound1;

    public void NextLevelButton(int index)
    {
        //SceneManager.LoadScene(index);
        PlaySnap();
        // Only play ads every 4 scenes
        if (index % 4 == 0)
        {
            var options = new ShowOptions { resultCallback = AfterAdLoadScene };
            nextScene = index;
            if (Advertisement.IsReady()) Advertisement.Show(options);
        }
        else SceneManager.LoadScene(index);

    }

    public void NextLevelButton(string levelName)
    {
        PlaySnap();
        SceneManager.LoadScene(levelName);
    }

    public void MainMenuButton()
    {
        var options = new ShowOptions { resultCallback = AfterAdLoadMainMenu };
        PlaySnap();
        if (Advertisement.IsReady()) Advertisement.Show(options);
        else SceneManager.LoadScene(0);

    }

    private void AfterAdLoadScene(ShowResult result)
    {
        SceneManager.LoadScene(nextScene);
        nextScene = 0;
    }

    private void AfterAdLoadMainMenu(ShowResult result)
    {
        SceneManager.LoadScene(0);
    }

    private void PlaySnap()
    {
        if (snapSound1 != null) snapSound1.Play();
    }
}