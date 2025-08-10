using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas), typeof(CanvasScaler))]
public class IntegerCanvasScaler : MonoBehaviour
{
    public Vector2 referenceResolution = new Vector2(320, 180);
    public int minScale = 1; // don’t shrink below 1x

    CanvasScaler scaler;

    void Awake()
    {
        scaler = GetComponent<CanvasScaler>();
        var canvas = GetComponent<Canvas>();
        canvas.pixelPerfect = true;
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize; // key for integer scaling
        ApplyScale();
    }

    void OnRectTransformDimensionsChange() => ApplyScale();
    void Update() => ApplyScale(); // handle orientation/resize in editor/mobile

    void ApplyScale()
    {
        if (scaler == null) return;
        float sx = Screen.width / referenceResolution.x;
        float sy = Screen.height / referenceResolution.y;
        int scale = Mathf.Max(minScale, Mathf.FloorToInt(Mathf.Min(sx, sy)));
        if (Mathf.Abs(scaler.scaleFactor - scale) > 0.001f)
            scaler.scaleFactor = scale;
    }
}