using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using HashZipEntry = howto_image_hash.Form1.HashZipEntry;

namespace howto_image_hash
{
    public partial class CompareForm : Form
    {
        private Logger _log;
        private ArchiveLoader _loader;

        public CompareForm(Logger log, ArchiveLoader loader)
        {
            _log = log;
            _loader = loader;
            InitializeComponent();
        }

        private void imageStat(PictureBox ctl, Label ctl2)
        {
            var info1 = new FileInfo(ctl.ImageLocation);
            var size1 = ctl.Image.Size;
            ctl2.Text = string.Format("{0},{1} [{2:0.00}K]", size1.Width, size1.Height, (double)info1.Length / 1024.0);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var sel = listBox1.SelectedItem as ScoreEntry;
            if (sel == null)
                return;

            return; // TODO
#if false
            var f1 = sel.F1.path;
            var f2 = sel.F2.path;

            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.ImageLocation = _loader.Extract(sel.F1.ZipFile, sel.F1.InnerPath);
            pictureBox2.ImageLocation = _loader.Extract(sel.F2.ZipFile, sel.F2.InnerPath);

            try
            {
                pictureBox1.Load();
                imageStat(pictureBox1, label1);
            }
            catch
            {

            }
            try
            {
                pictureBox2.Load();
                imageStat(pictureBox2, label2);
            }
            catch
            {

            }
#endif
        }

        private void DoShowDiff(bool stretch)
        {
            //var sel = listBox1.SelectedItem as ScoreEntry;
            //if (sel == null)
            //    return;
            //if (_diffDlg == null)
            //{
            //    _diffDlg = new ShowDiff(_loader,_log) { Owner = this };
            //}
            //_diffDlg.Stretch = stretch;
            //_diffDlg.Group = sel;
            //_diffDlg.ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DoShowDiff(false);
        }

        private List<HashZipEntry> _toCompare = new List<HashZipEntry>();

        private string _pathHistory;

        private int _hashSource; // track distinct hashfiles so can filter by file


        BackgroundWorker worker2;
        ConcurrentBag<ScoreEntry> _scores; // for parallelism
        List<ScoreEntry> _scoreList; // for viewing

        private void CompareAsync()
        {
            _log.logTimer("compareZ", true);

            _scores = new ConcurrentBag<ScoreEntry>();

            if (worker2 == null)
            {
                worker2 = new BackgroundWorker();
                worker2.DoWork += comp_dowork;
                worker2.ProgressChanged += comp_ProgressChanged;
                worker2.WorkerReportsProgress = true;
                worker2.RunWorkerCompleted += comp_RunWorkerCompleted;
            }

            progressBar1.Value = 0;
            worker2.RunWorkerAsync();
        }

        private void comp_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar1.Value = 0;

            _scoreList = _scores.ToList();
            _scores = null;

            _scoreList.Sort(ScoreEntry.Comparer);

            LoadList();
            _log.logTimer("compareZ_rwc");
        }

        private void LoadList()
        {
            FilterMatchingTree();

            listBox1.DataSource = _viewList;
            listBox1.SelectedIndex = -1;
            if (_viewList.Count > 0)
                listBox1.SelectedIndex = 0;
        }

