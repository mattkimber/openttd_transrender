using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using Transrender.Util;


namespace Transrender.Palettes
{
    public class TTDPalette : IPalette
    {
        private ColorPalette _palette = null;

        private HashSet<int> _priorityColours = new HashSet<int> { 15 };
        private HashSet<int> _maskColours = new HashSet<int> { 81, 82, 83, 84, 85, 86, 87, 199, 200, 201, 202, 203, 204, 205 };

        private ColourFlag[] _colourFlags;

        public TTDPalette()
        {
            _colourFlags = new ColourFlag[256];

            for(var i = 0; i < 256; i++)
            {
                ColourFlag f = 0;
                if (_priorityColours.Contains(i)) f = ColourFlag.TakesPriority;
                if (_maskColours.Contains(i)) f = ColourFlag.IsMaskColour;
                _colourFlags[i] = f;
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.Synchronized)]
        private void SetPalette()
        {
            // Need to create a bitmap as palettes cannot be instantiated.
            var bitmap = new Bitmap(1, 1, PixelFormat.Format8bppIndexed);
            _palette = bitmap.Palette;

            for (var i = 0; i < TtdPalette.Length / 3; i++)
            {
                _palette.Entries[i] = Color.FromArgb(TtdPalette[i, 0], TtdPalette[i, 1], TtdPalette[i, 2]);
            }
        }

        public ColorPalette Palette
        {
            get
            {
                if (_palette == null)
                {
                    SetPalette();
                }

                return _palette;
            }
        }

        public byte GetGreyscaleEquivalent(double index)
        {
            var range = GetRange(index);
            var minimum = GetRangeMinimum(range);
            var maximum = GetRangeMaximum(range);
            var size = maximum - minimum;
            return (byte)(0.0 + ((index - (double)minimum) / size) * 255.0);
        }

        public byte GetRangeMidpoint(double index)
        {
            var range = GetRange(index);
            var midpoint = (GetRangeMaximum(range) + GetRangeMinimum(range)) / 2.0;
            return (byte)midpoint;
        }

        public int GetRange(double index)
        {
            for (var i = 0; i < TtdRangeBoundaries.Length; i++)
            {
                if (index < TtdRangeBoundaries[i])
                {
                    return i;
                }
            }

            return TtdRangeBoundaries.Length - 1;
        }

        public double GetRangeMaximum(int rangeIndex)
        {
            if (rangeIndex >= TtdRangeBoundaries.Length)
            {
                rangeIndex = TtdRangeBoundaries.Length - 1;
            }
            if (rangeIndex < 0)
            {
                rangeIndex = 0;
            }

            return (double)TtdRangeBoundaries[rangeIndex] - 1;
        }

        public double GetRangeMinimum(int rangeIndex)
        {
            if (rangeIndex >= TtdRangeBoundaries.Length)
            {
                rangeIndex = TtdRangeBoundaries.Length;
            }
            if (rangeIndex < 1)
            {
                rangeIndex = 1;
            }

            return TtdRangeBoundaries[rangeIndex - 1];
        }


        private ColourFlag GetColourBehaviour(int index)
        {
            return _colourFlags[index];
        }

        private byte GetShiftedColour(byte colour, int amount)
        {
            var range = GetRange(colour);
            var result = colour + amount;

            if(result > GetRangeMaximum(range))
            {
                result = (byte)GetRangeMaximum(range);
            }
            else if(result < GetRangeMinimum(range))
            {
                result = (byte)GetRangeMinimum(range);
            }

            return (byte)result;
        }

        public bool IsSpecialColour(byte colour)
        {
            return _colourFlags[colour] == ColourFlag.TakesPriority;
        }

        public bool IsMaskColour(byte colour)
        {
            return _colourFlags[colour] == ColourFlag.IsMaskColour;
        }

        private double GetPositionInRange(byte colour)
        {
            var range = GetRange(colour);
            var min = GetRangeMinimum(range);
            var max = GetRangeMaximum(range);

            return (colour - min) / (max - min);
        }

