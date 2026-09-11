using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI Components")]
    [SerializeField] private GameObject dialogueCanvas;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image leftPortrait;
    [SerializeField] private Image rightPortrait;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void StartDialogue(string speakerName, string dialogue, Sprite leftSprite, Sprite rightSprite)
    {
        dialogueCanvas.SetActive(true);

        speakerNameText.text = speakerName;
        dialogueText.text = dialogue;

        if (leftSprite != null)
        {
            leftPortrait.gameObject.SetActive(true);
            leftPortrait.sprite = leftSprite;
        }
        else
        {
            leftPortrait.gameObject.SetActive(false);
        }

        if (rightSprite != null)
        {
            rightPortrait.gameObject.SetActive(true);
            rightPortrait.sprite = rightSprite;
        }
        else
        {
            rightPortrait.gameObject.SetActive(false);
        }
    }

    public void EndDialogue()
    {
        dialogueCanvas.SetActive(false);
    }
}