using UnityEngine;
using UnityEngine.InputSystem;

public class StartABandGame : MicroGamePlayer
{
    [SerializeField] InputActionReference changeChordAction; // Vector2, left stick
    [SerializeField] InputActionReference playNoteAction;    // Vector2, right stick
    [SerializeField] AudioSource guitarSource;
    [SerializeField] AudioClip C, D, E, F, G, A, B, D1;
    [SerializeField] float wheelDeadzone = 0.2f; // ignore stick near center
    [SerializeField] float strumThreshold = 0.5f;
    [SerializeField] float strumReleaseThreshold = 0.25f; // must fall back below this before it can re-trigger
    [SerializeField] SheetMusicDisplay sheetMusic;

    public enum NOTE
    {
        C,
        D,
        E,
        F,
        G,
        A,
        B,
        D1,
    }

    // Slice index (0-7, counterclockwise from 0°/right) -> note shown in that wedge.
    // Matches a wheel whose dividing lines sit at 0°, 45°, 90°, 135°... (not centered on them).
    private static readonly NOTE[] wheelOrder =
    {
        NOTE.D,  // [0, 45)
        NOTE.C,  // [45, 90)
        NOTE.D1, // [90, 135)   -> displayed as "C1"
        NOTE.B,  // [135, 180)
        NOTE.A,  // [180, 225)
        NOTE.G,  // [225, 270)
        NOTE.F,  // [270, 315)
        NOTE.E,  // [315, 360)
    };

    private NOTE currentChord = NOTE.C;
    private int lastStrumDirection = 0; // -1 down, 0 neutral, 1 up

    private void OnEnable()
    {
        if (changeChordAction != null)
            changeChordAction.action.Enable();
        if (playNoteAction != null)
            playNoteAction.action.Enable();

        if (sheetMusic != null)
            sheetMusic.OnSheetFilled += HandleSheetFilled;
    }

    private void OnDisable()
    {
        if (changeChordAction != null)
            changeChordAction.action.Disable();
        if (playNoteAction != null)
            playNoteAction.action.Disable();

        if (sheetMusic != null)
            sheetMusic.OnSheetFilled -= HandleSheetFilled;
    }

    public override void Update()
    {
        if (!canInteract) return;
        ChangeChord();
        PlayChord();
    }

    public void ChangeChord()
    {
        if (changeChordAction == null) return;
        Vector2 stick = changeChordAction.action.ReadValue<Vector2>();
        if (stick.magnitude < wheelDeadzone) return; // keep last chord if stick is centered

        float angle = Mathf.Atan2(stick.y, stick.x) * Mathf.Rad2Deg; // -180..180, 0 = right
        if (angle < 0f) angle += 360f; // 0..360

        // Divide the circle into 8 equal 45° slices starting at 0°, matching
        // the wheel's drawn lines (which sit ON the compass directions, not
        // centered between them), then look up which note lives in that slice.
        int slice = Mathf.FloorToInt(angle / 45f) % 8;
        currentChord = wheelOrder[slice];
    }


    public void PlayChord()
    {
        if (playNoteAction == null) return;
        float y = playNoteAction.action.ReadValue<Vector2>().y;

        if (lastStrumDirection == 0)
        {
            // Not currently in a strum — check if we've crossed the trigger threshold
            if (y > strumThreshold)
            {
                lastStrumDirection = 1;
                Strum();
            }
            else if (y < -strumThreshold)
            {
                lastStrumDirection = -1;
                Strum();
            }
        }
        else
        {
            // Currently in a strum — only clear it once we're back near center
            if (Mathf.Abs(y) < strumReleaseThreshold)
            {
                lastStrumDirection = 0;
            }
        }
    }

    private void Strum()
    {
        AudioClip clip = GetClipForNote(currentChord);
        if (clip != null && guitarSource != null)
            guitarSource.PlayOneShot(clip);

        if (sheetMusic != null)
            sheetMusic.AddNote(currentChord);
    }

    private void HandleSheetFilled()
    {
        hasResolved = true;
    }

    private AudioClip GetClipForNote(NOTE note)
    {
        switch (note)
        {
            case NOTE.C: return C;
            case NOTE.D: return D;
            case NOTE.E: return E;
            case NOTE.F: return F;
            case NOTE.G: return G;
            case NOTE.A: return A;
            case NOTE.B: return B;
            case NOTE.D1: return D1;
            default: return null;
        }
    }
}