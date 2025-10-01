using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class HighlightableCounter : MonoBehaviour
{
    [Header("Outline Material (use o shader Custom/OutlineUnlit)")]
    [SerializeField] private Material outlineMaterial;

    // Objetos criados para outline
    private List<GameObject> outlineObjects = new List<GameObject>();
    // Meshes bakeados (somente se houver SkinnedMeshRenderer)
    private List<Mesh> bakedMeshes = new List<Mesh>();

    private void Awake()
    {
        if (outlineMaterial == null)
        {
            Debug.LogWarning($"[{nameof(HighlightableCounter)}] outlineMaterial não atribuído em '{name}'. Highlight não funcionará.");
            return;
        }

        // Pega todos os renderers (ativos/inativos) neste objeto e filhos
        var renderers = GetComponentsInChildren<Renderer>(true);

        int idx = 0;
        foreach (var rend in renderers)
        {
            // Ignora renderers do tipo ParticleSystemRenderer etc - nós queremos mesh/skinned
            Mesh meshToUse = null;
            bool isSkinned = false;

            if (rend is MeshRenderer meshRend)
            {
                var mf = meshRend.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                    meshToUse = mf.sharedMesh;
            }
            else if (rend is SkinnedMeshRenderer skinned)
            {
                // Bake uma cópia do mesh na pose atual
                Mesh baked = new Mesh();
                skinned.BakeMesh(baked);
                meshToUse = baked;
                bakedMeshes.Add(baked);
                isSkinned = true;
            }

            if (meshToUse == null)
            {
                // pula renderers que não têm mesh útil
                continue;
            }

            // Cria um GameObject filho para o outline
            GameObject go = new GameObject($"__outline_{name}_{idx++}");
            go.transform.SetParent(rend.transform, false); // parent no renderer para alinhar pivô
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            // Adiciona MeshFilter + MeshRenderer
            var mfOutline = go.AddComponent<MeshFilter>();
            mfOutline.mesh = meshToUse;

            var mrOutline = go.AddComponent<MeshRenderer>();
            mrOutline.sharedMaterial = outlineMaterial;

            // Garanta que o outline não interfira com colisores/logic
            go.layer = rend.gameObject.layer;

            // Desativado por padrão
            go.SetActive(false);

            outlineObjects.Add(go);
        }
    }

    private void OnDestroy()
    {
        // Limpeza: destruir meshes bakeados criados para skinned meshes
        foreach (var m in bakedMeshes)
        {
            if (m != null) Destroy(m);
        }
        bakedMeshes.Clear();

        // Destroy outline objects
        foreach (var go in outlineObjects)
        {
            if (go != null) Destroy(go);
        }
        outlineObjects.Clear();
    }

    /// <summary>Ativa o highlight (liga os clones com material de outline).</summary>
    public void EnableHighlight()
    {
        if (outlineObjects == null) return;
        foreach (var go in outlineObjects)
            if (go != null) go.SetActive(true);
    }

    /// <summary>Desativa o highlight.</summary>
    public void DisableHighlight()
    {
        if (outlineObjects == null) return;
        foreach (var go in outlineObjects)
            if (go != null) go.SetActive(false);
    }

    /// <summary>Toggle.</summary>
    public void ToggleHighlight(bool on)
    {
        if (on) EnableHighlight(); else DisableHighlight();
    }
}
