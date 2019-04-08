using System;

namespace howto_image_hash
{
    public partial class MasterDetail : MasterDetailBase
    {
        public MasterDetail(Logger log, ArchiveLoader loader)
            : base(log, loader)
        {
            InitializeComponent();

            // TODO hack, hack
            base.label1 = label1;
            base.label2 = label2;
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectPixPair(listBox2.SelectedItem as ScoreEntry2, pictureBox1, pictureBox2);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // user has selected a zip
            var sel = listBox1.SelectedItem as ScoreEntry;
            var det = selectZipPair(sel);
            listBox2.DataSource = det;
        }

        private void ClearForLoad()
        {
            listBox1.DataSource = null;
            listBox2.DataSource = null;
            pictureBox1.Image = null;
            pictureBox2.Image = null;
            label1.Text = "";
            label2.Text = "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // load a htih_pz.txt file
            ClearForLoad();
            loadHashFile();
        }

        public override void updateProgress(int value)
        {
            progressBar1.Value = value;
        }

        public override void LoadList()
        {
            var viewlist = FilterMatchingTree();

            listBox1.DataSource = viewlist;
            listBox1.SelectedIndex = -1;
            if (viewlist.Count > 0)
                listBox1.SelectedIndex = 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ClearForLoad();
            Reset();
        }

        public override void SetNote(string text)
        {
        }
    }
}
