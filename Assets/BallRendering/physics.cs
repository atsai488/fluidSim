using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class balls : MonoBehaviour
{  
    public float ballRadius = 0.1f;   
    private int steps = 30; 
    public int numBalls;
    private List<Vector2> position;
    private List<Vector2> velocity;
    public List<Vector2> acceleration;
    public float dampingFactor = 0.8f;
    public LineRenderer circleRendererPrefab;
    public Material whiteMaterial; 
    private List<LineRenderer> circleRenderers; 

    void Start()
    {
        // Initialize lists
        position = new List<Vector2>(numBalls);
        velocity = new List<Vector2>(numBalls);
        acceleration = new List<Vector2>(numBalls);
        circleRenderers = new List<LineRenderer>(numBalls);

        for (int i = 0; i < numBalls; i++)
        {
            int xDisplacement = i % 10;
            int yDisplacement = i / 10;
            Vector2 spawnPos = new Vector2(xDisplacement * 2, yDisplacement * 2);
            position.Add(spawnPos);
            velocity.Add(Vector2.zero);
            acceleration.Add(new Vector2(0, -9.81f));

            LineRenderer circleRenderer = Instantiate(circleRendererPrefab, transform);
            circleRenderer.material = whiteMaterial;
            circleRenderers.Add(circleRenderer);
        }
    }

    void Update()
    {
        for (int i = 0; i < numBalls; i++)
        {
            moveBall(i);
            boundaryCollision(i);
            DrawCircle(circleRenderers[i], ballRadius, position[i].x, position[i].y);
        }
    }

    void DrawCircle(LineRenderer circleRenderer, float ballRadius, float xPos, float yPos)
    {
        circleRenderer.positionCount = steps + 1;
        circleRenderer.loop = true; 

        for (int i = 0; i <= steps; i++)
        {
            float circumference = (float)i / steps;
            float currentRad = circumference * 2 * Mathf.PI;
            float xScale = Mathf.Cos(currentRad);
            float yScale = Mathf.Sin(currentRad);
            float x = xScale * ballRadius;
            float y = yScale * ballRadius;
            Vector2 currentPos = new Vector2(xPos + x, yPos + y);
            circleRenderer.SetPosition(i, currentPos);
        }
    }

    void ballCollision()
    {
        
    }

    void moveBall(int i)
    {
        position[i] += velocity[i] * Time.deltaTime;
        velocity[i] += acceleration[i] * Time.deltaTime;
    }

    void boundaryCollision(int i)
    {
        Vector2 halfBoundsSize = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        halfBoundsSize -= Vector2.one * ballRadius;

        // Check horizontal boundaries
        if (Mathf.Abs(position[i].x) > halfBoundsSize.x)
        {
            position[i] = new Vector2(Mathf.Sign(position[i].x) * halfBoundsSize.x, position[i].y);
            velocity[i] = new Vector2(velocity[i].x * -1 * dampingFactor, velocity[i].y);
        }

        // Check vertical boundaries
        if (Mathf.Abs(position[i].y) > halfBoundsSize.y)
        {
            position[i] = new Vector2(position[i].x, Mathf.Sign(position[i].y) * halfBoundsSize.y);
            velocity[i] = new Vector2(velocity[i].x, velocity[i].y * -1 * dampingFactor);
        }
    }
}
