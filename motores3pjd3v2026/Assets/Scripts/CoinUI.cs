using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Ugui : MonoBehaviour
{
    public static Ugui Instance { get; private set; }

    [Header("Mostradores de Moedas")]
    public TextMeshProUGUI p1ScoreText;
    public TextMeshProUGUI p2ScoreText;

    [Header("Painel de Vitória")]
    public GameObject winnerPanel;
    public TextMeshProUGUI winnerText;
  

    
    public int targetScore = 10;

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

    private void AtualizarTextoMoedas(int playerID, int totalMoedas)
    {
        if (playerID == 1 && p1ScoreText != null)
            p1ScoreText.text = $"P1 Moedas: {totalMoedas}";
        else if (playerID == 2 && p2ScoreText != null)
            p2ScoreText.text = $"P2 Moedas: {totalMoedas}";

        if (targetScore > 0 && totalMoedas >= targetScore)
        {
            PlayerOM.TriggerWin(playerID);
        }
    }

    public void ExibirVencedor(int winnerPlayerID)
    {
        if (winnerPanel != null) winnerPanel.SetActive(true);
        if (winnerText != null) winnerText.text = $"JOGADOR {winnerPlayerID} VENCEU!";
        Time.timeScale = 0f;
    }

   
}