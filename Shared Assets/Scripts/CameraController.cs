using System.Threading;
using UnityEngine;
using Yarn.Unity;

#nullable enable

#if !USE_CINEMACHINE
// If cinemachine isn't installed, preserve references
using CinemachineCamera = UnityEngine.Component;
using CinemachineBrain = UnityEngine.Component;
#pragma warning disable CS1998 // Method will run synchronously and doesn't need 'async'
#else
using Unity.Cinemachine;
#endif

namespace Yarn.Unity.Samples
{
    /// <summary>
    /// A Dialogue Presenter that activates a camera when dialogue begins, and
    /// optionally changes the target of that camera to a named object in the
    /// scene.
    /// </summary>
    public class CameraController : DialoguePresenterBase
    {
        /// <summary>
        /// The name of the node header that this object looks for when trying
        /// to find a target to aim at.
        /// </summary>
        private const string targetGroupHeaderKey = "target";

        /// <summary>
        /// The <see cref="DialogueRunner"/> responsible for managing this
        /// presenter.
        /// </summary>
        [SerializeField] DialogueRunner? dialogueRunner;

        /// <summary>
        /// The Cinemachine camera that should be made active when dialogue
        /// begins.
        /// </summary>
        [SerializeField] CinemachineCamera? cinemachineTargetCamera;

        private void OnValidate()
        {
            if (dialogueRunner != null)
            {
                return;
            }

            // Attempt to find the dialogue runner that includes this presenter
            // in its presenters list.
            foreach (var runner in FindObjectsByType<DialogueRunner>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                foreach (var presenter in runner.DialoguePresenters)
                {
                    if (presenter == this)
                    {
                        // We found the runner that manages us. Store that.
                        this.dialogueRunner = runner;
                        return;
                    }
                }
            }
        }

        void Awake()
        {
            if (cinemachineTargetCamera != null && (dialogueRunner == null || !dialogueRunner.IsDialogueRunning))
            {
                // Dialogue is not running, so turn off the dialogue camera
                cinemachineTargetCamera.gameObject.SetActive(false);
            }
        }

        public override YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken token)
        {
            return YarnTask.CompletedTask;
        }

        public override YarnTask<DialogueOption?> RunOptionsAsync(DialogueOption[] dialogueOptions, CancellationToken cancellationToken)
        {
            return DialogueRunner.NoOptionSelected;
        }

        public override async YarnTask OnDialogueStartedAsync()
        {
            // When dialogue starts, check to see if the current dialogue has a
            // 'target:' header. If it does, find a game object with this name,
            // and tell the dialogue camera to track it. Additionally, wait
            // until the blend has finished before allowing dialogue to start.

#if !USE_CINEMACHINE
            // Cinemachine isn't available, so there's nothing we can do.
            return;
#else
            if (dialogueRunner == null)
            {
                return;
            }
            if (dialogueRunner.Dialogue.CurrentNode == null)
            {
                return;
            }
            if (cinemachineTargetCamera == null)
            {
                return;
            }

            foreach (var pair in dialogueRunner.Dialogue.GetHeaders(dialogueRunner.Dialogue.CurrentNode))
            {
                if (pair.Key == targetGroupHeaderKey)
                {
                    // ok now we can use this to get the right target
                    var go = GameObject.Find(pair.Value);

                    if (go == null)
                    {
                        Debug.LogWarning($"unable to find the game object named {pair.Value}");
                        break;
                    }

                    // Set up and activate the camera
                    cinemachineTargetCamera.Follow = go.transform;
                    cinemachineTargetCamera.gameObject.SetActive(true);

                    // Wait a frame in order to let the potential blend start
                    await YarnTask.Yield();

                    // Get the brain controlling this camera
                    var brain = CinemachineCore.FindPotentialTargetBrain(cinemachineTargetCamera);

                    if (brain == null)
                    {
                        // No brain found that control this camera. Odd! We
                        // can't get figure out if we're blending or not, so
                        // we'll just return here.
                        return;
                    }

                    // Wait for the blend to finish before returning
                    while (brain.IsBlending)
                    {
                        await YarnTask.Yield();
                    }

                    return;
                }
            }
#endif
        }

        public override YarnTask OnDialogueCompleteAsync()
        {
            if (cinemachineTargetCamera != null)
            {
                cinemachineTargetCamera.gameObject.SetActive(false);
            }
            return YarnTask.CompletedTask;
        }
    }
}