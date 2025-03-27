using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMoving : MonoBehaviour
{
    [SerializeField] float speed = 0.3f;

    // Update is called once per frame
    void Update()
    {
        transform.Translate(new Vector2(Time.deltaTime * speed, 0f));
    }
}
