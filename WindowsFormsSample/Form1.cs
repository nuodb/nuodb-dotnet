using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsSample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'testDataSet.HOCKEY' table. You can move, or remove it, as needed.
            this.hOCKEYTableAdapter.Fill(this.testDataSet.HOCKEY);

            // This LINQ query could be also reusing the this.testDataSet.HOCKEY data set, instead of reloading the table
            var players = from player in this.hOCKEYTableAdapter.GetData()
                          where player.POSITION == "Fan"
                          select new
                          {
                              name = player.NAME,
                              team = player.TEAM
                          };
            this.bestFan.Text = players.First().name;
        }
    }
}