        private void comp_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var val = Math.Max(Math.Min(e.ProgressPercentage, 100),0);
            progressBar1.Value = val;
        }

        private void comp_dowork(object sender, DoWorkEventArgs e)
        {
            // TODO consider converting _toCompare to an array?
            // TODO consider Task.Factory, not BackgroundWorker

            var ziplist = _zipDict.Keys.ToArray();
            int zipCount = ziplist.Length;
            if (zipCount < 1)
                return;

            int totCount = (int)(Math.Pow(zipCount, 2) / 2);
            int donCount = 0;
            int maxCount = zipCount - 1;
            int listCount = zipCount;
            System.Threading.Tasks.Parallel.For(0, maxCount, dex =>
            {
                var zipFiles1 = _zipDict[ziplist[dex]].ToList();
                for (int dex2 = dex + 1; dex2 < listCount; dex2++)
                {
                    var zipFiles2 = _zipDict[ziplist[dex2]].ToList();
                    int matches = CalcScore(zipFiles1, zipFiles2);
                    
                    int score;
                    score = (int)(((double)matches / zipFiles1.Count) * 100.0);

                    donCount++;
                    if (donCount % 10 == 0)
                    {
                        double perc = 100.0 * donCount / totCount;
                        worker2.ReportProgress((int)perc);
                    }

                    if (score != 0)
                    {
                        ScoreEntry se = new ScoreEntry();
                        se.zipfile1 = ziplist[dex];
                        se.zip1count = zipFiles1.Count;
                        se.zipfile2 = ziplist[dex2];
                        se.zip2count = zipFiles2.Count;
                        se.score = score;
                        _scores.Add(se);
                    }
                }
            });
        }

        private int CalcScoreP(HashZipEntry fd1, HashZipEntry fd2)
        {
            return ham_dist(fd1.phash, fd2.phash);
        }

        private int CalcScore(List<HashZipEntry> ziplist1, List<HashZipEntry> ziplist2)
        {
            // Calculate the number of matching files in zip1 vs zip2
            int match_count = 0;
            int unmatch_count = 0;
            for (int dex1 = 0; dex1 < ziplist1.Count; dex1++)
            {
                var hze1 = ziplist1[dex1];
                bool match = false;
                for (int dex2 = 0; dex2 < ziplist2.Count; dex2++)
                {
                    var hze2 = ziplist2[dex2];

                    var ascore = CalcScoreP(hze1, hze2);
                    if (ascore < 10)
                    {
                        match = true;
                        break; // found a match, don't need to keep testing
                    }
                }
                if (match)
                    match_count++;
                else
                    unmatch_count++;
            }

            return match_count;
        }

        public static int ham_dist(ulong hash1, ulong hash2)
        {
            ulong x = hash1 ^ hash2;

            const ulong m1 = 0x5555555555555555UL;
            const ulong m2 = 0x3333333333333333UL;
            const ulong h01 = 0x0101010101010101UL;
            const ulong m4 = 0x0f0f0f0f0f0f0f0fUL;

            x -= (x >> 1) & m1;
            x = (x & m2) + ((x >> 2) & m2);
            x = (x + (x >> 4)) & m4;
            return (int)((x * h01) >> 56);
        }


        private int CountBitsSet(uint v)
        {
            uint c;
            c = (v & 0x55555555) + ((v >> 1) & 0x55555555);
            c = (c & 0x33333333) + ((c >> 2) & 0x33333333);
            c = (c & 0x0F0F0F0F) + ((c >> 4) & 0x0F0F0F0F);
            c = (c & 0x00FF00FF) + ((c >> 8) & 0x00FF00FF);
            c = (c & 0x0000FFFF) + ((c >> 16) & 0x0000FFFF);
            return (int)c;
        }

        private int CountBitsSet(ulong v)
        {
            uint v1 = (uint)(v & 0xFFFFFFFF);
            uint v2 = (uint)(v >> 32);
            return CountBitsSet(v1) + CountBitsSet(v2);
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            var fbd = new FolderBrowserDialog();
            if (!string.IsNullOrEmpty(_pathHistory))
                fbd.SelectedPath = _pathHistory;
            fbd.ShowNewFolderButton = false;
            if (fbd.ShowDialog() != DialogResult.OK)
                return;
            var path = fbd.SelectedPath;
            _pathHistory = path;

            //var toload1 = Path.Combine(path, "htih.txt");
            var toload2 = Path.Combine(path, "htih_pz.txt");
            if (!File.Exists(toload2))
            {
                MessageBox.Show("Folder has not been zip hashed!");
                return;
            }

            ClearForLoad();

            _hashSource++; // new file loaded

            using (var sr = new StreamReader(toload2))
            {
                while (sr.Peek() >= 0)
                {
                    var line = sr.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var parts = line.Split('|');
                    var he = new HashZipEntry();
                    he.ZipFile = parts[0];
                    he.InnerPath = parts[1];
                    he.phash = ulong.Parse(parts[2]);
                    he.source = _hashSource;
                    _toCompare.Add(he);

                    // Need a set of ZIPs to compare
                    if (_zipDict.ContainsKey(he.ZipFile))
                    {
                        var filelist = _zipDict[he.ZipFile];
                        filelist.Add(he);
                    }
                    else
                    {
                        var filelist = new ConcurrentBag<HashZipEntry>();
                        filelist.Add(he);
                        _zipDict[he.ZipFile] = filelist;
                    }
                }
            }

            _log.log(string.Format("to compare - Zips:{0} Files:{1}", _zipDict.Keys.Count, _toCompare.Count));

            CompareAsync();
        }

        private ConcurrentDictionary<string, ConcurrentBag<HashZipEntry>> _zipDict = new ConcurrentDictionary<string, ConcurrentBag<HashZipEntry>>();

        private void ClearForLoad()
        {
            listBox1.DataSource = null;
            _viewList = null;
            pictureBox1.Image = null;
            pictureBox2.Image = null;
            label1.Text = "";
            label2.Text = "";
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearForLoad();
            _toCompare = new List<HashZipEntry>();
            _zipDict = new ConcurrentDictionary<string, ConcurrentBag<HashZipEntry>>();
        }

        private bool _filterSameTree;
        private List<ScoreEntry> _viewList; // possibly filtered list

        private void FilterMatchingTree()
        {
//            _viewList = _scoreList;
//#if false
            if (!_filterSameTree && hide_left.Count < 1 && hide_right.Count < 1)
                _viewList = _scoreList;
            else
            {
                _viewList = new List<ScoreEntry>();
                foreach (var entry in _scoreList)
                {
                    if (hide_left.Contains(entry.zipfile1))
                        continue;
//                  if (entry.F1.source == entry.F2.source && _filterSameTree)
//                        continue;

                    _viewList.Add(entry);
                }
            }
//#endif
        }

        private void btnFilterTree_Click(object sender, EventArgs e)
        {
            _filterSameTree = !_filterSameTree;
            LoadList();
        }

        private void btnDiffStretchL_Click(object sender, EventArgs e)
        {
            DoShowDiff(true);
        }

        private void btnClip_Click(object sender, EventArgs e)
        {
            var sel = listBox1.SelectedItem as ScoreEntry;
            if (sel == null)
                return;
            string text = sel.ToString();
            Clipboard.SetText(text);
        }

        private List<string> hide_left = new List<string>();
        private List<string> hide_right = new List<string>();

        private void btnHideL_Click(object sender, EventArgs e)
        {
            var sel = listBox1.SelectedItem as ScoreEntry;
            if (sel == null)
                return;
            if (!hide_left.Contains(sel.zipfile1))
                hide_left.Add(sel.zipfile1);
            LoadList();
        }
    }
}
