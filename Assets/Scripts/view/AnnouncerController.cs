using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class AnnouncerController : MonoBehaviour
{
    [SerializeField] private TMP_Text announcerText;
    [SerializeField] private CanvasGroup content;

    private NonRepeatingShuffleBag<string> regular;
    private NonRepeatingShuffleBag<string> hard;
    private Vector3 originalPosition;

    void Start()
    {
        var listOfKeys = new List<string>();
        listOfKeys.Add("phrase_01");
        listOfKeys.Add("phrase_02");
        //listOfKeys.Add("phrase_03");
        listOfKeys.Add("phrase_04");
        //listOfKeys.Add("phrase_05");
        listOfKeys.Add("phrase_06");
        listOfKeys.Add("phrase_07");
        listOfKeys.Add("phrase_08");
        //listOfKeys.Add("phrase_09");
        listOfKeys.Add("phrase_10");
        listOfKeys.Add("phrase_11");
        listOfKeys.Add("phrase_12");
        listOfKeys.Add("phrase_13");
        listOfKeys.Add("phrase_14");
        listOfKeys.Add("phrase_15");
        listOfKeys.Add("phrase_16");
        listOfKeys.Add("phrase_17");
        //listOfKeys.Add("phrase_18");

        this.originalPosition = this.content.transform.localPosition;
        this.regular = new NonRepeatingShuffleBag<string>(listOfKeys);
        this.hard = new NonRepeatingShuffleBag<string>(new List<string>()
        {
            "phrase_18",
            "phrase_19",
            "phrase_20",
        }); //TODO add harder phrases
        this.content.alpha = 0;
        this.content.gameObject.SetActive(false);
        ViewModel.Instance.OnAnnouncer += OnAnnouncer;
    }
    void OnDestroy()
    {
        ViewModel.Instance.OnAnnouncer -= OnAnnouncer;
    }

    private void OnAnnouncer()
    {
        this.content.alpha = 0;
        this.content.gameObject.SetActive(true);
        this.content.DOFade(1, 0.5f);
        string nextKey;

        var pm = PlayerModel.Instance.playerData;
        if (pm.difficultyIndex >= 5)
        {
            nextKey = this.hard.GetNext();
        }
        else
        {
            nextKey = this.regular.GetNext();
        }
        var nextPhrase = GetTextByKey(nextKey);
        this.announcerText.text = nextPhrase;
        new SoundCmd(nextKey).Run();
        this.content.transform.localPosition = this.originalPosition + Vector3.down * 100;
        this.content.transform.DOLocalMove(this.originalPosition, 0.5f).SetEase(Ease.OutBack);
        GameObjectHelper.DeactivateDelayed(5, this.content.gameObject);
    }

    private string GetTextByKey(string key)
    {
        //ElevenLabs_2026-06-18T10_36_45_Adam - Distinct, Deep and Engaging_pvc_sp100_s50_sb75_v3.mp3
        switch (key)
        {
            case "phrase_01": return "Good!";
            case "phrase_02": return "Good Job!";
            case "phrase_03": return "Nice!";//sound lame
            case "phrase_04": return "Well Done!";
            case "phrase_05": return "Great!";//sound lame
            case "phrase_06": return "Great Job!";
            case "phrase_07": return "Excellent!";
            case "phrase_08": return "Awesome!";
            case "phrase_09": return "Amazing!";//sounds lame
            case "phrase_10": return "Perfect!";
            case "phrase_11": return "Brilliant!";
            case "phrase_12": return "Impressive!";
            case "phrase_13": return "Beautiful!";
            case "phrase_14": return "Nicely done!";
            case "phrase_15": return "Fantastic!";
            case "phrase_16": return "Keep going!";
            case "phrase_17": return "Awesome work!";
            case "phrase_18": return "Unstoppable!";
            case "phrase_19": return "That was close!";
            case "phrase_20": return "That was tricky!";
        }
        Assert.IsTrue(false, "AnnouncerController: GetTextByKey: unknown key " + key);
        return "";
    }

}