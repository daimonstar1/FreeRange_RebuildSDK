
using System;
using UnityEngine;

namespace FRG.Core
{

    public class PreseedSnapshot : ScriptableObject {
        public PreseedInfo[] preseedInfo = ArrayUtil.Empty<PreseedInfo>();

        [Serializable]
        public class PreseedInfo
        {
            public AssetManagerRef reference;
            public int numberOfPreseeds;
            [InspectorHide]
            public string nameForSort;

            public PreseedInfo()
                : this(new AssetManagerRef(), 1, "")
            {
            }

            public PreseedInfo(AssetManagerRef reference, int numberOfPreseeds, string nameForSort)
            {
                this.reference = reference;
                this.numberOfPreseeds = numberOfPreseeds;
                this.nameForSort = nameForSort;
            }
        }
    }

}