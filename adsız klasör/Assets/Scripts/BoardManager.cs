using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;
// ... vs.


public class BoardManager : MonoBehaviour
{
    [Header("Board Settings")]
    public int rows = 10;       // M
    public int columns = 12;    // N
    public int colorCount = 6;  // K
    public float blockSize = 1f;
    public GameObject blockPrefab;

    [Header("Group Size Thresholds")]
    public int A = 4; // 4'ü aşınca iconA
    public int B = 7; // 7'yi aşınca iconB
    public int C = 9; // 9'u aşınca iconC

    [Header("Block Sprites")]
    public List<BlockData> blockDataList; // 6 eleman (her renk için)

    private Block[,] board;

    [Header("Game States")]
    public int movesLeft = 20;
    public int currentScore = 0;
    public static int bestScore = 0;
    public static int maxScore = 0;


    // Canvas üzerindeki referanslar
    public TMPro.TextMeshProUGUI movesText;  // MovesText'e bağlayacağız
    public TMPro.TextMeshProUGUI scoreText;  // ScoreText'e bağlayacağız
    public GameObject gameOverPanel;         // GameOverPanel'e bağlayacağız
    public TMPro.TextMeshProUGUI finalScoreText; // Paneldeki "Score: XX" yazısı
    public TMPro.TextMeshProUGUI maxScoreText;  // MaxScoreText UI 


    private void Start()
    {
        // 1) Oyunu başlatırken kaydedilmiş maxScore'u al:
        maxScore = PlayerPrefs.GetInt("maxScoreKey", 0);

        // 2) UI güncelle:
        UpdateMaxScoreUI();

        // 3) Board'u oluştur:
        GenerateBoard();
    }


