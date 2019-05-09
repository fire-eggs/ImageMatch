using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace howto_image_hash
{
    class PHash2
    {
        private readonly float [][] _dctMatrix;
        private readonly float [][] _dctTransp;

        public PHash2()
        {
            _dctMatrix = GenerateDctMatrix(32);
            _dctTransp = Transpose(_dctMatrix);
        }

        public ulong CalculateDctHash(string path)
        {
            var fpixels = transformImageF(path); //, 32, 32);
            if (fpixels == null)
                return 0;

            // Calculate dct
            var dctPixels = ComputeDct(fpixels);

            // Get 8*8 area from 1,1 to 8,8, ignoring lowest frequencies for improved detection
            var dctHashPixels = new float[64];
            for (var x = 0; x < 8; x++)
            {
                for (var y = 0; y < 8; y++)
                {
                    dctHashPixels[x + y * 8] = dctPixels[x + 1][y + 1];
                }
            }

            // Calculate median
            var pixelList = new List<float>(dctHashPixels);
            pixelList.Sort();
            // Even amount of pixels
            var median = (pixelList[31] + pixelList[32]) / 2;

            // Iterate pixels and set them to 1 if over median and 0 if lower.
            var hash = 0UL;
            for (var i = 0; i < 64; i++)
            {
                if (dctHashPixels[i] > median)
                {
                    hash |= (1UL << i);
                }
            }

            // Done
            return hash;
        }

        /// <summary>
        /// Compute DCT for the image.
        /// </summary>
        /// <param name="image">Image to calculate the dct.</param>
        /// <returns>DCT transform of the image</returns>
        private float[][] ComputeDct(float[] image)
        {
            // Get the size of dct matrix. We assume that the image is same size as dctMatrix
            var size = _dctMatrix.GetLength(0);

            // Make image matrix
            var imageMat = new float[size][];
            for (var i = 0; i < size; i++)
            {
                imageMat[i] = new float[size];
            }

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    imageMat[y][x] = image[x + y * size];
                }
            }

            return Multiply(Multiply(_dctMatrix, imageMat), _dctTransp);
        }

        /// <summary>
        /// Generates DCT coefficient matrix.
        /// </summary>
        /// <param name="size">Size of the matrix.</param>
        /// <returns>Coefficient matrix.</returns>
        private static float[][] GenerateDctMatrix(int size)
        {
            var matrix = new float[size][];
            for (int i = 0; i < size; i++)
            {
                matrix[i] = new float[size];
            }

            var c1 = Math.Sqrt(2.0f / size);

            for (var j = 0; j < size; j++)
            {
                matrix[0][j] = (float)Math.Sqrt(1.0d / size);
            }

            for (var j = 0; j < size; j++)
            {
                for (var i = 1; i < size; i++)
                {
                    matrix[i][j] = (float)(c1 * Math.Cos(((2 * j + 1) * i * Math.PI) / (2.0d * size)));
                }
            }
            return matrix;
        }

        /// <summary>
        /// Matrix multiplication.
        /// </summary>
        /// <param name="a">First matrix.</param>
        /// <param name="b">Second matric.</param>
        /// <returns>Result matrix.</returns>
        private static float[][] Multiply(float[][] a, float[][] b)
        {
            var n = a[0].Length;
            var c = new float[n][];
            for (var i = 0; i < n; i++)
            {
                c[i] = new float[n];
            }

            for (var i = 0; i < n; i++)
                for (var k = 0; k < n; k++)
                    for (var j = 0; j < n; j++)
                        c[i][j] += a[i][k] * b[k][j];
            return c;
        }

        /// <summary>
        /// Transposes square matrix.
        /// </summary>
        /// <param name="mat">Matrix to be transposed</param>
        /// <returns>Transposed matrix</returns>
        private static float[][] Transpose(float[][] mat)
        {
            var size = mat[0].Length;
            var transpose = new float[size][];

            for (var i = 0; i < size; i++)
            {
                transpose[i] = new float[size];
                for (var j = 0; j < size; j++)
                    transpose[i][j] = mat[j][i];
            }
            return transpose;
        }

        // Convert a WIC BitmapImage to a GDI+ bitmap
        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                try
                {
                    enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                }
                catch (NotSupportedException) // apparently this is thrown deliberately but uselessly
                {
                }
                enc.Save(outStream);
                return new Bitmap(outStream);
            }
        }

        private float[] transformImageF(string path)
        {
            try
            {
                // load with WIC
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(path);
                bi.DecodePixelHeight = 32;
                bi.DecodePixelWidth = 32;
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.EndInit();

                // TODO try accessing the WIC pixels without the conversion
                // TODO try using WIC convert to greyscale?

                // return the 32x32 image as an array of greyscale float values
                using (Bitmap shrunkBm = BitmapImage2Bitmap(bi))
                {
                    float[] output = new float[32 * 32];
                    int dex = 0;
                    for (int i = 0; i < 32; i++)
                    for (int j = 0; j < 32; j++)
                    {
                        Color clr = shrunkBm.GetPixel(j, i);
                        output[dex++] = (clr.R * 0.3f + clr.G * 0.59f + clr.B * 0.11f) / 255.0f;
                    }
                    return output;
                }
            }
            catch
            {
                return null;
            }
        }

    }
}
