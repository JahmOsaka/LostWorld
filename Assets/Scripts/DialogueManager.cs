using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI Components")]
    [SerializeField] private GameObject dialogueCanvas;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image leftPortrait;
    [SerializeField] private Image rightPortrait;

    [Header("Name Tags")]
    [SerializeField] private GameObject leftNamePanel;
    [SerializeField] private TextMeshProUGUI leftNameText;
    [SerializeField] private GameObject rightNamePanel;
    [SerializeField] private TextMeshProUGUI rightNameText;

    private Queue<DialogueLine> sentences;
    public bool isOpen = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        sentences = new Queue<DialogueLine>();
    }

    public void StartDialogue(DialogueLine[] dialogueLines, Sprite leftSprite, Sprite rightSprite)
    {
        isOpen = true;
        dialogueCanvas.SetActive(true);

        leftNamePanel.SetActive(false);
        rightNamePanel.SetActive(false);

        if (leftSprite != null) { leftPortrait.gameObject.SetActive(true); leftPortrait.sprite = leftSprite; }
        else leftPortrait.gameObject.SetActive(false);

        if (rightSprite != null) { rightPortrait.gameObject.SetActive(true); rightPortrait.sprite = rightSprite; }
        else rightPortrait.gameObject.SetActive(false);

        sentences.Clear();
        foreach (DialogueLine line in dialogueLines)
        {
            sentences.Enqueue(line);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine currentLine = sentences.Dequeue();
        dialogueText.text = currentLine.sentence;

        if (currentLine.isPlayerSpeaking)
        {
            leftNamePanel.SetActive(true);
            leftNameText.text = currentLine.speakerName;
            rightNamePanel.SetActive(false);

            leftPortrait.color = Color.white;
            rightPortrait.color = Color.gray;
        }
        else
        {
            rightNamePanel.SetActive(true);
            rightNameText.text = currentLine.speakerName;
            leftNamePanel.SetActive(false);

            leftPortrait.color = Color.gray;
            rightPortrait.color = Color.white;
        }
    }

    public void EndDialogue()
    {
        isOpen = false;
        dialogueCanvas.SetActive(false);
    }
}