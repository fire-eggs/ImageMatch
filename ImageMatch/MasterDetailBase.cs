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

namespace howto_image_hash
{
    public class MasterDetailBase : Form
    {
        private const int MAX_SCORE = 16;  // threshold for distance between 2 files to count as a 'match'
        private const int MAX_SCORE2 = 20; // not quite sure what this is?

        private Logger _log;
        private ArchiveLoader _loader;
        private int _hashSource; // track distinct hashfiles
        private List<HashZipEntry> _toCompare = new List<HashZipEntry>();
        private ConcurrentDictionary<string, ConcurrentBag<HashZipEntry>> _zipDict = new ConcurrentDictionary<string, ConcurrentBag<HashZipEntry>>();
        protected bool _filterSameTree;
        private List<ScoreEntry> _viewList; // possibly filtered list
        BackgroundWorker _worker2;
        HashSet<ScoreEntry> _scores;
        List<ScoreEntry> _scoreList; // for viewing
        private string _pathmemory;
        private List<string> _hideLeft = new List<string>();
        private List<string> _hideRight = new List<string>();
        private List<string> _toCleanup = new List<string>();
        private ShowDiff _diffDlg;

        [Obsolete("Designer only", true)]
        public MasterDetailBase()
        {
        }

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
                int bestscore = 99;
                ScoreEntry2 bestmatch = null;

                for (int dex2 = 0; dex2 < ziplist2.Count; dex2++)
                {
                    var hze2 = ziplist2[dex2];

                    var ascore = CalcScoreP(hze1, hze2);

                    // TODO would a VP-tree be a faster solution? [create a VPtree for ziplist1 and ziplist2 ONLY]
                    if (ascore < MAX_SCORE2 && ascore < bestscore)
                    {
                        bestmatch = new ScoreEntry2();
                        bestmatch.F1 = hze1;
                        bestmatch.F2 = hze2;
                        bestmatch.score = ascore;
                        matched = true;
                        rightMatch[dex2] = true;
                        bestscore = ascore;
                    }
                }