    private void GenerateBoard()
    {
        board = new Block[rows, columns];

        // Offset ekleyerek ortalamak istersen:
        // (Bu opsiyoneldir, istersen devre dışı bırak.)
        float xOffset = -(columns * blockSize) / 2f + (blockSize / 2f);
        float yOffset = (rows * blockSize) / 2f - (blockSize / 2f);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                int randomColor = Random.Range(0, colorCount);
                BlockData data = blockDataList[randomColor];

                // Pozisyon
                Vector2 pos = new Vector2(
                    c * blockSize + xOffset,
                    -r * blockSize + yOffset
                );

                // Prefab oluştur
                GameObject newBlock = Instantiate(blockPrefab, pos, Quaternion.identity);
                newBlock.transform.SetParent(transform);

                Block block = newBlock.GetComponent<Block>();
                block.InitBlock(randomColor, data.defaultIcon, r, c);

                board[r, c] = block;
            }
        }
    }

    // Komşuları bul (4 yönde)
    private List<Block> GetNeighbors(int row, int col)
    {
        List<Block> neighbors = new List<Block>();

        if (row > 0) neighbors.Add(board[row - 1, col]);
        if (row < rows - 1) neighbors.Add(board[row + 1, col]);
        if (col > 0) neighbors.Add(board[row, col - 1]);
        if (col < columns - 1) neighbors.Add(board[row, col + 1]);

        return neighbors;
    }

    // Aynı renk komşuları bul (BFS)
    public List<Block> GetConnectedBlocks(Block startBlock)
    {
        List<Block> connected = new List<Block>();
        bool[,] visited = new bool[rows, columns];
        int startColor = startBlock.colorID;

        Queue<Block> queue = new Queue<Block>();
        queue.Enqueue(startBlock);

        while (queue.Count > 0)
        {
            Block current = queue.Dequeue();
            int r = current.row;
            int c = current.column;

            if (!visited[r, c])
            {
                visited[r, c] = true;
                connected.Add(current);

                foreach (Block neighbor in GetNeighbors(r, c))
                {
                    if (!visited[neighbor.row, neighbor.column] &&
                        neighbor.colorID == startColor)
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        return connected;
    }

    // Blokları yok et
    public void RemoveBlocks(List<Block> blocksToRemove)
    {
        int blockCount = blocksToRemove.Count;
        currentScore += blockCount * 10;
        UpdateScoreUI();

        // Max Score kontrol
        if (currentScore > maxScore)
        {
            maxScore = currentScore;
            UpdateMaxScoreUI();

            // ÖNEMLİ: PlayerPrefs ile kaydet
            PlayerPrefs.SetInt("maxScoreKey", maxScore);
            PlayerPrefs.Save(); // Derhal diske yaz
        }

        // 3. Blokları yok et
        foreach (Block b in blocksToRemove)
        {
            board[b.row, b.column] = null;
            Destroy(b.gameObject);
        }

        ApplyGravity();
        FillEmptySpaces();
        UpdateAllBlockIcons();

        if (IsDeadlock())
        {
            ShuffleBoard();
        }
    }

    private void UpdateMaxScoreUI()
    {
        if (maxScoreText != null)
        {
            maxScoreText.text = "Max Score: " + maxScore;
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore;
        }
    }

    private void UpdateMovesUI()
    {
        if (movesText != null)
        {
            movesText.text = "Moves: " + movesLeft;
        }
    }
    private void GameOver()
    {
        // 1. Yüksek skor tutmak istersen
        if (currentScore > maxScore)
        {
            maxScore = currentScore;
        }

        // 2. Paneli aktif et
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // 3. Paneldeki finalScoreText'e skor yaz
        if (finalScoreText != null)
        {
            finalScoreText.text =
                "Game Over!\nYour Score: " + currentScore +
                "\nMax Score: " + maxScore;
        }

        // 4. Gerekirse oyunu durdur
        // Time.timeScale = 0; // Oyun fiziğini vs. dondurur
    }

    // Blokları düşür
    private void ApplyGravity()
    {
        for (int c = 0; c < columns; c++)
        {
            for (int r = rows - 1; r >= 0; r--)
            {
                if (board[r, c] == null)
                {
                    for (int nr = r - 1; nr >= 0; nr--)
                    {
                        if (board[nr, c] != null)
                        {
                            board[r, c] = board[nr, c];
                            board[nr, c] = null;

                            board[r, c].row = r;
                            Vector2 newPos = new Vector2(
                                c * blockSize - (columns * blockSize) / 2f + (blockSize / 2f),
                                -r * blockSize + (rows * blockSize) / 2f - (blockSize / 2f)
                            );
                            board[r, c].transform.position = newPos;

                            break;
                        }
                    }
                }
            }
        }
    }

    // Boş yerleri yeni bloklarla doldur
    private void FillEmptySpaces()
    {
        for (int c = 0; c < columns; c++)
        {
            for (int r = 0; r < rows; r++)
            {
                if (board[r, c] == null)
                {
                    int randomColor = Random.Range(0, colorCount);
                    BlockData data = blockDataList[randomColor];

                    float xOffset = -(columns * blockSize) / 2f + (blockSize / 2f);
                    float yOffset = (rows * blockSize) / 2f - (blockSize / 2f);

                    Vector2 pos = new Vector2(
                        c * blockSize + xOffset,
                        -r * blockSize + yOffset
                    );

                    GameObject newBlock = Instantiate(blockPrefab, pos, Quaternion.identity, transform);
                    Block block = newBlock.GetComponent<Block>();
                    block.InitBlock(randomColor, data.defaultIcon, r, c);

                    board[r, c] = block;
                }
            }
        }
    }

    // Grup ikonu güncelleme
    public void UpdateAllBlockIcons()
    {
        bool[,] visited = new bool[rows, columns];

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                if (board[r, c] != null && !visited[r, c])
                {
                    List<Block> group = GetConnectedBlocks(board[r, c]);
                    int groupSize = group.Count;

                    foreach (Block b in group)
                    {
                        visited[b.row, b.column] = true;
                        BlockData data = blockDataList[b.colorID];

                        Sprite newIcon;

                        // Eşiklere göre
                        if (groupSize < A + 1) // 1..4 => default
                        {
                            newIcon = data.defaultIcon;
                        }
                        else if (groupSize < B + 1) // 5..7 => iconA
                        {
                            newIcon = data.iconA;
                        }
                        else if (groupSize < C + 1) // 8..9 => iconB
                        {
                            newIcon = data.iconB;
                        }
                        else // 10 ve üstü => iconC
                        {
                            newIcon = data.iconC;
                        }

                        b.UpdateIcon(newIcon);
                    }
                }
            }
        }
    }

    // Deadlock kontrol
    public bool IsDeadlock()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                if (board[r, c] != null)
                {
                    var group = GetConnectedBlocks(board[r, c]);
                    if (group.Count >= 2)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    // Shuffle
    public void ShuffleBoard()
    {
        List<Block> allBlocks = new List<Block>();
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                allBlocks.Add(board[r, c]);
            }
        }

        // Karıştır
        for (int i = 0; i < allBlocks.Count; i++)
        {
            int rand = Random.Range(i, allBlocks.Count);
            var temp = allBlocks[i];
            allBlocks[i] = allBlocks[rand];
            allBlocks[rand] = temp;
        }

        // Tekrar yerleştir
        int index = 0;
        float xOffset = -(columns * blockSize) / 2f + (blockSize / 2f);
        float yOffset = (rows * blockSize) / 2f - (blockSize / 2f);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                board[r, c] = allBlocks[index];
                board[r, c].row = r;
                board[r, c].column = c;

                Vector2 newPos = new Vector2(
                    c * blockSize + xOffset,
                    -r * blockSize + yOffset
                );
                board[r, c].transform.position = newPos;

                index++;
            }
        }

        // Hala deadlock ise tekrar dene (sonsuz döngü riskine dikkat)
        if (IsDeadlock())
        {
            Debug.LogWarning("Still deadlocked, trying shuffle again...");
            ShuffleBoard();
        }
    }
    public void RestartGame()
    {
        // Sahneyi tekrar yükle
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Fare tıklamasını yakalayıp patlatma
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                Block clickedBlock = hit.collider.GetComponent<Block>();
                if (clickedBlock != null)
                {
                    List<Block> group = GetConnectedBlocks(clickedBlock);

                    // => HEM TEK BLOK HEM GRUP İÇİN HAMLE DÜŞ
                    movesLeft--;
                    UpdateMovesUI();

                    if (group.Count >= 2)
                    {
                        // Patlama
                        RemoveBlocks(group);
                    }
                    else
                    {
                        // Patlama yok => skor yok
                        // Sadece hamle düştü (yukarıda)
                    }

                    // Hamle sıfırlandı mı
                    if (movesLeft <= 0)
                    {
                        GameOver();
                    }
                }
            }
        }
    }
    public void ExitGame()
    {
        Debug.Log("Game is exiting..."); // Unity Editör'de test için log ekleyelim
        Application.Quit(); // Oyunu kapat
    }
}
