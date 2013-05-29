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
                // TODO: This line of code loads data into the 'testDataSet.SCORING' table. You can move, or remove it, as needed.
                this.sCORINGTableAdapter.Fill(this.testDataSet.SCORING);

                // This LINQ query works against a new in-memory dataset; it could be also reusing the this.testDataSet.SCORING 
                // data set, instead of reloading the table
                /*
                var bestPlayer = (from player in ctx.sCORINGTableAdapter.GetData()
                                   group player by player.PLAYERID into goals
                                   select new 
                                   {
                                      Player = goals.Key,
                                      CareerGoals = goals.Sum(player => player.G)
                                   }).OrderByDescending(p => p.CareerGoals).First();
                */

                // This LINQ query generates a SQL query on the fly, avoiding the need to load the entire table in memory
                testEntities ctx = new testEntities();
                var bestPlayer = (from player in ctx.SCORING
                                   /*group player by player.PLAYER.FIRSTNAME + " " + player.PLAYER.LASTNAME into goals*/
                                  group player by player.PLAYERID into goals
                                   select new 
                                   {
                                      Player = goals.Key,
                                      CareerGoals = goals.Sum(player => player.GOALS)
                                   }).OrderByDescending(p => p.CareerGoals).First();
                var playerName = from master in ctx.PLAYERS
                                 where master.PLAYERID == bestPlayer.Player
                                 select master.FIRSTNAME + " " + master.LASTNAME;
                this.bestScorer.Text = bestPlayer.Player + " (" + bestPlayer.CareerGoals + " goals)";

            }
            catch (Exception excp)
            {
                if (excp.InnerException != null)
                    MessageBox.Show(excp.InnerException.Message);
                else
                    MessageBox.Show(excp.Message);
            }
        }

        private void sCORINGBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            this.Validate();
            this.sCORINGBindingSource.EndEdit();
            this.tableAdapterManager.UpdateAll(this.testDataSet);

        }

        private void sCORINGBindingNavigatorSaveItem_Click_1(object sender, EventArgs e)
        {
            this.Validate();
            this.sCORINGBindingSource.EndEdit();
            this.tableAdapterManager.UpdateAll(this.testDataSet);

        }
    }
}
