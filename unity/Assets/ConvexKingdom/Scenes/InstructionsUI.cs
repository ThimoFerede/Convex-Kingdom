using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ConvexKingdom;

public class InstructionsUI : MonoBehaviour {
    public void StartGame() {
        SceneManager.LoadScene("Game");
    }
    public void GoToMenu() {
        SceneManager.LoadScene("Menu");
    }
}
