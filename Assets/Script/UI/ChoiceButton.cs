using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DeadAir.UI
{
    /// <summary>
    /// Componente per un singolo bottone di scelta.
    /// Prefab riutilizzabile istanziato dinamicamente da DialogueUI.
    /// 
    /// Responsabilità SINGOLA:
    /// - Mostrare il testo della scelta
    /// - Notificare quando viene cliccato
    /// 
    /// Design: Usa callback invece di eventi per semplicità —
    /// la comunicazione è 1:1 con DialogueUI, non broadcast.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ChoiceButton : MonoBehaviour
    {
        // ============================================
        // SERIALIZED FIELDS
        // ============================================

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _choiceText;
        [SerializeField] private Button _button;

        [Header("Styling")]
        [SerializeField] private Color _normalColor = new Color(1f, 0.69f, 0f);      // Ambra
        [SerializeField] private Color _hoverColor = new Color(1f, 1f, 1f);          // Bianco
        [SerializeField] private Color _pressedColor = new Color(0.8f, 0.55f, 0f);   // Ambra scuro

        // ============================================
        // PRIVATE STATE
        // ============================================

        private int _choiceIndex;
        private Action<int> _onClickCallback;

        // ============================================
        // UNITY LIFECYCLE
        // ============================================

        private void Awake()
        {
            Debug.Log($"[ChoiceButton] Awake - GameObject: {gameObject.name}");
            // Auto-reference se non assegnati nell'Inspector
            if (_button == null)
                _button = GetComponent<Button>();
            Debug.Log($"[ChoiceButton] Button component: {(_button != null ? "FOUND" : "MISSING")}");

            if (_choiceText == null)
                _choiceText = GetComponentInChildren<TextMeshProUGUI>();
            Debug.Log($"[ChoiceButton] TMP component: {(_choiceText != null ? "FOUND" : "MISSING")}");
        }

        private void Update()
        {
            // Debug: verifica se button è ancora valido
            if (_button != null && !_button.interactable)
            {
                Debug.LogWarning($"[ChoiceButton] Button {_choiceIndex} is NOT interactable!");
            }

            // Debug: log mouse click raw
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log($"[ChoiceButton] Mouse clicked - Button exists: {_button != null}, Callback exists: {_onClickCallback != null}");
            }
        }

        private void OnEnable()
        {
            Debug.Log($"[ChoiceButton] OnEnable - Registering click handler");
            if (_button != null)
            {
                _button.onClick.AddListener(HandleClick);
                Debug.Log($"[ChoiceButton] Button interactable: {_button.interactable}");
                Debug.Log($"[ChoiceButton] Button listener count: {_button.onClick.GetPersistentEventCount()}");
            }
            else
            {
                Debug.LogError("[ChoiceButton] Button is NULL in OnEnable!");
            }

        }

        private void OnDisable()
        {
            if (_button != null)
                _button.onClick.RemoveListener(HandleClick);
        }

        // ============================================
        // PUBLIC API
        // ============================================

        /// <summary>
        /// Configura il bottone con indice, testo e callback.
        /// Chiamato da DialogueUI quando istanzia il prefab.
        /// </summary>
        /// <param name="index">Indice della scelta (0-based)</param>
        /// <param name="text">Testo da mostrare</param>
        /// <param name="onClick">Callback quando cliccato</param>
        public void Setup(int index, string text, Action<int> onClick)
        {
            Debug.Log($"[ChoiceButton] Setup called - Index: {index}, Text: '{text}', Callback: {(onClick != null ? "SET" : "NULL")}");
            _choiceIndex = index;
            _onClickCallback = onClick;

            if (_choiceText != null)
            {
                // Formato: "> TESTO_SCELTA" per estetica terminale
                _choiceText.text = $"> {text}";
                _choiceText.color = _normalColor;
                Debug.Log($"[ChoiceButton] Text set to: '{_choiceText.text}'");
            }
            else
            {
                Debug.LogError("[ChoiceButton] _choiceText is NULL in Setup!");
            }

            ConfigureButtonColors();
        }

        // ============================================
        // PRIVATE METHODS
        // ============================================

        /// <summary>
        /// Gestisce il click sul bottone.
        /// </summary>
        private void HandleClick()
        {
            Debug.Log($"[ChoiceButton] HandleClick FIRED - Index: {_choiceIndex}");

            if (_onClickCallback != null)
            {
                Debug.Log($"[ChoiceButton] Invoking callback for choice {_choiceIndex}");
                _onClickCallback.Invoke(_choiceIndex);
            }
            else
            {
                Debug.LogError($"[ChoiceButton] Callback is NULL! Cannot invoke choice {_choiceIndex}");
            }
        }
        /// <summary>
        /// Configura i colori del bottone per hover/pressed states.
        /// </summary>
        private void ConfigureButtonColors()
        {
            if (_button == null)
                return;

            // Configura ColorBlock per transizioni
            ColorBlock colors = _button.colors;
            colors.normalColor = Color.clear;           // Background trasparente
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.1f);  // Leggero highlight
            colors.pressedColor = new Color(1f, 1f, 1f, 0.2f);
            colors.selectedColor = colors.highlightedColor;
            colors.fadeDuration = 0.1f;
            _button.colors = colors;
        }

        // ============================================
        // HOVER EFFECTS (opzionale, via EventTrigger o codice)
        // ============================================

        /// <summary>
        /// Chiamato quando il mouse entra nel bottone.
        /// Può essere collegato via EventTrigger nel prefab.
        /// </summary>
        public void OnPointerEnter()
        {
            Debug.Log($"[ChoiceButton] Mouse ENTER on choice {_choiceIndex}");
            if (_choiceText != null)
                _choiceText.color = _hoverColor;
        }

        /// <summary>
        /// Chiamato quando il mouse esce dal bottone.
        /// </summary>
        public void OnPointerExit()
        {
            Debug.Log($"[ChoiceButton] Mouse EXIT from choice {_choiceIndex}");
            if (_choiceText != null)
                _choiceText.color = _normalColor;
        }
    }
}
