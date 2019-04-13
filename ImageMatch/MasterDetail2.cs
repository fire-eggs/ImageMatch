using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace howto_image_hash
{
    public partial class MasterDetail2 : MasterDetailBase
    {
        public MasterDetail2( Logger log, ArchiveLoader load)
            : base(log, load)
        {
            InitializeComponent();

            this.button1.Click += new System.EventHandler(this.button1_Click);
            this.button2.Click += new System.EventHandler(this.button2_Click);
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            this.listBox2.SelectedIndexChanged += new System.EventHandler(this.listBox2_SelectedIndexChanged);
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
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

        private void button2_Click(object sender, EventArgs e)
        {
            ClearForLoad();
            Reset();
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
        }
    }
}
