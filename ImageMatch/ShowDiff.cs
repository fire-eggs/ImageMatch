using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;

namespace howto_image_hash
{
    public partial class ShowDiff : Form
    {
        private bool swap;
        private Size _mySize;
        private Point _myLoc;
        private ArchiveLoader _loader;
        private Logger _logger;

        private string img1;
        private string img2;

        public bool Diff { get; set; } // whether to show the difference or not

        public bool Stretch { get; set; }

        private ScoreEntry2 _group;

        public ScoreEntry2 Group
        {
            set
            {
                _group = value;

                // extract from the zipfile only once
                img1 = _loader.Extract(_group.F1.ZipFile, _group.F1.InnerPath);
                img2 = _loader.Extract(_group.F2.ZipFile, _group.F2.InnerPath);

                doImage();
            }
        }

        public ShowDiff(ArchiveLoader load, Logger log)
        {
            _loader = load;
            _logger = log;

            InitializeComponent();
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;

            button1.Click += Button1_Click;
            FormClosing += ShowDiff_FormClosing;
            Load += ShowDiff_Load;
        }

        private void ShowDiff_Load(object sender, EventArgs e)
        {
            if (_mySize.IsEmpty)
                return;
            Size = _mySize;
            Location = _myLoc;
        }

        private void ShowDiff_FormClosing(object sender, FormClosingEventArgs e)
        {
            pictureBox1.Image = null;
            _mySize = Size;
            _myLoc = Location;

            // clean up extracted files
            try
            {
                File.Delete(img1);
                File.Delete(img2);
            }
            catch { }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            swap = !swap;
            doImage();
        }

        private void doImage()
        {
            try
            {
                if (!swap)
                {
                    Text = Diff ? "Left vs Right" : "Left Image";
                    pictureBox1.Image = Diff ? kbrDiff(img1, img2, Stretch) : Image.FromFile(img1);
                }
                else
                {
                    Text = Diff ? "Right vs Left" : "Right Image";
                    pictureBox1.Image = Diff ? kbrDiff(img2, img1, Stretch) : Image.FromFile(img2);
                }
            }
            catch (Exception)
            {
            }
        }

        #region kbrDiff
        public static bool IsGraphic(string FileName)
        {
            System.Text.RegularExpressions.Regex Regex = new System.Text.RegularExpressions.Regex
                (@"\.ico$|\.tiff$|\.gif$|\.jpg$|\.jpeg$|\.png$|\.bmp$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            return Regex.IsMatch(FileName);
        }

        public static Bitmap kbrDiff(string FileName1, string FileName2, bool stretch)
        {
            // TODO extract returns a .tmp file
//            if (!IsGraphic(FileName1) || !IsGraphic(FileName2))
//                return new Bitmap(1, 1);

            using (var TempImage1 = new Bitmap(FileName1))
            using (var TempImage2 = new Bitmap(FileName2))
            {

                if (TempImage1.Height != TempImage2.Height ||
                    TempImage1.Width != TempImage2.Width)
                {
                    if (!stretch)
                        throw new Exception("Size mismatch");

                    // resize
                    int newH = Math.Max(TempImage1.Height, TempImage2.Height);
                    int newW = Math.Max(TempImage1.Width, TempImage2.Width);
                    Bitmap newImage1 = ResizeImage(TempImage1, newW, newH);
                    Bitmap newImage2 = ResizeImage(TempImage2, newW, newH);
                    return kbrDiff(newImage1, newImage2);
                }

                return kbrDiff(TempImage1, TempImage2);
            }
        }

        internal static BitmapData LockImage(Bitmap Image)
        {
            if (Image == null)
                throw new ArgumentNullException("Image");
            return Image.LockBits(new Rectangle(0, 0, Image.Width, Image.Height),
                ImageLockMode.ReadWrite, Image.PixelFormat);
        }
        internal static void UnlockImage(Bitmap Image, BitmapData ImageData)
        {
            if (Image == null)
                throw new ArgumentNullException("Image");
            if (ImageData == null)
                throw new ArgumentNullException("ImageData");
            Image.UnlockBits(ImageData);
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
                // TODO convert to supported format
                // 2011/12/17 this isn't working: resulting in 'black' pixels for many monochrome images
                //case PixelFormat.Format8bppIndexed:
                //    return 1;
                default:
                    throw new ArgumentException("unhandled image format");
            }
        }