                if (!matched || bestmatch == null)
                {
                    ScoreEntry2 se = new ScoreEntry2();
                    se.F1 = hze1;
                    se.score = 999 * 2;
                    retlist.Add(se);
                }
                else
                {
                    retlist.Add(bestmatch);
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

        internal string loadHashFile()
        {
            var fbd = new FolderBrowserDialog();
            fbd.ShowNewFolderButton = false;
            if (!string.IsNullOrEmpty(_pathmemory))
                fbd.SelectedPath = _pathmemory;
            if (fbd.ShowDialog() != DialogResult.OK)
                return null;
            var path = fbd.SelectedPath;
            loadHashFile(path);
            return path;
        }

        internal void loadHashFile(string path)
        {
            _log.log(string.Format("Loading {0}", path));

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

            _log.log(string.Format(" to compare - Zips:{0} Files:{1}", _zipDict.Keys.Count, _toCompare.Count));

            //CompareAsync();
            CompareVPTree();
        }

        private void CompareVPTree()
        {
            var ziplist = _zipDict.Keys.ToArray();
            int zipCount = ziplist.Length;
            if (zipCount < 1)
                return;

            SetStatus(string.Format("Hashes: {0} Archives: {1}", _hashSource, zipCount));

            _scores = new HashSet<ScoreEntry>(); // use a set so that AxB and BxA are not duplicated

            var tree = new VPTree<HashZipEntry>(CalcScoreP);
            var root = tree.make_vp(_toCompare);
            var ret = new List<HashZipEntry>();
            var thisfilematches = new HashSet<string>();
            var filesdone = new HashSet<HashZipEntry>();
            var zipsdone = new HashSet<string>();

            updateProgress(0);
            int doneCount = 0;

            var pairset = new HashSet<ScoreEntry2>();
            foreach (var azip in ziplist)
            {
                var filelist = _zipDict[azip];
                foreach (var afile in filelist)
                {
                    tree.query_vp(root, afile, 1, ret);

                    foreach (var aret in ret)
                    {
                        if (aret == afile)  // skip self
                            continue;
                        if (aret.ZipFile == afile.ZipFile) // skip self-zip matches
                            continue;
                        int dist = CalcScoreP(afile, aret); // reduce 'noise' by tossing too-distant matches
                        if (dist > MAX_SCORE)
                            continue;

                        ScoreEntry2 se2 = new ScoreEntry2();
                        se2.F1 = afile;
                        se2.F2 = aret;
                        se2.score = dist;
                        pairset.Add(se2);
                    }

                    ret.Clear();
                }



            //foreach (var azip in ziplist)
            //{
            //    zipsdone.Add(azip);
            //    var filelist = _zipDict[azip];
            //    var matchlist = new Dictionary<string, int>();

            //    foreach (var comp in filelist)
            //    {
            //        filesdone.Add(comp);

            //        tree.query_vp(root, comp, 1, ret);

            //        //int selfdups = ret.Where(x => x.ZipFile == azip).Count();
            //        //if (selfdups < 2)
            //            foreach (var aret in ret)
            //            {
            //                if (zipsdone.Contains(aret.ZipFile))
            //                    continue;
            //                if (filesdone.Contains(aret))
            //                    continue;
            //                thisfilematches.Add(aret.ZipFile);
            //            }

            //        ret.Clear();

            //        foreach (var zipmatch in thisfilematches)
            //            if (zipmatch != azip)
            //                if (matchlist.ContainsKey(zipmatch))
            //                    matchlist[zipmatch]++;
            //                else
            //                    matchlist.Add(zipmatch, 1);
            //    }

            //    thisfilematches.Clear();
                    //if (ret.Count > 1) // TODO won't this always be true [as 'comp' is in the tree and will match]
                    //{
                    //    foreach (var aret in ret)
                    //    {
                    //        // ignore a match against self or a match against self-zip
                    //        if (aret.Equals(comp) || aret.ZipFile == comp.ZipFile)
                    //            continue;

                    //        // each aret may be from a distinct zip
                    //        // need to turn into a set of zip+match counts
                    //        if (matchlist.ContainsKey(aret.ZipFile))
                    //        {
                    //            if (newfile)
                    //                matchlist[aret.ZipFile]++;
                    //            //newfile = false;
                    //        }
                    //        else
                    //        {
                    //            matchlist.Add(aret.ZipFile, 1);
                    //        }
                    //    }
                    //}

//                    ret.Clear();
//                }

                //// build ScoreEntry list based on number of matches for azip against other zips
                //foreach (var amatch in matchlist)
                //{
                //    string who = amatch.Key;
                //    int matches = amatch.Value;
                //    var zip2 = _zipDict[who];

                //    int score1 = (int)(((double)matches / filelist.Count) * 100.0);
                //    int score2 = (int)(((double)matches / zip2.Count) * 100.0);
                //    int score = Math.Max(score1, score2);

                //    //System.Diagnostics.Debug.Assert(score <= 100.0);

                //    if (score > 20)
                //    {
                //        ScoreEntry se = new ScoreEntry();
                //        se.zipfile1 = azip;
                //        se.zip1count = filelist.Count;
                //        se.zipfile2 = who;
                //        se.zip2count = zip2.Count;
                //        se.score = score;
                //        se.sameSource = filelist.First().source == zip2.First().source;

                //        _scores.Add(se);
                //    }
                //}

                doneCount++;
                if (doneCount % 5 == 0)
                {
                    int perc = (int)(100.0 * doneCount / zipCount);
                    updateProgress(perc);
                }
            }

            // Turn pairset into _scores
            var pairlist = pairset.ToList();
            _log.log(string.Format(" pair candidates:{0}", pairlist.Count));

            if (pairlist.Count != 0)
            {

                int matches = 0;
                HashZipEntry he = pairlist[0].F1;
                HashZipEntry he2 = pairlist[0].F2;
                foreach (var apair in pairlist)
                {
                    if (apair.F1.ZipFile == he.ZipFile)
                    {
                        if (apair.F2.ZipFile == he2.ZipFile)
                        {
                            matches++;
                        }
                        else
                        {
                            MakeScore(matches, he, he2);
                            he2 = apair.F2;
                            matches = 1;
                        }
                    }
                    else
                    {
                        MakeScore(matches, he, he2);
                        he = apair.F1;
                        he2 = apair.F2;
                        matches = 1;
                    }
                }

                // 20190426 the last entry was not processed as a possible candidate
                MakeScore(matches, he, he2);
            }

            updateProgress(0);
            _scoreList = _scores.ToList();
            _log.log(string.Format(" zip matches:{0}", _scoreList.Count));

            _scores = null;

            _scoreList.Sort(ScoreEntry.Comparer);

            LoadZipList();
        }

        private void MakeScore(int matches, HashZipEntry he1, HashZipEntry he2)
        {
            var zip1 = _zipDict[he1.ZipFile];
            var zip2 = _zipDict[he2.ZipFile];

            var foo = MakeDetailList(zip1.ToList(), zip2.ToList());
            int brutematches = 0;
            foreach (var bar in foo)
                if (bar.score < MAX_SCORE)
                    brutematches++;

            int score1 = (int)(((double)matches / zip1.Count) * 100.0);
            int score2 = (int)(((double)matches / zip2.Count) * 100.0);
            int score = Math.Max(score1, score2);

            int bscore1 = (int)(((double)brutematches / zip1.Count) * 100.0);
            int bscore2 = (int)(((double)brutematches / zip2.Count) * 100.0);
            int bscore = Math.Max(bscore1, bscore2);

            if (bscore > 20)
            {
                ScoreEntry se = new ScoreEntry();
                se.zipfile1 = he1.ZipFile;
                se.zip1count = zip1.Count;
                se.zipfile2 = he2.ZipFile;
                se.zip2count = zip2.Count;
                se.score = bscore;
                se.sameSource = zip1.First().source == zip2.First().source;

                _scores.Add(se);
            }

        }

        private void CompareAsync()
        {
            _log.logTimer("compareZ", true);

            _scores = new HashSet<ScoreEntry>();

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

            LoadZipList();
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

                    // when "R holds L", the score should be the % of matches against the L
                    // when "L holds R", the score should be the % of matches against the R
                    int score1 = (int)(((double)matches / zipFiles1.Count) * 100.0);
                    int score2 = (int)(((double)matches / zipFiles2.Count) * 100.0);
                    int score = Math.Max(score1, score2);

                    // progress bar update
                    donCount++;
                    if (donCount % 10 == 0)
                    {
                        double perc = 100.0 * donCount / totCount;
                        _worker2.ReportProgress((int)perc);
                    }

                    if (score > 5)
                    {
                        ScoreEntry se = new ScoreEntry();
                        se.zipfile1 = ziplist[dex];
                        se.zip1count = zipFiles1.Count;
                        se.zipfile2 = ziplist[dex2];
                        se.zip2count = zipFiles2.Count;
                        se.score = score;
                        se.sameSource = zipFiles1[0].source == zipFiles2[0].source;

                        //if (se.zipfile1.Contains("glitter") ||
                        //    se.zipfile2.Contains("glitter"))
                        //    System.Diagnostics.Debugger.Break();
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
                    if (ascore < MAX_SCORE)
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
            _hashSource = 0;
            _toCompare = new List<HashZipEntry>();
            _zipDict = new ConcurrentDictionary<string, ConcurrentBag<HashZipEntry>>();

            SetStatus(string.Format("Hashes: {0} Archives: {1}", _hashSource, 0));
        }

        internal List<ScoreEntry> FilterMatchingTree()
        {
            _viewList = new List<ScoreEntry>();
            if (_scoreList == null || _scoreList.Count == 0)
                return _viewList;

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
            LoadZipList();
        }

        private string separator;

        internal void report(StreamWriter sw, ScoreEntry se)
        {
            // don't include deleted files
            if (!File.Exists(se.zipfile1) || !File.Exists(se.zipfile2))
                return;

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
            LoadZipList();
        }

        internal void HideRight(ScoreEntry sel)
        {
            if (sel == null)
                return;
            if (!_hideRight.Contains(sel.zipfile2))
                _hideRight.Add(sel.zipfile2);
            LoadZipList();
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

        internal void DoDiff(ScoreEntry2 sel, bool showonly=false)
        {
            if (sel == null)
                return;
            if (_diffDlg == null)
            {
                _diffDlg = new ShowDiff(_loader,_log) { Owner = this };
            }
            _diffDlg.Stretch = true; // stretch;
            _diffDlg.Diff = !showonly;
            _diffDlg.Group = sel;

            _diffDlg.ShowDialog();

        }

        public virtual void updateProgress(int value) { throw new Exception(); }

        public virtual void LoadZipList() { throw new Exception(); }

        public virtual void SetNote(string text) { throw new Exception(); }

        public virtual void SetStatus(string text) { throw new Exception(); }
    }
}
