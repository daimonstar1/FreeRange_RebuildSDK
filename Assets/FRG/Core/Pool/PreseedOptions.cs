using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace FRG.Core
{
    public class PreseedOptions : ScriptableObject {
        public PreseedSnapshot snapshot = null;

        public static PreseedOptions instance { get { return ServiceLocator.ResolveAsset<PreseedOptions>(); } }
    }
}
