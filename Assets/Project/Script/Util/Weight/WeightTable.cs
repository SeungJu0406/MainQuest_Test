using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WeightUtility
{
    public class WeightTable<T>
    {
        private List<WeightElement<T>> _elements;
        private float _totalWeight;

        public WeightTable(IEnumerable<WeightElement<T>> elements = null)
        {
            if (elements != null)
            {
                _elements = elements.ToList();
            }
            else
            {
                _elements = new List<WeightElement<T>>();
            }
            _totalWeight = _elements.Sum(e => e.Weight);
        }

        public static TElement PickInstance<TElement>(List<TElement> elements) where TElement : IWeightElement
        {
            if (elements == null || !elements.Any())
            {
                return default;
            }
            WeightTable<TElement> weightTable = new WeightTable<TElement>();
            foreach (TElement element in elements)
            {
                weightTable.AddElement(element, element.Weight);
            }
            return weightTable.Pick();
        }
        public T Pick(System.Random seed = null)
        {
            float randomValue;

            if (seed == null)
            {
                randomValue = UnityEngine.Random.Range(0f, _totalWeight);
            }
            else
            {
                randomValue = (float)seed.NextDouble() * _totalWeight;
            }

            float cumulativeWeight = 0f;
            foreach (WeightElement<T> element in _elements)
            {
                cumulativeWeight += element.Weight;
                if (randomValue < cumulativeWeight)
                {
                    return element.Value;
                }
            }

            if(_elements.Count == 0)
            {
                return default;
            }
            T fallback = _elements.Last().Value;      
            return fallback;
        }

        public void AddElement(T element, float weight)
        {
            _elements.Add(new WeightElement<T>(element, weight));
            _totalWeight = _elements.Sum(e => e.Weight);
        }
        public void RemoveElement(T element)
        {
            int index = _elements.FindIndex(e => EqualityComparer<T>.Default.Equals(e.Value, element));
            if (index >= 0)
            {
                _elements.RemoveAt(index);
                _totalWeight = _elements.Sum(e => e.Weight);
            }
        }
        public void EditWeight(T element, float newWeight)
        {
            var weightElement = _elements.Find(e => EqualityComparer<T>.Default.Equals(e.Value, element));
            if (weightElement != null)
            {
                weightElement.SetWeight(newWeight);
                _totalWeight = _elements.Sum(e => e.Weight);
            }
        }
        public void Clear()
        {
            _elements.Clear();
            _totalWeight = 0f;
        }
    }
}