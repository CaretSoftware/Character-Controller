using System;
using Controller;
using UnityEngine;

//  Achievement  Popups  //
// Touch 1000 objects in a session
// Finish the first level in less than 100 steps
// Snek - Sneak past all cats
// Crew - Amass all cats
// Untouchable - Color at most 3 objects
// One down - Eight more to go
// 10 000 steps a day
// Paws - Pause the game
// Don't forget your Duolingo! - Play the game at 23:55
// Track-star - Finish the level under 30 seconds
// TODO You've got a point - Get a high score

public class AchievementManager : MonoBehaviour {
    private const int AchievementNoCatCount = 0;
    private const int AchievementAllCatsCount = 9;
    private const int AchievementTouchCount = 1_000;
    private const int AchievementNoTouchCount = 6;
    private const int AchievementStepCount = 10_000;
    private const int AchievementNoStepCount = 20;
    private const int AchievementOneDeath = 1;
    private const int AchievementNineDeaths = 9;
    private const int AchievementFinishTimeLimit = 25;
    
    private const bool OnLevelFinish = true;
    
    public delegate void AchievementText(string title, string description);
    public static AchievementText achievementTextDelegate;
    
    public delegate void OnLevelFinishDelegate();
    public static OnLevelFinishDelegate onLevelFinishedDelegate;
    
    public delegate void StepDelegate(int num);
    public static StepDelegate stepCountDelegate;
    
    public delegate void TouchCountDelegate(int num);
    public static TouchCountDelegate touchCountDelegate;
    
    public delegate void DeathCountDelegate(int num);
    public static DeathCountDelegate deathCountDelegate;
    
    public delegate void PauseDelegate(int num);
    public static PauseDelegate pauseDelegate;
    
    private static readonly TimeSpan CloseToMidnight = TimeSpan.Parse("23:55");

    private class Stealth : Achievement<Stealth> { public Stealth() : base(
            "Snek", "Sneak Past all Cats", AchievementNoCatCount, 
            i => AIInput.aiCongaLine.Count <= 0, OnLevelFinish) {} }

    private class AllCats : Achievement<AllCats> { public AllCats() : base(
            "Conga Line", "Gather All Cats", AchievementAllCatsCount, 
            i => AIInput.aiCongaLine.Count >= AchievementAllCatsCount, OnLevelFinish) { } }

    private class NoSteps : Achievement<NoSteps> { public NoSteps() : base(
            "No Steps", $"Finish With Less Than {AchievementNoStepCount} Steps", AchievementNoStepCount, 
            predicate: null, manualPop: OnLevelFinish, greaterThan: false) {} }
    
    private class TouchNoObjects : Achievement<TouchNoObjects> { public TouchNoObjects() : base(
            "Untouchable!", $"Touch Only {AchievementNoTouchCount} Objects", AchievementNoTouchCount, predicate: null, 
            manualPop: OnLevelFinish, greaterThan: false) {} }

    private class FinishUnderTimeLimit : Achievement<FinishUnderTimeLimit> { public FinishUnderTimeLimit() : base(
            "Gotta Go Fast!", $"Gone In {AchievementFinishTimeLimit} Seconds", AchievementFinishTimeLimit, 
            predicate: null, manualPop: OnLevelFinish, greaterThan: false) {} }

    private class Pause : Achievement<Pause> { public Pause() : base(
            "Paws", "Paused The Game", 1) {} }

    private class Duolingo : Achievement<Duolingo> { public Duolingo() : base(
            "Don't Forget Duolingo!", "Play The Game after 11.55 pm", 0, 
            i => DateTime.Now.TimeOfDay > CloseToMidnight) {} }

    private class OneDeath : Achievement<OneDeath> { public OneDeath() : base(
            "One Down!", "Eight More To Go!", AchievementOneDeath) {} }

    private class NineDeaths : Achievement<NineDeaths> { public NineDeaths() : base(
            "All Gone!", "Died Nine Times", AchievementNineDeaths) {} }

    private class ThousandTouches : Achievement<ThousandTouches> { public ThousandTouches() : base(
            "Wash Your Paws!", $"Touch {AchievementTouchCount} Objects in a Session", AchievementTouchCount) {} }

