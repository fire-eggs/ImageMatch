using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using HashZipEntry = howto_image_hash.Form1.HashZipEntry;

#pragma warning disable VSD0023 // {0} blocks should use braces to denote start and end.
#pragma warning disable VSD0010 // Warns when an exception catch block is empty.

namespace howto_image_hash
{
    public abstract class MasterDetailBase : Form
    {
        private Logger _log;
        private ArchiveLoader _loader;
        private int _hashSource;
        private List<HashZipEntry> _toCompare = new List<HashZipEntry>();
        private ConcurrentDictionary<string, ConcurrentBag<HashZipEntry>> _zipDict = new ConcurrentDictionary<string, ConcurrentBag<HashZipEntry>>();
        private bool _filterSameTree;
        private List<ScoreEntry> _viewList; // possibly filtered list
        BackgroundWorker _worker2;
        ConcurrentBag<ScoreEntry> _scores; // for parallelism
        List<ScoreEntry> _scoreList; // for viewing
        private string _pathmemory;
        private List<string> _hideLeft = new List<string>();
        private List<string> _hideRight = new List<string>();
        private List<string> _toCleanup = new List<string>();

        public MasterDetailBase(Logger log, ArchiveLoader loader)
        {
            _log = log;
            _loader = loader;
        }

        internal void setPix(ScoreEntry2 sel, bool first, PictureBox pbox, Label plab)
        {
            if (sel == null)
                return;

            string zipF;
            string fi;
            if (first)
            {
                zipF = sel.F1 == null ? "" : sel.F1.ZipFile;
                fi = sel.F1 != null ? sel.F1.InnerPath : "";
            }
            else
            {
                zipF = sel.F2 == null ? "" : sel.F2.ZipFile;
                fi = sel.F2 != null ? sel.F2.InnerPath : "";
            }

            if (string.IsNullOrEmpty(zipF) || string.IsNullOrEmpty(fi))
            {
                pbox.Image = null;
                plab.Text = "";
                return;
            }

            var imgF = _loader.Extract(zipF, fi);
            if (!string.IsNullOrEmpty(imgF)) // clean up all created temp files on close
                _toCleanup.Add(imgF);

            try
            {
                if (string.IsNullOrEmpty(imgF))
                {
                    pbox.Image = null;
                    plab.Text = "";
                    return;
                }

                // load image to picturebox with no file lock
                pbox.Image = Image.FromStream(new MemoryStream(File.ReadAllBytes(imgF))); // no file lock

                // set image stats to label
                var info1 = new FileInfo(imgF);
                var size1 = pbox.Image.Size;
                plab.Text = string.Format("{0},{1} [{2:0.00}K]", size1.Width, size1.Height, (double)info1.Length / 1024.0);
            }
            catch
            {

            }
        }

        internal List<ScoreEntry2> selectZipPair(ScoreEntry sel)
        {
            if (sel == null)
                return null;

            SetNote(sel.Note);

            var list1 = _zipDict[sel.zipfile1].ToList();
            var list2 = _zipDict[sel.zipfile2].ToList();
            var detail = MakeDetailList(list1, list2);
            detail.Sort(ScoreEntry2.Comparer);

            return detail;
        }

        private List<ScoreEntry2> MakeDetailList(List<HashZipEntry> ziplist1, List<HashZipEntry> ziplist2)
        {
            List<ScoreEntry2> retlist = new List<ScoreEntry2>();

            bool[] rightMatch = new bool[ziplist2.Count];

            // Make a list of matching files in zip1 vs zip2
            for (int dex1 = 0; dex1 < ziplist1.Count; dex1++)
            {
                var hze1 = ziplist1[dex1];

                bool matched = false;
                for (int dex2 = 0; dex2 < ziplist2.Count; dex2++)
                {
                    var hze2 = ziplist2[dex2];

                    var ascore = CalcScoreP(hze1, hze2);
                    if (ascore < 22)
                    {
                        ScoreEntry2 se = new ScoreEntry2();
                        se.F1 = hze1;
                        se.F2 = hze2;
                        se.score = ascore;
                        retlist.Add(se);
                        matched = true;
                        rightMatch[dex2] = true;
                        break;
                    }
                }

                if (!matched)
                {
                    ScoreEntry2 se = new ScoreEntry2();
                    se.F1 = hze1;
                    se.score = 999 * 2;
                    retlist.Add(se);
                }
            }

            for (int i = 0; i < rightMatch.Length; i++)
                if (!rightMatch[i])
                {
                    ScoreEntry2 se = new ScoreEntry2();
                    se.F2 = ziplist2[i];
                    se.score = 999 * 2;
                    retlist.Add(se);
                }

            return retlist;
        }

