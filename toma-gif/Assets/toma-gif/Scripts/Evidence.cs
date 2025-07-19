using System;
using System.Collections.Generic;
using UnityEngine;

namespace tomagif
{
    [CreateAssetMenu(fileName = "Evidence", menuName = "tomagif/Evidence")]
    public class Evidence : ScriptableObject
    {
        [field: SerializeField]
        public List<string> PositiveEvidences { get; private set; }

        [field: SerializeField]
        public List<string> NegativeEvidences { get; private set; }

        [field: SerializeField]
        public List<Element> Messages { get; private set; }

        [Serializable]
        public class Element
        {
            [field: SerializeField]
            public string Message { get; private set; }

            [field: SerializeField]
            public bool IsPositive { get; private set; }
        }
    }
}
