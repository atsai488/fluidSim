using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class balls: MonoBehaviour {
  public float ballRadius = 0.1f;
  private int steps = 30;
  public int numBalls;
  public float kernelRadius;
  private List < Vector2 > positions;
  public List < Vector2 > velocities;
  public float dampingFactor = 0.8f;
  public LineRenderer circleRendererPrefab;
  public Material whiteMaterial;
  private List < LineRenderer > circleRenderers;
  private Vector2 halfBoundsSize;
  private List < float > densities;
  public float pressureMultiplier = 1;
  public float restingDensity = 0.5f;

  void Start() {
    positions = new List < Vector2 > (numBalls);
    velocities = new List < Vector2 > (numBalls);
    circleRenderers = new List < LineRenderer > (numBalls);
    densities = new List < float > (numBalls);

    halfBoundsSize = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
    halfBoundsSize -= Vector2.one * ballRadius;

    for (int i = 0; i < numBalls; i++) {
    float randomX = UnityEngine.Random.Range(-halfBoundsSize.x, halfBoundsSize.x);
    float randomY = UnityEngine.Random.Range(-halfBoundsSize.y, halfBoundsSize.y);
    Vector2 spawnPos = new Vector2(randomX, randomY);

    positions.Add(spawnPos);
    velocities.Add(Vector2.zero);
    densities.Add(0f);

    LineRenderer circleRenderer = Instantiate(circleRendererPrefab, transform);
    circleRenderer.material = whiteMaterial;
    circleRenderers.Add(circleRenderer);
    }
  }

  void Update() {
    float deltaTime = Time.deltaTime;
    Parallel.For(0, numBalls, i => {
      moveBall(i, deltaTime);
      checkCollision();
      boundaryCollision(i);
    });
    // Check for collisions and resolve them
    Parallel.For(0, numBalls, i => {
        calculateDensity(i); 
    });
    for (int i = 0; i < numBalls; i++) {
        calculateAcceleration(i, deltaTime);
        DrawCircle(circleRenderers[i], ballRadius, positions[i].x, positions[i].y);
    }
  }
  void checkCollision() {
    for (int i = 0; i < numBalls; i++) {
      for (int j = i + 1; j < numBalls; j++) {
        handleCollision(i, j);
      }
    }
  }
  void handleCollision(int i, int j) {
    Vector2 dir = positions[i] - positions[j];
    float dist = dir.magnitude;
    float minDist = 2 * ballRadius;
    if (dist < minDist) {
      Vector2 norm = dir / dist;
      Vector2 relVel = velocities[i] - velocities[j];
      float relSpeed = Vector2.Dot(relVel, norm);

      if (relSpeed < 0) {
        float restitution = Mathf.Sqrt(0.8f);
        float impulse = (1 + restitution) * relSpeed / (1 / ballRadius + 1 / ballRadius);
        velocities[i] -= impulse * norm / ballRadius;
        velocities[j] += impulse * norm / ballRadius;
      }

      float overlap = 0.5f * (minDist - dist);
      positions[i] += overlap * norm;
      positions[j] -= overlap * norm;
    }
  }

  void calculateAcceleration(int i, float deltaTime) {
   
    velocities[i] += calculatePressure(i)/densities[i]*deltaTime;
    velocities[i] += externalForceCalculation(positions[i])/densities[i]*deltaTime;
    // viscosityForceCalculation();
  }

  void pressureForceCalculation() {
    // Placeholder method
  }

  void viscosityForceCalculation() {
    // Placeholder method
  }

  Vector2 externalForceCalculation(Vector2 Point) {
    return new Vector2(0, -5f);
  }

  void DrawCircle(LineRenderer circleRenderer, float ballRadius, float xPos, float yPos) {
    circleRenderer.positionCount = steps + 1;
    circleRenderer.loop = true;

    for (int i = 0; i <= steps; i++) {
      float circumference = (float) i / steps;
      float currentRad = circumference * 2 * Mathf.PI;
      float xScale = Mathf.Cos(currentRad);
      float yScale = Mathf.Sin(currentRad);
      float x = xScale * ballRadius;
      float y = yScale * ballRadius;
      Vector2 currentPos = new Vector2(xPos + x, yPos + y);
      circleRenderer.SetPosition(i, currentPos);
    }
  }

  Vector2 calculatePressure(int particle) {
    Vector2 pressureForce = Vector2.zero;
    int mass = 1;
    for (int i = 0; i < numBalls; i++) {
        if (i==particle) continue;
        Vector2 offset = positions[i] - positions[particle];
      float dist = offset.magnitude;
      Vector2 dir = dist == 0 ? getRandomDir() : offset / dist;
      float slope = smoothingKernelDerivative(kernelRadius, dist);
      float sharedPressureForce = sharedPressure(densities[i], densities[particle]);
      pressureForce -= sharedPressureForce * dir *slope *mass /densities[i];
    }
    return pressureForce;
  }
Vector2 getRandomDir() {
    float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2);
    Vector2 randomDir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    return randomDir;
}

float sharedPressure(float densityA, float desnityB){
  float pressureA = ConvertDensiyToPressure(densityA);
  float pressureB = ConvertDensiyToPressure(desnityB);
  return (pressureA + pressureB)/2;
}
float ConvertDensiyToPressure(float density){
    float densityError = density - restingDensity;
    float pressure = densityError * pressureMultiplier;
    return pressure;
}
  static float smoothingKernel(float radius, float dist) {
    float volume = Mathf.PI * Mathf.Pow(radius, 5) / 10;
    float val = Mathf.Max(0, radius - Mathf.Abs(dist));
    return val * val * val / volume;
  }

  static float smoothingKernelDerivative(float dist, float radius) {
    if (dist > radius) return 0;
    float f = radius - dist;
    float scale = -30 / Mathf.PI / Mathf.Pow(radius, 5);
    return scale * f * f;
  }

  void calculateDensity(int i) {
    int mass = 1;
    float density = 0f;
    foreach(Vector2 position in positions) {
        float dist = (positions[i] - position).magnitude;
        density += mass * smoothingKernel(kernelRadius, dist);
    }
    densities[i] = density;
  }

  void moveBall(int i, float deltaTime) {
    positions[i] += velocities[i] * deltaTime;
  }

  void boundaryCollision(int i) {
    // Check horizontal boundaries
    if (Mathf.Abs(positions[i].x) > halfBoundsSize.x) {
      positions[i] = new Vector2(Mathf.Sign(positions[i].x) * halfBoundsSize.x, positions[i].y);
      velocities[i] = new Vector2(velocities[i].x * -1 * dampingFactor, velocities[i].y);

    }

    // Check vertical boundaries
    if (Mathf.Abs(positions[i].y) > halfBoundsSize.y) {
      positions[i] = new Vector2(positions[i].x, Mathf.Sign(positions[i].y) * halfBoundsSize.y);
      velocities[i] = new Vector2(velocities[i].x, velocities[i].y * -1 * dampingFactor);

    }
  }
}