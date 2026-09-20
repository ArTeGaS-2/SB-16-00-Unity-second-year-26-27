using UnityEngine;

namespace LowPolyConstructor
{
    // Decorative only: no physics, navigation, collisions, or gameplay state.
    public sealed class LPKDecorativeFlight : MonoBehaviour
    {
        [Min(0)] public float radius = 1.2f;
        [Min(0)] public float speed = 0.8f;
        [Min(0)] public float hoverHeight = 0.15f;
        [Min(0)] public float wingFrequency = 14f;
        [Range(0, 80)] public float wingAngle = 35f;
        public int seed = 1;
        public Transform[] wings;
        Vector3 origin;
        Quaternion rotation;
        Quaternion[] wingRotations;
        float[] wingSides;

        void OnEnable()
        {
            origin = transform.localPosition;
            rotation = transform.localRotation;
            wingRotations = new Quaternion[wings == null ? 0 : wings.Length];
            wingSides = new float[wingRotations.Length];
            for (int i = 0; i < wingRotations.Length; i++)
                if (wings[i]) {
                    wingRotations[i] = wings[i].localRotation;
                    wingSides[i] = (wingRotations[i] * Vector3.right).x < 0 ? -1 : 1;
                }
        }

        void Update()
        {
            float t = Time.time * speed + seed * 2.39996f;
            Vector3 offset = new Vector3(Mathf.Sin(t) * radius,
                Mathf.Sin(t * 2.3f) * hoverHeight, Mathf.Sin(t * 1.37f) * radius * 0.65f);
            transform.localPosition = origin + rotation * offset;
            Vector3 direction = new Vector3(Mathf.Cos(t), 0, Mathf.Cos(t * 1.37f) * 0.8905f);
            if (direction.sqrMagnitude > 0.0001f)
                transform.localRotation = rotation * Quaternion.LookRotation(direction);
            float flap = Mathf.Sin(Time.time * wingFrequency * Mathf.PI * 2 + seed) * wingAngle;
            for (int i = 0; i < wingRotations.Length; i++)
                if (wings[i]) wings[i].localRotation = wingRotations[i] * Quaternion.Euler(0, 0, flap * wingSides[i]);
        }

        void OnDisable()
        {
            transform.localPosition = origin;
            transform.localRotation = rotation;
            if (wingRotations == null) return;
            for (int i = 0; i < wingRotations.Length; i++)
                if (wings[i]) wings[i].localRotation = wingRotations[i];
        }
    }
}
