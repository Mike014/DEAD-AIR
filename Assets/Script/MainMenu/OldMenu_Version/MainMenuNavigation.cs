using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DeadAir.UI
{
    /// <summary>
    /// Gestisce la navigazione del menu principale di DEAD AIR.
    /// Collega i bottoni Start Game e Exit alle rispettive funzioni.
    /// </summary>
    public class MainMenuNavigation : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button _startGameButton;
        [SerializeField] private Button _exitGameButton;

        [Header("Scene Settings")]
        [SerializeField] private string _gameSceneName = "Demo";

        // ============================================
        // UNITY LIFECYCLE
        // ============================================

        private void Start()
        {
            // Validazione componenti
            if (_startGameButton == null || _exitGameButton == null)
            {
                Debug.LogError("[MainMenu] Bottoni non assegnati nell'Inspector!");
                return;
            }

            // Collegamento eventi
            _startGameButton.onClick.AddListener(StartGame);
            _exitGameButton.onClick.AddListener(ExitGame);

            Debug.Log("[MainMenu] Menu Navigation inizializzato.");
        }

        private void OnDestroy()
        {
            // Cleanup per evitare memory leaks
            if (_startGameButton != null)
                _startGameButton.onClick.RemoveListener(StartGame);
            if (_exitGameButton != null)
                _exitGameButton.onClick.RemoveListener(ExitGame);
        }

        // ============================================
        // MENU ACTIONS
        // ============================================

        /// <summary>
        /// Avvia DEAD AIR caricando la scena di gioco.
        /// </summary>
        public void StartGame()
        {
            Debug.Log($"[MainMenu] Tentativo di caricare scena: '{_gameSceneName}'");

            // Ferma TUTTI gli AudioSource della scena MainMenu
            AudioSource[] allMenuAudio = FindObjectsOfType<AudioSource>();
            if (allMenuAudio.Length > 0)
            {
                Debug.Log($"[MainMenu] Fermando {allMenuAudio.Length} AudioSource...");
                foreach (var audio in allMenuAudio)
                {
                    audio.Stop();
                    Debug.Log($"[MainMenu] Fermato: {audio.name}");
                }
            }
            else
            {
                Debug.LogWarning("[MainMenu] Nessun AudioSource trovato!");
            }

            SceneManager.LoadScene(_gameSceneName);
        }
        /// <summary>
        /// Chiude l'applicazione.
        /// </summary>
        public void ExitGame()
        {
            Debug.Log("[MainMenu] Uscita dal gioco...");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
        }
    }
}