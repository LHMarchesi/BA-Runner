using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class BackgroundFitToCamera : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;

    [Header("Padding")]
    [SerializeField] private float widthPadding = 1.05f;
    [SerializeField] private float heightPadding = 1.05f;

    private void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        FitToCamera();
    }

    private void Update()
    {
        FitToCamera();
    }

    private void FitToCamera()
    {
        if (targetCamera == null)
            return;

        if (!targetCamera.orthographic)
        {
            FitPerspective();
            return;
        }

        FitOrthographic();
    }

    private void FitOrthographic()
    {
        float worldHeight =
            targetCamera.orthographicSize * 2f;

        float worldWidth =
            worldHeight * targetCamera.aspect;

        worldWidth *= widthPadding;
        worldHeight *= heightPadding;

        Vector3 scale = transform.localScale;

        scale.x =
            worldWidth / 10f;

        scale.z =
            worldHeight / 10f;

        transform.localScale = scale;
    }

    private void FitPerspective()
    {
        /*
         * Distancia desde el plano hasta la cámara.
         */
        float distance =
            Mathf.Abs(
                transform.position.z -
                targetCamera.transform.position.z
            );

        float verticalFov =
            targetCamera.fieldOfView *
            Mathf.Deg2Rad;

        float worldHeight =
            2f *
            distance *
            Mathf.Tan(
                verticalFov * 0.5f
            );

        float worldWidth =
            worldHeight *
            targetCamera.aspect;

        worldWidth *= widthPadding;
        worldHeight *= heightPadding;

        Vector3 scale =
            transform.localScale;

        scale.x =
            worldWidth / 10f;

        scale.z =
            worldHeight / 10f;

        transform.localScale =
            scale;
    }
}