using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Advertisements;

public class ButtonNextLevel : MonoBehaviour
{
    private int nextScene = 0;

    public void NextLevelButton(int index)
    {
        //SceneManager.LoadScene(index);
        if (index % 3 == 0)
        {
            var options = new ShowOptions { resultCallback = AfterAdLoadScene };
            nextScene = index;
            if (Advertisement.IsReady()) Advertisement.Show(options);
        }
        else SceneManager.LoadScene(index);

    }

    public void NextLevelButton(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }

    public void MainMenuButton()
    {
        var options = new ShowOptions { resultCallback = AfterAdLoadMainMenu };
        if (Advertisement.IsReady()) Advertisement.Show(options);
        //SceneManager.LoadScene(0);

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
}