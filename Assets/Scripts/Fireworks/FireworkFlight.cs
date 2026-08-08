using System.Collections;
using UnityEngine;

namespace Fireworks
{
    /// <summary>打上げ位置から開花位置までの移動だけを担当する。</summary>
    public sealed class FireworkFlight : MonoBehaviour
    {
        public IEnumerator Travel(Vector3 destination, float duration)
        {
            Vector3 startPosition = transform.position;
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                transform.position = Vector3.Lerp(startPosition, destination, elapsedTime / duration);
                yield return null;
            }

            transform.position = destination;
        }
    }
}
