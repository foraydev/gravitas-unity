using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactible : MonoBehaviour
{
    public GameObject ui;
    public bool reactToPlayer;
    public string interactionType;
    public Dialogue dialogue;

    bool facingRight = true;
    bool canInteract = true;
    GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        ui.SetActive(false);
        gameManager = GameManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (reactToPlayer)
        {
            if (gameManager.currentPlayerTransform.position.x > transform.position.x && !facingRight)
            {
                Flip();
            } else if (gameManager.currentPlayerTransform.position.x < transform.position.x && facingRight)
            {
                Flip();
            }
        }
        if (Vector3.Distance(gameManager.currentPlayerTransform.position, transform.position) < 2) {
            ui.SetActive(gameManager.gameState == "play");
            if (Input.GetButtonDown("Confirm") && canInteract)
            {
                canInteract = false;
                if (gameManager.gameState == "play")
                {
                    if (interactionType == "dialogue") {
                        FindObjectOfType<DialogueManager>().StartDialogue(dialogue);
                    } else if (interactionType == "savepoint") {
                        gameManager.UseSavePoint();
                    } else if (interactionType == "transition") {
                        gameManager.ChangeScene(gameObject.GetComponent<Transition>(), new PlayerState(gameManager.currentPlayerTransform.gameObject.GetComponent<Player>()));
                    }
                } else if (gameManager.gameState == "dialogue")
                {
                    FindObjectOfType<DialogueManager>().DisplayNextSentence();
                }
            } else if (Input.GetButtonUp("Confirm"))
            {
                canInteract = true;
            }
        } else {
            ui.SetActive(false);
        }
    }

    private void Flip()
	{
		facingRight = !facingRight;
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}
}
