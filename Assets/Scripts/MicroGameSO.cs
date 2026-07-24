using UnityEngine;

[CreateAssetMenu(fileName = "MicroGameSO", menuName = "Scriptable Objects/MicroGameSO")]
public class MicroGameSO : ScriptableObject
{
    public string uid;
    public string instruction; 
    public float instructionDuration;
    public Sprite p1Control, p2Control;


}
