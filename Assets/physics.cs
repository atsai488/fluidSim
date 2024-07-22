using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class physics : MonoBehaviour
{  
    public float ballRadius;    
    private Vector2 velocity;
    public Vector2 acceleration = new Vector2(0, -20f);
    public float dampingFactor = 0.8f;

    void Start()
    {
        ballRadius = transform.localScale.x / 2;
    }

    void Update()
    {
        moveBall();
        boundaryCollision();
    }

    void ballCollision(){
        
    }
    void moveBall()
    {
        transform.Translate(velocity * Time.deltaTime);
        velocity += acceleration * Time.deltaTime;
    }

    void boundaryCollision()
    {
        Vector2 halfBoundsSize = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        halfBoundsSize -= Vector2.one * ballRadius;

        if (Mathf.Abs(transform.position.x) > halfBoundsSize.x)
        {
            transform.position = new Vector2(Mathf.Sign(transform.position.x) * halfBoundsSize.x, transform.position.y);
            velocity.x *= -1;
        }

        if (Mathf.Abs(transform.position.y) > halfBoundsSize.y)
        {
            transform.position = new Vector2(transform.position.x, Mathf.Sign(transform.position.y) * halfBoundsSize.y);
            velocity.y *= -1 * dampingFactor;
        }
    }
}

