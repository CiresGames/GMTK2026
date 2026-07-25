using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SheetMusicDisplay : MonoBehaviour
{
    [System.Serializable]
    public struct GuitarNoteVisual
    {
        public StartABandGame.GUITAR_NOTE note;
        public Sprite noteSprite;
        public float staffYPosition;
    }

    [System.Serializable]
    public struct DrumNoteVisual
    {
        public StartABandGame.DRUM_NOTE note;
        public Sprite noteSprite;
        public float staffYPosition;
    }

    [Header("Staff Setup")]
    [SerializeField] RectTransform staffContainer;
    [SerializeField] GameObject noteIconPrefab;
    [SerializeField] float noteSpacingX = 60f;
    [SerializeField] List<GuitarNoteVisual> noteVisuals;
    [SerializeField] List<DrumNoteVisual> drumNoteVisuals;

    [Header("Win Condition")]
    [SerializeField] float startXOffset = 0f; // left padding before first note

    [Header("Completion Feedback")]
    [SerializeField] Color completionGoldColor = new Color(1f, 0.84f, 0f);
    [SerializeField] float delayBetweenNoteFeedbacks = 0f; // stagger, in seconds, between each note's feedback

    private readonly List<GameObject> placedNotes = new List<GameObject>();
    private Dictionary<StartABandGame.GUITAR_NOTE, GuitarNoteVisual> noteVisualLookup;
    private Dictionary<StartABandGame.DRUM_NOTE, DrumNoteVisual> drumNoteVisualLookup;

    public int notesRequiredToWin; // computed, not authored
    public System.Action OnSheetFilled;

    // Controls whether the completion feedback keeps looping.
    // Set to false (from another script, a UI button, etc.) to stop it after the current pass.
    public bool isFeedbackLooping = false;

    private Coroutine loopingFeedbackCoroutine;

    private void Awake()
    {
        noteVisualLookup = new Dictionary<StartABandGame.GUITAR_NOTE, GuitarNoteVisual>();
        foreach (var nv in noteVisuals)
            noteVisualLookup[nv.note] = nv;

        drumNoteVisualLookup = new Dictionary<StartABandGame.DRUM_NOTE, DrumNoteVisual>();
        if (drumNoteVisuals != null)
        {
            foreach (var nv in drumNoteVisuals)
                drumNoteVisualLookup[nv.note] = nv;
        }
    }

    public void AddNote(StartABandGame.GUITAR_NOTE note)
    {
        if (placedNotes.Count >= notesRequiredToWin) return;

        if (!noteVisualLookup.TryGetValue(note, out GuitarNoteVisual visual))
        {
            Debug.LogWarning($"No visual configured for note {note}");
            return;
        }

        PlaceIcon(visual.noteSprite, visual.staffYPosition);
    }

    public void AddNote(StartABandGame.DRUM_NOTE note)
    {
        if (placedNotes.Count >= notesRequiredToWin) return;

        if (!drumNoteVisualLookup.TryGetValue(note, out DrumNoteVisual visual))
        {
            Debug.LogWarning($"No visual configured for note {note}");
            return;
        }

        PlaceIcon(visual.noteSprite, visual.staffYPosition);
    }

    private void PlaceIcon(Sprite sprite, float staffYPosition)
    {
        GameObject icon = Instantiate(noteIconPrefab, staffContainer);
        RectTransform rt = icon.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(startXOffset + placedNotes.Count * noteSpacingX, staffYPosition);

        Image image = icon.GetComponent<Image>();
        if (image != null && sprite != null)
            image.sprite = sprite;

        placedNotes.Add(icon);

        if (placedNotes.Count >= notesRequiredToWin)
            OnSheetFilled?.Invoke();
    }

    public void ResetSheet()
    {
        foreach (var n in placedNotes)
            Destroy(n);
        placedNotes.Clear();
    }

    public void HandleSheetFilled()
    {

        StartFeedbackLoop();
    }

    // Starts (or restarts) the looping completion feedback.
    public void StartFeedbackLoop()
    {
        if (loopingFeedbackCoroutine != null)
            StopCoroutine(loopingFeedbackCoroutine);

        isFeedbackLooping = true;
        MMF_Player rootPlayer = GetComponent<MMF_Player>();
        if (rootPlayer != null)
            rootPlayer.PlayFeedbacks();
        loopingFeedbackCoroutine = StartCoroutine(LoopCompleteSheetFeedback());
    }

    // Call this to stop the loop. It finishes the current note it's on, then exits
    // rather than cutting off mid-note.
    public void StopFeedbackLoop()
    {
        isFeedbackLooping = false;
    }

    private IEnumerator LoopCompleteSheetFeedback()
    {
        while (isFeedbackLooping)
        {
            yield return StartCoroutine(CompleteSheetFeedback());
        }

        loopingFeedbackCoroutine = null;
    }

    public IEnumerator CompleteSheetFeedback()
    {
       
       

        // Go through each placed note in order, play its feedback (if it has one),
        // wait for it to finish, then tint it gold.
        foreach (GameObject note in placedNotes)
        {
            // Bail out mid-pass if the loop was stopped while this pass was running
            if (!isFeedbackLooping) yield break;

            if (note == null) continue;

            MMF_Player notePlayer = note.GetComponent<MMF_Player>();
            if (notePlayer != null)
            {
                notePlayer.PlayFeedbacks();
                //yield return new WaitUntil(() => !notePlayer.IsPlaying);
                yield return new WaitForSeconds(0.05f);
            }

            Image img = note.GetComponent<Image>();
            if (img != null)
                img.color = completionGoldColor;

            if (delayBetweenNoteFeedbacks > 0f)
                yield return new WaitForSeconds(delayBetweenNoteFeedbacks);
        }
    }

    public int NotesPlaced => placedNotes.Count;
    public int NotesRequired => notesRequiredToWin;
}