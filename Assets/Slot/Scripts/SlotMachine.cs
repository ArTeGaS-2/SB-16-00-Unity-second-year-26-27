using System.Collections;
using UnityEngine;

namespace ITFriends.Slots
{
    [DisallowMultipleComponent]
    public sealed class SlotMachine : MonoBehaviour
    {
        [Header("Модель")]
        [SerializeField] private Transform[] reels = new Transform[3];
        [SerializeField] private Transform leverPivot;
        [SerializeField] private Transform payoutExit;
        [SerializeField] private TextMesh statusLabel;
        [SerializeField] private Light jackpotLight;
        [Header("Виграш")]
        [SerializeField] private GameObject coinPrefab;
        [SerializeField] private Material[] catMaterials;
        [SerializeField] private Material coinEdgeMaterial;
        [Header("Обертання")]
        [SerializeField, Min(.5f)] private float spinDuration = 2.4f;
        [SerializeField, Min(.1f)] private float reelStopDelay = .4f;
        [SerializeField, Min(.02f)] private float coinInterval = .10f;
        [Header("Звук")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip startSound;
        [SerializeField] private AudioClip stopSound;
        [SerializeField] private AudioClip winSound;

        public bool IsSpinning { get; private set; }
        public int SpinCount { get; private set; }
        public int LastPayout { get; private set; }
        public int CoinsSpawned { get; private set; }
        public int[] LastResult => (int[])lastResult.Clone();
        private readonly int[] lastResult = new int[3];
        private Quaternion leverRest = Quaternion.identity;
        private const int SymbolCount = 6;

        private void Awake()
        {
            if (leverPivot) leverRest = leverPivot.localRotation;
            SetStatus("FREE PLAY", new Color(.56f, 1f, .68f));
            if (jackpotLight) jackpotLight.enabled = false;
        }

        public bool TrySpin()
        {
            if (!isActiveAndEnabled || IsSpinning) return false;
            if (reels == null || reels.Length != 3 || !reels[0] || !reels[1] || !reels[2] ||
                !payoutExit || !coinPrefab || catMaterials == null || catMaterials.Length == 0)
            {
                Debug.LogError("Слотмашина: не підключені барабани або монети.", this);
                return false;
            }
            // Без ставки: кожен завершений спін знову дозволяє натиснути E.
            IsSpinning = true;
            SpinCount++;
            LastPayout = 0;
            for (int i = 0; i < 3; i++) lastResult[i] = Random.Range(0, SymbolCount);
            StartCoroutine(Spin());
            return true;
        }

        private IEnumerator Spin()
        {
            SetStatus("GOOD LUCK", new Color(1f, .85f, .46f));
            if (jackpotLight) jackpotLight.enabled = false;
            Play(startSound);
            float[] starts = new float[3], ends = new float[3];
            for (int i = 0; i < 3; i++) {
                starts[i] = reels[i].localEulerAngles.x;
                ends[i] = -lastResult[i] * 60f;
                while (ends[i] > starts[i] - 1080f - i * 360f) ends[i] -= 360f;
            }
            float elapsed = 0;
            bool[] stopped = new bool[3];
            float total = spinDuration + reelStopDelay * 2;
            while (elapsed < total)
            {
                elapsed += Time.deltaTime;
                for (int i = 0; i < 3; i++) {
                    float t = Mathf.Clamp01(elapsed / (spinDuration + reelStopDelay * i));
                    float eased = 1f - Mathf.Pow(1f - t, 3f);
                    reels[i].localRotation = Quaternion.Euler(Mathf.Lerp(starts[i], ends[i], eased), 0, 0);
                    if (t >= 1 && !stopped[i]) { stopped[i] = true; Play(stopSound); }
                }
                if (leverPivot) {
                    float pull = elapsed < .25f ? elapsed / .25f : 1f - Mathf.Clamp01((elapsed - .25f) / .55f);
                    leverPivot.localRotation = leverRest * Quaternion.Euler(-48f * pull, 0, 0);
                }
                yield return null;
            }
            for (int i = 0; i < 3; i++) reels[i].localRotation = Quaternion.Euler(-lastResult[i] * 60f, 0, 0);
            if (leverPivot) leverPivot.localRotation = leverRest;
            LastPayout = CalculatePayout(lastResult[0], lastResult[1], lastResult[2]);
            if (LastPayout > 0) {
                SetStatus(LastPayout >= 15 ? "JACKPOT!" : "WIN  +" + LastPayout, new Color(1f, .87f, .42f));
                if (jackpotLight) jackpotLight.enabled = true;
                Play(winSound);
                for (int i = 0; i < LastPayout; i++) {
                    SpawnCoin();
                    yield return new WaitForSeconds(coinInterval);
                }
            } else SetStatus("TRY AGAIN", new Color(.75f, .86f, .78f));
            IsSpinning = false;
        }

        // BAR, дзвін, вишня, виноград, лимон, сімка. Порядок відповідає текстурі барабанів.
        public static int CalculatePayout(int a, int b, int c)
        {
            if (a == b && b == c) return a == 5 ? 20 : 10;
            return a == b || b == c || a == c ? 3 : 0;
        }

        private void SpawnCoin()
        {
            Vector3 position = payoutExit.position + transform.right * Random.Range(-.11f, .11f);
            GameObject coin = Instantiate(coinPrefab, position, transform.rotation * Quaternion.Euler(
                Random.Range(-15f, 15f), Random.Range(-20f, 20f), Random.Range(-30f, 30f)));
            coin.name = "CatCoin_" + (++CoinsSpawned).ToString("0000");
            // GravityGun.cs шукає саме Object на об'єкті Rigidbody.
            coin.tag = "Object";
            MeshRenderer renderer = coin.GetComponent<MeshRenderer>();
            if (renderer) renderer.sharedMaterials = new[] {
                coinEdgeMaterial, catMaterials[Random.Range(0, catMaterials.Length)]
            };
            Rigidbody body = coin.GetComponent<Rigidbody>();
            if (body) {
                body.linearVelocity = transform.TransformDirection(new Vector3(Random.Range(-.25f,.25f), 1.0f, -1.0f));
                body.angularVelocity = Random.insideUnitSphere * 4f;
            }
        }

        private void SetStatus(string text, Color color)
        {
            if (!statusLabel) return;
            statusLabel.text = text;
            statusLabel.color = color;
        }
        private void Play(AudioClip clip) { if (audioSource && clip) audioSource.PlayOneShot(clip); }
        private void OnDisable()
        {
            StopAllCoroutines();
            IsSpinning = false;
            if (leverPivot) leverPivot.localRotation = leverRest;
            if (audioSource) audioSource.Stop();
        }
    }
}
