using DialogueEditor;
using UnityEngine;

public class Conversation : MonoBehaviour
{
    [SerializeField] private NPCConversation conversation;
    public PlayerController _playerController;

    private void Awake()
    {
        _playerController = FindObjectOfType<PlayerController>();
    }

    private void Start()
    {
        ConversationManager.OnConversationStarted += SetConversationState;
        ConversationManager.Instance.StartConversation(conversation);
        ConversationManager.OnConversationEnded += RemoveConversationState;
    }

    private void RemoveConversationState()
    {
        Debug.Log("��������� ������");
        _playerController.isConversation = false;
    }

    private void SetConversationState()
    {
        Debug.Log("������ ������");
        _playerController.isConversation = true;
    }
}
