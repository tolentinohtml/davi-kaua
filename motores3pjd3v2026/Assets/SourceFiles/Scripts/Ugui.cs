using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Ugui : MonoBehaviour
{
    #region Singleton

    public static Ugui Instance { get; private set; }

    #endregion

    #region Inspector Fields

    [Header("Mostradores de Moedas")]
    public TextMeshProUGUI p1ScoreText;
    public TextMeshProUGUI p2ScoreText;

    [Header("Painel de Vitória")]
    public GameObject winnerPanel;
    public TextMeshProUGUI winnerText;

    [Header("Configurações de Vitória")]
    public int targetScore = 10;

    #endregion

    #region Unity LifeCycle & Subscriptions

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (winnerPanel != null) winnerPanel.SetActive(false);
    }

    private void OnEnable()
    {
        PlayerOM.OnCoinCountChanged += AtualizarTextoMoedas;
        PlayerOM.OnPlayerWon += ExibirVencedor;
    }

    private void OnDisable()
    {
        PlayerOM.OnCoinCountChanged -= AtualizarTextoMoedas;
        PlayerOM.OnPlayerWon -= ExibirVencedor;
    }

    private void Start()
    {
        AtualizarTextoMoedas(1, PlayerOM.GetCoins(1));
        AtualizarTextoMoedas(2, PlayerOM.GetCoins(2));
    }

    #endregion

    #region UI & Gameplay Logic

    private void AtualizarTextoMoedas(int playerID, int totalMoedas)
    {
        if (playerID == 1 && p1ScoreText != null)
            p1ScoreText.text = $"P1: {totalMoedas}";
        else if (playerID == 2 && p2ScoreText != null)
            p2ScoreText.text = $"P2: {totalMoedas}";

        if (targetScore > 0 && totalMoedas >= targetScore)
        {
            PlayerOM.TriggerWin(playerID);
        }
    }

    public void ExibirVencedor(int winnerPlayerID)
    {
        if (winnerPanel != null) winnerPanel.SetActive(true);
        if (winnerText != null) winnerText.text = $"JOGADOR {winnerPlayerID} Ganhou!";
        Time.timeScale = 0f;
    }

    #endregion
}