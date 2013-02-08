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
            try
            {
                // TODO: This line of code loads data into the 'testDataSet.HOCKEY' table. You can move, or remove it, as needed.
                this.hOCKEYTableAdapter.Fill(this.testDataSet.HOCKEY);

                // This LINQ query works against a new in-memory dataset; it could be also reusing the this.testDataSet.HOCKEY 
                // data set, instead of reloading the table
                /*
                var players = from player in this.hOCKEYTableAdapter.GetData()
                              where player.POSITION == "Fan"
                              select new
                              {
                                  name = player.NAME,
                                  team = player.TEAM
                              };
                this.bestFan.Text = players.First().name;
                */

                // This LINQ query generates a SQL query on the fly, avoiding the need to load the entire table in memory
                testEntities ctx = new testEntities();
                var players = from player in ctx.HOCKEY
                              where player.POSITION == "Fan"
                              select player;

                this.bestFan.Text = players.First().NAME;

            }
            catch (Exception excp)
            {
                MessageBox.Show(excp.InnerException.Message);
            }
        }
    }
}
