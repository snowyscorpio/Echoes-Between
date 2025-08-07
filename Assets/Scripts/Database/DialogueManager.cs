using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Data;

/// <summary>
/// Manages in-game dialogues: displaying character name, sentence text, and portrait with a typewriter effect.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    [Header("Dialogue Panel")]
    public GameObject dialoguePanel;                  // Root UI panel for dialogue

    [Header("NPC Side")]
    public GameObject npcFrame;                       // UI frame for NPC portrait and text
    public GameObject npcFrameBackground;             // Background behind NPC frame
    public Image npcImage;                            // NPC portrait image component
    public TMP_Text npcNameText;                      // UI text for NPC name
    public TMP_Text npcSentenceText;                  // UI text for NPC dialogue

    [Header("Player Side")]
    public GameObject playerFrame;                    // UI frame for player portrait and text
    public GameObject playerFrameBackground;          // Background behind player frame
    public Image playerImage;                         // Player portrait image component
    public TMP_Text playerNameText;                   // UI text for player name
    public TMP_Text playerSentenceText;               // UI text for player dialogue

    [Header("Skip Hint")]
    public GameObject skipText;                       // UI element prompting user to skip or advance

    [Header("End Screen")]
    public GameObject endScreenCanvas;                // UI canvas for end-of-dialogue screen
    public CanvasGroup endScreenCanvasGroup;          // CanvasGroup for fading end screen
    public TMP_Text endTitleText;                     // Title text on end screen
    public TMP_Text endSubtitleText;                  // Subtitle text on end screen

    private CanvasGroup skipCanvasGroup;              // CanvasGroup used to blink skipText
    private Coroutine blinkCoroutine;                 // Reference to blink coroutine

    private Queue<DialogueLine> dialogueQueue = new Queue<DialogueLine>(); // Queue of dialogue lines to display
    private bool isDialogueActive = false;            // Flag indicating dialogue is in progress
    private bool isTyping = false;                    // Flag indicating typewriter effect is active
    private Coroutine typingCoroutine;                // Reference to typing coroutine
    private DialogueLine currentLine;                 // Currently displayed dialogue line

    public static bool IsDialogueActive = false;      // Static flag for external checks
    private const int playerID = 1;                   // Constant ID representing the player

    /// <summary>
    /// Initialization entry point. Sets up dialogue visibility, loads seen flag, and schedules start if needed.
    /// </summary>
    void Start()
    {
        dialoguePanel.SetActive(false);                // Hide dialogue panel at start
        if (skipText != null) skipText.SetActive(false);// Hide skip text initially

        // Ensure GameManager has correct difficulty setting from scene name
        GameManager.Instance.SetLevelDifficultyFromScene();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetLevelDifficultyFromScene();

            try
            {
                GameManager.Instance.LoadDialogueFlagFromDB();     // Load flag indicating if dialogue was seen

                if (GameManager.Instance.HasSeenDialogue)         // Skip if already seen
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

        if (GameManager.Instance.HasSeenDialogue)                   // Double-check skip condition
        {
            Debug.Log("Dialogue already seen ? skipping.");
            return;
        }

        StartCoroutine(DelayedDialogueStart());                     // Begin dialogue after short delay
    }

    /// <summary>
    /// Waits a moment, then triggers loading dialogue for the current level difficulty.
    /// </summary>
    IEnumerator DelayedDialogueStart()
    {
        yield return new WaitForSeconds(1f);                        // Wait before starting dialogue
        int currentLevel = GameManager.Instance.LevelDifficulty;    // Get difficulty level
        Debug.Log("DialogueManager: Using LevelDifficulty = " + currentLevel);
        LoadDialogue(currentLevel);                                 // Load lines for this level
    }

    /// <summary>
    /// Handles user input during dialogue; advances lines or completes typing.
    /// </summary>
    void Update()
    {
        // Advance or skip typing on Enter or Space when dialogue is active
        if (isDialogueActive && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)))
        {
            if (isTyping)
            {
                StopCoroutine(typingCoroutine);                     // Stop typewriter effect
                CompleteTyping(currentLine);                        // Instantly fill full text
            }
            else
            {
                ShowNextLine();                                     // Show next queued line
            }
        }
    }

    /// <summary>
    /// Loads the dialogue data for the given level difficulty: characters, sentences, and prepares UI.
    /// </summary>
    void LoadDialogue(int levelDifficulty)
    {
        dialoguePanel.SetActive(true);                              // Show dialogue UI

        if (skipText != null)
        {
            skipText.SetActive(true);                               // Show skip hint
            // Get or add CanvasGroup for blinking effect
            skipCanvasGroup = skipText.GetComponent<CanvasGroup>() ?? skipText.AddComponent<CanvasGroup>();
            if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
            blinkCoroutine = StartCoroutine(BlinkSkipText());       // Start blinking skip hint
        }

        dialogueQueue.Clear();                                      // Clear previous queue

        CharacterData npc = null;
        CharacterData player = null;
        int npcID = -1;

        // Connect to DB and load NPC and player data
        using (IDbConnection connection = DatabaseManager.Instance.GetConnection())
        {
            // Query NPC for this level
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
                    // Construct NPC data from DB row
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
                    Debug.LogWarning("No NPC found.");               // No NPC for this level
                    dialoguePanel.SetActive(false);
                    if (skipText != null) skipText.SetActive(false);
                    return;
                }
            }

            // Query player data
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
                    // Construct player data from DB
                    player = new CharacterData
                    {
                        ID = playerID,
                        Name = pReader.GetString(0),
                        ImagePath = pReader.GetString(1)
                    };
                }
                else
                {
                    // Fallback defaults
                    player = new CharacterData
                    {
                        ID = playerID,
                        Name = "Player",
                        ImagePath = "Portraits/Unknown"
                    };
                }
            }

            // Query dialogue sentences
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
                    // Enqueue each line in order
                    dialogueQueue.Enqueue(new DialogueLine
                    {
                        ProviderID = sReader.GetInt32(0),
                        Text = sReader.GetString(1)
                    });
                }
            }

            // Update UI portraits and names
            npcNameText.text = npc.Name;
            npcImage.sprite = Resources.Load<Sprite>(npc.ImagePath) ?? Resources.Load<Sprite>("Portraits/Unknown");
            playerNameText.text = player.Name;
            playerImage.sprite = Resources.Load<Sprite>(player.ImagePath) ?? Resources.Load<Sprite>("Portraits/Unknown");

            DisablePlayer();                                       // Disable player movement during dialogue
            isDialogueActive = true;
            IsDialogueActive = true;
            ShowNextLine();                                        // Begin displaying lines
        }
    }

    /// <summary>
    /// Dequeues the next line and updates speaker UI, then starts typing it out.
    /// </summary>
    void ShowNextLine()
    {
        if (dialogueQueue.Count == 0)
        {
            EndDialogue();                                         // End when queue empty
            return;
        }

        currentLine = dialogueQueue.Dequeue();                    // Dequeue next line
        bool isPlayer = currentLine.ProviderID == playerID;       // Determine speaker

        // Set active UI based on speaker (player vs NPC)
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

        typingCoroutine = StartCoroutine(TypeSentence(currentLine)); // Start typewriter effect
    }

    /// <summary>
    /// Coroutine that types the given dialogue line character by character.
    /// </summary>
    IEnumerator TypeSentence(DialogueLine line)
    {
        isTyping = true;
        TMP_Text target = line.ProviderID == playerID ? playerSentenceText : npcSentenceText;

        // Reveal text one character at a time
        for (int i = 0; i <= line.Text.Length; i++)
        {
            target.text = line.Text.Substring(0, i);
            yield return new WaitForSeconds(0.02f);
        }

        isTyping = false;                                         // Typing complete
    }

    /// <summary>
    /// Immediately completes the typing animation, showing the full line at once.
    /// </summary>
    void CompleteTyping(DialogueLine line)
    {
        isTyping = false;
        TMP_Text target = line.ProviderID == playerID ? playerSentenceText : npcSentenceText;
        target.text = line.Text;                                  // Instantly display full text
    }

    /// <summary>
    /// Finalizes the dialogue: saves session, updates flags in database, hides UI, and restores player control.
    /// </summary>
    void EndDialogue()
    {
        isDialogueActive = false; // Mark dialogue as inactive

        // Hide skip hint and stop blinking animation
        if (skipText != null) skipText.SetActive(false);
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);

        int sessionId = GameManager.Instance.CurrentSessionID;
        Debug.Log($"[EndDialogue] Preparing to update hasSeenDialogue for sessionID={sessionId}");

        // Auto-save level before trying to update the Levels table (ensures sessionID exists)
        if (GameManager.Instance != null)
        {
            Vector2 spawnPos = new Vector2(-4.75f, -2.04f); // Default fixed spawn position
            SaveManager.SaveLevelAuto(spawnPos);            // Insert session row into Levels table
            Debug.Log("[DialogueManager] Auto-saved fixed spawn before updating hasSeenDialogue.");
        }

        // Now attempt to update the hasSeenDialogue flag in the Levels table
        using (IDbConnection connection = DatabaseManager.Instance.GetConnection())
        {
            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = @"
            UPDATE Levels
            SET hasSeenDialogue = 1
            WHERE sessionID = @sessionID";

            var sessionParam = cmd.CreateParameter();
            sessionParam.ParameterName = "@sessionID";
            sessionParam.Value = sessionId;
            cmd.Parameters.Add(sessionParam);

            int rowsAffected = cmd.ExecuteNonQuery();
            Debug.Log($"[EndDialogue] Rows affected by update: {rowsAffected}");

            if (rowsAffected == 0)
            {
                Debug.LogWarning("[EndDialogue] Update failed: no matching row found in Levels table.");
            }
            else
            {
                GameManager.Instance.HasSeenDialogue = true; // Update in-memory flag
                Debug.Log("[EndDialogue] hasSeenDialogue is now set to 1 in GameManager.");
            }
        }

        // If this is the final level, show the end screen and return to main menu
        if (GameManager.Instance.LevelDifficulty == 4)
        {
            StartCoroutine(ShowEndScreenAndReturn());
        }
        else
        {
            // Hide dialogue UI and re-enable player controls
            dialoguePanel.SetActive(false);
            IsDialogueActive = false;
            EnablePlayer();
        }
    }


    /// <summary>
    /// Displays the ending screen when dialogue concludes on the final level, then returns to session list.
    /// </summary>
    IEnumerator ShowEndScreenAndReturn()
    {
        dialoguePanel.SetActive(false);                           // Hide dialogue UI
        endScreenCanvas.SetActive(true);                          // Show end screen
        endScreenCanvasGroup.alpha = 0f;

        endTitleText.text = "THE END";
        endSubtitleText.text = "Be a good human, enjoy life.";

        // Stop all particle effects
        foreach (var ps in Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None))
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // Fade in end screen over 2 seconds
        float timer = 0f;
        while (timer < 2f)
        {
            timer += Time.deltaTime;
            endScreenCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / 2f);
            yield return null;
        }

        yield return new WaitForSeconds(7f);                      // Keep end screen visible
        UnityEngine.SceneManagement.SceneManager.LoadScene(2);    // Return to session list
    }

    /// <summary>
    /// Continuously blinks the skip text hint to draw attention while dialogue is active.
    /// </summary>
    IEnumerator BlinkSkipText()
    {
        // Continuously blink skip hint
        while (true)
        {
            skipCanvasGroup.alpha = 1f;
            yield return new WaitForSeconds(0.5f);
            skipCanvasGroup.alpha = 0.3f;
            yield return new WaitForSeconds(0.5f);
        }
    }

    /// <summary>
    /// Disables player movement by disabling its PlayerMovement component.
    /// </summary>
    void DisablePlayer()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player?.GetComponent<PlayerMovement>() != null)
            player.GetComponent<PlayerMovement>().enabled = false; // Disable player controls
    }

    /// <summary>
    /// Re-enables player movement after dialogue ends.
    /// </summary>
    void EnablePlayer()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player?.GetComponent<PlayerMovement>() != null)
        {
            player.GetComponent<PlayerMovement>().enabled = true;
            Debug.Log("PlayerMovement enabled after dialogue.");
        }
        else
        {
            Debug.LogWarning("Player or PlayerMovement not found.");
        }
    }


    // Internal data class representing a character definition
    class CharacterData
    {
        public int ID;
        public string Name;
        public string ImagePath;
    }

    // Internal data class representing a single line of dialogue
    class DialogueLine
    {
        public int ProviderID;
        public string Text;
    }
}