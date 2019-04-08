using System;

namespace howto_image_hash
{
    public class ScoreEntry
    {
        public int score;  // percent overlap: zip1 vs zip2

        public string zipfile1;
        public int zip1count;
        public string zipfile2;
        public int zip2count;

        public bool sameSource; // zip1 & zip2 from same source
        public string Note;

        public string status()
        {
            string status;
            if (score == 100)
            {
                if (zip1count == zip2count)
                    status = "Match";
                else if (zip2count > zip1count)
                    status = "R holds L";
                else status = "huh?";
            }
            else
            {
                if (zip1count == zip2count)
                    status = "Mixed";
                else if (zip1count < zip2count)
                    status = "R holds L";
                else
                    status = "L holds R";
            }
            return status;
        }

        public override string ToString()
        {
            // used to display in the listbox
            return string.Format(" {0,3} | {5,10} | ({1}) {2} | ({3}) {4}", score, zip1count, zipfile1, zip2count, zipfile2, status());
        }

        public static int Comparer(ScoreEntry x, ScoreEntry y)
        {
            int val = y.score - x.score;
            if (val == 0)
                val = string.Compare(x.zipfile1, y.zipfile1, StringComparison.Ordinal); // same value: sort by name
            return val;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            ScoreEntry obj2 = obj as ScoreEntry;
            if (obj2 == null) return false;

            // This considers "A vs B" to be equivalent to "B vs A"
            return (this.zipfile1 == obj2.zipfile1 && this.zipfile2 == obj2.zipfile2) ||
                   (this.zipfile2 == obj2.zipfile1 && this.zipfile1 == obj2.zipfile2);
        }

        public override int GetHashCode()
        {
            // This considers "A vs B" to be equivalent to "B vs A"
            return zipfile1.GetHashCode() + zipfile2.GetHashCode();
        }
    }
}