        public ShaderResult GetCombinedColour(List<ShaderResult> colours)
        {
            colours = colours.Where(c => c != null).ToList();

            if(colours.Any(c => !c.Has32BitData))
            {
                colours = colours.Select(
                    c => c.Has32BitData ? c :
                    new ShaderResult
                    {
                        PaletteColour = c.PaletteColour,
                        A = 0,
                        R = _palette.Entries[c.PaletteColour].R,
                        G = _palette.Entries[c.PaletteColour].G,
                        B = _palette.Entries[c.PaletteColour].B,
                        M = IsMaskColour(c.PaletteColour) ? (byte)200 : (byte)0,
                        Has32BitData = true
                    }).ToList();
            }
            
            if(colours.Select(c => GetRange(c.PaletteColour)).Distinct().Count() == 1)
            {
                return new ShaderResult
                {
                    PaletteColour = (byte)colours.Average(c => c.PaletteColour),
                    A = (byte)colours.Average(c => c.A),
                    R = (byte)colours.Average(c => c.R),
                    G = (byte)colours.Average(c => c.G),
                    B = (byte)colours.Average(c => c.B),
                    M = (colours.Count(c => c.M != 0) >= colours.Count() / 2) ? colours.Select(c => c.M).Mode() : (byte)0,
                    Has32BitData = true
                };
            }

            if (colours.Any(c => GetColourBehaviour(c.PaletteColour) == ColourFlag.TakesPriority))
            {
                var paletteColour = colours.First(c => GetColourBehaviour(c.PaletteColour) == ColourFlag.TakesPriority).PaletteColour;

                return new ShaderResult
                {
                    PaletteColour = paletteColour,
                    A = 0,
                    R = _palette.Entries[paletteColour].R,
                    G = _palette.Entries[paletteColour].G,
                    B = _palette.Entries[paletteColour].B,
                    M = (byte)0,
                    Has32BitData = true
                };
            }

            if (colours.Any(c => c.PaletteColour != 0))
            {
                var colour = colours.Where(c => c.PaletteColour != 0).GroupBy(c => c.PaletteColour).OrderByDescending(c => c.Count()).First().First();
                var averageRangePosition = colours.Where(c => c.PaletteColour != 0).Average(c => GetPositionInRange(c.PaletteColour));
                var range = GetRange(colour.PaletteColour);
                var min = GetRangeMinimum(range);
                var max = GetRangeMaximum(range);

                return new ShaderResult
                {
                    PaletteColour = (byte)(min + ((max - min) * averageRangePosition)),
                    A = (byte)colours.Average(c => c.A),
                    R = (byte)colours.Average(c => c.R),
                    G = (byte)colours.Average(c => c.G),
                    B = (byte)colours.Average(c => c.B),
                    M = (colours.Count(c => c.M != 0) >= colours.Count() / 2) ? colours.Select(c => c.M).Mode() : (byte)0,
                    Has32BitData = true
                };
            }

            return new ShaderResult { PaletteColour = 0, Has32BitData = false };
        }

        // Palette indexes where TTD ranges start and end
        // Used to identify whether we can move to a pixel up or down
        // when shading.
        private static readonly int[] TtdRangeBoundaries = {
            1,16,24,32,40,48,54,60,64,70,80,88,96,
            104,112,122,128,136,144,154,160,162,170,
            176,178,192,198,206,210,215,227,232,239,
            240,241,242,244,245, 252,253,255
        };

