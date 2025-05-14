using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public DialogueUI dialogueUI;
    private List<Character> currentCharacters;
    private int currentIndex = 0;

    void Start()
    {
        LoadCharactersForCurrentLevel();
        ShowCurrent();
    }

    public void LoadCharactersForCurrentLevel()
    {
        currentCharacters = new List<Character>();

        if (GameManager.Instance == null || GameManager.Instance.currentLevel == null)
        {
            Debug.LogWarning("No current level loaded.");
            return;
        }

        int currentLevelID = GameManager.Instance.currentLevel.levelID;
        var all = Character.LoadAllCharacters();

        foreach (var character in all)
        {
            if (character.levelID == currentLevelID)
                currentCharacters.Add(character);
        }

        currentIndex = 0;
    }

    public void ShowCurrent()
    {
        if (currentCharacters == null || currentCharacters.Count == 0) return;
        if (currentIndex < 0 || currentIndex >= currentCharacters.Count) return;

        dialogueUI.ShowCharacter(currentCharacters[currentIndex]);
    }

    public void ShowNext()
    {
        if (currentCharacters == null || currentCharacters.Count == 0) return;
        currentIndex = (currentIndex + 1) % currentCharacters.Count;
        ShowCurrent();
    }

    public void ShowPrevious()
    {
        if (currentCharacters == null || currentCharacters.Count == 0) return;
        currentIndex = (currentIndex - 1 + currentCharacters.Count) % currentCharacters.Count;
        ShowCurrent();
    }
}
