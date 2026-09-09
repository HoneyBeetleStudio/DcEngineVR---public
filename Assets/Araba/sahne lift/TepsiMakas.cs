using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class TepsiMakas : MonoBehaviour
{
    public Transform sase;
    public Transform platform;
    public Transform[] makaslar;
    public float kalinlik = 0.2f;
    public float yanPay = 0.22f;
    public float caprazPay = 0.05f;
    public float gomme = 0.28f;

    void OnEnable()
    {
        OtomatikBagla();
        Guncelle();
#if UNITY_EDITOR
        EditorApplication.update -= EditorTick;
        EditorApplication.update += EditorTick;
#endif
    }

    void OnDisable()
    {
#if UNITY_EDITOR
        EditorApplication.update -= EditorTick;
#endif
    }

#if UNITY_EDITOR
    void EditorTick()
    {
        if (!this)
            return;
        Guncelle();
    }
#endif

    void Update()
    {
        Guncelle();
    }

    void LateUpdate()
    {
        Guncelle();
    }

    void OtomatikBagla()
    {
        if (sase == null)
            sase = transform.Find("Sase");
        if (platform == null)
            platform = transform.Find("Platform");

        if (makaslar != null && makaslar.Length > 0)
            return;

        var bulunan = new System.Collections.Generic.List<Transform>();
        foreach (Transform t in GetComponentsInChildren<Transform>(true))
        {
            if (t != null && t.name == "Makas")
                bulunan.Add(t);
        }
        makaslar = bulunan.ToArray();
    }

    public void Guncelle()
    {
        if (sase == null || platform == null || makaslar == null || makaslar.Length == 0)
            OtomatikBagla();
        if (sase == null || platform == null || makaslar == null || makaslar.Length == 0)
            return;

        Renderer saseR = sase.GetComponent<Renderer>();
        Transform tabla = platform.Find("Tabla");
        Renderer tablaR = tabla != null
            ? tabla.GetComponent<Renderer>()
            : platform.GetComponentInChildren<Renderer>();

        float altY = (saseR != null ? saseR.bounds.max.y : sase.position.y) - gomme;
        float ustY = (tablaR != null ? tablaR.bounds.min.y : platform.position.y) + gomme;
        if (ustY < altY + 0.02f)
            ustY = altY + 0.02f;

        Vector3 sag = transform.right;
        Vector3 ileri = transform.forward;

        for (int i = 0; i < makaslar.Length; i++)
        {
            Transform m = makaslar[i];
            if (m == null)
                continue;

            float yan = (i % 2 == 0) ? -1f : 1f;
            Vector3 yanVec = sag * (yan * yanPay * Mathf.Abs(transform.lossyScale.x));
            Vector3 capraz = ileri * (caprazPay * Mathf.Abs(transform.lossyScale.z));

            Vector3 alt = sase.position + yanVec;
            alt.y = altY;
            Vector3 ust = platform.position + yanVec;
            ust.y = ustY;

            if (yan < 0f)
            {
                alt += capraz;
                ust -= capraz;
            }
            else
            {
                alt -= capraz;
                ust += capraz;
            }

            Vector3 delta = ust - alt;
            float uzunluk = delta.magnitude;
            if (uzunluk < 0.01f)
                continue;

            m.SetPositionAndRotation((alt + ust) * 0.5f, Quaternion.LookRotation(delta.normalized, transform.up));

            Transform ebeveyn = m.parent != null ? m.parent : transform;
            float parentZ = Mathf.Max(Mathf.Abs(ebeveyn.lossyScale.z), 0.0001f);
            float parentX = Mathf.Max(Mathf.Abs(ebeveyn.lossyScale.x), 0.0001f);
            float parentY = Mathf.Max(Mathf.Abs(ebeveyn.lossyScale.y), 0.0001f);
            m.localScale = new Vector3(kalinlik / parentX, kalinlik / parentY, uzunluk / parentZ);
        }
    }
}
