namespace ChivServ
{
    partial class Init
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label_passwd = new System.Windows.Forms.Label();
            this.label_IPAddr = new System.Windows.Forms.Label();
            this.label_Port = new System.Windows.Forms.Label();
            this.pass = new System.Windows.Forms.TextBox();
            this.addr = new System.Windows.Forms.TextBox();
            this.port = new System.Windows.Forms.TextBox();
            this.Proceed = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label_passwd
            // 
            this.label_passwd.AutoSize = true;
            this.label_passwd.Location = new System.Drawing.Point(17, 78);
            this.label_passwd.Name = "label_passwd";
            this.label_passwd.Size = new System.Drawing.Size(62, 12);
            this.label_passwd.TabIndex = 0;
            this.label_passwd.Text = "Password";
            // 
            // label_IPAddr
            // 
            this.label_IPAddr.AutoSize = true;
            this.label_IPAddr.Location = new System.Drawing.Point(12, 15);
            this.label_IPAddr.Name = "label_IPAddr";
            this.label_IPAddr.Size = new System.Drawing.Size(67, 12);
            this.label_IPAddr.TabIndex = 1;
            this.label_IPAddr.Text = "IP Address";
            // 
            // label_Port
            // 
            this.label_Port.AutoSize = true;
            this.label_Port.Location = new System.Drawing.Point(33, 48);
            this.label_Port.Name = "label_Port";
            this.label_Port.Size = new System.Drawing.Size(27, 12);
            this.label_Port.TabIndex = 2;
            this.label_Port.Text = "Port";
            // 
            // pass
            // 
            this.pass.Location = new System.Drawing.Point(91, 75);
            this.pass.Name = "pass";
            this.pass.Size = new System.Drawing.Size(100, 21);
            this.pass.TabIndex = 3;
            // 
            // addr
            // 
            this.addr.Location = new System.Drawing.Point(91, 12);
            this.addr.Name = "addr";
            this.addr.Size = new System.Drawing.Size(100, 21);
            this.addr.TabIndex = 4;
            this.addr.Text = "127.0.0.1";
            // 
            // port
            // 
            this.port.Location = new System.Drawing.Point(91, 45);
            this.port.Name = "port";
            this.port.Size = new System.Drawing.Size(100, 21);
            this.port.TabIndex = 5;
            this.port.Text = "27960";
            // 
            // Proceed
            // 
            this.Proceed.Location = new System.Drawing.Point(24, 107);
            this.Proceed.Name = "Proceed";
            this.Proceed.Size = new System.Drawing.Size(75, 35);
            this.Proceed.TabIndex = 6;
            this.Proceed.Text = "Proceed";
            this.Proceed.UseVisualStyleBackColor = true;
            this.Proceed.Click += new System.EventHandler(this.Proceed_Click);
            // 
            // Cancel
            // 
            this.Cancel.Location = new System.Drawing.Point(116, 107);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 35);
            this.Cancel.TabIndex = 7;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // Init
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(203, 154);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.Proceed);
            this.Controls.Add(this.port);
            this.Controls.Add(this.addr);
            this.Controls.Add(this.pass);
            this.Controls.Add(this.label_Port);
            this.Controls.Add(this.label_IPAddr);
            this.Controls.Add(this.label_passwd);
            this.Name = "Init";
            this.Text = "Init";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_passwd;
        private System.Windows.Forms.Label label_IPAddr;
        private System.Windows.Forms.Label label_Port;
        private System.Windows.Forms.TextBox pass;
        private System.Windows.Forms.TextBox addr;
        private System.Windows.Forms.TextBox port;
        private System.Windows.Forms.Button Proceed;
        private System.Windows.Forms.Button Cancel;
    }
}