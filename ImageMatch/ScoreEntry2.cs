using System;
using HashZipEntry = howto_image_hash.Form1.HashZipEntry;

namespace howto_image_hash
{
    public class ScoreEntry2
    {
        public int score;

        internal HashZipEntry F1 { get; set; }
        internal HashZipEntry F2 { get; set; }

        public override string ToString()
        {
            return string.Format(" {0} | {1} | {2}", score / 2, F1.InnerPath, F2.InnerPath);
        }

        public static int Comparer(ScoreEntry2 x, ScoreEntry2 y)
        {
            int val = x.score - y.score;
            if (val == 0)
                val = string.Compare(x.F1.InnerPath, y.F1.InnerPath, StringComparison.Ordinal); // same value: sort by name
            return val;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            ScoreEntry2 obj2 = obj as ScoreEntry2;
            if (obj2 == null) return false;

            // This considers "A vs B" to be equivalent to "B vs A"
            return (this.F1.InnerPath == obj2.F1.InnerPath && this.F2.InnerPath == obj2.F2.InnerPath) ||
                   (this.F2.InnerPath == obj2.F1.InnerPath && this.F1.InnerPath == obj2.F2.InnerPath);
        }

        public override int GetHashCode()
        {
            // This considers "A vs B" to be equivalent to "B vs A"
            return F1.InnerPath.GetHashCode() + F2.InnerPath.GetHashCode();
        }
    }
}
