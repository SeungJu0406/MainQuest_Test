using System;
using UnityEngine;
using UnityEngine.Events;
using Utility;

namespace NSJ_MVVM
{
    public class BasePanel : MonoBehaviour
    {
        [HideInInspector] public BaseCanvas Canvas;

        public bool IsAllwaysActive;

        [SerializeField] protected BaseGroup[] _groups;

        public event UnityAction<int> OnGroupChanged;
        void Awake()
        {
            BindGroup();
        }

        public void ChangeGroup<TEnum>(TEnum view) where TEnum : Enum
        {
            int boxIndex = Util.ToIndex(view);

            ChangeGroup(boxIndex);
        }
        public void ChangeGroup(int index)
        {
            for (int i = 0; i < _groups.Length; i++)
            {
                if (_groups[i].IsAllwaysActive == true)
                    continue; // Always active group should not change visibility
                _groups[i].gameObject.SetActive(i == index);
            }
            ChangeGroupAfter(index);
            OnGroupChanged?.Invoke(index); // Notify listeners about the group change
        }
        public void ChangeGroup(string name)
        {
            int index = 0;
            for (int i = 0; i < _groups.Length; i++)
            {
                if (_groups[i].IsAllwaysActive == true)
                    continue; // 항상 활성화된 패널은 제외
                _groups[i].gameObject.SetActive(_groups[i].gameObject.name == name);
                if (_groups[i].gameObject.name == name)
                    index = i;
            }
            ChangeGroupAfter(index);
            OnGroupChanged?.Invoke(index);
        }
        protected virtual void ChangeGroupAfter(int index) { }

        private void BindGroup()
        {
            _groups = GetComponentsInChildren<BaseGroup>(true);
            foreach (BaseGroup group in _groups)
            {
                group.Panel = this;
            }
        }
    }
}