using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;

    /// <summary>
    /// What do we aim at
    /// </summary>
    [SerializeField]
    private Transform target = null;
    /// <summary>
    /// How much does the mouse pos get averaged into the pos?
    /// </summary>
    [SerializeField,Range(0,1)]
    private float mouseInfluence = 0;

    /// <summary>
    /// How fast does the camera move?
    /// </summary>
    [SerializeField]
    private float cameraSpeed = 0;

    [SerializeField]
    private float cameraShakeDecreaseRate = 10;

    [SerializeField]
    private ParallaxPlane[] parallaxLayers = new ParallaxPlane[0];

    /// <summary>
    /// How much camera shake is there
    /// </summary>
    private float shakeAmount = 0;
    
    /// <summary>
    /// Refrence to the camera
    /// </summary>
    private new Camera camera;

    [System.Serializable]
    public struct ParallaxPlane
    {
        public Transform transform;
        public Vector2 parallaxAmount;
        public void Set(Vector3 pos)
        {
            pos.z = 0;
            transform.position = pos * parallaxAmount;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        camera = GetComponent<Camera>();
    }

    /// <summary>
    /// Move the camera
    /// </summary>
    private void Update()
    {
        if (target == null) return;

        float dt = Time.deltaTime;

        Vector3 pos = target.position;
        if (mouseInfluence > 0)
        {
            Vector3 mousePos = camera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            pos = Vector3.Lerp(pos, mousePos, mouseInfluence);
        }

        pos.z = -10;

        transform.position = Vector3.Lerp(transform.position,pos, dt * cameraSpeed);
        if (shakeAmount > 0)
        {
            transform.position += (Vector3)(Random.insideUnitCircle * shakeAmount);
            shakeAmount -= dt * cameraShakeDecreaseRate;
        }

        //Move parallax layers
        foreach (ParallaxPlane plane in parallaxLayers)
        {
            plane.Set(transform.position);
        }
    }

    public void AddCameraShake(float amt)
    {
        shakeAmount = Mathf.Min(shakeAmount + amt, 5);
    }
}
