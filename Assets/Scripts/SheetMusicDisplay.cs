using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SheetMusicDisplay : MonoBehaviour
{
    [System.Serializable]
    public struct NoteVisual
    {
        public StartABandGame.NOTE note;
        public Sprite noteSprite;
        public float staffYPosition;
    }

    [Header("Staff Setup")]
    [SerializeField] RectTransform staffContainer;
    [SerializeField] GameObject noteIconPrefab;
    [SerializeField] float noteSpacingX = 60f;
    [SerializeField] List<NoteVisual> noteVisuals;

    // REMOVED: [SerializeField] int notesRequiredToWin = 8;
    [Header("Win Condition")]
    [SerializeField] float startXOffset = 0f; // NEW: left padding before first note

    private readonly List<GameObject> placedNotes = new List<GameObject>();
    private Dictionary<StartABandGame.NOTE, NoteVisual> noteVisualLookup;
    private int notesRequiredToWin; // NEW: computed, not authored

    public System.Action OnSheetFilled;

    private void Awake()
    {
        noteVisualLookup = new Dictionary<StartABandGame.NOTE, NoteVisual>();
        foreach (var nv in noteVisuals)
            noteVisualLookup[nv.note] = nv;

        RecalculateCapacity(); // NEW
    }

    // NEW
    private void RecalculateCapacity()
    {
        if (staffContainer == null || noteSpacingX <= 0f)
        {
            notesRequiredToWin = 0;
            return;
        }

        float usableWidth = staffContainer.rect.width - startXOffset;
        notesRequiredToWin = Mathf.Max(1, Mathf.FloorToInt(usableWidth / noteSpacingX) + 1);
    }

    public void AddNote(StartABandGame.NOTE note)
    {
        if (placedNotes.Count >= notesRequiredToWin) return;

        if (!noteVisualLookup.TryGetValue(note, out NoteVisual visual))
        {
            Debug.LogWarning($"No visual configured for note {note}");
            return;
        }

        GameObject icon = Instantiate(noteIconPrefab, staffContainer);
        RectTransform rt = icon.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(startXOffset + placedNotes.Count * noteSpacingX, visual.staffYPosition); // startXOffset added

        Image image = icon.GetComponent<Image>();
        if (image != null && visual.noteSprite != null)
            image.sprite = visual.noteSprite;

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

    public int NotesPlaced => placedNotes.Count;
    public int NotesRequired => notesRequiredToWin;
}