using UnityEngine;

public class BumpySurfaceGenerator : MonoBehaviour
{
    [Header("区域尺寸(米)")]
    public Vector2 areaSize = new Vector2(16f, 4f);

    [Header("密度/强度")]
    public float spacing = 0.30f;     // 越小越密
    public float minHeight = 0.08f;   // 抬高，变“狠”
    public float maxHeight = 0.18f;

    [Header("凸起平面尺寸(米)")]
    public Vector2 bumpSizeRange = new Vector2(0.25f, 0.55f);

    [Header("噪声控制")]
    public float perlinScale = 1.6f;
    public float perlinThreshold = 0.46f;

    [ContextMenu("Generate Bumps (Strong)")]
    public void Generate()
    {
        // 清空旧的
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        Vector3 origin = transform.position - new Vector3(areaSize.x * 0.5f, 0, areaSize.y * 0.5f);
        int nx = Mathf.Max(1, Mathf.RoundToInt(areaSize.x / spacing));
        int nz = Mathf.Max(1, Mathf.RoundToInt(areaSize.y / spacing));

        for (int ix = 0; ix < nx; ix++)
            for (int iz = 0; iz < nz; iz++)
            {
                Vector3 pos = origin + new Vector3(ix * spacing, 0, iz * spacing);
                if (Mathf.PerlinNoise(pos.x * perlinScale, pos.z * perlinScale) < perlinThreshold) continue;

                float h = Random.Range(minHeight, maxHeight);
                float sx = Random.Range(bumpSizeRange.x, bumpSizeRange.y);
                float sz = Random.Range(bumpSizeRange.x, bumpSizeRange.y);

                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);   // 自带 BoxCollider
                go.name = $"bump_{ix}_{iz}";
                go.transform.SetParent(transform, false);
                go.transform.position = new Vector3(pos.x, h * 0.5f, pos.z); // 让方块“冒头”
                go.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                go.transform.localScale = new Vector3(sx, h, sz);

                go.GetComponent<BoxCollider>().isTrigger = false;

                // 不可见，仅碰撞
                var mr = go.GetComponent<MeshRenderer>();
                if (mr) mr.enabled = false;

                go.tag = "Bump";
            }
    }
}
