using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    // Stack degli stati. Lo stato in cima è quello attivo.
    private Stack<IMenuState> _stateStack;
    
    // Riferimenti ai pannelli UI (assegnati da Inspector).
    [Header("UI Panels")]
    [SerializeField] private GameObject _mainMenuPanel;
    [SerializeField] private GameObject _optionsPanel;
    [SerializeField] private GameObject _creditsPanel;
    
    [Header("Scene Management")]
    [SerializeField] private string _gameSceneName = "Demo";
    
    // Proprietà pubbliche per accesso ai pannelli dagli stati.
    public GameObject MainMenuPanel => _mainMenuPanel;
    public GameObject OptionsPanel => _optionsPanel;
    public GameObject CreditsPanel => _creditsPanel;
    
    private void Awake()
    {
        _stateStack = new Stack<IMenuState>();
    }
    
    private void Start()
    {
        // Nascondi tutti i pannelli all'inizio.
        HideAllPanels();
        
        // Stato iniziale: MainMenu.
        PushState(new MainMenuState(this));
    }
    
    private void Update()
    {
        // Delega Update allo stato corrente.
        if (_stateStack.Count > 0)
        {
            _stateStack.Peek().Update(this);
        }
        
        // Gestione input "Back" (Escape).
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleBackInput();
        }
    }
    
    // PUSH: aggiunge stato in cima.
    public void PushState(IMenuState newState)
    {
        // Exit sullo stato corrente (se esiste) — viene "coperto".
        if (_stateStack.Count > 0)
        {
            _stateStack.Peek().Exit(this);
        }
        
        _stateStack.Push(newState);
        newState.Enter(this);
    }
    
    // POP: rimuove stato corrente, torna al precedente.
    public void PopState()
    {
        if (_stateStack.Count > 0)
        {
            IMenuState oldState = _stateStack.Pop();
            oldState.Exit(this);
        }
        
        // Riattiva lo stato sotto (se esiste).
        if (_stateStack.Count > 0)
        {
            _stateStack.Peek().Enter(this);
        }
    }
    
    // Gestisce il tasto Back/Escape.
    private void HandleBackInput()
    {
        // Non fare nulla se siamo allo stato root (MainMenu).
        if (_stateStack.Count <= 1)
        {
            return;
        }
        
        // Chiedi allo stato corrente se vuole gestire il Back.
        if (_stateStack.Peek().HandleBack(this))
        {
            PopState();
        }
    }
    
    // Utility per nascondere tutti i pannelli.
    private void HideAllPanels()
    {
        if (_mainMenuPanel != null) _mainMenuPanel.SetActive(false);
        if (_optionsPanel != null) _optionsPanel.SetActive(false);
        if (_creditsPanel != null) _creditsPanel.SetActive(false);
    }
    
    // ============================================
    // START GAME - Caricamento scena gameplay
    // ============================================
    
    public void StartGame()
    {
        Debug.Log($"[MenuController] Caricamento scena: {_gameSceneName}");
        
        // Cleanup audio menu
        StopAllMenuAudio();
        
        // Load game scene
        SceneManager.LoadScene(_gameSceneName);
    }
    
    private void StopAllMenuAudio()
    {
        AudioSource[] menuAudio = FindObjectsOfType<AudioSource>();
        if (menuAudio.Length > 0)
        {
            Debug.Log($"[MenuController] Fermando {menuAudio.Length} AudioSource...");
            foreach (var audio in menuAudio)
            {
                audio.Stop();
            }
        }
    }
    
    // ============================================
    // QUIT GAME
    // ============================================
    
    public void QuitGame()
    {
        Debug.Log("[MenuController] Uscita dal gioco...");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}