using UnityEngine;

public class DanceInputHandler : MonoBehaviour
{
    [SerializeField] private KeyCode upKey = KeyCode.UpArrow;
    [SerializeField] private KeyCode downKey = KeyCode.DownArrow;
    [SerializeField] private KeyCode leftKey = KeyCode.LeftArrow;
    [SerializeField] private KeyCode rightKey = KeyCode.RightArrow;

    private DanceGameManager danceGameManager;

    private void Start()
    {
        danceGameManager = FindObjectOfType<DanceGameManager>();
    }

    public void Update()
    {
        if (danceGameManager != null && !danceGameManager.IsPlaying)
            return;

        if (Input.GetKeyDown(upKey)) HitNoteWithTag("UpNote");
        if (Input.GetKeyDown(downKey)) HitNoteWithTag("DownNote");
        if (Input.GetKeyDown(leftKey)) HitNoteWithTag("LeftNote");
        if (Input.GetKeyDown(rightKey)) HitNoteWithTag("RightNote");
    }

    private void HitNoteWithTag(string noteTag)
    {
        GameObject note = GameObject.FindWithTag(noteTag);
        if (note != null)
        {
            DanceNote danceNote = note.GetComponent<DanceNote>();
            if (danceNote != null)
                danceNote.TryHit();
        }
    }
}
