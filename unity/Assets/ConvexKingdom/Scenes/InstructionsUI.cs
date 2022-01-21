using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ConvexKingdom;

public class InstructionsUI : MonoBehaviour {
    public void StartGame() {
        SceneManager.LoadScene("Game");
    }
}
