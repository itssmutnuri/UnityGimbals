using UnityEngine;

// what modes are there,
public enum ModeType { Translate, Rotate, Scale }

// holder class for the data for each mode.
public class Mode
{
    public ModeType type;
    public LayerMask layer;
    public Color highlightColor;
    public float? minValue;
    public float? maxValue;

    public Mode(ModeType type, LayerMask layer, Color highlightColor,
                float? minValue = null, float? maxValue = null)
    {
        this.type = type;
        this.layer = layer;
        this.highlightColor = highlightColor;
        this.minValue = minValue;
        this.maxValue = maxValue;
    }
}