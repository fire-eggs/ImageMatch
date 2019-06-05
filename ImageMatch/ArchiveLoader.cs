using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SevenZip;

namespace howto_image_hash
{
    public class ArchiveLoader
    {
        private readonly Logger _logger;

        public ArchiveLoader(Logger logger)
        {
            _logger = logger;

            string dllPath = "";
            try
            {

                //Get the location of the 7z dll (location .EXE is in)
                string executableName = System.Reflection.Assembly.GetExecutingAssembly().Location;
                FileInfo executableFileInfo = new FileInfo(executableName);

                dllPath = Path.Combine(executableFileInfo.DirectoryName, "7z.dll");

                //load the 7zip dll
                SevenZipBase.SetLibraryPath(dllPath);
            }
            catch (Exception e)
            {
                var ex = new Exception(string.Format("Unable to load 7z.dll from: {0}", dllPath), e.InnerException);
                _logger.log(ex);
                throw ex;
            }

        }

        public delegate void ArchiveFileCallback(string zipFileName, string archiveFileName, string outfilepath);

        private List<string> _toCleanUp = new List<string>();

        public int GetFileCount(string archive)
        {
            try
            {
                var extractor = new SevenZipExtractor(archive);
                return (int)extractor.FilesCount;
            }
            catch (Exception ex)
            {
                _logger.log(ex.Message + "|" + archive);
                return 0;
            }
        }

        public void Process(string archive, ArchiveFileCallback callback)
        {
            try
            {
                if (isImageExt(Path.GetExtension(archive)))
                    return;

                var extractor = new SevenZipExtractor(archive);
                var fileNames = extractor.ArchiveFileNames;
                string tmpPath = Path.GetTempFileName(); // one temp. file per archive - saves on # of temp files created
                foreach (var fileName in fileNames)
                {
                    if (!isImageExt(Path.GetExtension(fileName)))
                        continue;

                    using (FileStream fs = new FileStream(tmpPath, FileMode.Create))
                    {
                        extractor.ExtractFile(fileName, fs);
                    }

                    callback(archive, fileName, tmpPath);
                }
                _toCleanUp.Add(tmpPath); // can't delete, consumer may still be using it
                extractor.Dispose();
            }
            catch (Exception ex)
            {
                _logger.log(ex.Message + "|" + archive);
            }
        }

        public void Cleanup()
        {
            foreach (var file in _toCleanUp)
            {
                File.Delete(file);
            }
            _toCleanUp.Clear();
        }

        public string Extract(string archive, string fileName)
        {
            try
            {
                using (var extractor = new SevenZipExtractor(archive))
                {
                    if (!isImageExt(Path.GetExtension(fileName)))
                    {
                        return string.Empty;
                    }

                    string tmpPath = Path.GetTempFileName();
                    using (FileStream fs = new FileStream(tmpPath, FileMode.Create))
                    {
                        extractor.ExtractFile(fileName, fs);
                    }
                    _toCleanUp.Add(tmpPath); // can't delete, consumer may still be using it
                    return tmpPath;
                }
            }
#pragma warning disable CS0168 // Variable is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
            {
                //_logger.log(ex);
            }
            return string.Empty;
        }

        public static string[] extArray = { ".JPG", ".JPEG", ".PNG", ".BMP", ".GIF", ".JPE" };
        public static bool isImageExt(string ext)
        {
            return extArray.Any(s => s.Equals(ext, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
