using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Checkers
{
    public class ChipComponent : BaseClickComponent
    {
        protected override void Start()
        {
            base.Start();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            if(Pair != null)
                Pair.OnPointerEnter(eventData);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (Pair != null)
                Pair.OnPointerExit(eventData);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (Pair != null)
                CallBackClickEvent(this);            
        }

        private void OnTriggerEnter(Collider other)
        {
            CallBackOnTriggerEnter(other);
        }

    }
}