        internal static unsafe Color GetPixel(BitmapData Data, int x, int y, int PixelSizeInBytes)
        {
            if (Data == null)
                throw new ArgumentNullException("Data");
            byte* DataPointer = (byte*)Data.Scan0;
            DataPointer = DataPointer + (y * Data.Stride) + (x * PixelSizeInBytes);
            if (PixelSizeInBytes == 1)
            {
                return Color.FromArgb(255, DataPointer[0], DataPointer[0], DataPointer[0]); // Assuming greyscale!!
            }
            if (PixelSizeInBytes == 3)
            {
                return Color.FromArgb(DataPointer[2], DataPointer[1], DataPointer[0]);
            }
            return Color.FromArgb(DataPointer[3], DataPointer[2], DataPointer[1], DataPointer[0]);
        }

        internal static unsafe void SetPixel(BitmapData Data, int x, int y, Color PixelColor, int PixelSizeInBytes)
        {
            if (Data == null)
                throw new ArgumentNullException("Data");
            if (PixelColor == null)
                throw new ArgumentNullException("PixelColor");
            byte* DataPointer = (byte*)Data.Scan0;
            DataPointer = DataPointer + (y * Data.Stride) + (x * PixelSizeInBytes);
            if (PixelSizeInBytes == 3)
            {
                DataPointer[2] = PixelColor.R;
                DataPointer[1] = PixelColor.G;
                DataPointer[0] = PixelColor.B;
                return;
            }
            DataPointer[3] = PixelColor.A;
            DataPointer[2] = PixelColor.R;
            DataPointer[1] = PixelColor.G;
            DataPointer[0] = PixelColor.B;
        }

        public static Bitmap kbrDiff(Bitmap Image1, Bitmap Image2)
        {
            if (Image1 == null)
                throw new ArgumentNullException("Image1");
            if (Image2 == null)
                throw new ArgumentNullException("Image2");
            if (Image1.Height != Image2.Height ||
                Image1.Width != Image2.Width)
                throw new Exception("Size mismatch");

            Bitmap NewBitmap = new Bitmap(Image1.Width, Image1.Height);
            BitmapData NewData = LockImage(NewBitmap);
            BitmapData OldData1 = LockImage(Image1);
            BitmapData OldData2 = LockImage(Image2);
            int NewPixelSize = GetPixelSize(NewData);
            int OldPixelSize1 = GetPixelSize(OldData1);
            int OldPixelSize2 = GetPixelSize(OldData2);
            for (int x = 0; x < NewBitmap.Width; ++x)
            {
                for (int y = 0; y < NewBitmap.Height; ++y)
                {
                    Color Pixel1 = GetPixel(OldData1, x, y, OldPixelSize1);
                    Color Pixel2 = GetPixel(OldData2, x, y, OldPixelSize2);

                    int clrDiff = Math.Abs(Pixel1.R - Pixel2.R +
                                                  Pixel1.G - Pixel2.G +
                                                  Pixel1.B - Pixel2.B);
                    if (clrDiff < 10)
                        SetPixel(NewData, x, y, Color.Black, NewPixelSize);
                    else
                        SetPixel(NewData, x, y,
                            Color.FromArgb(Pixel1.R, Pixel1.G, Pixel1.B),
                            NewPixelSize);
                }
            }
            UnlockImage(NewBitmap, NewData);
            UnlockImage(Image1, OldData1);
            UnlockImage(Image2, OldData2);
            Image1.Dispose();
            Image2.Dispose();
            return NewBitmap;
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        #endregion

    }
}
