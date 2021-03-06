﻿using OpenTK;
using OpenTKLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace Testing
{
	public class UI : Form
	{
		public OpenGLUC OpenGLControl;
		private Panel panelOpenTK; //Move this for Moving Renderer
		private Label label1;
		private Label label2;
		private PerformanceCheck pc = new PerformanceCheck();


		public List<Particle> GetPoints()
		{
			var list = OpenGLControl.OGLControl.GLrender.RenderableObjects;
			List<Particle> Particles = new List<Particle>();
			var p1 = Parallel.ForEach(list.ToArray(), ((RenderableObject f) =>
			{
				var list2 = new List<Particle>();
				foreach (Vector3 f2 in f.PointCloud.Vectors)
				{
					list2.Add(new Particle(f2.X, f2.Y, f2.Z));
				}
				Particles.AddRange(list2); //used for parallel
			}));
			while (!p1.IsCompleted)
			{
				Thread.Sleep(1);
			}
			return Particles;
		}

		public void InitializeComponent()
		{
			this.panelOpenTK = new System.Windows.Forms.Panel();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// panelOpenTK
			// 
			this.panelOpenTK.BackColor = System.Drawing.SystemColors.ControlDark;
			this.panelOpenTK.Location = new System.Drawing.Point(0, 0);
			this.panelOpenTK.Name = "panelOpenTK";
			this.panelOpenTK.Size = new System.Drawing.Size(887, 580);
			this.panelOpenTK.TabIndex = 1;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 587);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(35, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "label1";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(13, 604);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(35, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "label2";
			// 
			// UI
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(886, 757);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.panelOpenTK);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "UI";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "OpenTK Form";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.UI_FormClosed);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		public UI()
		{
			Thread.CurrentThread.CurrentUICulture = new CultureInfo(CultureInfo.CurrentCulture.LCID);
			InitializeComponent();

			AddOpenGLControl();

			if (!GLSettings.IsInitializedFromSettings)
				GLSettings.InitFromSettings();

			this.Height = GLSettings.Height;
			this.Width = GLSettings.Width;
			this.Text = "UI";

			Timer timer = new System.Windows.Forms.Timer()
			{
				Interval = 1000,
			};
			timer.Tick += Tick;
			timer.Enabled = true;
		}

		private void Tick(object sender, EventArgs e)
		{
			label1.Text = $"CPU Usage: {pc.CpuUsage()}%";

			label2.Text = $"Point Count: {GetPoints().Count()}";
		}

		private void AddOpenGLControl()
		{
			this.OpenGLControl = new OpenGLUC();
			this.SuspendLayout();
			// 
			// openGLControl1
			// 
			this.OpenGLControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OpenGLControl.Location = new System.Drawing.Point(0, 0);
			this.OpenGLControl.Name = "openGLControl1";
			this.OpenGLControl.Size = new System.Drawing.Size(854, 453);
			this.OpenGLControl.TabIndex = 0;

			panelOpenTK.Controls.Add(this.OpenGLControl);

			this.ResumeLayout(false);
		}

		protected override void OnClosed(EventArgs e)
		{
			GlobalVariables.FormFast = null;
			GLSettings.Height = this.Height;
			GLSettings.Width = this.Width;

			GLSettings.SaveSettings();
			base.OnClosed(e);
		}
		public void AddVerticesAsModel(string name, PointCloudVertices myPCLList)
		{
			this.OpenGLControl.AddVertexListAsModel(name, myPCLList);
		}
		public void ShowModel(Model myModel)
		{
			this.OpenGLControl.ShowModel(myModel);
		}

		public void ShowPointCloudOpenGL(PointCloud myP, bool removeOthers)
		{
			if (removeOthers)
				this.OpenGLControl.RemoveAllModels();

			Model myModel = new Model();
			myModel.PointCloud = myP;

			this.OpenGLControl.OGLControl.GLrender.AddModel(myModel);

		}
		/// <summary>
		/// at least source points should be non zero
		/// </summary>
		/// <param name="myPCLTarget"></param>
		/// <param name="myPCLSource"></param>
		/// <param name="myPCLResult"></param>
		/// <param name="changeColor"></param>
		public void Show3PointCloudOpenGL(PointCloud myPCLSource, PointCloud myPCLTarget, PointCloud myPCLResult, bool changeColor)
		{

			this.OpenGLControl.RemoveAllModels();

			//target in green

			if (myPCLTarget != null)
			{

				if (changeColor)
				{
					myPCLTarget.Colors = ColorExtensions.ToVector3Array(myPCLTarget.Vectors.GetLength(0), 0, 255, 0);

				}
				ShowPointCloudOpenGL(myPCLTarget, false);

			}

			if (myPCLSource != null)
			{
				//source in white

				if (changeColor)
					myPCLSource.Colors = ColorExtensions.ToVector3Array(myPCLSource.Vectors.GetLength(0), 255, 255, 255);

				ShowPointCloudOpenGL(myPCLSource, false);

			}

			if (myPCLResult != null)
			{

				//transformed in red
				if (changeColor)
					myPCLResult.Colors = ColorExtensions.ToVector3Array(myPCLResult.Vectors.GetLength(0), 255, 0, 0);

				ShowPointCloudOpenGL(myPCLResult, false);



			}

		}

		/// <summary>
		/// at least source points should be non zero
		/// </summary>
		/// <param name="myPCLTarget"></param>
		/// <param name="myPCLSource"></param>
		/// <param name="myPCLResult"></param>
		/// <param name="changeColor"></param>
		public void Show3PointClouds(PointCloudVertices myPCLSource, PointCloudVertices myPCLTarget, PointCloudVertices myPCLResult, bool changeColor)
		{

			this.OpenGLControl.RemoveAllModels();

			//target in green
			List<System.Drawing.Color> myColors;
			if (myPCLTarget != null)
			{

				if (changeColor)
				{
					myColors = ColorExtensions.ToColorList(myPCLTarget.Count, 0, 255, 0, 255);
					PointCloudVertices.SetColorToList(myPCLTarget, myColors);
				}
				this.OpenGLControl.ShowPointCloud("ICP Target", myPCLTarget);

			}

			if (myPCLSource != null)
			{
				//source in white
				myColors = ColorExtensions.ToColorList(myPCLSource.Count, 255, 255, 255, 255);
				if (changeColor)
					PointCloudVertices.SetColorToList(myPCLSource, myColors);
				this.OpenGLControl.ShowPointCloud("ICP To be matched", myPCLSource);

			}

			if (myPCLResult != null)
			{

				//transformed in red
				myColors = ColorExtensions.ToColorList(myPCLResult.Count, 255, 0, 0, 255);
				if (changeColor)
					PointCloudVertices.SetColorToList(myPCLResult, myColors);
				this.OpenGLControl.ShowPointCloud("ICP Solution", myPCLResult);

			}

		}

		public void ClearModels()
		{
			OpenGLControl.RemoveAllModels();
		}

		public bool UpdateFirstModel(PointCloudVertices pc)
		{
			//ClearModels();


			ShowPointCloud(pc);
			return true;
		}

		public void ShowPointCloud(PointCloudVertices pc)
		{

			this.OpenGLControl.ShowPointCloud("Color Point Cloud", pc);

		}


		private void OpenTKForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (GlobalVariables.FormFast != null)
				GlobalVariables.FormFast.Dispose();

			GlobalVariables.FormFast = null;
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			this.OpenGLControl.OGLControl.MouseWheelActions(e);
			base.OnMouseWheel(e);
		}

		public bool UpdatePointCloud(PointCloudVertices pc)
		{
			if (pc != null && pc.Count > 0)
				this.OpenGLControl.ShowPointCloud("Color Point Cloud", pc);
			return true;
		}

			public void IPCOnTwoPointClouds()
		{

			this.OpenGLControl.RemoveAllModels();
			//this.OpenGLControl.OpenTwoTrialPointClouds();

		}

		#region hide
		private void UI_FormClosed(object sender, FormClosedEventArgs e)
		{
			OnClosed(e);
		}
#endregion
	}
}
