using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class balls : MonoBehaviour
{  
    public float ballRadius = 0.1f;   
    private int steps = 30; 
    public int numBalls;
    public float kernelRadius;
    private List<Vector2> positions;
    private List<Vector2> velocities;
    public List<Vector2> accelerations;
    public float dampingFactor = 0.8f;
    public LineRenderer circleRendererPrefab;
    public Material whiteMaterial; 
    private List<LineRenderer> circleRenderers; 
    private Vector2 halfBoundsSize;


    void Start()
    {
        // Initialize lists
        positions = new List<Vector2>(numBalls);
        velocities = new List<Vector2>(numBalls);
        accelerations = new List<Vector2>(numBalls);
        circleRenderers = new List<LineRenderer>(numBalls);
        for (int i = 0; i < numBalls; i++)
        {
            int xDisplacement = i % 10;
            int yDisplacement = i / 10;
            Vector2 spawnPos = new Vector2(xDisplacement * 2, yDisplacement * 2);
            positions.Add(spawnPos);
            velocities.Add(Vector2.zero);
            accelerations.Add(new Vector2(0, -9.81f));
            LineRenderer circleRenderer = Instantiate(circleRendererPrefab, transform);
            circleRenderer.material = whiteMaterial;
            circleRenderers.Add(circleRenderer);
        }
        halfBoundsSize = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        halfBoundsSize -= Vector2.one * ballRadius;
    }

    void Update()
    {
        float deltaTime = Time.deltaTime;
        Parallel.For(0, numBalls, i =>
        {
            moveBall(i, deltaTime);
            boundaryCollision(i);
        });
        for (int i = 0; i < numBalls; i++)
        {
            DrawCircle(circleRenderers[i], ballRadius, positions[i].x, positions[i].y);
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
    // Vector2 calculateGradient(Vector2 point){
    //     Vector2 propertyGradient = Vector2.zero;
    //     for (int i = 0; i < numBalls; i++)
    //     {
    //         float dist = (positions[i] - point).magnitude;
    //         float slope = smoothingKernalDerivative(kernelRadius, dist);
    //         propertyGradient+= 
    //     }
    // }
    static float smoothingKernel(float radius, float dist)
    {
        //volume is included to allow for dynamic smoothing radius
        float volume = Mathf.PI*Mathf.Pow(radius,5)/10;
        float val = Mathf.Max(0, radius - dist);
        return val * val * val / volume;
    }
    static float smoothingKernalDerivative(float dist, float radius)
    {
        if (dist > radius) return 0;
        float f = radius - dist;
        float scale = -30 / Mathf.PI / Mathf.Pow(radius, 5);
        return scale * f * f;
    }
    float calculateDensity(Vector2 pos)
    {
        int mass = 1;
        float density = 0f;
        foreach (Vector2 position in positions){
            float dist = (pos - position).magnitude;
            density += mass * smoothingKernel(kernelRadius, dist);
        }
        return density;
    }

    void moveBall(int i, float deltaTime)
    {
        positions[i] += velocities[i] * deltaTime;
        velocities[i] += accelerations[i] * deltaTime;
    }

    void boundaryCollision(int i)
    {
       // Check horizontal boundaries
        if (Mathf.Abs(positions[i].x) > halfBoundsSize.x)
        {
            positions[i] = new Vector2(Mathf.Sign(positions[i].x) * halfBoundsSize.x, positions[i].y);
            velocities[i] = new Vector2(velocities[i].x * -1 * dampingFactor, velocities[i].y);
        }

        // Check vertical boundaries
        if (Mathf.Abs(positions[i].y) > halfBoundsSize.y)
        {
            positions[i] = new Vector2(positions[i].x, Mathf.Sign(positions[i].y) * halfBoundsSize.y);
            velocities[i] = new Vector2(velocities[i].x, velocities[i].y * -1 * dampingFactor);
        }
    }
}
