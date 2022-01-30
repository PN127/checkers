using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace Checkers
{
    public abstract class BaseClickComponent : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        protected GameManager _gm;
        protected bool stepBlack;
        protected GameObject selectedFigure;
        private string _instance = " (Instance)";

        //Меш игрового объекта
        protected MeshRenderer _mesh;
        //Список материалов на меше объекта
        protected Material[] _meshMaterials = new Material[3];


        [Tooltip("Цветовая сторона игрового объекта"), SerializeField]
        private ColorType _color;

        /// <summary>
        /// Возвращает цветовую сторону игрового объекта
        /// </summary>
        public ColorType GetColor => _color;

        public Material[] GetMaterials => _meshMaterials;

        /// <summary>
        /// Возвращает или устанавливает пару игровому объекту
        /// </summary>
        /// <remarks>У клеток пара - фишка, у фишек - клетка</remarks>
        public BaseClickComponent Pair { get; set; }
        /// <summary>
        /// Устанавливает материал
        /// </summary>
        public void InstallMaterials(int index = 0)
        {
            if (index < 0 || index > 2)
            {
                Debug.LogError("Попытка добавить несуществующий материал. Индекс может быть равен только 0, 1 или 2");
                return;
            }
            _mesh.material = _meshMaterials[index];
        }

        public bool CleaningName(CellComponent component)
        {
            var name = component._mesh.material.name.Replace(_instance, string.Empty);
            if (name == component._meshMaterials[1].name)
                return true;
            return false;
        }

        /// <summary>
        /// Событие клика на игровом объекте
        /// </summary>
        public event ClickEventHandler OnClickEventHandler;

        /// <summary>
        /// Событие наведения и сброса наведения на объект
        /// </summary>
        public event FocusEventHandler OnFocusEventHandler;

        public event TriggerEnterEventHandler OnTriggerEnterEventHandler;


        //При навадении на объект мышки, вызывается данный метод
        //При наведении на фишку, должна подсвечиваться клетка под ней
        //При наведении на клетку - подсвечиваться сама клетка
        public abstract void OnPointerEnter(PointerEventData eventData);

        //Аналогично методу OnPointerEnter(), но срабатывает когда мышка перестает
        //указывать на объект, соответственно нужно снимать подсветку с клетки
        public abstract void OnPointerExit(PointerEventData eventData);

        //При нажатии мышкой по объекту, вызывается данный метод
        public abstract void OnPointerClick(PointerEventData eventData);


        //Этот метод можно вызвать в дочерних классах (если они есть) и тем самым пробросить вызов
        //события из дочернего класса в родительский
        protected void CallBackFocusEvent(CellComponent target, bool isSelect)
        {
            OnFocusEventHandler?.Invoke(target, isSelect);
        }
        protected void CallBackClickEvent(BaseClickComponent target)
        {
            OnClickEventHandler?.Invoke(target);
        }
        protected void CallBackOnTriggerEnter(Collider other)
        {
            OnTriggerEnterEventHandler?.Invoke(other);
        }

        protected virtual void Start()
        {
            _gm = GameManager.gm;
            _mesh = GetComponent<MeshRenderer>();
            //Этот список будет использоваться для набора материалов у меша,
            //в данном ДЗ достаточно массива из 3 элементов
            //1 элемент - родной материал меша, он не меняется
            //2 элемент - материал при наведении курсора на клетку/выборе фишки
            //3 элемент - материал клетки, на которую можно передвинуть фишку
            _meshMaterials[0] = _mesh.material;
            _meshMaterials[1] = _gm.green;
            _meshMaterials[2] = _gm.yellow;
        }
    }

    public enum ColorType
    {
        White,
        Black
    }

    public delegate void ClickEventHandler(BaseClickComponent component);
    public delegate void FocusEventHandler(CellComponent component, bool isSelect);
    public delegate void TriggerEnterEventHandler(Collider other);
}