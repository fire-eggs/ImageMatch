using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace howto_image_hash
{
    public partial class MasterDetail3 : MasterDetailBase
    {
        ScoreEntry _oldSel;
        DrawAnce.MruStripMenu _mru;

        public MasterDetail3(Logger log, ArchiveLoader load)
            : base(log, load)
        {
            InitializeComponent();

            ToolStripDropDownMenu tsm = new ToolStripDropDownMenu();
            splitButton1.SplitMenuStrip = tsm;

            _mru = new DrawAnce.MruStripMenu(tsm, onMru, 7);

            LoadSettings(); // NOTE: _must_ go after _mru creation

            splitButton1.Click += this.btnLoad_Click;
        }

        private void onMru(int number, string filename)
        {
            if (!Directory.Exists(filename))
            {
                _mru.RemoveFile(number);
                MessageBox.Show("The path no longer exists: " + filename);
                return;
            }

            // TODO process could fail for some reason, in which case remove the file from the MRU list
            _mru.SetFirstFile(number);
            loadHashFile(filename);
        }

        private void MasterDetail3_FormClosing(object sender, FormClosingEventArgs e)
        {
            WindowState = FormWindowState.Normal; // don't save windows bounds when minimized
            SaveSettings();
            pictureBox1.Image = pictureBox2.Image = null; // clear handles
            Cleanup();
            Owner.WindowState = FormWindowState.Normal;
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            // load a htih_pz.txt file
            ClearForLoad();
            string path = loadHashFile();
            if (path != null)
                _mru.AddFile(path);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearForLoad();
            Reset();
        }

        private void btnFilter_Click(object sender, EventArgs e)
        {
            ToggleFilter();
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
            Report();
        }

        private void listZipPairs_SelectedIndexChanged(object sender, EventArgs e)
        {
            // user has selected a zip
            if (_oldSel != null)
                AddNote(_oldSel, txtNote.Text);  // "auto-update" note

            var sel = listZipPairs.SelectedItem as ScoreEntry;
            var det = selectZipPair(sel);
            listFilePairs.DataSource = det;

            _oldSel = sel;
        }

        private void listFilePairs_SelectedIndexChanged(object sender, EventArgs e)
        {
            var se = listFilePairs.SelectedItem as ScoreEntry2;
            btnDiff.Enabled = !(se == null || se.F1 == null || se.F2 == null);
            setPix(se, true, pictureBox1, label1);
            setPix(se, false, pictureBox2, label2);
        }

        private void btnHideL_Click(object sender, EventArgs e)
        {
            HideLeft(listZipPairs.SelectedItem as ScoreEntry);
        }

        private void btnHideR_Click(object sender, EventArgs e)
        {
            HideRight(listZipPairs.SelectedItem as ScoreEntry);
        }

        private void ClearForLoad()
        {
            listZipPairs.DataSource = null;
            listFilePairs.DataSource = null;
            pictureBox1.Image = null;
            pictureBox2.Image = null;
            label1.Text = "";
            label2.Text = "";
            txtNote.Text = "";
        }

        public override void LoadZipList()
        {
            var viewlist = FilterMatchingTree();

            listZipPairs.DataSource = viewlist;
            listZipPairs.SelectedIndex = -1;
            if (viewlist.Count > 0)
                listZipPairs.SelectedIndex = 0;
            else
                MessageBox.Show("No zip file matches found");
        }

        public override void updateProgress(int value)
        {
            progressBar1.Value = value;
        }

        public override void SetNote(string text)
        {
            txtNote.Text = text;
        }

        public override void SetStatus(string text)
        {
            lblStatus.Text = text;
        }

        private void BtnDiff_Click(object sender, EventArgs e)
        {
            var sel = listFilePairs.SelectedItem as ScoreEntry2;
            DoDiff(sel);
        }

        private void BtnShow_Click(object sender, EventArgs e)
        {
            var sel = listFilePairs.SelectedItem as ScoreEntry2;
            DoDiff(sel, true);
        }

        private void BtnDelLeft_Click(object sender, EventArgs e)
        {
            var sel = listZipPairs.SelectedItem as ScoreEntry;
            if (sel == null)
                return;
            try
            {
                File.Delete(sel.zipfile1);
            }
            catch { }
        }

        private void BtnDelRight_Click(object sender, EventArgs e)
        {
            var sel = listZipPairs.SelectedItem as ScoreEntry;
            if (sel == null)
                return;
            try
            {
                File.Delete(sel.zipfile2);
            }
            catch { }
        }

        #region Settings
        private DASettings _mysettings;
        private List<string> _fileHistory;

        private void LoadSettings()
        {
            _mysettings = DASettings.Load();

            // No existing settings. Use default.
            if (_mysettings.Fake)
            {
                StartPosition = FormStartPosition.CenterScreen;
            }
            else
            {
                // restore windows position
                StartPosition = FormStartPosition.Manual;
                Top = _mysettings.WinTop;
                Left = _mysettings.WinLeft;
                Height = _mysettings.WinHigh;
                Width = _mysettings.WinWide;
                _fileHistory = _mysettings.PathHistory ?? new List<string>();
                _fileHistory.Remove(null);
                _mru.SetFiles(_fileHistory.ToArray());
            }
        }

        private void SaveSettings()
        {
            var bounds = DesktopBounds;
            _mysettings.WinTop = Location.Y;
            _mysettings.WinLeft = Location.X;
            _mysettings.WinHigh = bounds.Height;
            _mysettings.WinWide = bounds.Width;
            _mysettings.Fake = false;
            _mysettings.PathHistory = _mru.GetFiles().ToList();
            _mysettings.Save();
        }
        #endregion

    }
}
