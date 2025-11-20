/*
Yarn Spinner is licensed to you under the terms found in the file LICENSE.md.
*/

namespace YarnSpinner.Unity.Samples.NodeInternals
{
    using UnityEngine;
    using Yarn.Unity;
    using Yarn;
    using System.Linq;
    using System.Collections.Generic;

    #nullable enable
    public class CommandAssetPreloader : MonoBehaviour
    {
        public DialogueReference? dialogueReference;

        void Start()
        {
            var runner = FindAnyObjectByType<DialogueRunner>();
            runner.AddCommandHandler("preload_command_assets", PreloadAssets);
        }
        
        // this command is what does the actual "preloading" of the assets
        // it does this by moving through all of the instructions in the node
        // when an instruction is a command
        // and it is one of the commands we care about
        // we use the parameters of the node to work out what asset to load
        // Note: we don't actually do any asset loading, we store a string into a set
        // but the principle is the same
        // in this case we are doing all of this through a command because then you can control when the load happens in Yarn
        // but you could just as easily do this in Start or at some other convenient time
        private void PreloadAssets()
        {
            // just some checks to make sure we actually have a valid node to grab information out of
            if (dialogueReference == null)
            {
                return;
            }
            if (dialogueReference.project == null)
            {
                return;
            }
            if (dialogueReference.nodeName == null)
            {
                return;
            }
            if (dialogueReference.project.Program.Nodes == null)
            {
                return;
            }
            if (!dialogueReference.project.Program.Nodes.TryGetValue(dialogueReference.nodeName, out var node))
            {
                return;
            }

            // now that we have our node
            foreach (var instruction in node.Instructions)
            {
                // if the instruction isn't a RunCommand instruction we don't care about it
                // the majority of instructions nodes will be RunLine instructions
                if (instruction.InstructionTypeCase != Instruction.InstructionTypeOneofCase.RunCommand)
                {
                    continue;
                }

                // now that we have the command we want to split it the same way we do in the dialogue runner
                var elements = DialogueRunner.SplitCommandText(instruction.RunCommand.CommandText).ToArray();

                // all the commands we care about have only a single parameter
                // so if it has more (or less) than we can skip it
                // for different scenarios you would likely handle this inside the switch instead
                if (elements.Length != 2)
                {
                    continue;
                }

                // the first element of a command is always it's name
                // so we can use this to determine if it's a command we want to handle
                // if it is then we use the parameters (the rest of the command string)
                // to work out what asset to "load"
                switch (elements[0])
                {
                    case "set_background":
                        // now we "preload" the "asset"
                        backgrounds.Add(elements[1]);
                        break;
                    
                    case "set_music":
                        // now we "preload" the "asset"
                        music.Add(elements[1]);
                        break;
                    
                    case "character_avatar":
                        // now we "preload" the "asset"
                        avatars.Add(elements[1]);
                        break;
                }
            }
        }
        
        // these three sets represent our preloaded assets
        // in an actual game you'd want something a lot more well designed
        // and something that actually keeps assets around instead of just a string
        // but for a sample this works fine
        static HashSet<string> backgrounds = new();
        static HashSet<string> music = new();
        static HashSet<string> avatars = new();

        // this command clears the cached "assets"
        [YarnCommand("clear_preload")]
        public static void ClearCachedAsset()
        {
            backgrounds.Clear();
            music.Clear();
            avatars.Clear();
        }

        // these three commands represent the commands that need to load an asset
        // and then do something with them
        // in this case we don't actually load any assets or 
        [YarnCommand("set_background")]
        public static async YarnTask SetBackground(string backgroundAsset)
        {
            if (backgrounds.Add(backgroundAsset))
            {
                Debug.LogWarning($"{backgroundAsset} is not already \"loaded\", pretending to do that now");
                // perform some long load
                await YarnTask.Delay(1000);
            }
            else
            {
                Debug.Log($"{backgroundAsset} has already been \"loaded\"");
            }
            Debug.Log($"\"setting\" the background to be: {backgroundAsset}");
            // now do the thing with the background asset
        }

        [YarnCommand("set_music")]
        public static async YarnTask SetMusic(string musicAsset)
        {
            if (music.Add(musicAsset))
            {
                Debug.LogWarning($"{musicAsset} is not already \"loaded\", pretending to do that now");
                // perform some long load
                await YarnTask.Delay(1000);
            }
            else
            {
                Debug.Log($"{musicAsset} has already been \"loaded\"");
            }
            Debug.Log($"\"setting\" the music to be: {musicAsset}");
            // now do the thing with the music asset
        }

        [YarnCommand("character_avatar")]
        public static async YarnTask ShowAvatar(string characterName)
        {
            if (avatars.Add(characterName))
            {
                Debug.LogWarning($"{characterName} is not already \"loaded\", pretending to do that now");
                // perform some long load
                await YarnTask.Delay(1000);
            }
            else
            {
                Debug.Log($"{characterName} has already been \"loaded\"");
            }
            Debug.Log($"\"showing\" the avatar for: {characterName}");
            // now do the thing with the avatar asset
        }
    }
}