using UnityEngine;

/// <summary>Divides a cube into an even grid when the game starts.</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(BoxCollider))]
public class CubeSplitter : MonoBehaviour
{
    [Header("Кількість частин по кожній осі")]
    [SerializeField, Min(1)] private int partsX = 2;
    [SerializeField, Min(1)] private int partsY = 1;
    [SerializeField, Min(1)] private int partsZ = 2;

    private bool hasSplit;

    private void Start()
    {
        Split();
    }

    public void Split()
    {
        if (hasSplit) return;

        int xCount = Mathf.Max(1, partsX);
        int yCount = Mathf.Max(1, partsY);
        int zCount = Mathf.Max(1, partsZ);
        if ((long)xCount * yCount * zCount > 10000)
        {
            Debug.LogError("CubeSplitter: максимум 10000 частин. Зменш кількість в Inspector.", this);
            return;
        }

        MeshFilter sourceFilter = GetComponent<MeshFilter>();
        MeshRenderer sourceRenderer = GetComponent<MeshRenderer>();
        BoxCollider sourceCollider = GetComponent<BoxCollider>();
        Mesh mesh = sourceFilter.sharedMesh;
        if (mesh == null)
        {
            Debug.LogError("CubeSplitter: у куба немає Mesh.", this);
            return;
        }

        hasSplit = true;
        Bounds bounds = mesh.bounds;
        Vector3 fraction = new Vector3(1f / xCount, 1f / yCount, 1f / zCount);
        Vector3 cellSize = Vector3.Scale(bounds.size, fraction);

        for (int x = 0; x < xCount; x++)
        for (int y = 0; y < yCount; y++)
        for (int z = 0; z < zCount; z++)
        {
            GameObject piece = new GameObject($"Block_{x}_{y}_{z}");
            piece.layer = gameObject.layer;
            piece.tag = gameObject.tag;
            piece.transform.SetParent(transform, false);
            // Work in local space so the original rotation and stretch are preserved.
            Vector3 center = bounds.min + Vector3.Scale(
                cellSize, new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
            piece.transform.localPosition = center - Vector3.Scale(bounds.center, fraction);
            piece.transform.localScale = fraction;

            piece.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = piece.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = sourceRenderer.sharedMaterials;
            renderer.enabled = sourceRenderer.enabled;
            renderer.shadowCastingMode = sourceRenderer.shadowCastingMode;
            renderer.receiveShadows = sourceRenderer.receiveShadows;

            BoxCollider collider = piece.AddComponent<BoxCollider>();
            collider.center = bounds.center;
            collider.size = bounds.size;
            collider.sharedMaterial = sourceCollider.sharedMaterial;
            collider.isTrigger = sourceCollider.isTrigger;
            collider.enabled = sourceCollider.enabled;
        }

        sourceRenderer.enabled = false;
        sourceCollider.enabled = false;
    }

    private void OnValidate()
    {
        partsX = Mathf.Max(1, partsX);
        partsY = Mathf.Max(1, partsY);
        partsZ = Mathf.Max(1, partsZ);
    }
}
