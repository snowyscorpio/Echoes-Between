using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Data;

public class DialogueManager : MonoBehaviour
{
    [Header("Dialogue Panel")]
    public GameObject dialoguePanel;

    [Header("NPC Side")]
    public GameObject npcFrame;
    public GameObject npcFrameBackground;
    public Image npcImage;
    public TMP_Text npcNameText;
    public TMP_Text npcSentenceText;

    [Header("Player Side")]
    public GameObject playerFrame;
    public GameObject playerFrameBackground;
    public Image playerImage;
    public TMP_Text playerNameText;
    public TMP_Text playerSentenceText;

    [Header("Skip Hint")]
    public GameObject skipText;

    [Header("End Screen")]
    public GameObject endScreenCanvas;
    public CanvasGroup endScreenCanvasGroup;
    public TMP_Text endTitleText;
    public TMP_Text endSubtitleText;

    private CanvasGroup skipCanvasGroup;
    private Coroutine blinkCoroutine;

    private Queue<DialogueLine> dialogueQueue = new Queue<DialogueLine>();
    private bool isDialogueActive = false;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private DialogueLine currentLine;

    public static bool IsDialogueActive = false;
    private const int playerID = 1;

    void Start()
    {
        dialoguePanel.SetActive(false);
        if (skipText != null) skipText.SetActive(false);

        GameManager.Instance.SetLevelDifficultyFromScene();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetLevelDifficultyFromScene();

            try
            {
                GameManager.Instance.LoadDialogueFlagFromDB();

                if (GameManager.Instance.HasSeenDialogue)
                {
                    Debug.Log("Dialogue already seen – skipping.");
                    return;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("Dialogue flag not loaded – probably running scene directly. " + ex.Message);
            }
        }

        if (GameManager.Instance.HasSeenDialogue)
        {
            Debug.Log("Dialogue already seen ? skipping.");
            return;
        }

        StartCoroutine(DelayedDialogueStart());
    }

    IEnumerator DelayedDialogueStart()
    {
        yield return new WaitForSeconds(1f);
        int currentLevel = GameManager.Instance.LevelDifficulty;
        Debug.Log("DialogueManager: Using LevelDifficulty = " + currentLevel);
        LoadDialogue(currentLevel);
    }

