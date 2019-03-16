
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FRG.Core
{
    public class PreseedSnapshotGenerator : ScriptableWizard
	{
		public string snapshotName = "Default";
        public List<PreseedSnapshot.PreseedInfo> preseedInfo = new List<PreseedSnapshot.PreseedInfo>();
        public bool referenceThisSnapshotForUseInGame = true;
        private bool initialized = false;

		public static void CreateWizard()
		{
			PreseedSnapshotGenerator wizzie = ScriptableWizard.DisplayWizard<PreseedSnapshotGenerator>( "Snapshot Generator", "Generate" );
			wizzie.minSize = new Vector2( 350, 500 );
		}

		void OnWizardUpdate()
		{
			if( !initialized )
			{
                Dictionary<int, int> pools = Pool.GetPreseedCounts();

                helpString = "Set the name of the snapshot.asset to be generated.";
				snapshotName = EditorPrefs.GetString( "LastPreseedSnapshotName", "Default" );

				initialized = true;

                foreach (KeyValuePair<int, int> kvp in pools)
				{
                    int prefabInstanceId = kvp.Key;
                    int count = kvp.Value;
                    
                    GameObject go = EditorUtility.InstanceIDToObject(prefabInstanceId) as GameObject;
                    if (go == null)
                    {
                        Debug.LogError("Could not find prefab with instance id " + prefabInstanceId.ToString());
                        continue;
                    }

                    AssetManagerRef amr = AssetManagerUtil.ReferenceAsset(go);
                    if (!amr.IsValid)
                    {
                        Debug.LogError("Could not reference asset at " + amr.ToString());
                        continue;
                    }

                    PreseedSnapshot.PreseedInfo newInfo = new PreseedSnapshot.PreseedInfo(amr, count, go.name);
					preseedInfo.Add( newInfo );
				}

				preseedInfo.Sort( Alphabetize );
			}
		}

		static int Alphabetize(PreseedSnapshot.PreseedInfo a, PreseedSnapshot.PreseedInfo b )
		{
            return PathUtil.NaturalCompareOrdinal(a.nameForSort, b.nameForSort);
		}

		void OnWizardCreate()
		{
			string directory = StandardEditorPaths.CoreData + "PreseedSnapshots/";
			string filename = snapshotName + "_snapshot.asset";

			string fullpath = directory + filename;

			EditorPrefs.SetString( "LastPreseedSnapshotName", snapshotName );

            PreseedSnapshot snapshot = ScriptableObject.CreateInstance<PreseedSnapshot>();
            snapshot.preseedInfo = preseedInfo.ToArray();

            PathUtil.CreateDirectoryRecursively(directory);
            AssetDatabase.CreateAsset(snapshot, fullpath);
            AssetDatabase.ImportAsset(fullpath);

            if (referenceThisSnapshotForUseInGame) {
                PreseedOptions.instance.snapshot = snapshot;
            }
		}

		[MenuItem("FRG/Editor/Take Preseed Snapshot", priority = 101)]
		static void TakePreseedSnapshot()
		{
			PreseedSnapshotGenerator.CreateWizard();
		}
	}
}
