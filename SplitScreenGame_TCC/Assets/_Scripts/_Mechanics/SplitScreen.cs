using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SplitScreen : MonoBehaviour
{
	public enum SplitMode { Dynamic, FixedVertical }

	[Header("Configuração")]
	[SerializeField] private SplitMode splitMode = SplitMode.Dynamic;

	public Transform player1;
	public Transform player2;
	public float splitDistance = 5;
	public Color splitterColor;
	public float splitterWidth;

	private GameObject camera1;
	private GameObject camera2;
	private GameObject split;
	private GameObject splitter;
	private GameObject uiSplitter;

	void Start()
	{
		// Referência da câmera principal
		camera1 = Camera.main.gameObject;
		Camera c1 = camera1.GetComponent<Camera>();

		// Cria a segunda câmera
		camera2 = new GameObject("Generated Splitscreen Camera");
		Camera c2 = camera2.AddComponent<Camera>();
		c2.depth = c1.depth - 1;
		c2.cullingMask = ~(1 << LayerMask.NameToLayer("TransparentFX"));

		// Splitter (barra de divisão)
		splitter = GameObject.CreatePrimitive(PrimitiveType.Quad);
		splitter.transform.parent = gameObject.transform;
		splitter.transform.localPosition = Vector3.forward;
		splitter.transform.localScale = new Vector3(5, splitterWidth / 10, 1);
		splitter.transform.localEulerAngles = Vector3.zero;
		splitter.SetActive(false);

		// Quad para render da segunda câmera
		split = GameObject.CreatePrimitive(PrimitiveType.Quad);
		split.transform.parent = splitter.transform;
		split.transform.localPosition = new Vector3(0, -(1 / (splitterWidth / 10)), 0.0001f);
		split.transform.localScale = new Vector3(1, 2 / (splitterWidth / 10), 1);
		split.transform.localEulerAngles = Vector3.zero;

		// Materiais
		Material tempMat = new Material(Shader.Find("Unlit/Color"));
		tempMat.color = splitterColor;
		splitter.GetComponent<Renderer>().material = tempMat;
		splitter.layer = LayerMask.NameToLayer("TransparentFX");

		Material tempMat2 = new Material(Shader.Find("Mask/SplitScreen"));
		split.GetComponent<Renderer>().material = tempMat2;
		split.layer = LayerMask.NameToLayer("TransparentFX");

		if (splitMode == SplitMode.FixedVertical)
		{
			SetupFixedVertical();
		}
	}

	void LateUpdate()
	{
		if (splitMode == SplitMode.Dynamic)
		{
			UpdateDynamicSplit();
		}
		else if (splitMode == SplitMode.FixedVertical)
		{
			UpdateFixedVertical();
		}
	}

	// ----------------------
	// MODO DINÂMICO (original)
	// ----------------------
	private void UpdateDynamicSplit()
	{
		float zDistance = player1.position.z - player2.transform.position.z;
		float distance = Vector3.Distance(player1.position, player2.transform.position);

		float angle;
		if (player1.transform.position.x <= player2.transform.position.x)
			angle = Mathf.Rad2Deg * Mathf.Acos(zDistance / distance);
		else
			angle = Mathf.Rad2Deg * Mathf.Asin(zDistance / distance) - 90;

		splitter.transform.localEulerAngles = new Vector3(0, 0, angle);

		Vector3 midPoint = (player1.position + player2.position) / 2;

		if (distance > splitDistance)
		{
			Vector3 offset = midPoint - player1.position;
			offset.x = Mathf.Clamp(offset.x, -splitDistance / 2, splitDistance / 2);
			offset.y = Mathf.Clamp(offset.y, -splitDistance / 2, splitDistance / 2);
			offset.z = Mathf.Clamp(offset.z, -splitDistance / 2, splitDistance / 2);
			midPoint = player1.position + offset;

			Vector3 midPoint2 = player2.position - offset;

			if (!splitter.activeSelf)
			{
				splitter.SetActive(true);
				camera2.SetActive(true);

				camera2.transform.position = camera1.transform.position;
				camera2.transform.rotation = camera1.transform.rotation;
			}
			else
			{
				camera2.transform.position = Vector3.Lerp(camera2.transform.position, midPoint2 + new Vector3(0, 13, -10), Time.deltaTime * 5);
				Quaternion newRot2 = Quaternion.LookRotation(midPoint2 - camera2.transform.position);
				camera2.transform.rotation = Quaternion.Lerp(camera2.transform.rotation, newRot2, Time.deltaTime * 5);
			}
		}
		else
		{
			if (splitter.activeSelf)
			{
				splitter.SetActive(false);
				camera2.SetActive(false);
			}
		}

		camera1.transform.position = Vector3.Lerp(camera1.transform.position, midPoint + new Vector3(0, 13, -10), Time.deltaTime * 5);
		Quaternion newRot = Quaternion.LookRotation(midPoint - camera1.transform.position);
		camera1.transform.rotation = Quaternion.Lerp(camera1.transform.rotation, newRot, Time.deltaTime * 5);
	}

	// ----------------------
	// MODO FIXO VERTICAL
	// ----------------------
	private void SetupFixedVertical()
	{
		camera2.SetActive(true);

		Camera c1 = camera1.GetComponent<Camera>();
		Camera c2 = camera2.GetComponent<Camera>();

		// Divide a tela: esquerda e direita
		c1.rect = new Rect(0f, 0f, 0.5f, 1f);
		c2.rect = new Rect(0.5f, 0f, 0.5f, 1f);

		// Cria o splitter de UI se ainda não existir
		if (uiSplitter == null)
		{
			Canvas canvas = new GameObject("SplitCanvas").AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			canvas.sortingOrder = 9; // garante que fique por cima

			GameObject quad = new GameObject("VerticalSplitter");
			quad.transform.SetParent(canvas.transform, false);

			RectTransform rt = quad.AddComponent<RectTransform>();
			rt.anchorMin = new Vector2(0.5f, 0f);
			rt.anchorMax = new Vector2(0.5f, 1f);
			rt.pivot = new Vector2(0.5f, 0.5f);
			rt.sizeDelta = new Vector2(12, 0);

			Image img = quad.AddComponent<Image>();
			img.color = splitterColor.a == 0 ? Color.black : splitterColor; // fallback vermelho se alpha = 0

			uiSplitter = quad;
		}

		uiSplitter.SetActive(true);
	}

	private void UpdateFixedVertical()
	{
		// Câmera 1 segue player 1
		camera1.transform.position = Vector3.Lerp(camera1.transform.position, player1.position + new Vector3(0, 13, -10), Time.deltaTime * 5);
		Quaternion rot1 = Quaternion.LookRotation(player1.position - camera1.transform.position);
		camera1.transform.rotation = Quaternion.Lerp(camera1.transform.rotation, rot1, Time.deltaTime * 5);

		// Câmera 2 segue player 2
		camera2.transform.position = Vector3.Lerp(camera2.transform.position, player2.position + new Vector3(0, 13, -10), Time.deltaTime * 5);
		Quaternion rot2 = Quaternion.LookRotation(player2.position - camera2.transform.position);
		camera2.transform.rotation = Quaternion.Lerp(camera2.transform.rotation, rot2, Time.deltaTime * 5);
	}
}
