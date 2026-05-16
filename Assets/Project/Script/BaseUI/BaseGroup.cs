using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace NSJ_MVVM
{
    public class BaseGroup : MonoBehaviour
    {
        [HideInInspector] public BasePanel Panel;
        [HideInInspector] public BaseCanvas Canvas => Panel.Canvas;

        public bool IsAllwaysActive;

        [SerializeField] protected BaseView[] _views;

        void Awake()
        {
            BindGroup();
        }
        public void ChangeView<TEnum>(TEnum view) where TEnum : Enum
        {
            int boxIndex = Util.ToIndex(view);

            ChangeView(boxIndex);
        }

        public void ChangeView(int index)
        {
            for (int i = 0; i < _views.Length; i++)
            {
                if (_views[i].IsAllwaysActive == true)
                    continue; // Always active view should not change visibility
                _views[i].gameObject.SetActive(i == index);
            }
            ChangeViewAfter(index);
        }
        protected virtual void ChangeViewAfter(int index) { }

        private void BindGroup()
        {
            _views = GetComponentsInChildren<BaseView>(true);
            foreach (BaseView view in _views)
            {
                view.Group = this;
            }
        }

       
    }
}