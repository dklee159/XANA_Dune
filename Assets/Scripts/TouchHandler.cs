using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchHandler : MonoBehaviour
{
    [SerializeField] GameObject board;

    public void TouchingCrab()
    {
        board.GetComponent<InputManager>().OnTouchingCrab();
    }
    
    public void TouchingFinish()
    {
        board.GetComponent<InputManager>().force = 500f;
        SandGameManager.Instance.GameOver();
    }
}
