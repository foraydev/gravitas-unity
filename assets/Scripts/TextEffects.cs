using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TextEffects : MonoBehaviour
{
    public GameManager gameManager;

    //Variables for Text Change
    TextMeshProUGUI textMesh;
    Dictionary<string, string> styles = new Dictionary<string, string>();
    string textLastFrame = "";

    // Start is called before the first frame update
    void Start()
    {
        styles.Add("Chaos Clan", "purple");
        styles.Add("E.R.A.", "green");
        styles.Add("Mechanized Brotherhood", "red");
        styles.Add("wavy text", "wavy");
        styles.Add("shaky text", "shaky");
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    void Update() {
        if (textLastFrame != textMesh.text) {
            FormatNewText();
        }
        textLastFrame = textMesh.text;
        textMesh.ForceMeshUpdate();
        // Loops each link tag
        foreach (TMP_LinkInfo link in textMesh.textInfo.linkInfo) {
            if (link.GetLinkID() == "purple") {
                MakeTextColor(link, new Color32(115, 3, 252, 255));
            }
            if (link.GetLinkID() == "green") {
                MakeTextColor(link, new Color32(65, 173, 73, 255));
            }
            if (link.GetLinkID() == "red") {
                MakeTextColor(link, new Color32(255, 0, 0, 255));
            }
            if (link.GetLinkID() == "wavy") {
                MakeTextWavy(link);
            }
            if (link.GetLinkID() == "shaky") {
                MakeTextShaky(link);
            }
        }
        textMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.All); // IMPORTANT! applies all vertex and color changes.
    }

    void FormatNewText() {
        // reset the links data so that old links aren't applied to new text.
        Array.Clear(textMesh.textInfo.linkInfo, 0, textMesh.textInfo.linkInfo.Length);
        string tempText = textMesh.text;
        tempText = tempText.Replace("{player}", GameManager.Instance.currentPlayer);
        tempText = tempText.Replace("Chaos Clan", "<sprite=0>Chaos Clan");
        tempText = tempText.Replace("Dark Piranha", "<sprite=1>Dark Piranha");
        tempText = tempText.Replace("E.R.A.", "<sprite=4>E.R.A.");
        tempText = tempText.Replace("Mechanized Brotherhood", "<sprite=5>Mechanized Brotherhood");
        foreach (string word in styles.Keys) {
            tempText = tempText.Replace(word, "<link=\""+styles[word]+"\">"+word+"</link>");
        }
        textMesh.SetText(tempText);
    }

    void MakeTextColor(TMP_LinkInfo link, Color32 color) {
        for (int i = link.linkTextfirstCharacterIndex; i < link.linkTextfirstCharacterIndex + link.linkTextLength; i++) {
            TMP_CharacterInfo charInfo = textMesh.textInfo.characterInfo[i]; // Gets info on the current character
            int materialIndex = charInfo.materialReferenceIndex; // Gets the index of the current character material
            Color32[] newColors = textMesh.textInfo.meshInfo[materialIndex].colors32;
            Vector3[] newVertices = textMesh.textInfo.meshInfo[materialIndex].vertices;
            // Loop all vertexes of the current characters
            for (int j = 0; j < 4; j++)
            {
                if (charInfo.character == ' ') continue; // Skips spaces
                int vertexIndex = charInfo.vertexIndex + j;
                Color32 newCol = color;
                newColors[vertexIndex] = newCol;
            }
        }
    }

    void MakeTextWavy(TMP_LinkInfo link) {
        for (int i = link.linkTextfirstCharacterIndex; i < link.linkTextfirstCharacterIndex + link.linkTextLength; i++) {
            TMP_CharacterInfo charInfo = textMesh.textInfo.characterInfo[i]; // Gets info on the current character
            int materialIndex = charInfo.materialReferenceIndex; // Gets the index of the current character material
            Color32[] newColors = textMesh.textInfo.meshInfo[materialIndex].colors32;
            Vector3[] newVertices = textMesh.textInfo.meshInfo[materialIndex].vertices;
            // Loop all vertexes of the current characters
            for (int j = 0; j < 4; j++)
            {
                if (charInfo.character == ' ') continue; // Skips spaces
                int vertexIndex = charInfo.vertexIndex + j;
                Vector3 offset = new Vector2(0.0f, Mathf.Cos((Time.realtimeSinceStartup * 3.0f) + (vertexIndex * 0.1f))) * 5f;
                newVertices[vertexIndex] += offset;
            }
        }
    }
    void MakeTextShaky(TMP_LinkInfo link) {
        float shakeIntensity = 1.5f;
        Vector3 offset = new Vector2(UnityEngine.Random.Range(-shakeIntensity, shakeIntensity), UnityEngine.Random.Range(-shakeIntensity, shakeIntensity));
        for (int i = link.linkTextfirstCharacterIndex; i < link.linkTextfirstCharacterIndex + link.linkTextLength; i++) {
            TMP_CharacterInfo charInfo = textMesh.textInfo.characterInfo[i]; // Gets info on the current character
            int materialIndex = charInfo.materialReferenceIndex; // Gets the index of the current character material
            Color32[] newColors = textMesh.textInfo.meshInfo[materialIndex].colors32;
            Vector3[] newVertices = textMesh.textInfo.meshInfo[materialIndex].vertices;
            // Loop all vertexes of the current characters
            for (int j = 0; j < 4; j++)
            {
                if (charInfo.character == ' ') continue; // Skips spaces
                int vertexIndex = charInfo.vertexIndex + j;
                newVertices[vertexIndex] += offset;
            }
        }
    }
}