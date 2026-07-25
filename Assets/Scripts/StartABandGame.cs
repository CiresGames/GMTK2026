using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class StartABandGame : MicroGamePlayer
{
    [Header("Guitar (Player 1)")]
    [SerializeField] InputAction changeChordAction; // Vector2, left stick
    [SerializeField] InputAction playNoteAction;    // Vector2, right stick
    [SerializeField] AudioSource guitarSource;
    [SerializeField] AudioClip C, D, E, F, G, A, B, D1;
    [SerializeField] Sprite spriteC, spriteD, spriteE, spriteF, spriteG, spriteA, spriteB, spriteD1;
    [SerializeField] Image characterGuitar; 
    [SerializeField] float wheelDeadzone = 0.2f; // ignore stick near center
    [SerializeField] float strumThreshold = 0.5f;
    [SerializeField] float strumReleaseThreshold = 0.25f; // must fall back below this before it can re-trigger
    [SerializeField] SheetMusicDisplay guitarSheetMusic;

    [Header("Drums (Player 2)")]
    [SerializeField] InputAction cymbalAction; // button, left shoulder
    [SerializeField] InputAction hatAction;    // button, right shoulder
    [SerializeField] InputAction kickAction;   // float, left trigger
    [SerializeField] InputAction snareAction;  // float, right trigger
    [SerializeField] AudioSource drumSource;
    [SerializeField] AudioClip cymbalClip, hatClip, kickClip, snareClip;
    [SerializeField] float triggerThreshold = 0.5f;
    [SerializeField] float triggerReleaseThreshold = 0.25f;
    [SerializeField] SheetMusicDisplay drumSheetMusic;


    [SerializeField] AudioClip successMusic; 

    [Header("Players")]
    [SerializeField] PlayerInput p1, p2;
    [SerializeField] GameObject p1Instruction, p2Intstruction; 

    public enum GUITAR_NOTE
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


    public enum DRUM_NOTE
    {
        Cymbal,
        Hat,
        Kick,
        Snare,
    }

    // Slice index (0-7, counterclockwise from 0°/right) -> note shown in that wedge.
    // Matches a wheel whose dividing lines sit at 0°, 45°, 90°, 135°... (not centered on them).
    private static readonly GUITAR_NOTE[] wheelOrder =
    {
        GUITAR_NOTE.D,  // [0, 45)
        GUITAR_NOTE.C,  // [45, 90)
        GUITAR_NOTE.D1, // [90, 135)   -> displayed as "C1"
        GUITAR_NOTE.B,  // [135, 180)
        GUITAR_NOTE.A,  // [180, 225)
        GUITAR_NOTE.G,  // [225, 270)
        GUITAR_NOTE.F,  // [270, 315)
        GUITAR_NOTE.E,  // [315, 360)
    };

    private GUITAR_NOTE currentChord = GUITAR_NOTE.C;
    private int lastStrumDirection = 0; // -1 down, 0 neutral, 1 up

    // Drum button/trigger edge-tracking
    private bool cymbalHeld = false;
    private bool hatHeld = false;
    private int lastKickDirection = 0;  // reuse the "0 = not pressed" pattern for the analog trigger
    private int lastSnareDirection = 0;

    // Track whether each player's sheet has been filled
    private bool guitarSheetFilled = false;
    private bool drumSheetFilled = false;

    private void Awake()
    {
        changeChordAction = p1.actions["Guitar/ChangeChord"];
        playNoteAction = p1.actions["Guitar/PlayChord"];

        cymbalAction = p2.actions["Drum/Cymbal"];
        hatAction = p2.actions["Drum/Hat"];
        kickAction = p2.actions["Drum/Kick"];
        snareAction = p2.actions["Drum/Snare"];
    }

    private void Start()
    {
        var gp1 = GamepadAssigner.Instance.GetGamepadForPlayer(0);
        if (gp1 != null)
            p1.SwitchCurrentControlScheme("Gamepad", gp1);

        var gp2 = GamepadAssigner.Instance.GetGamepadForPlayer(1);
        if (gp2 != null)
            p2.SwitchCurrentControlScheme("Gamepad", gp2);

        Debug.Log($"gp1: {gp1?.displayName ?? "NULL"}, gp2: {gp2?.displayName ?? "NULL"}");
    }


    private void OnEnable()
    {
        changeChordAction.Enable();
        playNoteAction.Enable();

        cymbalAction.Enable();
        hatAction.Enable();
        kickAction.Enable();
        snareAction.Enable();

        if (guitarSheetMusic != null)
            guitarSheetMusic.OnSheetFilled += HandleGuitarSheetFilled;

        if (drumSheetMusic != null)
            drumSheetMusic.OnSheetFilled += HandleDrumSheetFilled;
    }

    private void OnDisable()
    {
        changeChordAction.Disable();
        playNoteAction.Disable();

        cymbalAction.Disable();
        hatAction.Disable();
        kickAction.Disable();
        snareAction.Disable();

        if (guitarSheetMusic != null)
            guitarSheetMusic.OnSheetFilled -= HandleGuitarSheetFilled;

        if (drumSheetMusic != null)
            drumSheetMusic.OnSheetFilled -= HandleDrumSheetFilled;
    }

    public override void Update()
    {
        if (!canInteract) return;
        if (hasResolved) return; 
        ChangeChord();
        PlayChord();
        PlayDrums();
    }

    public void ChangeChord()
    {
        if (changeChordAction == null) return;
        Vector2 stick = changeChordAction.ReadValue<Vector2>();
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
        float y = playNoteAction.ReadValue<Vector2>().y;

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

    public void PlayDrums()
    {
        // Shoulder buttons: simple digital press/release
        if (cymbalAction != null)
        {
            bool pressed = cymbalAction.ReadValue<float>() > 0.5f;
            if (pressed && !cymbalHeld)
                HitDrum(DRUM_NOTE.Cymbal, cymbalClip);
            cymbalHeld = pressed;
        }

        if (hatAction != null)
        {
            bool pressed = hatAction.ReadValue<float>() > 0.5f;
            if (pressed && !hatHeld)
                HitDrum(DRUM_NOTE.Hat, hatClip);
            hatHeld = pressed;
        }

        // Triggers: analog, so use threshold/release like the guitar strum
        if (kickAction != null)
        {
            float v = kickAction.ReadValue<float>();
            if (lastKickDirection == 0)
            {
                if (v > triggerThreshold)
                {
                    lastKickDirection = 1;
                    HitDrum(DRUM_NOTE.Kick, kickClip);
                }
            }
            else if (v < triggerReleaseThreshold)
            {
                lastKickDirection = 0;
            }
        }

        if (snareAction != null)
        {
            float v = snareAction.ReadValue<float>();
            if (lastSnareDirection == 0)
            {
                if (v > triggerThreshold)
                {
                    lastSnareDirection = 1;
                    HitDrum(DRUM_NOTE.Snare, snareClip);
                }
            }
            else if (v < triggerReleaseThreshold)
            {
                lastSnareDirection = 0;
            }
        }
    }

    private void Strum()
    {
        AudioClip clip = GetClipForNote(currentChord);
        if (clip != null && guitarSource != null)
            guitarSource.PlayOneShot(clip);

        if (guitarSheetMusic != null)
            guitarSheetMusic.AddNote(currentChord);

        ChangeGuitarSprite(currentChord); 
    }

    private void HitDrum(DRUM_NOTE note, AudioClip clip)
    {
        if (clip != null && drumSource != null)
            drumSource.PlayOneShot(clip);

        if (drumSheetMusic != null)
            drumSheetMusic.AddNote(note);
    }

    private void HandleGuitarSheetFilled()
    {
        guitarSheetFilled = true;
        CheckResolved();
    }

    private void HandleDrumSheetFilled()
    {
        drumSheetFilled = true;
        CheckResolved();
    }

    private void CheckResolved()
    {
        // Resolve once both sheets are filled. If either sheet doesn't
        // exist (left null in the inspector), don't wait on it.
        bool guitarDone = guitarSheetFilled || guitarSheetMusic == null;
        bool drumDone = drumSheetFilled || drumSheetMusic == null;

        if (guitarDone && drumDone)
            hasResolved = true;
    }

    private AudioClip GetClipForNote(GUITAR_NOTE note)
    {
        switch (note)
        {
            case GUITAR_NOTE.C: return C;
            case GUITAR_NOTE.D: return D;
            case GUITAR_NOTE.E: return E;
            case GUITAR_NOTE.F: return F;
            case GUITAR_NOTE.G: return G;
            case GUITAR_NOTE.A: return A;
            case GUITAR_NOTE.B: return B;
            case GUITAR_NOTE.D1: return D1;
            default: return null;
        }
    }

    private Sprite GetSpriteForNote(GUITAR_NOTE note)
    {
        switch (note)
        {
            case GUITAR_NOTE.C: return spriteC;
            case GUITAR_NOTE.D: return spriteD;
            case GUITAR_NOTE.E: return spriteE;
            case GUITAR_NOTE.F: return spriteF;
            case GUITAR_NOTE.G: return spriteG;
            case GUITAR_NOTE.A: return spriteA;
            case GUITAR_NOTE.B: return spriteB;
            case GUITAR_NOTE.D1: return spriteD1;
            default: return null;
        }
    }

    private void ChangeGuitarSprite(GUITAR_NOTE note)
    {
        characterGuitar.sprite = GetSpriteForNote(note);  
    }


    public override void DisplayInstructions(bool flag)
    {

        base.DisplayInstructions(flag); 
        p1Instruction.SetActive(flag);
        p2Intstruction.SetActive(flag); 
    }

    public override IEnumerator Success()
    {
        yield return null;

        guitarSource.PlayOneShot(successMusic);

        guitarSheetMusic.StartFeedbackLoop();
        drumSheetMusic.StartFeedbackLoop();

        yield return new WaitForSeconds(successMusic.length);

        guitarSheetMusic.StopFeedbackLoop();
        drumSheetMusic.StopFeedbackLoop();

        StartCoroutine(GameManager.Instance.microGameManager.CompleteGame(GameManager.Instance.microGameManager.currentGameIndex)); 
    }

   

}