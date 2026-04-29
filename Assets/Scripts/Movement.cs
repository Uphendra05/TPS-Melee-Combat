using UnityEngine;

public class Movement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.transform.localPosition = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.AddComponent<Rigidbody>();

    }
}
