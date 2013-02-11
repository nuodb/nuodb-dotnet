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
                var positions = from player in hockey.HOCKEY
                                orderby player.NAME
                                select new
                                {
                                    player.POSITION
                                };
                DropDownList1.DataValueField = "Position";
                DropDownList1.DataTextField = "Position";
                DropDownList1.DataSource = positions.Distinct();
                DataBind();
            }

        }

        protected void DropDownList1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var players = from player in hockey.HOCKEY
                          where player.POSITION == DropDownList1.SelectedValue
                          orderby player.NAME
                          select new
                          {
                              player.NAME,
                              player.TEAM
                          };
            GridView1.DataSource = players;
            DataBind();
        }
    }
}
