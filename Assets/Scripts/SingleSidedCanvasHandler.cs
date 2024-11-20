using TMPro;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class SingleSidedCanvasHandler : MonoBehaviour
{
    private void Start()
    {
        // Get all TextMeshPro components in children
        TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>();

        foreach (var text in texts)
        {
            // Set the material to cull back faces
            text.fontMaterial.SetFloat("_Cull", 1);

            // If that doesn't work, try modifying the shared material
            text.fontSharedMaterial.SetFloat("_Cull", 1);
        }

        // Optional: Orient towards camera
        if (Camera.main != null)
        {
            transform.forward = Camera.main.transform.forward;
        }
    }
}