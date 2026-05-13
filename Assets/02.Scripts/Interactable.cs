using UnityEngine;

public class Interactable : MonoBehaviour
{
    public enum InteractionType { Chest, NPC }

    [SerializeField] private InteractionType type; 
    [SerializeField] private string objectName;    

    public string GetInteractionMessage()
    {
        switch (type)
        {
            case InteractionType.Chest: return $"[E] {objectName} 열기";
            case InteractionType.NPC: return $"[E] {objectName}와 대화하기";
            default: return "[E] 상호작용";
        }
    }
}