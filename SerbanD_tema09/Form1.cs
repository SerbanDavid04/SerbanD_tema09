using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace SerbanD_tema09
{
    public partial class Form1 : Form
    {
        GLControl glControl;
        NumericUpDown numericCycles;
        Button buttonTrage;
        Label labelResult;
        Timer timer;

        int[] textures = new int[4];
        int[] slots = new int[3];

        int cyclesLeft = 0;
        bool spinning = false;

        Random rng = new Random();

        public Form1()
        {
            InitializeComponent();
            InitUI();
        }

        private void InitUI()
        {
            // GLControl
            glControl = new GLControl(new GraphicsMode(32, 24, 0, 4));
            glControl.Dock = DockStyle.Top;
            glControl.Height = 350;
            glControl.Load += GlControl_Load;
            glControl.Paint += GlControl_Paint;
            glControl.Resize += GlControl_Resize;
            Controls.Add(glControl);

            // NumericUpDown
            numericCycles = new NumericUpDown();
            numericCycles.Minimum = 1;
            numericCycles.Maximum = 50;
            numericCycles.Value = 10;
            numericCycles.Location = new Point(20, 360);
            numericCycles.Width = 80;
            Controls.Add(numericCycles);

            var lbl = new Label();
            lbl.Text = "(0.5s fiecare)";
            lbl.Location = new Point(120, 362);
            lbl.AutoSize = true;
            Controls.Add(lbl);

            // Button Trage
            buttonTrage = new Button();
            buttonTrage.Text = "TRAGE!";
            buttonTrage.Size = new Size(120, 40);
            
            buttonTrage.Location = new Point(650, 350);

            buttonTrage.Font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold);
            buttonTrage.FlatStyle = FlatStyle.Flat;
            buttonTrage.FlatAppearance.BorderSize = 0;
            buttonTrage.BackColor = Color.FromArgb(0, 180, 80);
            buttonTrage.ForeColor = Color.White;

            // hover
            buttonTrage.MouseEnter += (s, e) =>
            {
                buttonTrage.BackColor = Color.FromArgb(0, 210, 100);
            };
            buttonTrage.MouseLeave += (s, e) =>
            {
                buttonTrage.BackColor = Color.FromArgb(0, 180, 80);
            };

            buttonTrage.Click += ButtonTrage_Click;
            Controls.Add(buttonTrage);



            // Rezultat 
            labelResult = new Label();
            labelResult.Font = new Font(FontFamily.GenericSansSerif, 18, FontStyle.Bold);
            labelResult.AutoSize = false;
            labelResult.Size = new Size(400, 50);
            labelResult.Location = new Point(300, 380);
            labelResult.TextAlign = ContentAlignment.MiddleLeft;
            labelResult.BackColor = Color.Transparent;
            Controls.Add(labelResult);


            // Timer 
            timer = new Timer();
            timer.Interval = 500;
            timer.Tick += Timer_Tick;
        }


        private void GlControl_Load(object sender, EventArgs e)
        {
            glControl.MakeCurrent();

            GL.ClearColor(Color.FromArgb(10, 80, 35));


            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            SetupViewport();
            LoadTextures();
            Randomize();
        }

        private void GlControl_Resize(object sender, EventArgs e)
        {
            if (!glControl.Context.IsCurrent)
                glControl.MakeCurrent();

            SetupViewport();
            glControl.Invalidate();
        }

        private void SetupViewport()
        {
            GL.Viewport(0, 0, glControl.Width, glControl.Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, glControl.Width, 0, glControl.Height, -1, 1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }

        // Textures 
        private void LoadTextures()
        {
            string[] files =
            {
                "Images/img0.png",
                "Images/img1.png",
                "Images/img2.png",
                "Images/img3.png"
            };

            GL.GenTextures(4, textures);

            for (int i = 0; i < 4; i++)
                LoadTexture(files[i], textures[i]);
        }

        private void LoadTexture(string file, int id)
        {
            GL.BindTexture(TextureTarget.Texture2D, id);

            if (!System.IO.File.Exists(file))
            {
                byte[] white = { 255, 255, 255, 255 };
                GL.TexImage2D(TextureTarget.Texture2D, 0,
                    PixelInternalFormat.Rgba, 1, 1, 0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                    PixelType.UnsignedByte, white);
            }
            else
            {
                using (Bitmap bmp = new Bitmap(file))
                {
                    bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

                    BitmapData data = bmp.LockBits(
                        new Rectangle(0, 0, bmp.Width, bmp.Height),
                        ImageLockMode.ReadOnly,
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    GL.TexImage2D(
                        TextureTarget.Texture2D, 0,
                        PixelInternalFormat.Rgba,
                        data.Width, data.Height, 0,
                        OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                        PixelType.UnsignedByte,
                        data.Scan0);

                    bmp.UnlockBits(data);
                }
            }

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        }


        private void GlControl_Paint(object sender, PaintEventArgs e)
        {
            if (!glControl.Context.IsCurrent)
                glControl.MakeCurrent();

            GL.Clear(ClearBufferMask.ColorBufferBit);

            float w = glControl.Width;
            float h = glControl.Height;

            float slotW = w / 4;
            float slotH = h * 0.8f;
            float cy = h / 2;

            for (int i = 0; i < 3; i++)
            {
                float cx = (i + 1) * w / 4;
                DrawSlot(slots[i], cx, cy, slotW, slotH);
            }

            glControl.SwapBuffers();
        }

        private void DrawSlot(int index, float cx, float cy, float w, float h)
        {
            float hw = w / 2, hh = h / 2;
            float left = cx - hw, right = cx + hw, bottom = cy - hh, top = cy + hh;

            GL.BindTexture(TextureTarget.Texture2D, textures[index]);

            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(0, 0); GL.Vertex2(left, bottom);
            GL.TexCoord2(1, 0); GL.Vertex2(right, bottom);
            GL.TexCoord2(1, 1); GL.Vertex2(right, top);
            GL.TexCoord2(0, 1); GL.Vertex2(left, top);
            GL.End();

            GL.BindTexture(TextureTarget.Texture2D, 0);

            // contur
            GL.Color3(Color.White);
            GL.LineWidth(3f);
            GL.Begin(PrimitiveType.LineLoop);
            GL.Vertex2(left, bottom);
            GL.Vertex2(right, bottom);
            GL.Vertex2(right, top);
            GL.Vertex2(left, top);
            GL.End();
            GL.Color3(Color.White);
        }


        private void Randomize()
        {
            for (int i = 0; i < 3; i++)
                slots[i] = rng.Next(0, 4);
        }

        private void ButtonTrage_Click(object sender, EventArgs e)
        {
            if (spinning) return;

            cyclesLeft = (int)numericCycles.Value;
            spinning = true;
            labelResult.Text = "";
            labelResult.BackColor = Color.Transparent;   

            timer.Start();
        }


        private void Timer_Tick(object sender, EventArgs e)
        {
            Randomize();
            glControl.Invalidate();

            cyclesLeft--;
            if (cyclesLeft <= 0)
            {
                timer.Stop();
                spinning = false;
                CheckWin();
            }
        }

        private void CheckWin()
        {
            bool win = (slots[0] == slots[1] && slots[1] == slots[2]);

            if (win)
            {
                labelResult.ForeColor = Color.Gold;
                labelResult.Text = "AI CASTIGAT!";
            }
            else
            {
                labelResult.ForeColor = Color.FromArgb(220, 50, 50);
                labelResult.Text = "AI PIERDUT!";
            }
        }

    }
}
