using UnityEngine;
using UnityEngine.UI;

public class ObjectiveArrow : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private RectTransform arrowUI;
    [SerializeField] private Transform player;
    [SerializeField] private Camera playerCamera;

    private Transform target;

    private void Update()
    {
        // ?? Se a câmera ainda não foi atribuída (Player 2), tenta encontrar automaticamente
        if (playerCamera == null)
        {
            var split = FindObjectOfType<SplitScreen>();
            if (split != null)
            {
                GameObject camObj = GameObject.Find("Generated Splitscreen Camera");
                if (camObj != null)
                {
                    playerCamera = camObj.GetComponent<Camera>();
                    Debug.Log("[ObjectiveArrow] Câmera do Player 2 vinculada automaticamente!");
                }
            }

            // Se ainda não achou, sai do Update neste frame
            if (playerCamera == null) return;
        }

        // ?? Se faltar referência, desativa seta
        if (arrowUI == null || player == null || target == null || playerCamera == null)
        {
            if (arrowUI != null) arrowUI.gameObject.SetActive(false);
            return;
        }

        arrowUI.gameObject.SetActive(true);

        // Direção entre player e objetivo
        Vector3 dir = target.position - player.position;
        Vector3 camForward = playerCamera.transform.forward;
        camForward.y = 0;

        float angle = Vector3.SignedAngle(camForward, dir, Vector3.up);
        arrowUI.localEulerAngles = new Vector3(0, 0, -angle);

        // Converte posição para a tela
        Vector3 screenPos = playerCamera.WorldToScreenPoint(target.position);
        if (screenPos.z < 0) screenPos *= -1;

        // Mantém dentro da tela
        screenPos.x = Mathf.Clamp(screenPos.x, 80f, Screen.width - 80f);
        screenPos.y = Mathf.Clamp(screenPos.y, 80f, Screen.height - 80f);

        arrowUI.position = screenPos;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (arrowUI != null)
            arrowUI.gameObject.SetActive(target != null);
    }

    public void SetPlayerCamera(Camera cam)
    {
        playerCamera = cam;
    }

    public Camera GetPlayerCamera()
    {
        return playerCamera;
    }
}
