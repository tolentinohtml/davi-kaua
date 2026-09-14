using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Singleton

    // Instância estática para acesso global ao GameManager (Padrão Singleton)
    public static GameManager Instance { get; private set; }

    #endregion

    #region Inspector Fields

    [Header("Configurações de Cenas")]
    [Tooltip("Nome da cena principal de gameplay")]
    public string GameplaySceneName = "Gameplay";

    [Tooltip("Nome da cena de interface carregada em modo aditivo")]
    public string GUISceneName = "GUI";

    #endregion

    #region Unity LifeCycle

    private void Awake()
    {
        // Garante a existência de apenas uma instância ativa do GameManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Impede a destruição do objeto ao trocar de cena
        }
        else
        {
            Destroy(gameObject); // Destrói duplicatas da cena
        }
    }

    private void Start()
    {
        // Inicia o fluxo do jogo carregando as cenas necessárias
        CarregarJogo();
    }

    #endregion

    #region Game Management Logic

    /// <summary>
    /// Reseta as moedas/pontuações e carrega as cenas de jogo e interface.
    /// </summary>
    public void CarregarJogo()
    {
        PlayerOM.ResetScores();
        SceneManager.LoadScene(GameplaySceneName, LoadSceneMode.Single);
        SceneManager.LoadScene(GUISceneName, LoadSceneMode.Additive);
    }

    /// <summary>
    /// Restaura o fluxo de tempo (despausa) e reinicia a partida.
    /// </summary>
    public void ReiniciarPartida()
    {
        Time.timeScale = 1f; // Restaura a velocidade normal do jogo (caso estivesse pausado no painel de vitória)
        CarregarJogo();
    }

    #endregion
}