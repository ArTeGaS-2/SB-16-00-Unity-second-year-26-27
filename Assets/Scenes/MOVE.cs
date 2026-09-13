using UnityEngine;

public class MOVE : MonoBehaviour
{
    void Update()
    {
        transform.Translate(
            0,      // X
            0,      // Y
            0.01f); // Z

        transform.Rotate(0, 0, 1);
    }
}
