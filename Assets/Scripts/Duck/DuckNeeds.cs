[System.Serializable]
public class PetNeed
{
    public string name;
    public float value;
    public float decayRate;

    public void Decay(float deltaHours)
    {
        value -= decayRate * deltaHours;
    }

    public void Recover(float amount)
    {
        value += amount;
    }

    public void Clamp()
    {
        value = UnityEngine.Mathf.Clamp(value, 0f, 100f);
    }
}
