using UnityEngine;

/// <summary>
/// Drives the two motions every celestial body needs:
///   1. Self-rotation - the body spinning on its own axis (its "day").
///   2. Orbit motion   - the body travelling around a parent body
///                       (planets around the Sun, moons around a planet).
///
/// Attach this to the Sun, every planet and every moon.
/// Leave 'orbitTarget' empty for the Sun (it only spins, it does not orbit).
/// Compatible with Unity 2022.3.62f1.
/// </summary>
[DisallowMultipleComponent]
public class CelestialMotion : MonoBehaviour
{
    [Header("Self-Rotation (spinning on its own axis)")]
    [Tooltip("Turn the body's daily spin on or off.")]
    public bool enableSelfRotation = true;

    [Tooltip("Axis the body spins around. (0,1,0) = an upright spin.")]
    public Vector3 selfRotationAxis = Vector3.up;

    [Tooltip("Spin speed in degrees per second.")]
    public float selfRotationSpeed = 25f;

    [Header("Orbit Motion (travelling around another body)")]
    [Tooltip("The body to orbit. Sun for planets, a planet for moons. " +
             "Leave empty for the Sun so it stays in the centre.")]
    public Transform orbitTarget;

    [Tooltip("Axis the orbit travels around. (0,1,0) = a flat orbit.")]
    public Vector3 orbitAxis = Vector3.up;

    [Tooltip("Orbit speed in degrees per second.")]
    public float orbitSpeed = 12f;

    // Update is called once per frame.
    void Update()
    {
        // --- Self-rotation: spin in place around the chosen local axis ---
        if (enableSelfRotation && selfRotationAxis != Vector3.zero)
        {
            transform.Rotate(selfRotationAxis.normalized,
                             selfRotationSpeed * Time.deltaTime,
                             Space.Self);
        }

        // --- Orbit: revolve around the target body ---
        if (orbitTarget != null && orbitAxis != Vector3.zero)
        {
            transform.RotateAround(orbitTarget.position,
                                   orbitAxis.normalized,
                                   orbitSpeed * Time.deltaTime);
        }
    }
}
