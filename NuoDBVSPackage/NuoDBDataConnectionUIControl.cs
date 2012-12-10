using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Data;

namespace NuoDB.VisualStudio.DataTools
{
    class NuoDBDataConnectionUIControl : DataConnectionUIControl
    {
        private System.Windows.Forms.TextBox textBoxServer;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxUsername;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxDatabase;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxSchema;
        private System.Windows.Forms.Label label1;

        public NuoDBDataConnectionUIControl()
        {
            System.Diagnostics.Trace.WriteLine("NuoDBDataConnectionUIControl()");
            InitializeComponent();
        }

        public override void LoadProperties()
        {
            System.Diagnostics.Trace.WriteLine("NuoDBDataConnectionUIControl::LoadProperties()");

            try
            {
                object obj = ConnectionProperties["Server"];
                if(obj is string)
                    this.textBoxServer.Text = (string)obj;
                obj = ConnectionProperties["Database"];
                if (obj is string)
                    this.textBoxDatabase.Text = (string)obj;
                obj = ConnectionProperties["User"];
                if (obj is string)
                    this.textBoxUsername.Text = (string)obj;
                obj = ConnectionProperties["Password"];
                if (obj is string)
                    this.textBoxPassword.Text = (string)obj;
                obj = ConnectionProperties["Schema"];
                if (obj is string)
                    this.textBoxSchema.Text = (string)obj;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }
        }

        private void SetProperty(object sender, EventArgs e)
        {
            if (sender.Equals(this.textBoxServer))
            {
                this.ConnectionProperties["Server"] = this.textBoxServer.Text;
            }
            else if (sender.Equals(this.textBoxDatabase))
            {
                this.ConnectionProperties["Database"] = this.textBoxDatabase.Text;
            }
            else if (sender.Equals(this.textBoxUsername))
            {
                this.ConnectionProperties["User"] = this.textBoxUsername.Text;
            }
            else if (sender.Equals(this.textBoxPassword))
            {
                this.ConnectionProperties["Password"] = this.textBoxPassword.Text;
            }
            else if (sender.Equals(this.textBoxSchema))
            {
                this.ConnectionProperties["Schema"] = this.textBoxSchema.Text;
            }
        }

        private void InitializeComponent()
        {
            this.textBoxServer = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxUsername = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxDatabase = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxSchema = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // textBoxServer
            // 
            this.textBoxServer.Location = new System.Drawing.Point(100, 0);
            this.textBoxServer.Name = "textBoxServer";
            this.textBoxServer.Size = new System.Drawing.Size(200, 20);
            this.textBoxServer.TabIndex = 1;
            this.textBoxServer.TextChanged += new System.EventHandler(this.SetProperty);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Broker address";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(0, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Username";
            // 
            // textBoxUsername
            // 
            this.textBoxUsername.Location = new System.Drawing.Point(100, 56);
            this.textBoxUsername.Name = "textBoxUsername";
            this.textBoxUsername.Size = new System.Drawing.Size(200, 20);
            this.textBoxUsername.TabIndex = 5;
            this.textBoxUsername.TextChanged += new System.EventHandler(this.SetProperty);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(0, 84);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Password";
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Location = new System.Drawing.Point(100, 84);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.Size = new System.Drawing.Size(200, 20);
            this.textBoxPassword.TabIndex = 7;
            this.textBoxPassword.TextChanged += new System.EventHandler(this.SetProperty);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(0, 28);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Database";
            // 
            // textBoxDatabase
            // 
            this.textBoxDatabase.Location = new System.Drawing.Point(100, 28);
            this.textBoxDatabase.Name = "textBoxDatabase";
            this.textBoxDatabase.Size = new System.Drawing.Size(200, 20);
            this.textBoxDatabase.TabIndex = 3;
            this.textBoxDatabase.TextChanged += new System.EventHandler(this.SetProperty);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(0, 112);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(81, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Default schema";
            // 
            // textBoxSchema
            // 
            this.textBoxSchema.Location = new System.Drawing.Point(100, 112);
            this.textBoxSchema.Name = "textBoxSchema";
            this.textBoxSchema.Size = new System.Drawing.Size(200, 20);
            this.textBoxSchema.TabIndex = 9;
            this.textBoxSchema.TextChanged += new System.EventHandler(this.SetProperty);
            // 
            // NuoDBDataConnectionUIControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.textBoxSchema);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBoxDatabase);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBoxPassword);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxUsername);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxServer);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "NuoDBDataConnectionUIControl";
            this.Size = new System.Drawing.Size(300, 154);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
