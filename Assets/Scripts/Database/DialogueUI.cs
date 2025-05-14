using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    public TMP_Text characterNameText;
    public TMP_Text dialogueText;
    public Image characterPortraitImage;

    public void ShowCharacter(Character character)
    {
        if (character == null) return;

        characterNameText.text = character.characterName;

        var sentences = Sentence.LoadByProviderID(character.characterID);
        dialogueText.text = sentences.Count > 0 ? sentences[0].sentence : "[No dialogue]";

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
}
