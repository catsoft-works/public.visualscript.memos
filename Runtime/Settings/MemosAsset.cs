using VisualScript.Icons;

namespace VisualScript.Runtime.Memos
{
    public class MemosAsset : TAsset<MemosRepository>
    {
        public override IIcon Icon => new IconNote(ColorTheme.Type.TextLight);

        public override string Name => "Memos";
    }
}