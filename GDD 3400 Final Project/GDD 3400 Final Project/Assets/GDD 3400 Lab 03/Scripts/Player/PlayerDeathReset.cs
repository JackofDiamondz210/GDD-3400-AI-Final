using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PlayerDeathReset : MonoBehaviour
{
    //quick reset level and is mentioned at text at tend of level
    private void Update()
    {
        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            ResetLevel();
        }
    }
    //if you hit an enemy with either collider it will reset the level
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hurt"))
        {
            ResetLevel();
        }
    }
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("Hurt"))
        {
            ResetLevel();
        }
    }

    //reseting the level to be played again
    void ResetLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
}
