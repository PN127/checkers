using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Checkers
{
    public class CellComponent : BaseClickComponent
    {
        private Dictionary<NeighborType, CellComponent> _neighbors = new Dictionary<NeighborType, CellComponent>();
        private string _alphabet;
        
        /// <summary>
        /// Возвращает соседа клетки по указанному направлению
        /// </summary>
        /// <param name="type">Перечисление направления</param>
        /// <returns>Клетка-сосед или null</returns>
        public CellComponent GetNeighbors(NeighborType type)
        {
            if (_neighbors[type] == null) return null;
            return _neighbors[type];
        }

        protected override void Start()
        {
            base.Start();
            _alphabet = "ABCDEFGH";
            if(GetColor == ColorType.Black)
                CompletionNeighbors(gameObject.name);
        }

        protected void CompletionNeighbors(string name)
        {
            char letter = Convert.ToChar(name.Substring(0, 1));
            int numbLetter = _alphabet.IndexOf(letter);
            int number = Convert.ToInt32(name.Substring(1, 1));

            if (numbLetter > 0 && number < 8)
            {
                string neighborName = Convert.ToString(_alphabet[numbLetter - 1]) + Convert.ToString((number + 1));
                CellComponent SoughtObjectCell = GameObject.Find(neighborName).GetComponent<CellComponent>();
                _neighbors.Add(NeighborType.TopLeft, SoughtObjectCell);
            }
            else
                _neighbors.Add(NeighborType.TopLeft, null);

            if (numbLetter < 7 && number < 8)
            {
                string neighborName = Convert.ToString(_alphabet[numbLetter + 1]) + Convert.ToString((number + 1));
                CellComponent SoughtObjectCell = GameObject.Find(neighborName).GetComponent<CellComponent>();
                _neighbors.Add(NeighborType.TopRight, SoughtObjectCell);
            }
            else
                _neighbors.Add(NeighborType.TopRight, null);

            if (numbLetter > 0 && number > 1)
            {
                string neighborName = Convert.ToString(_alphabet[numbLetter - 1]) + Convert.ToString((number - 1));
                CellComponent SoughtObjectCell = GameObject.Find(neighborName).GetComponent<CellComponent>();
                _neighbors.Add(NeighborType.BottomLeft, SoughtObjectCell);
            }
            else
                _neighbors.Add(NeighborType.BottomLeft, null);

            if (numbLetter < 7 && number > 1)
            {
                string neighborName = Convert.ToString(_alphabet[numbLetter + 1]) + Convert.ToString((number - 1));
                CellComponent SoughtObjectCell = GameObject.Find(neighborName).GetComponent<CellComponent>();
                _neighbors.Add(NeighborType.BottomRight, SoughtObjectCell);
            }
            else
                _neighbors.Add(NeighborType.BottomRight, null);
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            CallBackFocusEvent(this, true);

        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            CallBackFocusEvent(this, false);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            CallBackClickEvent(this);
        }
    }

    /// <summary>
    /// Тип соседа клетки
    /// </summary>
    public enum NeighborType : byte
    {
        /// <summary>
        /// Верхняя левая клетка от данной
        /// </summary>
        TopLeft,
        /// <summary>
        /// Верхняя правая клетка от данной
        /// </summary>
        TopRight,
        /// <summary>
        /// Нижняя левая клетка от данной
        /// </summary>
        BottomLeft,
        /// <summary>
        /// Нижняя правая клетка от данной
        /// </summary>
        BottomRight
    }
}