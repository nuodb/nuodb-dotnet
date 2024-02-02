using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApplicationSample
{
    public partial class _Default : System.Web.UI.Page
    {
        hockeyEntities hockey;

        protected void Page_Load(object sender, EventArgs e)
        {
            hockey = new hockeyEntities();

            if (!IsPostBack)
            {
                var teams = from team in hockey.TEAMS
                            select new
                            {
                                team.TEAMID,
                                team.NAME
                            };
                DropDownList1.DataValueField = "TEAMID";
                DropDownList1.DataTextField = "NAME";
                DropDownList1.DataSource = teams.Distinct().OrderBy(t => t.NAME);
                DataBind();
            }

        }

        protected void DropDownList1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var players = from player in
                              (from roster in hockey.SCORING
                               where roster.TEAMID == DropDownList1.SelectedValue
                               select new
                               {
                                   NAME = roster.PLAYERS.FIRSTNAME + " " + roster.PLAYERS.LASTNAME,
                                   GOALS = roster.GOALS,
                                   TEAM = roster.TEAMS.NAME
                               })
                          group player by player.NAME into goals
                          select new
                          {
                              NAME = goals.Key,
                              GOALS = goals.Sum(s => s.GOALS)
                          };
            GridView1.DataSource = players.OrderByDescending(p => p.GOALS);
            DataBind();
        }
    }
}
