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
            // Auto-reference se non assegnati nell'Inspector
            if (_button == null)
                _button = GetComponent<Button>();
            
            if (_choiceText == null)
                _choiceText = GetComponentInChildren<TextMeshProUGUI>();
        }
        
        private void OnEnable()
        {
            if (_button != null)
                _button.onClick.AddListener(HandleClick);
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
            _choiceIndex = index;
            _onClickCallback = onClick;
            
            if (_choiceText != null)
            {
                // Formato: "> TESTO_SCELTA" per estetica terminale
                _choiceText.text = $"> {text}";
                _choiceText.color = _normalColor;
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
            _onClickCallback?.Invoke(_choiceIndex);
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
            if (_choiceText != null)
                _choiceText.color = _hoverColor;
        }
        
        /// <summary>
        /// Chiamato quando il mouse esce dal bottone.
        /// </summary>
        public void OnPointerExit()
        {
            if (_choiceText != null)
                _choiceText.color = _normalColor;
        }
    }
}
