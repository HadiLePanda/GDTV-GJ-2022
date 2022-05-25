using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GenerateRow : MonoBehaviour
{
    public int rows;
    public float spacing;
    public int columns;
    public GameObject placeObject;
    public float noiseFactor;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Generate()
    {
        if (rows > 0)
        {
            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    Place(x, y);
                }
            }
        }
    }

    public void Clear()
    {
        List<Transform> toDestroy = new List<Transform>();
        foreach(Transform t in transform)
        {
            toDestroy.Add(t);
        }
        foreach(Transform t in toDestroy)
        {
            DestroyImmediate(t.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Place(int x, int y)
    {
        float noise = Random.Range(-1f, 1f) * noiseFactor;
        Vector3 position = new Vector3(x * spacing + noise, 0, y * spacing + noise);
        GameObject newObj = Instantiate(placeObject, transform);
        newObj.transform.localPosition = position;
    }
}
