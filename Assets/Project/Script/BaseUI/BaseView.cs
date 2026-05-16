using System;
using Unity.VisualScripting;
using UnityEngine;

namespace NSJ_MVVM
{
    public abstract class BaseView : BaseUI
    {
        [HideInInspector] public BaseGroup Group;
        [HideInInspector] public BasePanel Panel => Group.Panel;
        [HideInInspector] public BaseCanvas Canvas => Group.Panel.Canvas;
        public bool IsAllwaysActive;
        protected override void Awake()
        {
            base.Awake();
            InitGetUI();
            InitAwake();
        }
        protected virtual void Start()
        {
            InitStart();
            SubscribeEvents();
        }

        protected virtual void OnSelectObject(GameObject select) { }

        protected abstract void InitGetUI();
        /// <summary>
        /// 뷰가 Awake 단계에서 초기화되는 메서드입니다.
        /// </summary>
        protected abstract void InitAwake();

        /// <summary>
        /// 뷰가 Start 단계에서 초기화되는 메서드입니다.
        /// </summary>
        protected abstract void InitStart();
        /// <summary>
        /// 뷰가 이벤트를 구독하는 메서드입니다. 이 메서드는 뷰가 Start 단계에서 호출됩니다.
        /// </summary>
        protected abstract void SubscribeEvents();
    }
}
