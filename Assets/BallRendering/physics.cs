using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    private List<float> desnsities;
    private List<Vector2> pressures;
    public float idealGasConstant = 1;
    public float restingDensity = 0.5f;
   void Start()
{
    positions = new List<Vector2>(numBalls);
    velocities = new List<Vector2>(numBalls);
    accelerations = new List<Vector2>(numBalls);
    circleRenderers = new List<LineRenderer>(numBalls);
    desnsities = new List<float>(numBalls);

    halfBoundsSize = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
    halfBoundsSize -= Vector2.one * ballRadius;

    for (int i = 0; i < numBalls; i++)
    {
        // Generate random positions within the screen boundaries
        float randomX = UnityEngine.Random.Range(-halfBoundsSize.x, halfBoundsSize.x);
        float randomY = UnityEngine.Random.Range(-halfBoundsSize.y, halfBoundsSize.y);
        Vector2 spawnPos = new Vector2(randomX, randomY);

        positions.Add(spawnPos);
        velocities.Add(Vector2.zero);
        accelerations.Add(new Vector2(0, 0));
        desnsities.Add(0f);
        
        LineRenderer circleRenderer = Instantiate(circleRendererPrefab, transform);
        circleRenderer.material = whiteMaterial;
        circleRenderers.Add(circleRenderer);
    }
}


    void Update()
    {
        float deltaTime = Time.deltaTime;
        Parallel.For(0, numBalls, i =>
        {
            moveBall(i, deltaTime);
            boundaryCollision(i);
        });
        calculateDensity();
        calculateAcceleration();
        for (int i = 0; i < numBalls; i++)
        {
            DrawCircle(circleRenderers[i], ballRadius, positions[i].x, positions[i].y);
        }
    }
    void calculateAcceleration(){
        for (int i = 0; i<numBalls; i++){
            accelerations[i] += calculatePressure(i);
            // accelerations[i] += externalForceCalculaiton(positions[i]);
        }
        // viscocityForceCalculation();
    }
    void pressureForceCalculation(){

    }
    void viscocityForceCalculation(){

    }
    void externalForceCalculaiton(Vector2 Point){
        // return new Vector2(0, -9.8f);
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

    Vector2 calculatePressure(int particle){
        Vector2 point = positions[particle];
        Vector2 propertyGradient = Vector2.zero;
        Vector2 basis = new Vector2(1, 0);
        int mass = 1;
        for (int i = 0; i < numBalls; i++)
        {
            float dist = (positions[i] - point).magnitude;
            float angle = Vector2.Angle(positions[i]-point, basis);
            float slope = smoothingKernalDerivative(kernelRadius, dist);
            float density = desnsities[i];
            Vector2 contribution = new Vector2(slope*Mathf.Cos(angle), slope*Mathf.Sin(angle));
            propertyGradient +=  idealGasConstant*(desnsities[i]-restingDensity)/desnsities[i]*contribution * mass;
        }
        return propertyGradient;
    }
    static float smoothingKernel(float radius, float dist)
    {
        //volume is included to allow for dynamic smoothing radius
        float volume = Mathf.PI*Mathf.Pow(radius,5)/10;
        float val = Mathf.Max(0, radius - Mathf.Abs(dist));
        return val * val * val / volume;
    }
    static float smoothingKernalDerivative(float dist, float radius)
    {
        if (dist > radius) return 0;
        float f = radius - dist;
        float scale = -30 / Mathf.PI / Mathf.Pow(radius, 5);
        return scale * f * f;
    }

    void calculateDensity()
    {
        for (int i = 0; i < numBalls; i++)
        {
            int mass = 1;
            float density = 0f;
            foreach (Vector2 position in positions){
                float dist = (positions[i] - position).magnitude;
                density += mass * smoothingKernel(kernelRadius, dist);
            }
            desnsities[i] = density;
        }
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
            accelerations[i] = Vector2.zero;
        }

        // Check vertical boundaries
        if (Mathf.Abs(positions[i].y) > halfBoundsSize.y)
        {
            positions[i] = new Vector2(positions[i].x, Mathf.Sign(positions[i].y) * halfBoundsSize.y);
            velocities[i] = new Vector2(velocities[i].x, velocities[i].y * -1 * dampingFactor);
            accelerations[i] = Vector2.zero;

        }
    }
}
