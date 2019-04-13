using System;

namespace howto_image_hash
{
    public partial class MasterDetail3 : MasterDetailBase
    {
        ScoreEntry _oldSel;

        public MasterDetail3(Logger log, ArchiveLoader load)
            : base(log, load)
        {
            InitializeComponent();
        }

        private void MasterDetail3_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            pictureBox1.Image = pictureBox2.Image = null; // clear handles
            Cleanup();
            Owner.WindowState = System.Windows.Forms.FormWindowState.Normal;
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            // load a htih_pz.txt file
            ClearForLoad();
            loadHashFile();
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
        }

        public override void updateProgress(int value)
        {
            progressBar1.Value = value;
        }

        public override void SetNote(string text)
        {
            txtNote.Text = text;
        }
    }
}
