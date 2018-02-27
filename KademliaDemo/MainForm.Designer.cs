namespace KademliaDemo
{
    partial class MainForm
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
            this.pnlFlowSharp = new System.Windows.Forms.Panel();
            this.btnStep = new System.Windows.Forms.Button();
            this.btnRun = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnBucketRefresh = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.nudPeerNumber = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.btnPublish = new System.Windows.Forms.Button();
            this.btnRepublish = new System.Windows.Forms.Button();
            this.ckNodesTopmost = new System.Windows.Forms.CheckBox();
            this.ckShowConnections = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPeerNumber)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlFlowSharp
            // 
            this.pnlFlowSharp.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlFlowSharp.Location = new System.Drawing.Point(197, 4);
            this.pnlFlowSharp.Name = "pnlFlowSharp";
            this.pnlFlowSharp.Size = new System.Drawing.Size(636, 510);
            this.pnlFlowSharp.TabIndex = 0;
            // 
            // btnStep
            // 
            this.btnStep.Location = new System.Drawing.Point(6, 19);
            this.btnStep.Name = "btnStep";
            this.btnStep.Size = new System.Drawing.Size(75, 23);
            this.btnStep.TabIndex = 1;
            this.btnStep.Text = "Step";
            this.btnStep.UseVisualStyleBackColor = true;
            this.btnStep.Click += new System.EventHandler(this.btnStep_Click);
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(97, 19);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(75, 23);
            this.btnRun.TabIndex = 2;
            this.btnRun.Text = "Run";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnRun);
            this.groupBox1.Controls.Add(this.btnStep);
            this.groupBox1.Location = new System.Drawing.Point(8, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(178, 57);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Bootstrapping";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnBucketRefresh);
            this.groupBox2.Location = new System.Drawing.Point(8, 75);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(178, 57);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Bucket Refresh";
            // 
            // btnBucketRefresh
            // 
            this.btnBucketRefresh.Enabled = false;
            this.btnBucketRefresh.Location = new System.Drawing.Point(52, 19);
            this.btnBucketRefresh.Name = "btnBucketRefresh";
            this.btnBucketRefresh.Size = new System.Drawing.Size(75, 23);
            this.btnBucketRefresh.TabIndex = 1;
            this.btnBucketRefresh.Text = "Step";
            this.btnBucketRefresh.UseVisualStyleBackColor = true;
            this.btnBucketRefresh.Click += new System.EventHandler(this.btnBucketRefresh_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnRepublish);
            this.groupBox3.Controls.Add(this.btnPublish);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.nudPeerNumber);
            this.groupBox3.Location = new System.Drawing.Point(8, 138);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(178, 103);
            this.groupBox3.TabIndex = 5;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Store";
            // 
            // nudPeerNumber
            // 
            this.nudPeerNumber.Location = new System.Drawing.Point(97, 19);
            this.nudPeerNumber.Name = "nudPeerNumber";
            this.nudPeerNumber.Size = new System.Drawing.Size(50, 20);
            this.nudPeerNumber.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Originating Peer:";
            // 
            // btnPublish
            // 
            this.btnPublish.Location = new System.Drawing.Point(52, 45);
            this.btnPublish.Name = "btnPublish";
            this.btnPublish.Size = new System.Drawing.Size(75, 23);
            this.btnPublish.TabIndex = 2;
            this.btnPublish.Text = "Publish";
            this.btnPublish.UseVisualStyleBackColor = true;
            this.btnPublish.Click += new System.EventHandler(this.btnPublish_Click);
            // 
            // btnRepublish
            // 
            this.btnRepublish.Location = new System.Drawing.Point(52, 74);
            this.btnRepublish.Name = "btnRepublish";
            this.btnRepublish.Size = new System.Drawing.Size(75, 23);
            this.btnRepublish.TabIndex = 3;
            this.btnRepublish.Text = "Republish";
            this.btnRepublish.UseVisualStyleBackColor = true;
            this.btnRepublish.Click += new System.EventHandler(this.btnRepublish_Click);
            // 
            // ckNodesTopmost
            // 
            this.ckNodesTopmost.AutoSize = true;
            this.ckNodesTopmost.Location = new System.Drawing.Point(13, 248);
            this.ckNodesTopmost.Name = "ckNodesTopmost";
            this.ckNodesTopmost.Size = new System.Drawing.Size(101, 17);
            this.ckNodesTopmost.TabIndex = 6;
            this.ckNodesTopmost.Text = "Nodes Topmost";
            this.ckNodesTopmost.UseVisualStyleBackColor = true;
            // 
            // ckShowConnections
            // 
            this.ckShowConnections.AutoSize = true;
            this.ckShowConnections.Checked = true;
            this.ckShowConnections.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckShowConnections.Location = new System.Drawing.Point(13, 271);
            this.ckShowConnections.Name = "ckShowConnections";
            this.ckShowConnections.Size = new System.Drawing.Size(115, 17);
            this.ckShowConnections.TabIndex = 7;
            this.ckShowConnections.Text = "Show Connections";
            this.ckShowConnections.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(834, 516);
            this.Controls.Add(this.ckShowConnections);
            this.Controls.Add(this.ckNodesTopmost);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.pnlFlowSharp);
            this.Name = "MainForm";
            this.Text = "Kademlia Demo";
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPeerNumber)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel pnlFlowSharp;
        private System.Windows.Forms.Button btnStep;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnBucketRefresh;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btnRepublish;
        private System.Windows.Forms.Button btnPublish;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown nudPeerNumber;
        private System.Windows.Forms.CheckBox ckNodesTopmost;
        private System.Windows.Forms.CheckBox ckShowConnections;
    }
}

