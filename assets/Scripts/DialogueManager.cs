using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Animator animator;

    Queue<string> sentences;
    public GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        sentences = new Queue<string>();
        HideDialogueBox();
        gameManager = GameManager.Instance;
    }

    public void StartDialogue(Dialogue dialogue)
    {
        if (gameManager.currentPlayer == "Axis") {
            GameObject.Find("Axis").GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        } else {
            GameObject.Find("Myst_Character").GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }
        ShowDialogueBox();
        nameText.SetText(dialogue.name);
        gameManager.gameState = "dialogue";
        sentences.Clear();
        foreach (string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
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
        string next = sentences.Dequeue();
        dialogueText.SetText(next);
    }

    public void EndDialogue()
    {
        HideDialogueBox();
        gameManager.gameState = "play";
        if (gameManager.currentPlayer == "Axis") {
            GameObject.Find("Axis").GetComponent<AxisPlayerController>().moveMode = "normal";
        } else {
            GameObject.Find("Myst_Character").GetComponent<MystPlayerController>().moveMode = "normal";
        }
    }

    public void ShowDialogueBox() {
		GameObject.Find("DialogueBox").transform.localScale = new Vector3(1f, 1f, 1f);
    }

    public void HideDialogueBox() {
		GameObject.Find("DialogueBox").transform.localScale = Vector3.zero;
    }
}
