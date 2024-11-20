using DG.Tweening;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "PaintingSettings", menuName = "Gallery/Painting Settings")]
public class PaintingSettings : ScriptableObject
{
    [Header("UI Settings")]
    public GameObject uiPrefab;
    public float uiOffset = 10f;

    [Header("Interaction Settings")]
    public float defaultInteractionDistance = 5f;
    public float bigPaintingInteractionDistance = 5f;
    public KeyCode interactionKey = KeyCode.E;

    [Header("Camera Settings")]
    public float zoomDistance = 5f;
    public float zoomDuration = 1f;

    [Header("UI Colors")]
    public Color titleColor = Color.white;
    public Color descriptionColor = new Color(0.8f, 0.8f, 0.8f, 1f);

    [Header("UI Fonts")]
    public TMP_FontAsset titleFont;
    public TMP_FontAsset descriptionFont;

    [Header("UI Sizes")]
    public float titleFontSize = 36f;
    public float descriptionFontSize = 24f;

    [Header("Animation Settings")]
    public float animationDuration = 0.3f;
    public Ease showEase = Ease.OutQuart;
    public Ease hideEase = Ease.InQuart;
    public float startScale = 0.01f;
    public float fadeDelay = 0.1f;
    public float textStagger = 0.05f;

    [Header("Typewriter Settings")]
    public float charactersPerSecond = 30f;
    public float typewriterDelay = 0.2f;
    public Ease typewriterEase = Ease.Linear;
}