    private class TenThousandSteps : Achievement<TenThousandSteps> { public TenThousandSteps() : base(
            "Daily Steps", "10.000 Steps a Day", AchievementStepCount) {} }
    
    // Resets with level reset
    private readonly Achievement<Stealth> stealth = new Stealth();
    private readonly Achievement<AllCats> allCats = new AllCats();
    private readonly Achievement<NoSteps> noSteps = new NoSteps(); 
    private readonly Achievement<TouchNoObjects> touchNoObjects = new TouchNoObjects();
    private readonly Achievement<FinishUnderTimeLimit> finishUnderTimeLimit = new FinishUnderTimeLimit();
    // Persistent through game session
    private static readonly Achievement<Pause> pause = new Pause(); 
    private static readonly Achievement<Duolingo> duolingo = new Duolingo();
    private static readonly Achievement<OneDeath> oneDeath = new OneDeath();
    private static readonly Achievement<NineDeaths> nineDeaths = new NineDeaths();
    private static readonly Achievement<ThousandTouches> thousandTouches = new ThousandTouches(); 
    private static readonly Achievement<TenThousandSteps> tenThousandSteps = new TenThousandSteps();

    public static int NumTotalAchievements { get; } = 11;
    public static int NumAchievedAchievements { get; private set; } = 0;

    private void Awake() {
#if UNITY_EDITOR
        achievementTextDelegate += DebugLog; 
#endif
        onLevelFinishedDelegate += LevelFinished;
        
        stepCountDelegate += noSteps.Count;
        touchCountDelegate += touchNoObjects.Count;
        deathCountDelegate += oneDeath.Count;
        deathCountDelegate += nineDeaths.Count;
        touchCountDelegate += thousandTouches.Count;
        stepCountDelegate += tenThousandSteps.Count;
        pauseDelegate += pause.Count;
        
        InvokeRepeating(nameof(RepeatingEverySecond), 1f, 1f);
    }

    // Debug variables & Methods
    private int debug;
    private void Count(int num) {
        debug += num;
        Debug.Log(debug);
    }

    private void RepeatingEverySecond() {
        finishUnderTimeLimit.Count(1);
        duolingo.CheckPop();
    }

    private void LevelFinished() {
        stealth.CheckPop();
        allCats.CheckPop();
        noSteps.CheckPop();
        touchNoObjects.CheckPop();
        finishUnderTimeLimit.CheckPop();
    }

    private void OnDestroy() {
#if UNITY_EDITOR
        achievementTextDelegate -= DebugLog;
#endif
        onLevelFinishedDelegate -= LevelFinished;
        
        stepCountDelegate -= noSteps.Count;
        touchCountDelegate -= touchNoObjects.Count;
        deathCountDelegate -= oneDeath.Count;
        deathCountDelegate -= nineDeaths.Count;
        touchCountDelegate -= thousandTouches.Count;
        stepCountDelegate -= tenThousandSteps.Count;
        pauseDelegate -= pause.Count;
    }

    private abstract class Achievement<T> {
        private int count;
        private readonly string title;
        private readonly string description;
        private readonly Func<int, bool> predicate;
        private readonly bool manualPop;
        private static bool achieved;       // static to hinder achievement from popping on restarts

        protected Achievement(string title, string description, int limit, Func<int, bool> predicate = null, 
                bool manualPop = false, bool greaterThan = true) {
            
            this.title = title;
            this.description = description;
            this.predicate = predicate ?? (greaterThan 
                ? (Func<int, bool>)(i => i >= limit) 
                : (Func<int, bool>)(i => i <= limit));
            this.manualPop = manualPop;
        }

        public void Count(int num) {
            count += num;
            CheckPop(manualPop);
        }

        public void CheckPop(bool manualPop = false) {
            if (!achieved && !manualPop && predicate(count)) {
                achieved = true;
                ++NumAchievedAchievements;
                Achieve();
            }
        }

        private void Achieve() => achievementTextDelegate?.Invoke(title, description);
    }
    
#if UNITY_EDITOR
    private void OnGUI() {
        if (GUI.Button(new Rect(10, 10, 120, 40), "Test Achievements"))
            onLevelFinishedDelegate?.Invoke();
    }
#endif

    private void DebugLog(string t, string text) => Debug.Log($"\nACHIEVED {t} \n{text}");
}