        private static readonly int[,] TtdPalette = {
            {0, 0, 255}, {16, 16, 16},  {32, 32, 32},  {48, 48, 48},  {64, 64, 64},
            {80, 80, 80}, {100, 100, 100}, {116, 116, 116}, {132, 132, 132}, {148, 148, 148},
            {168, 168, 168},{184, 184, 184}, {200, 200, 200}, {216, 216, 216}, {232, 232, 232},
            {252, 252, 252}, {52, 60, 72},{68, 76, 92}, {88, 96, 112}, {108, 116, 132},
            {132, 140, 152}, {156, 160, 172}, {176, 184, 196},{204, 208, 220}, {48, 44, 4},
            {64, 60, 12}, {80, 76, 20}, {96, 92, 28}, {120, 120, 64}, {148, 148, 100},
            {176, 176, 132},{204, 204, 168}, {72, 44, 4}, {88, 60, 20}, {104, 80, 44}, {124, 104, 72},
            {152, 132, 92}, {184, 160, 120}, {212, 188, 148}, {244, 220, 176}, {64, 0, 4}, {88, 4, 16}, 
            {112, 16, 32}, {136, 32, 52},{160, 56, 76}, {188, 84, 108}, {204, 104, 124}, {220, 132, 144}, 
            {236, 156, 164}, {252, 188, 192},{252, 208, 0}, {252, 232, 60}, {252, 252, 128}, {76, 40, 0},
            {96, 60, 8}, {116, 88, 28}, {136, 116, 56}, {156, 136, 80}, {176, 156, 108}, {196, 180, 136}, 
            {68, 24, 0}, {96, 44, 4}, {128, 68, 8}, {156, 96, 16}, {184, 120, 24}, {212, 156, 32}, {232, 184, 16}, 
            {252, 212, 0}, {252, 248, 128}, {252, 252, 192}, {32, 4, 0}, {64, 20, 8}, {84, 28, 16}, {108, 44, 28}, 
            {128, 56, 40}, {148, 72, 56}, {168, 92, 76}, {184, 108, 88}, {196, 128, 108}, {212, 148, 128}, {8, 52, 0}, 
            {16, 64, 0}, {32, 80, 4}, {48, 96, 4}, {64, 112, 12}, {84, 132, 20}, {104, 148, 28}, {128, 168, 44}, 
            {28, 52, 24}, {44, 68, 32}, {60, 88, 48}, {80, 104, 60}, {104, 124, 76}, {128, 148, 92}, {152, 176, 108},
            {180, 204, 124}, {16, 52, 24}, {32, 72, 44}, {56, 96, 72},{76, 116, 88}, {96, 136, 108}, {120, 164, 136}, 
            {152, 192, 168}, {184, 220, 200}, {32, 24, 0}, {56, 28, 0},{72, 40, 4}, {88, 52, 12}, {104, 64, 24}, 
            {124, 84, 44}, {140, 108, 64}, {160, 128, 88}, {76, 40, 16}, {96, 52, 24}, {116, 68, 40}, {136, 84, 56}, 
            {164, 96, 64}, {184, 112, 80}, {204, 128, 96}, {212, 148, 112}, {224, 168, 128}, {236, 188, 148}, {80, 28, 4},
            {100, 40, 20}, {120, 56, 40}, {140, 76, 64}, {160, 100, 96}, {184, 136, 136}, {36, 40, 68}, {48, 52, 84}, 
            {64, 64, 100}, {80, 80, 116}, {100, 100, 136}, {132, 132, 164}, {172, 172, 192}, {212, 212, 224}, {40, 20, 112}, 
            {64, 44, 144}, {88, 64, 172}, {104, 76, 196}, {120, 88, 224}, {140, 104, 252}, {160, 136, 252}, {188, 168, 252}, 
            {0, 24, 108}, {0, 36, 132}, {0, 52, 160}, {0, 72, 184},  {0, 96, 212}, {24, 120, 220}, {56, 144, 232}, {88, 168, 240},
            {128, 196, 252}, {188, 224, 252}, {16, 64, 96},  {24, 80, 108}, {40, 96, 120}, {52, 112, 132}, {80, 140, 160}, 
            {116, 172, 192}, {156, 204, 220}, {204, 240, 252}, {172, 52, 52}, {212, 52, 52}, {252, 52, 52}, {252, 100, 88}, 
            {252, 144, 124}, {252, 184, 160}, {252, 216, 200},{252, 244, 236}, {72, 20, 112}, {92, 44, 140}, {112, 68, 168},
            {140, 100, 196},  {168, 136, 224}, {200, 176, 248}, {208, 184, 255}, {232, 208, 252}, {60, 0, 0}, {92, 0, 0}, 
            {128, 0, 0}, {160, 0, 0}, {196, 0, 0}, {224, 0, 0}, {252, 0, 0}, {252, 80, 0}, {252, 108, 0}, {252, 136, 0},
            {252, 164, 0}, {252, 192, 0}, {252, 220, 0}, {252, 252, 0}, {204, 136, 8}, {228, 144, 4}, {252, 156, 0}, 
            {252, 176, 48}, {252, 196, 100},   {252, 216, 152}, {8, 24, 88}, {12, 36, 104}, {20, 52, 124}, {28, 68, 140}, 
            {40, 92, 164}, {56, 120, 188}, {72, 152, 216}, {100, 172, 224}, {92, 156, 52}, {108, 176, 64}, {124, 200, 76},
            {144, 224, 92}, {224, 244, 252}, {200, 236, 248}, {180, 220, 236}, {132, 188, 216}, {88, 152, 172}, {244, 0, 244}, 
            {245, 0, 245}, {246, 0, 246}, {247, 0, 247},  {248, 0, 248}, {249, 0, 249}, {250, 0, 250}, {251, 0, 251}, 
            {252, 0, 252}, {253, 0, 253}, {254, 0, 254}, {255, 0, 255}, {76, 24, 8}, {108, 44, 24}, {144, 72, 52},
            {176, 108, 84}, {210, 146, 126}, {252, 60, 0}, {252, 84, 0}, {252, 104, 0},  {252, 124, 0}, {252, 148, 0},
            {252, 172, 0}, {252, 196, 0}, {64, 0, 0}, {255, 0, 0}, {48, 48, 0}, {64, 64, 0}, {80, 80, 0}, {255, 255, 0},
            {32, 68, 112}, {36, 72, 116}, {40, 76, 120}, {44, 80, 124},{48, 84, 128}, {72, 100, 144}, {100, 132, 168}, 
            {216, 244, 252}, {96, 128, 164}, {68, 96, 140}, {255, 255, 255}};

    }
}
