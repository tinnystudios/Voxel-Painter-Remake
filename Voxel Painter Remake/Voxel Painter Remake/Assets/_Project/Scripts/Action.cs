using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace core
{
    [System.Serializable]
    public class Action : IUnifiedContainer<IAction>
    {
        public Action(IAction iAction) {
            Result = iAction;
        }
    }
}
