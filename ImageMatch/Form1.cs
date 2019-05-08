using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace howto_image_hash
{
    public partial class Form1 : Form
    {
        private ArchiveLoader _zipload;
        private Logger _log;

        public Form1(string autopath)
        {
            _log = new Logger();
            _zipload = new ArchiveLoader(_log);
            _hasher = new PHash2();

            if (!string.IsNullOrEmpty(autopath))
            {
                progressBar1 = new ProgressBar();
                _path = autopath;
                HashEm();
                Close();
            }

            InitializeComponent();
        }

        private static int tick;
        internal static void logit(string msg, bool first = false)
        {
            int delta = 0;
            if (first)
                tick = Environment.TickCount;
            else
                delta = Environment.TickCount - tick;

            using (StreamWriter sw = new StreamWriter(File.Open(@"e:\htih.log", FileMode.Append)))
            {
                sw.WriteLine("{0}|{1}|", msg, delta);
            }
        }

        // Scale an image.
        private Bitmap ScaleTo(Bitmap bm, int wid, int hgt,
            InterpolationMode interpolation_mode)
        {
            Bitmap new_bm = new Bitmap(wid, hgt);
            using (Graphics gr = Graphics.FromImage(new_bm))
            {
                RectangleF source_rect = new RectangleF(-0.5f, -0.5f, bm.Width, bm.Height);
                Rectangle dest_rect = new Rectangle(0, 0, wid, hgt);
                gr.InterpolationMode = interpolation_mode;
                gr.DrawImage(bm, dest_rect, source_rect, GraphicsUnit.Pixel);
            }
            return new_bm;
        }

        // Convert an image to monochrome.
        private Bitmap ToMonochrome(Image image)
        {
            // Make the ColorMatrix.
            ColorMatrix cm = new ColorMatrix(new float[][]
            {
                new float[] {0.299f, 0.299f, 0.299f, 0, 0},
                new float[] {0.587f, 0.587f, 0.587f, 0, 0},
                new float[] {0.114f, 0.114f, 0.114f, 0, 0},
                new float[] { 0, 0, 0, 1, 0},
                new float[] { 0, 0, 0, 0, 1}
            });
            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(cm);

            // Draw the image onto the new bitmap while
            // applying the new ColorMatrix.
            Point[] points =
            {
                new Point(0, 0),
                new Point(image.Width, 0),
                new Point(0, image.Height),
            };
            Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);

            // Make the result bitmap.
            Bitmap bm = new Bitmap(image.Width, image.Height);
            using (Graphics gr = Graphics.FromImage(bm))
            {
                //gr.DrawImage(image, points, rect,
                //    GraphicsUnit.Pixel, attributes);
                gr.DrawImage(image, rect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
            }

            // Return the result.
            return bm;
        }

        private ulong getRowHash(Bitmap bm)
        {
            ulong hash = 0;
            int bit = 63;
            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                {
                    if (bm.GetPixel(c + 1, r).R >= bm.GetPixel(c, r).R)
                        hash |= 1UL << bit;
                    bit--;
                }
            return hash;
        }

        private ulong getColHash(Bitmap bm)
        {
            ulong hash = 0;
            int bit = 63;
            for (int c = 0; c < 8; c++)
                for (int r = 0; r < 8; r++)
                { 
                    if (bm.GetPixel(c, r + 1).R >= bm.GetPixel(c, r).R)
                        hash |= 1UL << bit;
                    bit--;
                }
            return hash;
        }

        // Return the hashcode for this 9x9 image.
        private string GetHashCode(Bitmap bm)
        {
            string row_hash = "";
            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                    if (bm.GetPixel(c + 1, r).R >= bm.GetPixel(c, r).R)
                        row_hash += "1";
                    else
                        row_hash += "0";

            string col_hash = "";
            for (int c = 0; c < 8; c++)
                for (int r = 0; r < 8; r++)
                    if (bm.GetPixel(c, r + 1).R >= bm.GetPixel(c, r).R)
                        col_hash += "1";
                    else
                        col_hash += "0";

            return row_hash + "," + col_hash;
        }

        private Bitmap LoadBitmapUnlocked(string path)
        {
            var bytes = File.ReadAllBytes(path);
            var ms = new MemoryStream(bytes);
            return (Bitmap)Image.FromStream(ms);
        }

        // https://martincarlsen.com/fast-image-resizing-using-wpf-rendering-support/
        //private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        //{
        //    return new Bitmap(bitmapImage.StreamSource);
        //}

        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        private Bitmap BitmapImage2Bitmap(BitmapFrame bitmapImage)
        {
            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(bitmapImage);
                //enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        public static BitmapFrame ReadBitmapFrame(MemoryStream photoStream)
        {
            var photoDecoder = BitmapDecoder.Create(
                photoStream,
                BitmapCreateOptions.PreservePixelFormat,
                BitmapCacheOption.None);
            return photoDecoder.Frames[0];
        }

        public static BitmapFrame FastResize(BitmapFrame photo, int width, int height)
        {
            var target = new TransformedBitmap(
                photo,
                new ScaleTransform(
                    width / photo.Width,
                    height / photo.Height,
                    0, 0));
            return BitmapFrame.Create(target);
        }

        internal class HashZipEntry
        {
            public string ZipFile;
            public string InnerPath;
            public ulong  phash;
            public int    source; // to filter by hashfile (CompareForm)
        }

        private ConcurrentBag<HashZipEntry> _hashedZ;

        private string[] _allFiles;
        private void GetAllFiles(string path)
        {
            // https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/how-to-iterate-file-directories-with-plinq
            var allfiles = from dir in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
                           .AsParallel()
                            select dir;
            _allFiles = allfiles.ToArray();
        }

        void phashZip_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
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
            progressBar1.Value = 0;

            _log.logTimer("hashPIX_TZ_rwc");
        }

        TaskScheduler _guiContext;
        int _totCount;
        int _doneCount;

        private void phashZip_doWork(object sender, DoWorkEventArgs e)
        {
            var token = Task.Factory.CancellationToken;
            List<Task> _allTasks = new List<Task>();

            _doneCount = 0;
            foreach (var oneFI in _allFiles)
            {
                _allTasks.Add(Task.Factory.StartNew(() => _zipload.Process(oneFI.ToLowerInvariant(), do_hashZ)));
                Task.Factory.StartNew(() => OneThreadDone(), token, TaskCreationOptions.None, _guiContext); // TODO consider switching to ContinueWith
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
        }

        private void OneThreadDone()
        {
            _doneCount++;
            //if (_doneCount % 10 == 0)
                progressBar1.Value = (int)((double)_doneCount / _totCount * 100.0);
        }

        private void HashFilePixel(string file)
        {
#if false
            try
            {
                //1. load bitmap
                using (var bmp = new Bitmap(file))
                {
                    //2. 'pixelate' to a 9x9 array of lum values
                    var res = Pixelate(bmp, 9, 9);

                    //3. calculate row/col hashes
                    var rowHash = getRowHashLum(res);
                    var colHash = getColHashLum(res);

                    var he = new HashEntry();
                    he.path = file;
                    he.rowhash = rowHash;
                    he.colhash = colHash;
                    _hashed.Add(he);
                }
            }
            catch
            {
                return;
            }

#endif
        }

        public static int GetPixelSize(BitmapData Data)
        {
            if (Data == null)
                throw new ArgumentNullException("Data");
            switch (Data.PixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                    return 3;
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                case PixelFormat.Format32bppRgb:
                    return 4;
                case PixelFormat.Format8bppIndexed:
                    return 1;
                default:
                    throw new ArgumentException("unhandled image format");
            }
        }

        private static int Clamp(int Value, int Max, int Min)
        {
            Value = Value > Max ? Max : Value;
            return Value < Min ? Min : Value;
        }

        public unsafe int[,] Pixelate(Bitmap OriginalImage, int targetX, int targetY)
        {
            int[,] res = new int[targetX, targetY];

            var PixelSizeY = OriginalImage.Height / targetY;
            var PixelSizeX = OriginalImage.Width / targetX;


            BitmapData OldData = OriginalImage.LockBits(new Rectangle(0, 0, OriginalImage.Width, OriginalImage.Height),
                ImageLockMode.ReadWrite, OriginalImage.PixelFormat);
            int OldPixelSize = GetPixelSize(OldData);

            if (OldPixelSize == 1)
            {
                OriginalImage.UnlockBits(OldData);
                OriginalImage = ConvertTo24(OriginalImage);
                OldData = OriginalImage.LockBits(new Rectangle(0, 0, OriginalImage.Width, OriginalImage.Height),
                                ImageLockMode.ReadWrite, OriginalImage.PixelFormat);
                OldPixelSize = GetPixelSize(OldData);
            }


            byte* basePtr = (byte*)OldData.Scan0;
            int strideVal = OldData.Stride;

            int PX = PixelSizeX / 2;
            int PY = PixelSizeY / 2;
            int W = OriginalImage.Width;
            int H = OriginalImage.Height;

            int resY = 0;
            for (int y = PY; y <= H - PY; y += PixelSizeY)
            {
                int resX = 0;
                for (int x = PX; x <= W - PX; x += PixelSizeX)
                {
                    int RValue = 0;
                    int GValue = 0;
                    int BValue = 0;

                    int MinY = Clamp(y - PY, H, 0);
                    int MaxY = Clamp(y + PY, H, 0);
                    int MinX = Clamp(x - PX, W, 0);
                    int MaxX = Clamp(x + PX, W, 0);

                    byte* DataPointer = basePtr + MinY * strideVal;

                    for (int y2 = MinY; y2 < MaxY; ++y2)
                    {
                        byte* ScanPointer = DataPointer + (MinX * OldPixelSize);
                        for (int x2 = MinX; x2 < MaxX; ++x2)
                        {
                            if (OldPixelSize == 1)
                            {
                                // Assuming greyscale
                                RValue += ScanPointer[0];
                                GValue += ScanPointer[0];
                                BValue += ScanPointer[0];
                            }
                            else
                            {
                                RValue += ScanPointer[2];
                                GValue += ScanPointer[1];
                                BValue += ScanPointer[0];
                            }

                            ScanPointer += OldPixelSize;
                        }

                        DataPointer += strideVal;
                    }
                    RValue = RValue / (PixelSizeX * PixelSizeY);
                    GValue = GValue / (PixelSizeX * PixelSizeY);
                    BValue = BValue / (PixelSizeX * PixelSizeY);

                    int Lum = (int)(RValue * 0.3 + GValue * 0.59 + BValue * 0.11);
                    res[resX, resY] = Lum;
                    resX++;
                }
                resY++;
            }
            OriginalImage.UnlockBits(OldData);
            return res;
        }

        private BackgroundWorker worker;

        private string _path;

        private void btnFolder_Click(object sender, EventArgs e)
        {
            // prompt user for a folder
            // open a hash output text file
            // for each image in folder, calc hash, write to text file
            // close text file
            var fbd = new FolderBrowserDialog();
            fbd.ShowNewFolderButton = false;
            if (fbd.ShowDialog() != DialogResult.OK)
                return;
            var path = fbd.SelectedPath;
            _path = path;
            Hashish();
        }

        private void Hashish()
        {
            Hasher ahash = new Hasher(_log, progressBar1);
            ahash.HashEm(_path);
        }

        private void HashEm()
        {
            _log.logTimer("hashEm", true);

            GetAllFiles(_path);
            _totCount = _allFiles.Length;
            if (_totCount == 0)
                return;

            _log.log("FileCount:" + _totCount.ToString());

            if (worker == null)
            {
                worker = new BackgroundWorker();
                worker.DoWork += phashZip_doWork;
                worker.ProgressChanged += phashZip_ProgressChanged;
                worker.WorkerReportsProgress = true;
                worker.RunWorkerCompleted += phashZip_RunWorkerCompleted;
            }
            progressBar1.Value = 0;
            _hashedZ = new ConcurrentBag<HashZipEntry>();
            _guiContext = TaskScheduler.FromCurrentSynchronizationContext();
            worker.RunWorkerAsync();
        }

        private void btnLoadHash_click(object sender, EventArgs e)
        {
            //CompareForm cf = new CompareForm(_log,_zipload);
            //cf.ShowDialog();
            MasterDetail cf = new MasterDetail(_log, _zipload);
            cf.ShowDialog();
        }

        private ulong getRowHashLum(int [,] lums)
        {
            ulong hash = 0;
            int bit = 63;
            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                {
                    if (lums[c+1,r] >= lums[c,r])
                        hash |= 1UL << bit;
                    bit--;
                }
            return hash;
        }

        private ulong getColHashLum(int[,] lums)
        {
            ulong hash = 0;
            int bit = 63;
            for (int c = 0; c < 8; c++)
                for (int r = 0; r < 8; r++)
                {
                    if (lums[c,r+1] >= lums[c,r])
                        hash |= 1UL << bit;
                    bit--;
                }
            return hash;
        }

        private PHash2 _hasher;

        private void do_hashZ(string zipfile, string archivefilename, string outfilepath)
        {
            try
            {
                var phash = _hasher.CalculateDctHash(outfilepath);
                HashZipEntry hze = new HashZipEntry();
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
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //CompareForm cf = new CompareForm(true);
            //cf.ShowDialog();
        }

        private static Bitmap ConvertTo24(Bitmap bmpIn)
        {
            Bitmap converted = new Bitmap(bmpIn.Width, bmpIn.Height, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(converted))
            {
                // Prevent DPI conversion
                g.PageUnit = GraphicsUnit.Pixel;
                // Draw the image
                g.DrawImageUnscaled(bmpIn, 0, 0);
            }
            return converted;
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            MasterDetail2 cf = new MasterDetail2(_log, _zipload);
            _ = cf.ShowDialog();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            MasterDetail3 cf = new MasterDetail3(_log, _zipload);
            cf.Owner = this;
            cf.Show();
        }
    }
}
