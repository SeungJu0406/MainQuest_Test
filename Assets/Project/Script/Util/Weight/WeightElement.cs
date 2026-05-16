using UnityEngine;

namespace WeightUtility
{
    public class WeightElement<T>
    {
        public T Value { get; private set; }
        public float Weight { get; private set; }

        public WeightElement(T element, float weight)
        {
            Value = element;
            Weight = weight;
        }

        public void SetWeight(float weight)
        {
            Weight = weight;
        }
    }
}