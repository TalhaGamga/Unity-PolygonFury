using UnityEngine;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

[System.Serializable]
public class FireEffect
{
    [SerializeField] private GameObject _ligthsParent;
    [SerializeField] private Light2D _spriteLight;
    [SerializeField] private Light2D _spotLight;

    [Header("Settings")]
    [SerializeField] private float _flashDuration = 0.1f;
    [SerializeField] private float _spriteMaxIntensity = 2f;
    [SerializeField] private float _spotMaxIntensity = 3f;
    [SerializeField] private float _spriteRotationRange = 30f;

    public void SimulateFireEffect()
    {
        if (_spriteLight == null || _spotLight == null) return;

        _ligthsParent.SetActive(true);

        DOTween.Kill(_spriteLight);
        DOTween.Kill(_spotLight);

        float randomAngle = Random.Range(-_spriteRotationRange, _spriteRotationRange);
        _spriteLight.transform.localRotation = Quaternion.Euler(0, 0, randomAngle);

        _spriteLight.intensity = 0f;
        _spotLight.intensity = 0f;

        Sequence seq = DOTween.Sequence();

        seq.AppendCallback(() =>
        {
            _spriteLight.intensity = _spriteMaxIntensity;
            _spotLight.intensity = _spotMaxIntensity;
        });

        seq.Append(DOTween.To(() => _spriteLight.intensity,
                              x => _spriteLight.intensity = x,
                              0f, _flashDuration).SetEase(Ease.OutQuad));

        seq.Join(DOTween.To(() => _spotLight.intensity,
                             x => _spotLight.intensity = x,
                             0f, _flashDuration).SetEase(Ease.OutQuad));

        seq.OnComplete(() => _ligthsParent.SetActive(false));

        seq.Play();
    }
}
