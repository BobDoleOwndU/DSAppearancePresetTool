using System;
using System.Windows.Forms;

namespace DSRAppearancePresetTool
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Character Appearance Preset|*.dsrchr";
            saveFileDialog.Title = "Choose an output location";

            if(saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                MemoryHandler.ReadAppearanceData().Write(saveFileDialog.FileName);
            } //if
        } //saveButton_Click

        private void loadButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Character Appearance Preset|*.dsrchr";
            openFileDialog.Title = "Select the file to load";

            if(openFileDialog.ShowDialog() == DialogResult.OK)
            {
                AppearanceData appearanceData = new AppearanceData();
                appearanceData.Read(openFileDialog.FileName);

                MemoryHandler.WriteAppearanceData(appearanceData);
            } //if
        } //loadButton_Click
    } //class
} //namespace
