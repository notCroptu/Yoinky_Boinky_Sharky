using UnityEngine;

public class InitFPS : MonoBehaviour
{
    void Awake()
    {
        Application.targetFrameRate = 90;
    }
}
