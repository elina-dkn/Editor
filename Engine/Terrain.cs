using Editor.Engine.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Editor.Engine
{
    internal class Terrain : ISelectable, IRenderable
    {
        public VertexPositionNormalTexture[] Vertices { get; set; } // Vertex array
        public VertexBuffer VertexBuffer { get; set; } // Vertex Buffer
        public int[] Indices { get; set; } // Index array
        public IndexBuffer IndexBuffer { get; set; } // Index buffer
        public float[,] Heights { get; set; } // Array of vertex heights
        public int Width { get; set; } // Number of vertices on x axis
        public int Length { get; set; } // Number of vertices on z axis
        public int Height { get; set; } // Terrain height factor
        public int VertexCount { get; set; } // Number of vertices
        public int IndexCount { get; set; } // Number of indices
        public GraphicsDevice Device { get; set; } // The graphicsdevice for rendering
        public Vector3 LightDirection { get; set; } // Direction light is emanating from
        public Texture2D HeightMap { get; set; } // Heightmap texture
        public Material Material { get; private set; }
        public bool Selected { get; set; }

        public Vector3 Position { get; set; } = Vector3.Zero;
        public Vector3 Rotation { get; set; } = Vector3.Zero;  
        public float Scale { get; set; } = 1.0f;

        public Terrain(Effect effect, Texture2D heightMap, Texture2D baseTexture, int height, GraphicsDevice device)
        {
            Material = new Material();  
            HeightMap = heightMap;
            Material.Diffuse = baseTexture;
            Material.Effect = effect;
            Device = device;
            Width = heightMap.Width;
            Length = heightMap.Height;
            Height = height;
            LightDirection = new Vector3(0, 1, 1);
            // 1 vertex per pixel
            VertexCount = Width * Length;
            // (Width-1) * (Length-1) cells, 2 triangles per cell, 3 indices per triangle
            IndexCount = (Width - 1) * (Length - 1) * 6;

            GetHeights();
            CreateVertices();
            CreateIndices();
            GenNormals();

            VertexBuffer.SetData<VertexPositionNormalTexture>(Vertices);
            IndexBuffer.SetData<int>(Indices);
        }

        private void GetHeights()
        {
            // Extract pixel data
            Color[] heightMapData = new Color[HeightMap.Width * HeightMap.Height];
            HeightMap.GetData<Color>(heightMapData);
            // Create heuights[,] array
            Heights = new float[Width, Length];
            // For each pixel
            for (int y = 0; y < Length; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    // Get color value (0 - 255)
                    float amt = heightMapData[y * Width + x].R;
                    // Scale to (0 - 1)
                    amt /= 255.0f;
                    // Multiply by max height to get final height\
                    Heights[x, y] = amt * Height;
                }
            }
        }

        private void CreateVertices()
        {
            VertexBuffer = new VertexBuffer(Device, typeof(VertexPositionNormalTexture), VertexCount, BufferUsage.WriteOnly);

            Vertices = new VertexPositionNormalTexture[VertexCount];
            for(int y = 0; y < Length; y++)
            {
                for(int x = 0; x < Width; x++)
                {
                    int index = y * Width + x;
                    Vertices[index] = new VertexPositionNormalTexture();
                    Vertices[index].Position = new Vector3(x, Heights[x, y], y);
                    Vertices[index].Normal = new Vector3(0, 0, 0);
                    Vertices[index].TextureCoordinate = new Vector2((float)x / Width, (float)y / Length);
                }
            }
        }

        private void CreateIndices() 
        { 
            IndexBuffer = new IndexBuffer(Device, IndexElementSize.ThirtyTwoBits, IndexCount, BufferUsage.WriteOnly);

            Indices = new int[IndexCount];
            int i = 0;
            // For each cell
            for(int y = 0; y < Length - 1; y++)
            {
                for(int x = 0; x < Width - 1; x++) 
                {
                    // Find the indices of the corners
                    int upperLeft = y * Width + x;
                    int upperRight = upperLeft + 1;
                    int lowerleft = upperLeft + Width;
                    int lowerRight = lowerleft + 1;
                    // Specify upper triangle
                    Indices[i++] = upperLeft;
                    Indices[i++] = upperRight;
                    Indices[i++] = lowerleft;
                    // Specify lower triangle
                    Indices[i++] = lowerleft;
                    Indices[i++] = upperRight;
                    Indices[i++] = lowerRight;
                }
            }
        }

        private void GenNormals()
        {
            // For each triangle
            for(int i = 0; i < IndexCount; i += 3)
            {
                // Find the position of each corner of the triangle
                Vector3 v1 = Vertices[Indices[i]].Position;
                Vector3 v2 = Vertices[Indices[i + 1]].Position;
                Vector3 v3 = Vertices[Indices[i + 2]].Position;
                // Cross the vectors bertween the cordners to get the normal
                Vector3 normal = Vector3.Cross(v1 - v3, v1 - v2);
                normal.Normalize();
                // Add the influence of the normal to each vertex in the triangle
                Vertices[Indices[i]].Normal += normal;
                Vertices[Indices[i + 1]].Normal += normal;
                Vertices[Indices[i + 2]].Normal += normal;  
            }
            // Average the influences of the triangles touching each vertex
            for(int i = 0; i < VertexCount; i++)
            {
                Vertices[i].Normal.Normalize();
            }
        }

        public Matrix GetTransform()
        {
            return Matrix.CreateScale(Scale) * Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z) * Matrix.CreateTranslation(Position);
        }
                
        public void Render()
        {
            /*
            Material.Effect.Parameters["World"]?.SetValue(Matrix.Identity);
            Material.Effect.Parameters["WorldViewProjection"]?.SetValue(Matrix.Identity * camera.View * camera.Projection);
            Material.Effect.Parameters["CameraPosition"]?.SetValue(camera.Position);
            Material.Effect.Parameters["View"]?.SetValue(camera.View);
            Material.Effect.Parameters["Projection"]?.SetValue(camera.Projection);
            Material.Effect.Parameters["TextureTiling"]?.SetValue(15.0f);
            Material.Effect.Parameters["LightDirection"]?.SetValue(LightDirection);
            Material.Effect.Parameters["Texture"]?.SetValue(Material.Diffuse);
            Material.Effect.Parameters["Tint"]?.SetValue(Selected);
            */

            Device.SetVertexBuffer(VertexBuffer);
            Device.Indices = IndexBuffer;

            foreach (EffectPass pass in Material.Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, IndexCount / 3);
            }
        }

        public void SetTexture(GameEditor game, string texture)
        {
            if(texture == "DefaultTexture")
            {
                Material.Diffuse = game.DefaultTexture;
            } else
            {
                Material.Diffuse = game.Content.Load<Texture>(texture);
            }
            Material.Diffuse.Tag = texture;
        }

        public void SetShader(GameEditor game, string shader)
        {
            if(shader == "DefaultEffect")
            {
                Material.Effect = game.DefaultEffect;
            } else
            {
                Material.Effect = game.Content.Load<Effect>(shader);
            }
            Material.Effect.Tag = shader;
        }
    }
}
