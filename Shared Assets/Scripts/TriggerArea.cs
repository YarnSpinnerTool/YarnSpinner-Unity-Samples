using UnityEngine;

#nullable enable

namespace Yarn.Unity.Samples
{
    [RequireComponent(typeof(Collider2D))]
    public class TriggerArea : MonoBehaviour
    {
        [SerializeField] DialogueRunner? dialogueRunner;

        [SerializeField] DialogueReference? dialogue;

        public async YarnTask OnPlayerEntered()
        {
            if (dialogueRunner == null || dialogue == null || dialogue.IsValid == false)
            {
                return;
            }
            if (dialogue.project == null || string.IsNullOrEmpty(dialogue.nodeName))
            {
                return;
            }

            dialogueRunner.SetProject(dialogue.project);
            dialogueRunner.StartDialogue(dialogue.nodeName);

            while (dialogueRunner.IsDialogueRunning)
            {
                await YarnTask.Yield();
            }
        }
    }
}