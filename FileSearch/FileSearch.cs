//
// <copyright company="Softerra">
//    Copyright (c) Softerra, Ltd. All rights reserved.
// </copyright>
//
// <summary>
//    Search form.
// </summary>
//
// <author email="mititch@softerra.com">Alex Mitin</author>
//
namespace FileSearch
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;
    using System.Windows.Forms;
    using Lib;
    using Visual;

    public partial class FileSearch : Form
    {
        private const Int32 THREAD_COUNT = 5;
        
        private const Int32 ELEMENT_COUNT = 5;
        
        private const Int32 TIMER_INTERVAL = 500;

        private const string FILE_NAME = "C:/Users/mititch/Downloads/bf/f2.txt";

        private const string SOME_STRING = "have very many outstanding loans but I do need to consolidate and move ";

        private System.Windows.Forms.Timer formTimer;
        
        private readonly Random random;

        private readonly VisualBinding[] visualBindings;

        private readonly Searcher searcher;
        
        public FileSearch()
        {
            this.searcher = new Searcher(FILE_NAME, THREAD_COUNT);
            this.formTimer = new System.Windows.Forms.Timer();
            this.random = new Random();
            this.visualBindings = new VisualBinding[ELEMENT_COUNT];
            InitializeComponent();
        }

        private void FileSearch_Load(object sender, EventArgs e)
        {
            this.visualBindings[0] = new VisualBinding
            {
                TextBox = this.textBox1,
                CancelBtn = this.cancel1,
                SearchBtn = this.search1,
                Label = this.result1
            };
            visualBindings[1] = new VisualBinding
            {
                TextBox = this.textBox2,
                CancelBtn = this.cancel2,
                SearchBtn = this.search2,
                Label = this.result2
            };
            visualBindings[2] = new VisualBinding
            {
                TextBox = this.textBox3,
                CancelBtn = this.cancel3,
                SearchBtn = this.search3,
                Label = this.result3
            };
            visualBindings[3] = new VisualBinding
            {
                TextBox = this.textBox4,
                CancelBtn = this.cancel4,
                SearchBtn = this.search4,
                Label = this.result4
            };
            visualBindings[4] = new VisualBinding
            {
                TextBox = this.textBox5,
                CancelBtn = this.cancel5,
                SearchBtn = this.search5,
                Label = this.result5
            };

            foreach (var visualBinding in visualBindings)
            {
                visualBinding.TextBox.Text = String.Format("{0} {1}", SOME_STRING, random.Next(100));
                visualBinding.CancelBtn.Enabled = false;
                visualBinding.Label.Text = "0";
                visualBinding.SearchBtn.Click += new EventHandler(SearchBtn_Click);
                visualBinding.CancelBtn.Click += new EventHandler(CancelBtn_Click);
            }

            formTimer.Tick += new EventHandler((s, ea) => { RefreshResults(); });
            formTimer.Interval = TIMER_INTERVAL;
            formTimer.Start();
        }

        void CancelBtn_Click(object sender, EventArgs e)
        {
            VisualBinding vb = visualBindings.FirstOrDefault(x => x.CancelBtn.Equals((Button)sender));
            vb.SearchBtn.Enabled = true;
            vb.CancelBtn.Enabled = false;
            vb.Result.Cancel();
            vb.Result.Dispose(); 
            vb.Result = null;
        }

        void  SearchBtn_Click(object sender, EventArgs e)
        {
            VisualBinding vb = visualBindings.FirstOrDefault(x => x.SearchBtn.Equals((Button) sender));
            vb.SearchBtn.Enabled = false;
            vb.CancelBtn.Enabled = true;
            vb.Result = this.searcher.Search(vb.TextBox.Text);
            vb.Label.Text = "0";
        }

        private void refresh_Click(object sender, EventArgs e)
        {
            RefreshResults();
        }

        private void startAll_Click(object sender, EventArgs e)
        {
            foreach (var visualBinding in visualBindings)
            {
                Result result = visualBinding.Result;
                if (result == null)
                {
                    visualBinding.SearchBtn.Enabled = false;
                    visualBinding.CancelBtn.Enabled = true;
                    visualBinding.Result = this.searcher.Search(visualBinding.TextBox.Text);
                }
            }
        }

        private void collect_Click(object sender, EventArgs e)
        {
            GC.Collect();
        }

        private void RefreshResults()
        {
            foreach (VisualBinding visualBinding in visualBindings)
            {
                Result result = visualBinding.Result;
                if (result != null)
                {
                    visualBinding.Label.Text = result.Value.ToString();
                }
            }
        }

        private void clear_Click(object sender, EventArgs e)
        {
            foreach (VisualBinding visualBinding in visualBindings)
            {
                    visualBinding.Label.Text = "0"; 
            }
        }


    }
}
