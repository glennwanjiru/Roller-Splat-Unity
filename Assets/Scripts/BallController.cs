using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public Rigidbody rb;
    public float speed = 15;

    [Header("Audio Settings")]
    public AudioClip swooshSound;
    public AudioClip stopSound;
    public float volume = 1;

    [Header("Particle Effects")]
    public ParticleSystem stopParticleEffect;


    public int minSwipeRecognition = 500;

    private AudioSource audioSource;
    private bool isTraveling;
    private Vector3 travelDirection;

    private Vector2 swipePosLastFrame;
    private Vector2 swipePosCurrentFrame;
    private Vector2 currentSwipe;

    private Vector3 nextCollisionPosition;

    private Color solveColor;

    private void Start()
    {
        solveColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.9f, 1f);
        GetComponent<MeshRenderer>().material.color = solveColor;
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = volume;
    }

    private void FixedUpdate()
    {
        // Set the ball's speed when it should travel
        if (isTraveling)
        {
            rb.velocity = travelDirection * speed;
        }

        // Paint the ground
        Collider[] hitColliders = Physics.OverlapSphere(transform.position - (Vector3.up / 2), .05f);
        int i = 0;
        while (i < hitColliders.Length)
        {
            GroundPiece ground = hitColliders[i].transform.GetComponent<GroundPiece>();

            if (ground && !ground.isColored)
            {
                ground.Colored(solveColor);
            }

            i++;
        }

        // Check if we have reached our destination
        if (nextCollisionPosition != Vector3.zero)
        {
            if (Vector3.Distance(transform.position, nextCollisionPosition) < 1)
            {
                isTraveling = false;
                travelDirection = Vector3.zero;
                nextCollisionPosition = Vector3.zero;

                // Play stop sound when the ball stops moving
                PlayStopSound();
                PlayStopParticleEffect();


            }
        }


        if (isTraveling)
            return;

        // Swipe mechanism
        if (Input.GetMouseButton(0))
        {
            // Where is the mouse now?
            swipePosCurrentFrame = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

            if (swipePosLastFrame != Vector2.zero)
            {
                // Calculate the swipe direction
                currentSwipe = swipePosCurrentFrame - swipePosLastFrame;

                if (currentSwipe.sqrMagnitude < minSwipeRecognition) // Minimum amount of swipe recognition
                    return;

                currentSwipe.Normalize(); // Normalize it to only get the direction, not the distance

                // Up/Down swipe
                if (currentSwipe.x > -0.5f && currentSwipe.x < 0.5f)
                {
                    SetDestination(currentSwipe.y > 0 ? Vector3.forward : Vector3.back);
                }

                // Left/Right swipe
                if (currentSwipe.y > -0.5f && currentSwipe.y < 0.5f)
                {
                    SetDestination(currentSwipe.x > 0 ? Vector3.right : Vector3.left);
                }
            }

            swipePosLastFrame = swipePosCurrentFrame;
        }

        if (Input.GetMouseButtonUp(0))
        {
            swipePosLastFrame = Vector2.zero;
            currentSwipe = Vector2.zero;
        }
    }
    private void PlayStopParticleEffect()
    {
        if (stopParticleEffect != null)
        {
            // Instantiate the particle system at the ball's position when it stops
            ParticleSystem stopEffectInstance = Instantiate(stopParticleEffect, transform.position, Quaternion.identity);
            stopEffectInstance.Play();

            // Destroy the particle system after its duration
            Destroy(stopEffectInstance.gameObject, stopEffectInstance.main.duration);
        }
    }

    public void PlayMoveSound()
    {
        if (!audioSource.isPlaying || audioSource.clip != swooshSound)
        {
            audioSource.clip = swooshSound;
            audioSource.Play();
        }
    }

    public void PlayStopSound()
    {
        if (!audioSource.isPlaying || audioSource.clip != stopSound)
        {
            audioSource.clip = stopSound;
            audioSource.Play();
        }
    }

    private void SetDestination(Vector3 direction)
    {
        travelDirection = direction;

        // Check with which object we will collide
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, 100f))
        {
            nextCollisionPosition = hit.point;
        }

        isTraveling = true;

        // Play move sound when the ball starts moving
        PlayMoveSound();
    }
}
