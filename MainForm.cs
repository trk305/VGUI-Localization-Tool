using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace VGUILocalizationTool
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (cbLocal.Text != "")
                {
                    Properties.Settings.Default.DefLang = cbLocal.Text;
                }

                tbEnglish.Text = openFileDialog1.FileName;
                string path = Path.GetDirectoryName(tbEnglish.Text);
                openFileDialog1.InitialDirectory = path;
                string fileName = Path.GetFileNameWithoutExtension(tbEnglish.Text);
                int pos = fileName.IndexOf("_english");
                if (pos >= 0)
                {
                    fileName = fileName.Remove(pos);
                    pos++;
                }
                string ext = Path.GetExtension(tbEnglish.Text);
                var allfiles = 
                    from s in Directory.GetFiles(path+"\\", fileName + "*" + ext)
                    orderby s
                    select Path.GetFileNameWithoutExtension(s);
                cbLocal.Items.Clear();
                foreach (string fn in allfiles)
                {
                    if (fn.IndexOf("_english") == -1)
                    {
                        cbLocal.Items.Add(fn.Substring(pos));
                    }
                }

                localizationDataBindingSource.DataSource = new List<LocalizationData>();
                lblPerc.Text = "";
                if (Properties.Settings.Default.DefLang != "")
                {
                    int i = cbLocal.Items.IndexOf(Properties.Settings.Default.DefLang);

                    if (i >= 0)
                    {
                        cbLocal.SelectedIndex = i;
                    }
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            AddLocal local = new AddLocal();

            if (local.ShowDialog() == DialogResult.OK)
            {
                cbLocal.Items.Add(local.tbLocal.Text);
            }
        }

        ValveLocalizationFile file;

        bool Localized(string en, string lc)
        {
            if (lc == null)
            {
                return false;
            }
            else if (en.Length <= 2)
            {
                return true;
            }
            else if (en.Length == lc.Length)
            {
                if (en != lc)
                {
                    return true;
                }
                else
                {
                    foreach (char c in en)
                    {
                        if (Char.IsLetter(c))
                        {
                            return false;
                        }
                    }

                    // Skip text without letters
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        private void cbLocal_SelectedIndexChanged(object sender, EventArgs e)
        {
            file = new ValveLocalizationFile(tbEnglish.Text);
            List<LocalizationData> eng = file.ReadData("english");
            List<LocalizationData> loc = file.ReadData((string)cbLocal.SelectedItem);
            cbSaveWithEnglish.Checked = file.WithEnglishText;
            int tcount = 0;
            int lcount = 0;
            foreach (var en in eng)
            {
                if (en.ID == null)
                {
                    continue;
                }
                tcount++;

                var lc = (from l in loc where en.ID == l.ID select l).SingleOrDefault();
                en.English = en.Localized;
                en.DelimeterEnglish = en.DelimeterLocalized;
                if (lc != null && lc.Localized != null)
                {
                    en.EnglishTextChanged = (lc.English != null && en.English != lc.English);
                    en.EnglishOld = lc.English;
                    en.Localized = lc.Localized;
                    en.UseSlashN = lc.UseSlashN;
                    en.DelimeterLocalized = lc.DelimeterLocalized;
                    if (Localized(en.English, lc.Localized))
                    {
                        lcount++;
                    }
                }
            }
            if (tcount == 0)
            {
                tcount = 1;
            }
            lblPerc.Text = String.Format("{0:F}%", (1.0f * lcount / tcount) * 100);
            localizationDataBindingSource.DataSource = eng;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (file == null)
            {
                return;
            }
            List<LocalizationData> loc = (List<LocalizationData>)localizationDataBindingSource.DataSource;
            file.WithEnglishText = cbSaveWithEnglish.Checked;
            file.DontSaveNotLocalized = cbDontSaveNotLocalized.Checked;
            file.WriteData((string)cbLocal.SelectedItem, loc);
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            LocalizationData data;
            do 
            {
                localizationDataBindingSource.MoveNext();
                data = (LocalizationData) localizationDataBindingSource.Current;
            } while ((localizationDataBindingSource.Position + 1 < localizationDataBindingSource.Count) &&
                (data.ID == null || (!data.EnglishTextChanged && Localized(data.English, data.Localized))));
            this.Invalidate();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.InitialDir = openFileDialog1.InitialDirectory;
            Properties.Settings.Default.FindText = dialog.tbText.Text;
            Properties.Settings.Default.IDDefault = dialog.rbID.Checked;
            Properties.Settings.Default.EnglishDefault = dialog.rbEnglish.Checked;
            Properties.Settings.Default.LocalizedDefault = dialog.rbLocalized.Checked;
            Properties.Settings.Default.SaveBackup = cbSaveBackup.Checked;
            if (cbLocal.Text != "")
            {
                Properties.Settings.Default.DefLang = cbLocal.Text;
            }
            Properties.Settings.Default.Save();
        }

        FindTextDialog dialog = new FindTextDialog();

        internal void FindNext()
        {
            string text = dialog.tbText.Text.ToLower();
            int ind = dialog.rbID.Checked ? 0 : dialog.rbEnglish.Checked ? 1 : 2;
            if (text != "")
            {
                LocalizationData data;
                bool gotoNext;
                do
                {
                    localizationDataBindingSource.MoveNext();
                    data = (LocalizationData)localizationDataBindingSource.Current;
                    gotoNext = (localizationDataBindingSource.Position + 1 < localizationDataBindingSource.Count);
                    if (gotoNext)
                    {
                        if (data.ID != null)
                        {
                            switch (ind)
                            {
                            	case 0:
                                    gotoNext = (data.ID.ToLower().IndexOf(text) == -1);
                            		break;
                            	case 1:
                                    gotoNext = (data.English.ToLower().IndexOf(text) == -1);
                            		break;
                            	case 2:
                                    gotoNext = (data.Localized.ToLower().IndexOf(text) == -1);
                            		break;
                            }
                        }
                    }
                } while (gotoNext);
                this.Invalidate();
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Q)
            {
                btnNext_Click(null, null);
            }
            else if (e.Control && e.KeyCode == Keys.F)
            {
                btFind_Click(null, null);
            }
            else if (e.KeyCode == Keys.F3)
            {
                FindNext();            
            }
        }

        private void btFind_Click(object sender, EventArgs e)
        {
            dialog.mainForm = this;
            dialog.Show();
        }
    }
}
