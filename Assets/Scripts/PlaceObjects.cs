using UnityEngine;

public class PlaceObjects : MonoBehaviour
{
    [SerializeField] private Color highlightColor = new(0.2f, 0.2f, 0.2f);
    [SerializeField] private float highlightStrength = 0.5f;

    private Renderer[] _renderers;
    private Color[] _originalColors;
    private bool _isSelected;

    private void Awake()
    {
        CacheRenderers();
    }

    private void CacheRenderers()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        _originalColors = new Color[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
        {
            _originalColors[i] = _renderers[i].material.color;
        }
    }

    public void SetSelected(bool selected)
    {
        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] == null) continue;

            _renderers[i].material.color = _isSelected ?
                Color.Lerp(_originalColors[i], highlightColor, highlightStrength) : _originalColors[i];
        }

    }

    public void SetMaterial(Material material)
    {
        for (int i = 0; i < _renderers.Length; i++)
        {
            _renderers[i].material = material;
            _originalColors[i] = material.color;
        }

        if (_isSelected)
        {
            SetSelected(true);
        }

    }
}
