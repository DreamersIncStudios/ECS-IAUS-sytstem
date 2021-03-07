using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace AISenses
{
    public struct AlertLevel : IComponentData
    {
        public int Alert { get { return m_alert; } set { m_alert = Mathf.Clamp(value, 0, 100); } }

        private int m_alert;

    }
}