    void Update()
    {
        if (isDialogueActive && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)))
        {
            if (isTyping)
            {
                StopCoroutine(typingCoroutine);
                CompleteTyping(currentLine);
            }
            else
            {
                ShowNextLine();
            }
        }
    }

    void LoadDialogue(int levelDifficulty)
    {
        dialoguePanel.SetActive(true);
        if (skipText != null)
        {
            skipText.SetActive(true);
            skipCanvasGroup = skipText.GetComponent<CanvasGroup>() ?? skipText.AddComponent<CanvasGroup>();
            if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
            blinkCoroutine = StartCoroutine(BlinkSkipText());
        }

        dialogueQueue.Clear();

        CharacterData npc = null;
        CharacterData player = null;
        int npcID = -1;

        using (IDbConnection connection = DatabaseManager.Instance.GetConnection())
        {
            IDbCommand npcCmd = connection.CreateCommand();
            npcCmd.CommandText = @"
                SELECT characterID, characterName, characterAppearance
                FROM Characters
                WHERE characterID != @playerID AND levelDifficulty = @level
                LIMIT 1";

            var playerParam = npcCmd.CreateParameter();
            playerParam.ParameterName = "@playerID";
            playerParam.Value = playerID;
            npcCmd.Parameters.Add(playerParam);

            var levelParam = npcCmd.CreateParameter();
            levelParam.ParameterName = "@level";
            levelParam.Value = levelDifficulty;
            npcCmd.Parameters.Add(levelParam);

            using (IDataReader reader = npcCmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    npc = new CharacterData
                    {
                        ID = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        ImagePath = reader.GetString(2)
                    };
                    npcID = npc.ID;
                    Debug.Log("NPC found: " + npc.Name);
                }
                else
                {
                    Debug.LogWarning("No NPC found.");
                    dialoguePanel.SetActive(false);
                    if (skipText != null) skipText.SetActive(false);
                    return;
                }
            }

            IDbCommand playerCmd = connection.CreateCommand();
            playerCmd.CommandText = "SELECT characterName, characterAppearance FROM Characters WHERE characterID = @id";
            var pid = playerCmd.CreateParameter();
            pid.ParameterName = "@id";
            pid.Value = playerID;
            playerCmd.Parameters.Add(pid);

            using (IDataReader pReader = playerCmd.ExecuteReader())
            {
                if (pReader.Read())
                {
                    player = new CharacterData
                    {
                        ID = playerID,
                        Name = pReader.GetString(0),
                        ImagePath = pReader.GetString(1)
                    };
                }
                else
                {
                    player = new CharacterData
                    {
                        ID = playerID,
                        Name = "Player",
                        ImagePath = "Portraits/Unknown"
                    };
                }
            }

            IDbCommand sentenceCmd = connection.CreateCommand();
            sentenceCmd.CommandText = @"
                SELECT ProviderID, Sentence
                FROM Sentences
                WHERE (ProviderID = @playerID AND ReceiverID = @npcID)
                   OR (ProviderID = @npcID AND ReceiverID = @playerID)
                ORDER BY conversationID ASC";

            var npcParam = sentenceCmd.CreateParameter();
            npcParam.ParameterName = "@npcID";
            npcParam.Value = npcID;
            sentenceCmd.Parameters.Add(npcParam);

            var playerParam2 = sentenceCmd.CreateParameter();
            playerParam2.ParameterName = "@playerID";
            playerParam2.Value = playerID;
            sentenceCmd.Parameters.Add(playerParam2);

            using (IDataReader sReader = sentenceCmd.ExecuteReader())
            {
                while (sReader.Read())
                {
                    dialogueQueue.Enqueue(new DialogueLine
                    {
                        ProviderID = sReader.GetInt32(0),
                        Text = sReader.GetString(1)
                    });
                }
            }

            npcNameText.text = npc.Name;
            npcImage.sprite = Resources.Load<Sprite>(npc.ImagePath) ?? Resources.Load<Sprite>("Portraits/Unknown");

            playerNameText.text = player.Name;
            playerImage.sprite = Resources.Load<Sprite>(player.ImagePath) ?? Resources.Load<Sprite>("Portraits/Unknown");

            DisablePlayer();
            isDialogueActive = true;
            IsDialogueActive = true;
            ShowNextLine();
        }
    }

    void ShowNextLine()
    {
        if (dialogueQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        currentLine = dialogueQueue.Dequeue();
        bool isPlayer = currentLine.ProviderID == playerID;

        playerFrame.SetActive(isPlayer);
        playerFrameBackground.SetActive(isPlayer);
        playerNameText.gameObject.SetActive(isPlayer);
        playerImage.gameObject.SetActive(isPlayer);
        playerSentenceText.text = "";

        npcFrame.SetActive(!isPlayer);
        npcFrameBackground.SetActive(!isPlayer);
        npcNameText.gameObject.SetActive(!isPlayer);
        npcImage.gameObject.SetActive(!isPlayer);
        npcSentenceText.text = "";

        typingCoroutine = StartCoroutine(TypeSentence(currentLine));
    }

    IEnumerator TypeSentence(DialogueLine line)
    {
        isTyping = true;
        TMP_Text target = line.ProviderID == playerID ? playerSentenceText : npcSentenceText;

        for (int i = 0; i <= line.Text.Length; i++)
        {
            target.text = line.Text.Substring(0, i);
            yield return new WaitForSeconds(0.02f);
        }

        isTyping = false;
    }

    void CompleteTyping(DialogueLine line)
    {
        isTyping = false;
        TMP_Text target = line.ProviderID == playerID ? playerSentenceText : npcSentenceText;
        target.text = line.Text;
    }


    void EndDialogue()
    {
        // Mark the dialogue as no longer active
        isDialogueActive = false;
        IsDialogueActive = false;

        // Hide the "Press to skip" text and stop its blinking animation
        if (skipText != null) skipText.SetActive(false);
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);

        // Get current session ID from GameManager
        int sessionId = GameManager.Instance.CurrentSessionID;

        Debug.Log($"[EndDialogue] Trying to update hasSeenDialogue to 1 for sessionID={sessionId}");

        // Open connection to database and update hasSeenDialogue
        using (IDbConnection connection = DatabaseManager.Instance.GetConnection())
        {
            IDbCommand cmd = connection.CreateCommand();

            // Update hasSeenDialogue to 1 for this session
            cmd.CommandText = @"
            UPDATE Levels
            SET hasSeenDialogue = 1
            WHERE sessionID = @sessionID";

            var sessionParam = cmd.CreateParameter();
            sessionParam.ParameterName = "@sessionID";
            sessionParam.Value = sessionId;
            cmd.Parameters.Add(sessionParam);

            // Execute the update
            int rowsAffected = cmd.ExecuteNonQuery();
            Debug.Log($"[EndDialogue] Rows affected by update: {rowsAffected}");

            if (rowsAffected == 0)
            {
                // No row found to update
                Debug.LogWarning("[EndDialogue] Update failed: no matching row found in Levels table.");
            }
            else
            {
                // Successfully updated, set in GameManager memory too
                GameManager.Instance.HasSeenDialogue = true;
                Debug.Log("[EndDialogue] hasSeenDialogue is now set to 1 in GameManager.");
            }
        }

        // If this is the final level, show the ending screen
        if (GameManager.Instance.LevelDifficulty == 4)
        {
            StartCoroutine(ShowEndScreenAndReturn());
        }
        else
        {
            // Otherwise, close dialogue panel and re-enable player control
            dialoguePanel.SetActive(false);
            EnablePlayer();

            // Save the player's fixed spawn position after dialogue
            if (GameManager.Instance != null)
            {
                Vector2 spawnPos = new Vector2(-4.75f, -2.04f);
                SaveManager.SaveLevelAuto(spawnPos);
                Debug.Log("[DialogueManager] Auto-saved fixed spawn after dialogue.");
            }
        }
    }



    IEnumerator ShowEndScreenAndReturn()
    {
        dialoguePanel.SetActive(false);
        endScreenCanvas.SetActive(true);
        endScreenCanvasGroup.alpha = 0f;

        endTitleText.text = "THE END";
        endSubtitleText.text = "Be a good human, enjoy life.";

        foreach (var ps in Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None))
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        float timer = 0f;
        while (timer < 2f)
        {
            timer += Time.deltaTime;
            endScreenCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / 2f);
            yield return null;
        }

        yield return new WaitForSeconds(7f);
        UnityEngine.SceneManagement.SceneManager.LoadScene(2);
    }

    IEnumerator BlinkSkipText()
    {
        while (true)
        {
            skipCanvasGroup.alpha = 1f;
            yield return new WaitForSeconds(0.5f);
            skipCanvasGroup.alpha = 0.3f;
            yield return new WaitForSeconds(0.5f);
        }
    }

    void DisablePlayer()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player?.GetComponent<PlayerMovement>() != null)
            player.GetComponent<PlayerMovement>().enabled = false;
    }

    void EnablePlayer()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player?.GetComponent<PlayerMovement>() != null)
            player.GetComponent<PlayerMovement>().enabled = true;
    }

    class CharacterData
    {
        public int ID;
        public string Name;
        public string ImagePath;
    }

    class DialogueLine
    {
        public int ProviderID;
        public string Text;
    }
}