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
        StartCoroutine(DelayedDialogueStart());
    }

    IEnumerator DelayedDialogueStart()
    {
        yield return new WaitForSeconds(1f);


        GameManager.Instance.SetLevelDifficultyFromScene();

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
            skipCanvasGroup = skipText.GetComponent<CanvasGroup>();
            if (skipCanvasGroup == null)
                skipCanvasGroup = skipText.AddComponent<CanvasGroup>();

            if (blinkCoroutine != null)
                StopCoroutine(blinkCoroutine);
            blinkCoroutine = StartCoroutine(BlinkSkipText());
        }

        dialogueQueue.Clear();

        CharacterData npc = null;
        CharacterData player = null;
        int npcID = -1;

        using (IDbConnection connection = DatabaseManager.Instance.GetConnection())
        {
            // Load NPC
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
                        ID = reader.GetInt32(reader.GetOrdinal("characterID")),
                        Name = reader.GetString(reader.GetOrdinal("characterName")),
                        ImagePath = reader.GetString(reader.GetOrdinal("characterAppearance"))
                    };
                    npcID = npc.ID;
                    Debug.Log("NPC found: ID = " + npcID + ", Name = " + npc.Name);
                }
                else
                {
                    Debug.LogWarning("No NPC found for level " + levelDifficulty);
                    dialoguePanel.SetActive(false);
                    if (skipText != null) skipText.SetActive(false);
                    return;
                }
            }

            // Load Player (characterID = 1, always)
            IDbCommand playerCmd = connection.CreateCommand();
            playerCmd.CommandText = @"
            SELECT characterName, characterAppearance
            FROM Characters
            WHERE characterID = @id";

            var pid = playerCmd.CreateParameter();
            pid.ParameterName = "@id";
            pid.Value = playerID;
            playerCmd.Parameters.Add(pid);

            try
            {
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
                        Debug.LogWarning("Player not found in Characters table. Using default fallback.");
                        player = new CharacterData
                        {
                            ID = playerID,
                            Name = "Player",
                            ImagePath = "Portraits/Unknown"
                        };
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Failed to load Player character. Error: " + ex.Message);
                player = new CharacterData
                {
                    ID = playerID,
                    Name = "Player",
                    ImagePath = "Portraits/Unknown"
                };
            }

            // Load sentences
            IDbCommand sentenceCmd = connection.CreateCommand();
            sentenceCmd.CommandText = @"
            SELECT ProviderID, Sentence
            FROM Sentences
            WHERE 
                (ProviderID = @playerID AND ReceiverID = @npcID)
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
                    int provider = sReader.GetInt32(0);
                    string text = sReader.GetString(1);

                    dialogueQueue.Enqueue(new DialogueLine
                    {
                        ProviderID = provider,
                        Text = text
                    });
                }
            }

            // Set NPC UI
            npcNameText.text = npc.Name;
            var npcSprite = Resources.Load<Sprite>(npc.ImagePath);
            if (npcSprite == null)
                Debug.LogWarning("Could not load NPC image: " + npc.ImagePath);
            npcImage.sprite = npcSprite != null ? npcSprite : Resources.Load<Sprite>("Portraits/Unknown");

            // Set Player UI
            playerNameText.text = player.Name;
            var playerSprite = Resources.Load<Sprite>(player.ImagePath);
            if (playerSprite == null)
                Debug.LogWarning("Could not load Player image: " + player.ImagePath);
            playerImage.sprite = playerSprite != null ? playerSprite : Resources.Load<Sprite>("Portraits/Unknown");

            // Start dialogue
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
        bool isPlayerSpeaking = currentLine.ProviderID == playerID;


        playerFrame.SetActive(isPlayerSpeaking);
        playerFrameBackground.SetActive(isPlayerSpeaking);
        playerNameText.gameObject.SetActive(isPlayerSpeaking);
        playerImage.gameObject.SetActive(isPlayerSpeaking);
        playerSentenceText.text = isPlayerSpeaking ? "" : "";

        npcFrame.SetActive(!isPlayerSpeaking);
        npcFrameBackground.SetActive(!isPlayerSpeaking);
        npcNameText.gameObject.SetActive(!isPlayerSpeaking);
        npcImage.gameObject.SetActive(!isPlayerSpeaking);
        npcSentenceText.text = !isPlayerSpeaking ? "" : "";

        typingCoroutine = StartCoroutine(TypeSentence(currentLine));
    }


    IEnumerator TypeSentence(DialogueLine line)
    {
        isTyping = true;
        string sentence = line.Text;
        TMP_Text targetText = line.ProviderID == playerID ? playerSentenceText : npcSentenceText;

        for (int i = 0; i <= sentence.Length; i++)
        {
            targetText.text = sentence.Substring(0, i);
            yield return new WaitForSeconds(0.02f);
        }

        isTyping = false;
    }

    void CompleteTyping(DialogueLine line)
    {
        isTyping = false;
        TMP_Text targetText = line.ProviderID == playerID ? playerSentenceText : npcSentenceText;
        targetText.text = line.Text;
    }

    void EndDialogue()
    {
        isDialogueActive = false;
        IsDialogueActive = false;

        if (skipText != null) skipText.SetActive(false);
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        if (GameManager.Instance.LevelDifficulty == 4)
        {
            StartCoroutine(ShowEndScreenAndReturn());
        }
        else
        {
            dialoguePanel.SetActive(false);
            EnablePlayer();
        }
    }

    IEnumerator ShowEndScreenAndReturn()
    {
        dialoguePanel.SetActive(false);

        if (endScreenCanvas != null)
        {
            endScreenCanvas.SetActive(true);
            endScreenCanvasGroup.alpha = 0f;

            endTitleText.text = "THE END";
            endSubtitleText.text = "Be a good human, enjoy life.";

            ParticleSystem[] allParticles = Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);
            foreach (var ps in allParticles)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            float duration = 2f;
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                endScreenCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / duration);
                yield return null;
            }

            yield return new WaitForSeconds(7f);

            UnityEngine.SceneManagement.SceneManager.LoadScene(2);
        }
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
        if (player != null)
        {
            var move = player.GetComponent<PlayerMovement>();
            if (move != null)
                move.enabled = false;
        }
    }

    void EnablePlayer()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var move = player.GetComponent<PlayerMovement>();
            if (move != null)
                move.enabled = true;
        }
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
