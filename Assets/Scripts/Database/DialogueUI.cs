
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DialogueUI : MonoBehaviour
{
    public TMP_Text characterNameText;
    public TMP_Text dialogueText;
    public Image characterPortraitImage;

    private List<string> currentSentences;
    private int sentenceIndex = 0;

    public void ShowCharacter(Character character)
    {
        if (character == null) return;

        characterNameText.text = character.characterName;

        var sentences = Sentence.LoadByProviderID(character.characterID);
        currentSentences = new List<string>();
        foreach (var s in sentences)
        {
            currentSentences.Add(s.sentence);
        }

        sentenceIndex = 0;
        ShowSentence();

        var portrait = Resources.Load<Sprite>(character.characterAppearance);
        if (portrait != null)
        {
            characterPortraitImage.sprite = portrait;
            characterPortraitImage.enabled = true;
        }
        else
        {
            Debug.LogWarning($"Image not found at: {character.characterAppearance}");
            characterPortraitImage.enabled = false;
        }
    }

    public void ShowSentence()
    {
        if (currentSentences == null || currentSentences.Count == 0)
        {
            dialogueText.text = "[No dialogue]";
            return;
        }

        if (sentenceIndex >= 0 && sentenceIndex < currentSentences.Count)
        {
            dialogueText.text = currentSentences[sentenceIndex];
        }
    }

    public void ShowNextSentence()
    {
        if (currentSentences == null || currentSentences.Count == 0) return;

        sentenceIndex = (sentenceIndex + 1) % currentSentences.Count;
        ShowSentence();
    }

    public void ShowPreviousSentence()
    {
        if (currentSentences == null || currentSentences.Count == 0) return;

        sentenceIndex = (sentenceIndex - 1 + currentSentences.Count) % currentSentences.Count;
        ShowSentence();
    }

}
