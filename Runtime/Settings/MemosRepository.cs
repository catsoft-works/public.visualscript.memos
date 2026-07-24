using System;

namespace VisualScript.Runtime.Memos
{
    [Serializable] [Repository(ID)]
    public class MemosRepository : TRepository<MemosRepository>
    {
        private const string ID = "vs.memos";

        // REPOSITORY PROPERTIES: -----------------------------------------------------------------

        public override string Id => ID;

        // EDITOR METHODS: ------------------------------------------------------------------------

        #if UNITY_EDITOR

        [UnityEditor.InitializeOnEnterPlayMode]
        public static void InitializeOnEnterPlayMode() => Dispose();

        #endif
    }
}