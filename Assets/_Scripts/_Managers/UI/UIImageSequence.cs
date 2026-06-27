using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
public class UIImageSequence : MonoBehaviour
{
    [Header("Configuración")]
    public Sprite[] sprites;
    public float fps = 12f;
    public bool loop = true;

    private Image image;
    private int index = 0;
    private Coroutine animationCoroutine;

    void Awake()
    {
        image = GetComponent<Image>();
        if (sprites.Length > 0) image.sprite = sprites[0];
    }

    // Se ejecuta cada vez que el objeto se activa o desactiva
    void OnEnable()
    {
        // Reiniciar al primer frame al activar (opcional, quita esta línea si quieres que continúe donde quedó)
        index = 0;
        if (sprites.Length > 0) image.sprite = sprites[0];

        // Iniciar la animación solo si hay sprites y no está ya corriendo
        if (sprites.Length > 0 && animationCoroutine == null)
        {
            animationCoroutine = StartCoroutine(AnimateSequence());
        }
    }

    void OnDisable()
    {
        // Detener la animación inmediatamente al desactivar el objeto
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
    }

    IEnumerator AnimateSequence()
    {
        float delay = 1f / fps;

        while (true)
        {
            yield return new WaitForSeconds(delay);

            // Doble verificación de seguridad por si el objeto se desactiva inesperadamente
            if (!gameObject.activeSelf) break;

            index++;
            if (index >= sprites.Length)
            {
                if (loop)
                    index = 0;
                else
                    break; // Termina el bucle si no es loop
            }

            // Actualizar sprite solo si el índice es válido
            if (index < sprites.Length)
                image.sprite = sprites[index];
        }
    }
}