using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementDialogue : MonoBehaviour {

    [SerializeField] private Transform dialogueBox;
    [SerializeField] private TextMeshProUGUI dialogueBoxTitle;
    [SerializeField] private TextMeshProUGUI dialogueBoxDescription;
    [SerializeField] private Image radialFillImage;
    private Queue<(string title, string description)> queue = new Queue<(string, string)>();
    private Coroutine coroutine;
    private WaitForSecondsRealtime displayPause = new WaitForSecondsRealtime(2f);
    private static int numPops;
    
    private void Awake() => dialogueBox.localScale = Vector3.zero;

    private void Start() {
        AchievementManager.achievementTextDelegate += PopAchievement;
        numPops = AchievementManager.NumAchievedAchievements;
        radialFillImage.fillAmount = (float)numPops / (float)AchievementManager.NumTotalAchievements;
    }

    private void OnDestroy() => AchievementManager.achievementTextDelegate -= PopAchievement;
    
    private void PopAchievement(string title, string description) {
        queue.Enqueue((title, description));
        coroutine ??= StartCoroutine(Pop());
    }

    private IEnumerator Pop() {
        (string title, string description) = queue.Dequeue();
        dialogueBoxTitle.text = title;
        dialogueBoxDescription.text = description;
        SoundManager.PlaySound(Sound.PoppingCork, 1f, true);
        SoundManager.PlaySound(Sound.Achievement, .1f, true);
        float t = 0f;
        while (t < 1f) {
            dialogueBox.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, Ease.OutElastic(t));
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        dialogueBox.localScale = Vector3.one;
        
        t = 0f;
        float startPercentage = (float)(numPops++) / (float)AchievementManager.NumTotalAchievements;
        float targetPercentage = (float)numPops / (float)AchievementManager.NumTotalAchievements;
        while (t < 1f) {
            radialFillImage.fillAmount = Mathf.LerpUnclamped(startPercentage, targetPercentage, Ease.OutElastic(t));
                t += Time.unscaledDeltaTime * 2f;
            yield return null;
        }
        radialFillImage.fillAmount = targetPercentage; 

        yield return displayPause;

        t = 0f;
        while (t < 1f) {
            dialogueBox.localScale = Vector3.LerpUnclamped(Vector3.one, Vector3.zero, Ease.InBack(t));
            t += Time.unscaledDeltaTime * 2f;
            yield return null;
        }
        dialogueBox.localScale = Vector3.zero;

        coroutine = null;
        if (queue.Count > 0)
            coroutine = StartCoroutine(Pop());
    }
}
