using UnityEngine;
using UnityEngine.UI;

public class ObjectiveArrowSingle : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private RectTransform arrowUI;
    [SerializeField] private Transform player1;
    [SerializeField] private Transform player2;
    [SerializeField] private Camera camera1;
    [SerializeField] private Camera camera2;

    private Transform target;
    private Transform closestPlayer;
    private Camera activeCamera;

    void Update()
    {
        // ?? Verifica se a câmera do Player 2 já existe
        if (camera2 == null)
        {
            GameObject camObj = GameObject.Find("Generated Splitscreen Camera");
            if (camObj != null)
                camera2 = camObj.GetComponent<Camera>();
        }

        // ?? Verifica se há tudo necessário
        if (arrowUI == null || player1 == null || player2 == null || target == null)
        {
            if (arrowUI != null) arrowUI.gameObject.SetActive(false);
            return;
        }

        // ?? Define qual jogador está mais perto do objetivo
        float dist1 = Vector3.Distance(player1.position, target.position);
        float dist2 = Vector3.Distance(player2.position, target.position);

        if (dist1 <= dist2)
        {
            closestPlayer = player1;
            activeCamera = camera1;
        }
        else
        {
            closestPlayer = player2;
            activeCamera = camera2;
        }

        if (closestPlayer == null || activeCamera == null)
        {
            arrowUI.gameObject.SetActive(false);
            return;
        }

        arrowUI.gameObject.SetActive(true);

        // ?? Calcula a direção entre o player mais próximo e o objetivo
        Vector3 dir = target.position - closestPlayer.position;
        Vector3 camForward = activeCamera.transform.forward;
        camForward.y = 0;

        float angle = Vector3.SignedAngle(camForward, dir, Vector3.up);
        arrowUI.localEulerAngles = new Vector3(0, 0, -angle);

        // ?? Posição na tela
        Vector3 screenPos = activeCamera.WorldToScreenPoint(target.position);
        if (screenPos.z < 0) screenPos *= -1;

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
}
