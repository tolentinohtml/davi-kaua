using UnityEngine;
using TMPro;

public class Ugui : MonoBehaviour
{
    public static Ugui Instance { get; private set; }

    public TextMeshProUGUI p1ScoreText;
    public TextMeshProUGUI p2ScoreText;

    public GameObject winnerPanel;
    public TextMeshProUGUI winnerText;

    public int targetScore = 10;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (winnerPanel != null) winnerPanel.SetActive(false);
    }

    private void OnEnable()
    {
        PlayerOM.OnStarCountChanged += AtualizarTextoEstrelas;
        PlayerOM.OnPlayerWon += ExibirVencedor;
    }

    private void OnDisable()
    {
        PlayerOM.OnStarCountChanged -= AtualizarTextoEstrelas;
        PlayerOM.OnPlayerWon -= ExibirVencedor;
    }

    private void Start()
    {
        PlayerOM.ResetScores();
        AtualizarTextoEstrelas(1, PlayerOM.GetStars(1));
        AtualizarTextoEstrelas(2, PlayerOM.GetStars(2));
    }

    private void AtualizarTextoEstrelas(int playerID, int totalEstrelas)
    {
        if (playerID == 1 && p1ScoreText != null)
        {
            p1ScoreText.text = $"P1 Estrelas: {totalEstrelas}";
        }
        else if (playerID == 2 && p2ScoreText != null)
        {
            p2ScoreText.text = $"P2 Estrelas: {totalEstrelas}";
        }

        if (targetScore > 0 && totalEstrelas >= targetScore)
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
}