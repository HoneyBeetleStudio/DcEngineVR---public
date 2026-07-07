using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class LiftController : MonoBehaviour
{
    public Transform[] platformlar;
    public float minYukseklik = 0f;
    public float maxYukseklik = 1.7f;
    public float hiz = 0.35f;

    [Header("Klavye testi")]
    public Key testYukariTusu = Key.None;
    public Key testAsagiTusu = Key.None;

    public UnityEvent usteVardi;
    public UnityEvent altaVardi;

    int _yon;
    float _yukseklik;
    Vector3[] _baslangicLocalPozlar;

    public bool UstteMi => _yukseklik >= maxYukseklik - 0.001f;
    public bool AlttaMi => _yukseklik <= minYukseklik + 0.001f;
    public float Yukseklik => _yukseklik;
    public bool YukariCalisti { get; private set; }
    public int AktifYon { get; private set; }

    void Awake()
    {
        _baslangicLocalPozlar = new Vector3[platformlar.Length];
        for (int i = 0; i < platformlar.Length; i++)
        {
            if (platformlar[i] != null)
                _baslangicLocalPozlar[i] = platformlar[i].localPosition;
        }
    }

    public void YonAyarla(int yon)
    {
        _yon = Mathf.Clamp(yon, -1, 1);
    }

    void Update()
    {
        int yon = _yon;

        var klavye = Keyboard.current;
        if (yon == 0 && klavye != null)
        {
            if (testYukariTusu != Key.None && klavye[testYukariTusu].isPressed)
                yon = 1;
            else if (testAsagiTusu != Key.None && klavye[testAsagiTusu].isPressed)
                yon = -1;
        }

        AktifYon = yon;
        if (yon == 1)
            YukariCalisti = true;

        if (yon == 0)
            return;

        float yeni = Mathf.Clamp(_yukseklik + yon * hiz * Time.deltaTime, minYukseklik, maxYukseklik);
        if (Mathf.Approximately(yeni, _yukseklik))
            return;

        bool ustteydi = UstteMi;
        bool alttaydi = AlttaMi;
        _yukseklik = yeni;

        for (int i = 0; i < platformlar.Length; i++)
        {
            Transform p = platformlar[i];
            if (p == null)
                continue;
            Vector3 taban = p.parent != null
                ? p.parent.TransformPoint(_baslangicLocalPozlar[i])
                : _baslangicLocalPozlar[i];
            p.position = taban + Vector3.up * _yukseklik;
        }

        if (!ustteydi && UstteMi) usteVardi?.Invoke();
        if (!alttaydi && AlttaMi) altaVardi?.Invoke();
    }
}
