using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugTool : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] bool useDebugSpeed;
    [SerializeField] float debugSpeed;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (useDebugSpeed && Time.timeScale == 1.0f) Time.timeScale = debugSpeed;
        if (useDebugSpeed && Time.timeScale != debugSpeed) Time.timeScale = debugSpeed;
        if (!useDebugSpeed && Time.timeScale == debugSpeed) Time.timeScale = 1.0f;
    }
}
