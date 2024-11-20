using UnityEngine;
using System.Collections.Generic;

public class FootstepsSystem : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private List<AudioClip> footstepSounds;
    [SerializeField] private float minVolume = 0.8f;
    [SerializeField] private float maxVolume = 1.0f;
    [SerializeField] private float minPitch = 0.9f;
    [SerializeField] private float maxPitch = 1.1f;

    [Header("Movement Settings")]
    [SerializeField] private float stepInterval = 0.5f;
    private float stepTimer;

    private Vector2 lastMovement;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        audioSource.volume = 1f;
        audioSource.pitch = 1f;
    }

    private void Update()
    {
        Vector2 currentMovement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // Only update timer and play footsteps if we're actually moving
        if (currentMovement != Vector2.zero)
        {
            if (lastMovement == Vector2.zero)
            {
                // Just started moving, reset timer
                stepTimer = stepInterval;
            }

            stepTimer += Time.deltaTime;

            if (stepTimer >= stepInterval)
            {
                PlayRandomFootstep();
                stepTimer = 0f;
            }
        }

        lastMovement = currentMovement;
    }

    public void PlayRandomFootstep()
    {
        if (footstepSounds == null || footstepSounds.Count == 0) return;

        AudioClip randomClip = footstepSounds[Random.Range(0, footstepSounds.Count)];
        if (randomClip == null) return;

        audioSource.volume = Random.Range(minVolume, maxVolume);
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(randomClip);
    }
}