using UnityEngine;

public class StartScreen : MonoBehaviour
{
    public string FirstLevel;

    public void Start()
    {
        GameManager.Instance.Reset();
    }

    public void Update()
    {
        
        if (!Input.GetMouseButtonDown(0))
            return;

        Application.LoadLevel(FirstLevel);
    }
}
