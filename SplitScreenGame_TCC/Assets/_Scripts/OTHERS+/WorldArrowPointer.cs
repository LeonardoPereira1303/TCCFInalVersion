using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class WorldArrowPointer : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Transform player1;
    [SerializeField] private Transform player2;
    [SerializeField] private Camera camera1;
    [SerializeField] private Camera camera2;

    [Header("Ajustes da seta")]
    [SerializeField] private float arrowHeight = 2.0f;         // Altura em relação ao solo
    [SerializeField] private float offsetFromTarget = 1.5f;    // Distância para não encobrir o objetivo
    [SerializeField] private float arrowHeadLength = 0.5f;     // Tamanho da ponta
    [SerializeField] private float arrowHeadAngle = 25.0f;     // Ângulo da ponta

    private LineRenderer bodyLine;
    private LineRenderer headLine;
    private Transform target;
    private Transform closestPlayer;
    private Camera activeCamera;

    private void Awake()
    {
        // Corpo da seta
        bodyLine = GetComponent<LineRenderer>();
        bodyLine.positionCount = 2;
        bodyLine.startWidth = 0.08f;
        bodyLine.endWidth = 0.08f;
        bodyLine.material = new Material(Shader.Find("Sprites/Default"));
        bodyLine.startColor = Color.red;
        bodyLine.endColor = Color.red;

        // Cabeça da seta
        GameObject headObj = new GameObject("ArrowHead");
        headObj.transform.SetParent(transform);
        headLine = headObj.AddComponent<LineRenderer>();
        headLine.positionCount = 3;
        headLine.startWidth = 0.08f;
        headLine.endWidth = 0.0f;
        headLine.material = new Material(Shader.Find("Sprites/Default"));
        headLine.startColor = Color.red;
        headLine.endColor = Color.red;
    }

    private void Update()
    {
        // Verifica se a câmera do Player 2 já foi gerada
        if (camera2 == null)
        {
            GameObject camObj = GameObject.Find("Generated Splitscreen Camera");
            if (camObj != null)
                camera2 = camObj.GetComponent<Camera>();
        }

        if (player1 == null || player2 == null || target == null)
        {
            bodyLine.enabled = false;
            headLine.enabled = false;
            return;
        }

        // Define qual jogador está mais perto do objetivo
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
            bodyLine.enabled = false;
            headLine.enabled = false;
            return;
        }

        bodyLine.enabled = true;
        headLine.enabled = true;

        // Calcula a posição de início e fim da seta
        Vector3 startPos = closestPlayer.position + Vector3.up * arrowHeight;
        Vector3 dir = (target.position - startPos).normalized;
        Vector3 endPos = target.position - dir * offsetFromTarget + Vector3.up * arrowHeight;

        // Desenha o corpo
        bodyLine.SetPosition(0, startPos);
        bodyLine.SetPosition(1, endPos);

        // Desenha a ponta da seta
        DrawArrowHead(endPos, dir);
    }

    private void DrawArrowHead(Vector3 tipPosition, Vector3 direction)
    {
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * Vector3.forward;
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * Vector3.forward;

        headLine.SetPosition(0, tipPosition);
        headLine.SetPosition(1, tipPosition + right * arrowHeadLength);
        headLine.SetPosition(2, tipPosition + left * arrowHeadLength);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        bodyLine.enabled = target != null;
        headLine.enabled = target != null;
    }
}
