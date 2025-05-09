/*
Yarn Spinner is licensed to you under the terms found in the file LICENSE.md.
*/

using UnityEngine;

namespace Yarn.Unity.Samples
{
    public class ButtonBump : MonoBehaviour
    {
        public void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<Yarn.Unity.Samples.ValueUpdater>(out var updater))
            {
                updater.UpdateValue();
            }
        }
    }
}