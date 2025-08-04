using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Transform mainCameraTransform;

    void Start()
    {
        // Obtém a referência à câmera principal
        mainCameraTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        // Faz o objeto olhar para a câmera
        transform.LookAt(mainCameraTransform);
        transform.LookAt(2 * transform.position - mainCameraTransform.position);
    }
}
