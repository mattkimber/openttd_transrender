using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transrender.Palettes;
using Transrender.Projector;

namespace Transrender.Rendering
{
    public class RaycastRenderer : ISpriteRenderer
    {
        private VoxelShader _shader;
        private int[][] _shadowVector;
        private BitmapGeometry _geometry;
        private int _projection;

        private double cos_theta;
        private double sin_theta;

        public RaycastRenderer(int projection, BitmapGeometry geometry, VoxelShader shader, IProjector projector)
        {
            _shadowVector = projector.GetShadowVector(projection);
            _shader = shader;
            _geometry = geometry;
            _projection = projection;

            cos_theta = Math.Cos((Math.PI / 4) * (2 - projection));
            sin_theta = Math.Sin((Math.PI / 4) * (2 - projection));

            var vector = GetRayVector();
            var position = GetInitialPosition(10, 0);

            Console.WriteLine(string.Format("Projection: {0} Vector: {1:n2},{2:n2},{3:n2}", _projection, vector[0], vector[1], vector[2]));
            Console.WriteLine(string.Format("Init. pos.: {0} Vector: {1:n2},{2:n2},{3:n2}", _projection, position[0], position[1], position[2]));

        }

        public ShaderResult[][] GetPixels()
        {
            var scale = _geometry.Scale * BitmapGeometry.RenderScale;
            var width = _shader.Width > 64 ? (int)(128 * scale) : (int)(64 * scale);
            var height = _shader.Width > 64 ? (int)(80 * scale) : (int)(40 * scale);

            var result = GetInitialisedArray(width, height);

            for (var x = 0; x < width; x++)
            {
                for(var y = 0; y < height; y++)
                {
                    result[x][y] = CastRay(x / scale, y / scale);
                }
            }

            return result;
        }

        private double GetWidth()
        {
            var vector = new[] { _shader.Width / 2.0, _shader.Depth / 2.0, _shader.Height / 2.0};
            return Math.Abs(MultiplyByRotationMatrix(vector)[0]);
        }

        private double[] GetInitialPosition(double x, double y)
        {
            return MultiplyByRotationMatrix(new[] { -GetWidth() + x, -32.0, y - 40.0 });
        }

        private double[] GetRayVector()
        {
            return MultiplyByRotationMatrix(new[] { 0.0, 0.5, 0.25 });
        }

        private ShaderResult CastRay(double x, double y)
        {
            var vector = GetRayVector();

            var position = GetInitialPosition(x, y);

            for(var i = 0; i < 1000; i++)
            {
                var cur_x = (int)(position[0] + (_shader.Width / 2.0));
                var cur_y = (int)(position[1] + (_shader.Depth / 2.0));
                var cur_z = (int)(position[2] + (_shader.Height / 2.0));

                if(
                    (cur_x > _shader.Width && vector[0] > 0)  ||
                    (cur_y > _shader.Depth && vector[1] > 0)  ||
                    (cur_z > _shader.Height && vector[2] > 0) ||
                    (cur_x < 0 && vector[0] < 0) ||
                    (cur_y < 0 && vector[1] < 0) ||
                    (cur_z < 0 && vector[2] < 0))
                {
                    break;
                }

                if (!_shader.IsTransparent(cur_x, cur_y, cur_z))
                {
                    return _shader.ShadePixel(cur_x, cur_y, cur_z, _shadowVector);
                }

                position = Add(position, vector);
            }

            return ShaderResult.Transparent();
        }
    
        private double[] Add(double[] input1, double[] input2)
        {
            if(input1.Length != input2.Length)
            {
                throw new ArgumentException("Inputs must be the same length");
            }

            return input1.Select((v, i) => v + input2[i]).ToArray();
        }

        private double[] MultiplyByRotationMatrix(double[] input)
        {
            if (input.Length != 3)
            {
                throw new ArgumentException("Can only multiply x,y,z triplet");
            }

            return new[]
            {
                (input[0] * cos_theta) + (input[1] * sin_theta),
                (input[0] * -sin_theta) + (input[1] * cos_theta),
                input[2]
            };
        }

        private ShaderResult[][] GetInitialisedArray(int width, int height)
        {
            var result = new ShaderResult[width][];
            for (var i = 0; i < width; i++)
            {
                result[i] = new ShaderResult[height];
            }

            return result;
        }
    }
}
