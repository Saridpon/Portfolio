using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonClick : MonoBehaviour
{
    // Start is called before the first frame update
    public void restart(){
        SceneManager.LoadScene(1);
    }
    public void start(){
        SceneManager.LoadScene(1);
    }
}
