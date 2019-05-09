using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

/*
 * High level hashing process. Extract files from archives,
 * hash them, write to storage.
 */

namespace howto_image_hash
{
    public class Hasher
    {
        private readonly Logger _log;
        private readonly ProgressBar _progressbar;
        TaskScheduler _guiContext;
        int _totCount;
        int _doneCount;
        private string _path;
        private ConcurrentBag<Form1.HashZipEntry> _hashedZ;
        private BackgroundWorker _worker;
        private string[] _allFiles;
        private readonly ArchiveLoader _zipload;
        private readonly PHash2 _hasher;
        private int _totCount2;

        public Hasher(Logger log, ProgressBar progressBar)
        {
            _log = log;
            _progressbar = progressBar; // TODO callback?
            _zipload = new ArchiveLoader(_log);
            _hasher = new PHash2();
        }

        public void HashEm(string path)
        {
            _path = path;
            _log.logTimer("hashEm", true);

            _allFiles = GetAllFiles(_path);
            _totCount = _allFiles.Length;
            if (_totCount == 0)
                return;

            _totCount2 = 0;
            _log.log("File Count:" + _totCount);

            // TODO is a background worker useful?
            if (_worker == null)
            {
                _worker = new BackgroundWorker();
                _worker.DoWork += phashZip_doWork;
                _worker.ProgressChanged += phashZip_ProgressChanged;
                _worker.WorkerReportsProgress = true;
                _worker.RunWorkerCompleted += phashZip_RunWorkerCompleted;
            }
            _progressbar.Value = 0;
            _hashedZ = new ConcurrentBag<Form1.HashZipEntry>();
            _guiContext = TaskScheduler.FromCurrentSynchronizationContext();
            _worker.RunWorkerAsync();
        }

        private static string[] GetAllFiles(string path)
        {
            // https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/how-to-iterate-file-directories-with-plinq
            var allfiles = from dir in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
                    .AsParallel()
                select dir;
            return allfiles.ToArray();
        }

        void phashZip_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            _progressbar.Value = e.ProgressPercentage;
        }

        void phashZip_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            using (var outFile = new StreamWriter(Path.Combine(_path, "htih_pz.txt")))
            {
                foreach (var he in _hashedZ.ToList())
                {
                    outFile.WriteLine("{0}|{1}|{2}", he.ZipFile, he.InnerPath, he.phash);
                }
            }
            _progressbar.Value = 0;

            _log.logTimer("hashPIX_TZ_rwc");
        }

        private readonly System.Threading.CancellationToken token = Task.Factory.CancellationToken;

        private void phashZip_doWork(object sender, DoWorkEventArgs e)
        {
            List<Task> _allTasks = new List<Task>();

            _doneCount = 0;
            foreach (var oneFI in _allFiles)
            {
                int fc = _zipload.GetFileCount(oneFI);
                if (fc < 1)
                    continue;
                _totCount2 += fc;
                _allTasks.Add(Task.Factory.StartNew(() => _zipload.Process(oneFI, do_hashZ)));
            }

            var allTasks2 = _allTasks.ToArray();
            _allTasks = null;
            Task.WaitAll(allTasks2);
            foreach (var task in allTasks2)
            {
                task.Dispose();
            }
            allTasks2 = null;

            _zipload.Cleanup();

            _log.log("Inner File Count:" + _totCount2);
        }

        private void OneThreadDone()
        {
            _doneCount++;
            if (_doneCount % 10 == 0)
                _progressbar.Value = (int)((double)_doneCount / _totCount2 * 100.0);
        }

        private void do_hashZ(string zipfile, string archivefilename, string outfilepath)
        {
            try
            {
                var phash = _hasher.CalculateDctHash(outfilepath);
                Form1.HashZipEntry hze = new Form1.HashZipEntry();
                hze.ZipFile = zipfile;
                hze.InnerPath = archivefilename;
                hze.phash = phash;
                _hashedZ.Add(hze);
            }
            catch (Exception ex)
            {
                _log.log(string.Format("{0}-{1}-{2}", zipfile, archivefilename, outfilepath));
                _log.log(ex);
            }
            Task.Factory.StartNew(() => OneThreadDone(), token, TaskCreationOptions.None, _guiContext); // TODO consider switching to ContinueWith
        }

    }
}
