using System;
using System.Numerics;
using Transrender.Palettes;
using Transrender.Lighting;
using Transrender.Rendering;

namespace TransrenderLib.Rendering
{
    public class SimpleRaycastRenderer : ISpriteRenderer
    {
        private int _projection;
        private BitmapGeometry _geometry;
        private VoxelShader _shader;
        private ILightingVectors _lightingVectors;

        public SimpleRaycastRenderer(int projection, BitmapGeometry geometry, VoxelShader shader, ILightingVectors lightingVectors)
        {
            _projection = projection;
            _geometry = geometry;
            _shader = shader;
            _lightingVectors = lightingVectors;
        }

        int _x, _y, _z;

        Vector3 _a, _b, _c, _d, _cur, _normal;

        float _widthF, _heightF, _depthF;

        public void InitVectors()
        {
            var spriteSize = 126;

            var x = (float)Math.Cos(((4-_projection) / 4.0) * Math.PI);
            var y = (float)Math.Sin(((4-_projection) / 4.0) * Math.PI);

            var renderDirection = Vector3.Normalize(new Vector3(x, y, (float)Math.Sin((30.0 / 180) * Math.PI)));
            var renderNormal = Vector3.Normalize(new Vector3(y, -x, 0));
            var renderDirectionNoZ = new Vector3(renderDirection.X, renderDirection.Y, 0);

            // Get the plane normal of the render direction
            var planeNormal = Vector3.Normalize(Vector3.Cross(renderNormal, renderDirectionNoZ));

            var scaleVector = new Vector3(spriteSize);
            var halfScaleVector = Vector3.Multiply(scaleVector, 0.5f);

            var midpoint = new Vector3(126 / 2, 40 / 2, 40 / 2);
            midpoint = Vector3.Add(midpoint, Vector3.Multiply(renderDirection, 100.0f));

            var scaledPlaneNormal = Vector3.Multiply(planeNormal, halfScaleVector);

            var scaledRenderNormal = Vector3.Multiply(renderNormal, halfScaleVector);

            // Get the viewing window
            _c = Vector3.Add(midpoint, scaledRenderNormal);
            _d = Vector3.Subtract(midpoint, scaledRenderNormal);

            _a = Vector3.Add(_d, scaledPlaneNormal);
            _b = Vector3.Add(_c, scaledPlaneNormal);

            _c = Vector3.Subtract(_c, scaledPlaneNormal);
            _d = Vector3.Subtract(_d, scaledPlaneNormal);


            _normal = Vector3.Subtract(Vector3.Zero, renderDirection);
        }

        public void InitRay(float u, float v)
        {
            var abu = Vector3.Lerp(_a, _b, u);
            var dcu = Vector3.Lerp(_d, _c, u);
            _cur = Vector3.Lerp(abu, dcu, v);

            MoveToIntersectionCoordinate();

            _x = (int)_cur.X;
            _y = (int)_cur.Y;
            _z = (int)_cur.Z;
        }

        public void MoveToIntersectionCoordinate()
        {
            if (IsOutside()) return;

            var dist = -1.0f;

            if(_normal.X > 0.5)
            {
                // Starting at negative co-ordinates and moving toward
                dist = ((0 - _cur.X) / _normal.X);
            }
            else if(_normal.X < -0.5)
            {
                // Starting at positive co-ordinates and moving toward
                dist = ((_widthF - _cur.X) / _normal.X);
            }

            if (dist > 0)
            {
                var distVector = Vector3.Multiply(_normal, dist);
                _cur = Vector3.Add(distVector, _cur);
            }

            dist = -1.0f;

            if(_normal.Y > 0.5)
            {
                dist = ((0 - _cur.Y) / _normal.Y);
            }
            else if(_normal.Y < -0.5)
            {
                dist = ((_depthF - _cur.Y) / _normal.Y);
            }

            if(dist > 0)
            {
                var distVector = Vector3.Multiply(_normal, dist);
                _cur = Vector3.Add(distVector, _cur);
            }

            dist = ((_heightF - _cur.Z) / _normal.Z);
            if (dist > 0)
            {
                var distVector = Vector3.Multiply(_normal, dist);
                _cur = Vector3.Add(distVector, _cur);
            }

        }

        public void StepVector()
        {
            _cur = Vector3.Add(_cur, _normal);
        }

        public bool IsInsideObject()
        {
            return _cur.X >= 0 && _cur.Y >= 0 && _cur.Z >= 0 && _cur.X < _widthF && _cur.Y < _depthF && _cur.Z < _heightF;
        }

        public bool IsOutside()
        {
            return (
                (_cur.X < 0 && _normal.X <= 0) ||
                (_cur.Y < 0 && _normal.Y <= 0) ||
                (_cur.Z < 0 && _normal.Z <= 0) ||
                (_cur.X > _widthF && _normal.X >= 0) ||
                (_cur.Y > _depthF && _normal.Y >= 0) ||
                (_cur.Z > _heightF && _normal.Z >= 0)
                );
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

            _widthF = _shader.Width;
            _heightF = _shader.Height;
            _depthF = _shader.Depth;

            InitVectors();

            var result = new ShaderResult[width][];
            for (var i = 0; i < width; i++)
            {
                result[i] = new ShaderResult[height];
                for (var j = 0; j < height; j++)
                {
                    InitRay((float)i / width, (float)j / height);

                    while(!IsOutside())
                    {
                        if (IsInsideObject())
                        {
                            _x = (int)_cur.X;
                            _y = (int)_cur.Y;
                            _z = (int)_cur.Z;

                            if (!_shader.IsTransparent(_x, _y, _z))
                            {
                                result[i][j] = _shader.ShadePixel(_x, _y, _z, _projection, _lightingVectors.GetLightingVector(_projection));
                                break;
                            }
                        }

                        StepVector();
                    }

                }
            }

            return result;
        }

    }
}
