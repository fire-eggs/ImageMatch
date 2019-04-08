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

            pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;

            // TODO hack, hack
            base.label1 = label1;
            base.label2 = label2;

            FormClosing += MasterDetail3_FormClosing;
        }

        private void MasterDetail3_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            pictureBox1.Image = pictureBox2.Image = null; // clear handles
            Cleanup();
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

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // user has selected a zip
            if (_oldSel != null)
                AddNote(_oldSel, txtNote.Text);  // "auto-update" note

            var sel = listBox1.SelectedItem as ScoreEntry;
            var det = selectZipPair(sel);
            listBox2.DataSource = det;

            _oldSel = sel;
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectPixPair(listBox2.SelectedItem as ScoreEntry2, pictureBox1, pictureBox2);
        }

        private void btnHideL_Click(object sender, EventArgs e)
        {
            HideLeft(listBox1.SelectedItem as ScoreEntry);
        }

        private void btnHideR_Click(object sender, EventArgs e)
        {
            HideRight(listBox1.SelectedItem as ScoreEntry);
        }

        private void ClearForLoad()
        {
            listBox1.DataSource = null;
            listBox2.DataSource = null;
            pictureBox1.Image = null;
            pictureBox2.Image = null;
            label1.Text = "";
            label2.Text = "";
            txtNote.Text = "";
        }

        public override void LoadList()
        {
            var viewlist = FilterMatchingTree();

            listBox1.DataSource = viewlist;
            listBox1.SelectedIndex = -1;
            if (viewlist.Count > 0)
                listBox1.SelectedIndex = 0;
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
