using UnityEngine;
using UnityEngine.InputSystem;

namespace ITFriends.Slots
{
    [DisallowMultipleComponent]
    public sealed class SlotPlayerInteractor : MonoBehaviour
    {
        [Header("Наведення")]
        [SerializeField] private Camera viewCamera;
        [SerializeField, Min(0.5f)] private float interactionDistance = 3.5f;
        [SerializeField] private LayerMask interactionLayers = ~0;
        [SerializeField] private bool showPrompt = true;
        public SlotMachine CurrentTarget { get; private set; }
        public Camera ViewCamera { get => viewCamera; set => viewCamera = value; }

        private void Awake()
        {
            if (!viewCamera) viewCamera = GetComponentInChildren<Camera>();
            if (!viewCamera) viewCamera = Camera.main;
        }

        private void Update()
        {
            CurrentTarget = FindTarget();
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                TryInteract();
        }

        public SlotMachine FindTarget()
        {
            if (!viewCamera) return null;
            Ray ray = new Ray(viewCamera.transform.position, viewCamera.transform.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit, interactionDistance,
                    interactionLayers, QueryTriggerInteraction.Ignore)) return null;
            return hit.collider.GetComponentInParent<SlotMachine>();
        }

        public bool TryInteract()
        {
            CurrentTarget = FindTarget();
            return CurrentTarget && CurrentTarget.TrySpin();
        }

        private void OnGUI()
        {
            if (!showPrompt || !CurrentTarget) return;
            float width = Mathf.Min(460, Screen.width - 24);
            Rect rect = new Rect((Screen.width - width) / 2, Screen.height - 105, width, 64);
            string text = CurrentTarget.IsSpinning ? "БАРАБАНИ ОБЕРТАЮТЬСЯ…" : "[ E ]  КРУТИТИ";
            if (!CurrentTarget.IsSpinning) text += "\nБезкоштовно · необмежені спроби";
            GUI.Box(rect, GUIContent.none);
            GUIStyle style = new GUIStyle(GUI.skin.label) {
                alignment = TextAnchor.MiddleCenter, fontSize = 18, wordWrap = true
            };
            style.normal.textColor = new Color(1f, .88f, .58f);
            GUI.Label(rect, text, style);
        }
    }
}
