using UnityEngine;

public class DanceInputHandler : MonoBehaviour
{
    [SerializeField] private KeyCode upKey = KeyCode.UpArrow;
    [SerializeField] private KeyCode downKey = KeyCode.DownArrow;
    [SerializeField] private KeyCode leftKey = KeyCode.LeftArrow;
    [SerializeField] private KeyCode rightKey = KeyCode.RightArrow;

    public void Update()
    {
        if (Input.GetKeyDown(upKey)) HitNote("UpNote");
        if (Input.GetKeyDown(downKey)) HitNote("DownNote");
        if (Input.GetKeyDown(leftKey)) HitNote("LeftNote");
        if (Input.GetKeyDown(rightKey)) HitNote("RightNote");
    }

    private void HitNote(string noteTag)
    {
        GameObject note = GameObject.FindWithTag(noteTag);
        if (note != null)
        {
            note.GetComponent<DanceNote>().TryHit();
        }
    }
}
