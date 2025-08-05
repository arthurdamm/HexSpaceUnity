using UnityEngine;

public class InputSanityCheck : MonoBehaviour
{
    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        if (Mathf.Abs(h) > 0.01f)
        {
            Debug.Log("Horizontal axis input: " + h);
        }
    }
}