using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ballSpawner : MonoBehaviour
{
    public GameObject ballPrefab;
    public int numberOfBalls = 10;
    // Start is called before the first frame update
    void Start()
    {
        spawnBalls();
    }

    void spawnBalls(){
        for (int i = 0; i < numberOfBalls; i++){
            int xDisplacement = i % 10;
            int yDisplacement = i / 10;
            Vector2 spawnPos = new Vector2(xDisplacement*2, yDisplacement*2);
            Instantiate(ballPrefab, spawnPos, Quaternion.identity);
        }
    }
}
