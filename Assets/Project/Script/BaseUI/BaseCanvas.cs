using System;
using UnityEngine;
using UnityEngine.Events;
using Utility;

namespace NSJ_MVVM
{
    public class BaseCanvas : MonoBehaviour
    {
        [SerializeField] protected BasePanel[] _panels;
        public virtual int DefaultPanelIndex => 0; // 기본 패널 인덱스

        public event UnityAction<int> OnPanelChanged;

        private int _curPanelIndex = -1;
        void Awake()
        {
            BindPanel();
        }

        /// <summary>
        /// 패널을 변경합니다. Enum 타입을 사용하여 패널을 지정할 수 있습니다.
        /// </summary>
        /// <typeparam name="TEnum"></typeparam> 
        public void ChangePanel<TEnum>(TEnum panel) where TEnum : Enum
        {
            int panelIndex = Util.ToIndex(panel);
            ChangePanel(panelIndex);
        }
        public void ChangePanel(int index)
        {
            if(_curPanelIndex == index)
                return; // 이미 활성화된 패널이면 무시
            _curPanelIndex = index;
            for (int i = 0; i < _panels.Length; i++)
            {
                if (_panels[i].IsAllwaysActive == true)
                    continue; // 항상 활성화된 패널은 제외
                _panels[i].gameObject.SetActive(i == index);
            }
            ChangePanelAfter(index);
            OnPanelChanged?.Invoke(index); // 패널 변경 이벤트 호출
        }

        public void ChangePanel(string name)
        {
            int index = 0;
            for (int i = 0; i < _panels.Length; i++)
            {
                if (_panels[i].IsAllwaysActive == true)
                    continue; // 항상 활성화된 패널은 제외
                _panels[i].gameObject.SetActive(_panels[i].gameObject.name == name);
                if (_panels[i].gameObject.name == name)
                    index = i;
            }
            ChangePanelAfter(index);
            OnPanelChanged?.Invoke(index); // 패널 변경 이벤트 호출
        }

        protected virtual void ChangePanelAfter(int index) { }

        /// <summary>
        /// 패널을 바인딩합니다. 각 패널의 Canvas 속성을 설정하여 상호작용할 수 있도록 합니다.
        /// </summary>
        private void BindPanel()
        {
            _panels = GetComponentsInChildren<BasePanel>(true);
            foreach (BasePanel panel in _panels)
            {
                panel.Canvas = this;
            }
        }
    }
}