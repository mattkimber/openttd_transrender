using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Transrender.Palettes;
using Transrender.Projector;
using Transrender.VoxelUtils;

namespace Transrender.Rendering
{
    public class RayDefinition
    {
        public Vector3 StartLocation { get; set; }
        public Vector3 Step { get; set; }
        public Vector3 XStartStep { get; set; }
        public Vector3 YStartStep { get; set; }
    }

    public class RaycastRenderer : ISpriteRenderer
    {
        private int _projection;
        private BitmapGeometry _geometry;
        private VoxelShader _shader;
        private IProjector _projector;

        private Vector3[] startLocations = new[]
         {
            new Vector3(0,0,-1),
            new Vector3(0,0,-1),
            new Vector3(0,1,-1),
            new Vector3(0,1,-1),
            new Vector3(1,0,-1),
            new Vector3(1,0,-1),
            new Vector3(1,-1,-1),
            new Vector3(1,-1,-1)
        };

        private Vector3[] steps = new[]
        {
            new Vector3(1f,    0f,    1f),
            new Vector3(1f,    -0.707f, 1f),
            new Vector3(0f,    -1f,   1f),
            new Vector3(0.707f, -0.707f, 1f),
            new Vector3(-1f,   0f,    1f),
            new Vector3(-1f,   0f,    1f),
            new Vector3(0f,    1f,    1f),
            new Vector3(0f,    1f,    1f)
        };

        private Vector3[] xSteps = new[]
{
            new Vector3(0f, 1f, 0f),
            new Vector3(1f - 0.707f, 0.707f, 0f),
            new Vector3(1f, 0f, 0f),
            new Vector3(0.5f, 0.5f, 0f),
            new Vector3(0f, 1f, 0f),
            new Vector3(0f, 1f, 0f),
            new Vector3(-1f,0f, 0f),
            new Vector3(-1f,0f, 0f)
        };

        public RaycastRenderer(int projection, BitmapGeometry geometry, VoxelShader shader, IProjector projector)
        {
            _projection = projection;
            _geometry = geometry;
            _shader = shader;
            _projector = projector;
        }

        public ShaderResult[][] GetPixels()
        {
            /*
             * Get the width and height of the current pixel box
             * TODO: this is kind of horrific and hacky these days!
             */
            var renderScale = (_geometry.Scale) * BitmapGeometry.RenderScale;

            var width = (int)(_geometry.GetSpriteWidth(_projection) * (renderScale / _geometry.Scale));
            var height = (int)(_geometry.GetSpriteHeight(_projection) * (renderScale / _geometry.Scale));
            
            if (_shader.Width > 64)
            {
                width = width * 2;
                height = height * 2;
            }

            var cos_theta = Math.Cos((Math.PI / 4) * (2 - _projection));
            var sin_theta = Math.Sin((Math.PI / 4) * (2 - _projection));

            /*
             * Set up the initial conditions:
             * Ray start location
             * Ray step
             * Change in ray start location per x/y pixel
             */
            var rayDefinition = new RayDefinition
            {
                StartLocation = new Vector3(_shader.Width, _shader.Width, _shader.Width / 2) * startLocations[_projection],
                Step = new Vector3(1f, 1f, 0.5f) * steps[_projection],
                XStartStep = xSteps[_projection] * new Vector3((float)(1/renderScale)),
                YStartStep = new Vector3(0, 0, (float)(1 / renderScale))
            };


            /*
             * Iterate through all pixels in the box
             */
            var result = new ShaderResult[width][];
            for (var i = 0; i < width; i++)
            {
                result[i] = new ShaderResult[height];
                for (var j = 0; j < height; j++)
                {
                    // Cast the ray for this location
                    var xOffset = rayDefinition.XStartStep * i;
                    var yOffset = rayDefinition.YStartStep * j;
                    var ray = rayDefinition.StartLocation + xOffset + yOffset;

                    while(true)
                    {
                        if(
                            (ray.X < 0 && rayDefinition.Step.X <= 0) ||
                            (ray.X > _shader.Width && rayDefinition.Step.X >= 0) ||
                            (ray.Y < 0 && rayDefinition.Step.Y <= 0) ||
                            (ray.Y > _shader.Width && rayDefinition.Step.Y >= 0) ||
                            (ray.Z < 0 && rayDefinition.Step.Z <= 0) ||
                            (ray.Z > _shader.Width && rayDefinition.Step.Z >= 0)
                            )
                        {
                            break;
                        }

 
                        var voxelSpace = ray.Round();
                                                       
                        if (voxelSpace.X < _shader.Width && voxelSpace.Y < _shader.Depth && voxelSpace.Z < _shader.Height 
                            && voxelSpace.X > 0 && voxelSpace.Y > 0 && voxelSpace.Z > 0)
                        {
                            if (!_shader.IsTransparent((int)voxelSpace.X, (int)voxelSpace.Y, (int)voxelSpace.Z))
                            {
                                var pixel = _shader.ShadePixel((int)voxelSpace.X, (int)voxelSpace.Y, (int)voxelSpace.Z, _projection, _projector.GetLightingVector(_projection));
                                result[i][j] = pixel;
                                break;
                            }
                        }
 
                        ray += rayDefinition.Step;
                    }
                }
            }

            return result;
        }
    }
}
