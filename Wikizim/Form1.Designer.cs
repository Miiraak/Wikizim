namespace Wikizim
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            buttonNext = new Button();
            textBox = new TextBox();
            label1 = new Label();
            SuspendLayout();
            // 
            // buttonNext
            // 
            buttonNext.Location = new Point(102, 71);
            buttonNext.Name = "buttonNext";
            buttonNext.Size = new Size(75, 23);
            buttonNext.TabIndex = 0;
            buttonNext.Text = "Next";
            buttonNext.UseVisualStyleBackColor = true;
            buttonNext.Click += ButtonNext_Click;
            // 
            // textBox
            // 
            textBox.Location = new Point(12, 31);
            textBox.Name = "textBox";
            textBox.ReadOnly = true;
            textBox.Size = new Size(254, 23);
            textBox.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(50, 15);
            label1.TabIndex = 2;
            label1.Text = ".zim File";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            ClientSize = new Size(278, 106);
            Controls.Add(label1);
            Controls.Add(textBox);
            Controls.Add(buttonNext);
            Cursor = Cursors.Cross;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Form1";
            Text = "Wikizim";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button buttonNext;
        private TextBox textBox;
        private Label label1;
    }
}
