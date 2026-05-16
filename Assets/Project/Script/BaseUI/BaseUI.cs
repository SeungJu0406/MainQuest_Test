using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NSJ_MVVM
{
    public class BaseUI : PointHandler
    {
        private Dictionary<string, GameObject> gameObjectDic;
        private Dictionary<(string, System.Type), Component> componentDic;

        protected bool _isBind = false;
        protected virtual void Awake()
        {
            Bind();
        }

        // 빠른 시간에 게임오브젝트만 바인딩
        protected void Bind()
        {
            if (gameObjectDic != null) return; // 이미 바인딩된 경우 재시도 X

            Transform[] transforms = GetComponentsInChildren<Transform>(true);
            gameObjectDic = new Dictionary<string, GameObject>(transforms.Length << 2);
            foreach (Transform child in transforms)
            {
                gameObjectDic.TryAdd(child.gameObject.name, child.gameObject);
            }

            componentDic = new Dictionary<(string, System.Type), Component>();

            _isBind = true;
        }

        // 비교적 오랜 시간에 게임오브젝트와 모든 컴포넌트 바인딩
        protected void BindAll()
        {
            Transform[] transforms = GetComponentsInChildren<Transform>(true);
            gameObjectDic = new Dictionary<string, GameObject>(transforms.Length << 2);

            foreach (Transform child in transforms)
            {
                gameObjectDic.TryAdd(child.gameObject.name, child.gameObject);
            }

            Component[] components = GetComponentsInChildren<Component>(true);
            componentDic = new Dictionary<(string, System.Type), Component>(components.Length << 4);
            foreach (Component child in components)
            {
                componentDic.TryAdd((child.gameObject.name, child.GetType()), child);
            }

             _isBind = true;
        }

        // 이름이 name인 UI 게임오브젝트 가져오기
        // GetUI("Key") : Key 이름의 게임오브젝트 가져오기
        public GameObject GetUI(in string name)
        {
            if (gameObjectDic == null)
                Bind(); // 방어적 호출

            gameObjectDic.TryGetValue(name, out GameObject gameObject);
            return gameObject;
        }

        // 이름이 name인 UI에서 컴포넌트 TStatus 가져오기
        // GetUI<Image>("Key") : Key 이름의 게임오브젝트에서 Image 컴포넌트 가져오기
        public T GetUI<T>(in string name) where T : Component
        {
            if (gameObjectDic == null || componentDic == null)
                Bind(); // 방어적 호출

            (string, System.Type) key = (name, typeof(T));

            if (componentDic.TryGetValue(key, out Component component))
            {
                return component as T;
            }

            if (!gameObjectDic.TryGetValue(name, out GameObject gameObject))
            {
                return null;
            }

            component = gameObject.GetComponent<T>();
            if (component == null)
            {
                return null;
            }

            componentDic.TryAdd(key, component);
            return component as T;
        }

        protected override void OnPointEnter(PointerEventData eventData) { }

        protected override void OnPointExit(PointerEventData eventData) { }

        protected override void OnPointClick(PointerEventData eventData) { }
    }
}