        internal void loadHashFile()
        {
            var fbd = new FolderBrowserDialog();
            fbd.ShowNewFolderButton = false;
            if (!string.IsNullOrEmpty(_pathmemory))
                fbd.SelectedPath = _pathmemory;
            if (fbd.ShowDialog() != DialogResult.OK)
                return;
            var path = fbd.SelectedPath;
            _pathmemory = path;

            //var toload1 = Path.Combine(path, "htih.txt");
            var toload2 = Path.Combine(path, "htih_pz.txt");
            if (!File.Exists(toload2))
            {
                MessageBox.Show("Folder has not been zip hashed!");
                return;
            }

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

        private void CompareAsync()
        {
            _log.logTimer("compareZ", true);

            _scores = new ConcurrentBag<ScoreEntry>();

            if (_worker2 == null)
            {
                _worker2 = new BackgroundWorker();
                _worker2.DoWork += comp_dowork;
                _worker2.ProgressChanged += comp_ProgressChanged;
                _worker2.WorkerReportsProgress = true;
                _worker2.RunWorkerCompleted += comp_RunWorkerCompleted;
            }

            updateProgress(0);
            _worker2.RunWorkerAsync();
        }

        private void comp_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var val = Math.Max(Math.Min(e.ProgressPercentage, 100), 0);
            updateProgress(val);
        }

        private void comp_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            updateProgress(0);

            _scoreList = _scores.ToList();
            _scores = null;

            _scoreList.Sort(ScoreEntry.Comparer);

            LoadList();
            _log.logTimer("compareZ_rwc");
        }

        private int CalcScoreP(HashZipEntry fd1, HashZipEntry fd2)
        {
            return CompareForm.ham_dist(fd1.phash, fd2.phash);
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
            Parallel.For(0, maxCount, dex =>
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
                        _worker2.ReportProgress((int)perc);
                    }

                    if (score != 0)
                    {
                        ScoreEntry se = new ScoreEntry();
                        se.zipfile1 = ziplist[dex];
                        se.zip1count = zipFiles1.Count;
                        se.zipfile2 = ziplist[dex2];
                        se.zip2count = zipFiles2.Count;
                        se.score = score;
                        se.sameSource = zipFiles1[0].source == zipFiles2[0].source;
                        _scores.Add(se);
                    }
                }
            });
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

        internal void Reset()
        {
            _toCompare = new List<HashZipEntry>();
            _zipDict = new ConcurrentDictionary<string, ConcurrentBag<HashZipEntry>>();
        }

        internal List<ScoreEntry> FilterMatchingTree()
        {
            _viewList = new List<ScoreEntry>();
            foreach (var entry in _scoreList)
            {
                if (entry.sameSource && _filterSameTree)
                    continue;

                if (_hideLeft.Contains(entry.zipfile1))
                    continue;

                if (_hideRight.Contains(entry.zipfile2))
                    continue;

                if (entry.score < 10)
                    continue; // TODO need to examine more closely; why is hentairead/C have so many 'dups'?

                _viewList.Add(entry);
            }
            return _viewList;
        }

        internal void ToggleFilter()
        {
            _filterSameTree = !_filterSameTree;
            LoadList();
        }

        private string separator;

        internal void report(StreamWriter sw, ScoreEntry se)
        {
            if (separator == null)
                separator = new string('-', 50);
            sw.WriteLine("{0,3} - {1}", se.score, se.status());
            sw.WriteLine("L: ({0,3}) {1}", se.zip1count, se.zipfile1);
            sw.WriteLine("R: ({0,3}) {1}", se.zip2count, se.zipfile2);
            if (!string.IsNullOrWhiteSpace(se.Note))
                sw.WriteLine(se.Note);
            sw.WriteLine(separator);
        }

        internal void Report()
        {
            var ofd = new SaveFileDialog();
            if (!string.IsNullOrEmpty(_pathmemory))
                ofd.InitialDirectory = _pathmemory;
            if (DialogResult.OK != ofd.ShowDialog())
                return;
            var fn = ofd.FileName;
            _pathmemory = Path.GetDirectoryName(fn);

            using (var sw = new StreamWriter(fn, false))
                foreach (var se in _viewList)
                    report(sw, se);
        }

        internal void HideLeft(ScoreEntry sel)
        {
            if (sel == null)
                return;
            if (!_hideLeft.Contains(sel.zipfile1))
                _hideLeft.Add(sel.zipfile1);
            LoadList();
        }

        internal void HideRight(ScoreEntry sel)
        {
            if (sel == null)
                return;
            if (!_hideRight.Contains(sel.zipfile2))
                _hideRight.Add(sel.zipfile2);
            LoadList();
        }

        internal void AddNote(ScoreEntry sel, string note)
        {
            if (sel == null)
                return;
            sel.Note = note;
        }

        internal void Cleanup()
        {
            // delete any temp files
            foreach (var afile in _toCleanup)
            {
                try
                {
                    File.Delete(afile);
                }
                catch
                {
                    // TODO file still locked?
                }
            }
        }            

        public abstract void updateProgress(int value);
        public abstract void LoadList();

        public abstract void SetNote(string text);
    }
}
