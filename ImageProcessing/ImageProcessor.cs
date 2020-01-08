using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ImageProcessing
{
    public class ImageProcessor
    {
        private uint[] _pixels;
        private List<LeafToken> _allTokens = null;
        private int _width, _height;

        public const uint PIXEL_MASK = 0x000000ff;

        private const uint COLOR_BACKGROUND = 0xffffffff;
        private const uint COLOR_FOREGROUND = 0xff000000;
        private const uint COLOR_SKELETON = 0xff0000ff;
        private const uint COLOR_REMOVABLE = 0xff00ffff;
        private const uint COLOR_GOODLINE = 0xff00ff00;
        private const uint COLOR_BADLINE = 0xffff0000;
        private const uint COLOR_DONE = 0xfffffe00;
        private const uint COLOR_POINT_MARK = 0xffff00ff;
        private const uint COLOR_POINT_DONE = 0xffffff00;

        private static int LINE_MIN = 20;
        private static int POINT_DIFF = 20;

        private long _pointsPos;
        private int _rootX, _rootY;

        public ImageProcessor(int width, int height, List<LeafToken> alltokens)
        {
            _allTokens = alltokens;
            _width = width;
            _height = height;
            _pixels = new uint[width * height];
        }
        public ImageProcessor(Image image)
        {
            _width = image.Width;
            _height = image.Height;
            _pixels = new uint[_width * _height];
            FillPixels(image as Bitmap);
        }

        private void FillPixels(Bitmap image)
        {
            unsafe
            {
                BitmapData bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, image.PixelFormat);
                int bytesPerPixel = Bitmap.GetPixelFormatSize(image.PixelFormat) / 8;
                int heightIn_pixels = image.Height;
                int widthInBytes = image.Width * bytesPerPixel;
                byte* PtrFirstPixel = (byte*)bitmapData.Scan0;
                Parallel.For(0, heightIn_pixels, y =>
                {
                    int br = 0;
                    byte* currentLine = PtrFirstPixel + (y * bitmapData.Stride);
                    for (int x = 0; x < widthInBytes; x += bytesPerPixel)
                    {
                        int oldBlue = currentLine[x];
                        int oldGreen = currentLine[x + 1];
                        int oldRed = currentLine[x + 2];
                        int newColor = (int)(oldRed * 0.2125 + oldGreen * 0.7154 + oldBlue * 0.072);
                        _pixels[y * _width + br++] = (uint)newColor;
                    }
                });
                image.UnlockBits(bitmapData);
            }
        }
        public Image GetImage()
        {
            var gch_pixels = GCHandle.Alloc(_pixels, GCHandleType.Pinned);
            var image = new Bitmap(_width, _height, _width * sizeof(uint),
                        PixelFormat.Format32bppPArgb,
                        gch_pixels.AddrOfPinnedObject());

            gch_pixels.Free();
            return image;
        }
        public void Clear()
        {
            for (int i = 0; i < _pixels.Length; i++)
            {
                _pixels[i] = COLOR_BACKGROUND;
            }
        }
        public void SetPixel(int x, int y, uint color)
        {
            if (x < 0 || y < 0 || x >= _width || y >= _height)
            {
                return;
            }
            _pixels[y * _width + x] = color;
        }

        public void SetGrayPixel(int x, int y, uint gray)
        {
            if (x < 0 || y < 0 || x >= _width || y >= _height)
            {
                return;
            }
            if (gray < 0) gray = 0;
            if (gray > 255) gray = 255;

            _pixels[y * _width + x] = COLOR_FOREGROUND | gray | gray << 8 | gray << 16;
        }

        public uint GetPixel(int x, int y)
        {
            if (x < 0) x = 0;
            if (y < 0) y = 0;
            if (x >= _width) x = _width - 1;
            if (y >= _height) y = _height - 1;

            return _pixels[y * _width + x];
        }

        public uint GetGrayPixel(int x, int y)
        {
            if (x < 0) x = 0;
            if (y < 0) y = 0;
            if (x >= _width) x = _width - 1;
            if (y >= _height) y = _height - 1;

            return (_pixels[y * _width + x] & PIXEL_MASK) / 3 +
                    ((_pixels[y * _width + x] >> 8) & PIXEL_MASK) / 3 +
                    ((_pixels[y * _width + x] >> 16) & PIXEL_MASK) / 3;
        }
        public void DrawCircle(int x, int y, int r, uint color)
        {
            for (int i = 0; i < 360; i += 5)
            {
                SetPixel((int)Math.Round(x + Math.Sin(i) * r), (int)Math.Round(y + Math.Cos(i) * r), color);
            }
        }
        public void DrawSquare(int x, int y, int r, uint color)
        {
            int i;

            for (i = x - (r / 2); i <= x + (r / 2); i++)
            {
                SetPixel(i, y - (r / 2), color);
                SetPixel(i, y + (r / 2), color);
            }

            for (i = y - (r / 2); i <= y + (r / 2); i++)
            {
                SetPixel(x - (r / 2), i, color);
                SetPixel(x + (r / 2), i, color);
            }
        }
        void DrawLine(int x0, int y0, int x1, int y1, uint color)
        {
            int ex = x1 - x0;
            int ey = y1 - y0;
            int dx, dy, height;

            if (ex > 0)
            {
                dx = 1;
            }
            else if (ex < 0)
            {
                dx = -1;
                ex = -ex;
            }
            else dx = 0;

            if (ey > 0)
            {
                dy = 1;
            }
            else if (ey < 0)
            {
                dy = -1;
                ey = -ey;
            }
            else dy = 0;

            int x = x0, y = y0;
            if (ex > ey)
            {
                height = 2 * ey - ex;
                while (x != x1)
                {
                    if (height >= 0)
                    {
                        height -= 2 * ex;
                        y += dy;
                    }
                    height += 2 * ey;
                    x += dx;
                    SetPixel(x, y, color);
                }
            }
            else
            {
                height = 2 * ex - ey;
                while (y != y1)
                {
                    if (height >= 0)
                    {
                        height -= 2 * ey;
                        x += dx;
                    }
                    height += 2 * ex;
                    y += dy;
                    SetPixel(x, y, color);
                }
            }
        }
        public void EdgeDetect(uint threshold)
        {
            int x, y, sum;
            uint max = 0, min = 0;

            ImageProcessor Source = new ImageProcessor(_width, _height, _allTokens);
            Source.Clear();

            for (x = _width - 2; x > 0; x--)
            {
                for (y = _height - 2; y > 0; y--)
                {
                    uint dx = (GetGrayPixel(x - 1, y + 1) + GetGrayPixel(x, y + 1) + GetGrayPixel(x + 1, y + 1))
                            -
                            (GetGrayPixel(x - 1, y - 1) + GetGrayPixel(x, y - 1) + GetGrayPixel(x + 1, y - 1));

                    uint dy = (GetGrayPixel(x + 1, y - 1) + GetGrayPixel(x + 1, y) + GetGrayPixel(x + 1, y + 1))
                            -
                            (GetGrayPixel(x - 1, y - 1) + GetGrayPixel(x - 1, y) + GetGrayPixel(x - 1, y + 1));

                    uint z = (uint)(Math.Sqrt(dx * dx + dy * dy) / 3);

                    max = z > max ? z : max;
                    min = z < min ? z : min;

                    Source.SetPixel(x, y, z);
                }
            }

            float a, b;

            a = 255f / (max - min);
            b = a * min;

            for (x = _width - 1; x >= 0; x--)
            {
                for (y = _height - 1; y >= 0; y--)
                {
                    SetPixel(x, y, (uint)(((a * Source.GetPixel(x, y) + b) > threshold ? COLOR_FOREGROUND : COLOR_BACKGROUND)));
                }
            }
        }
        public void Thinning()
        {
            bool remain, skel;
            int j;
            int x, y;
            remain = true;

            while (remain)
            {
                remain = false;
                for (j = 0; j <= 6; j += 2) 
                {
                    for (x = 0; x < _width; x++)
                    {
                        for (y = 0; y < _height; y++)
                        {
                            if (GetPixel(x, y) == COLOR_FOREGROUND && (_pixels[Neighbour(x, y, j)] == COLOR_BACKGROUND))
                            {
                                if (MatchPatterns(x, y))
                                {
                                    SetPixel(x, y, COLOR_SKELETON);   
                                }
                                else
                                {
                                    SetPixel(x, y, COLOR_REMOVABLE);
                                    remain = true;
                                }
                            }
                        }
                    }

                    for (x = 0; x < _width; x++)
                    {
                        for (y = 0; y < _height; y++)
                        {
                            if (GetPixel(x, y) == COLOR_REMOVABLE)
                            {
                                SetPixel(x, y, COLOR_BACKGROUND);
                            }
                        }
                    }
                }
            }
        }
        private int Neighbour(int x, int y, int j)
        {
            switch (j)
            {
                case 0:
                    x++;
                    break;
                case 1:
                    x++;
                    y--;
                    break;
                case 2:
                    y--;
                    break;
                case 3:
                    x--;
                    y--;
                    break;
                case 4:
                    x--;
                    break;
                case 5:
                    x--;
                    y++;
                    break;
                case 6:
                    y++;
                    break;
                case 7:
                    x++;
                    y++;
                    break;
            }

            if (x >= _width - 1) x = _width - 1;
            if (x < 0) x = 0;
            if (y >= _height - 1) y = _height - 1;
            if (y < 0) y = 0;

            return y * _width + x;
        }
        private bool MatchPatterns(int x, int y)
        {
            if (x >= _width - 1) x = _width - 1;
            if (x < 0) x = 0;
            if (y >= _height - 1) y = _height - 1;
            if (y < 0) y = 0;

            if (_pixels[Neighbour(x, y, 0)] == COLOR_BACKGROUND &&
                    _pixels[Neighbour(x, y, 4)] == COLOR_BACKGROUND &&
                    (_pixels[Neighbour(x, y, 1)] != COLOR_BACKGROUND ||        // A A A
                            _pixels[Neighbour(x, y, 2)] != COLOR_BACKGROUND ||        // 0 P 0
                            _pixels[Neighbour(x, y, 3)] != COLOR_BACKGROUND) &&        // B B B
                    (_pixels[Neighbour(x, y, 5)] != COLOR_BACKGROUND ||
                            _pixels[Neighbour(x, y, 6)] != COLOR_BACKGROUND ||
                            _pixels[Neighbour(x, y, 7)] != COLOR_BACKGROUND))
            {
                return true;
            }
            else if (_pixels[Neighbour(x, y, 2)] == COLOR_BACKGROUND &&
                  _pixels[Neighbour(x, y, 6)] == COLOR_BACKGROUND &&
                  (_pixels[Neighbour(x, y, 7)] != COLOR_BACKGROUND ||    // B 0 A
                          _pixels[Neighbour(x, y, 0)] != COLOR_BACKGROUND ||    // B P A
                          _pixels[Neighbour(x, y, 1)] != COLOR_BACKGROUND) &&    // B 0 A
                  (_pixels[Neighbour(x, y, 3)] != COLOR_BACKGROUND ||
                          _pixels[Neighbour(x, y, 4)] != COLOR_BACKGROUND ||
                          _pixels[Neighbour(x, y, 5)] != COLOR_BACKGROUND))
            {
                return true;
            }
            else if (_pixels[Neighbour(x, y, 7)] == COLOR_SKELETON &&
                  _pixels[Neighbour(x, y, 0)] == COLOR_BACKGROUND &&
                  _pixels[Neighbour(x, y, 6)] == COLOR_BACKGROUND &&
                  (_pixels[Neighbour(x, y, 1)] != COLOR_BACKGROUND ||    
                          _pixels[Neighbour(x, y, 2)] != COLOR_BACKGROUND ||    
                          _pixels[Neighbour(x, y, 3)] != COLOR_BACKGROUND ||    
                          _pixels[Neighbour(x, y, 4)] != COLOR_BACKGROUND ||
                          _pixels[Neighbour(x, y, 5)] != COLOR_BACKGROUND))
            {
                return true;
            }
            else if (_pixels[Neighbour(x, y, 5)] == COLOR_SKELETON &&
                  _pixels[Neighbour(x, y, 4)] == COLOR_BACKGROUND &&
                  _pixels[Neighbour(x, y, 6)] == COLOR_BACKGROUND &&
                  (_pixels[Neighbour(x, y, 7)] != COLOR_BACKGROUND ||    
                          _pixels[Neighbour(x, y, 0)] != COLOR_BACKGROUND ||    
                          _pixels[Neighbour(x, y, 1)] != COLOR_BACKGROUND ||    
                          _pixels[Neighbour(x, y, 2)] != COLOR_BACKGROUND ||
                          _pixels[Neighbour(x, y, 3)] != COLOR_BACKGROUND))
            {
                return true;
            }
            else if (_pixels[Neighbour(x, y, 3)] == COLOR_SKELETON &&
                  _pixels[Neighbour(x, y, 2)] == COLOR_BACKGROUND &&
                  _pixels[Neighbour(x, y, 4)] == COLOR_BACKGROUND &&
                  (_pixels[Neighbour(x, y, 5)] != COLOR_BACKGROUND ||    
                          _pixels[Neighbour(x, y, 6)] != COLOR_BACKGROUND ||    
                          _pixels[Neighbour(x, y, 7)] != COLOR_BACKGROUND ||    
                          _pixels[Neighbour(x, y, 0)] != COLOR_BACKGROUND ||
                          _pixels[Neighbour(x, y, 1)] != COLOR_BACKGROUND))
            {
                return true;
            }
            else if (_pixels[Neighbour(x, y, 1)] == COLOR_SKELETON &&
                  _pixels[Neighbour(x, y, 0)] == COLOR_BACKGROUND &&
                  _pixels[Neighbour(x, y, 2)] == COLOR_BACKGROUND &&
                  (_pixels[Neighbour(x, y, 3)] != COLOR_BACKGROUND ||    
                          _pixels[Neighbour(x, y, 4)] != COLOR_BACKGROUND ||    
                          _pixels[Neighbour(x, y, 5)] != COLOR_BACKGROUND ||    
                          _pixels[Neighbour(x, y, 6)] != COLOR_BACKGROUND ||
                          _pixels[Neighbour(x, y, 7)] != COLOR_BACKGROUND))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void CheckLines(int minline)
        {
            int x, y;
            uint length = 0;

            LINE_MIN = minline;

            for (y = 0; y < _height; y++)
            {
                for (x = 0; x < _width; x++)
                {
                    if (GetPixel(x, y) == COLOR_SKELETON)
                    {
                        length = CheckLineLength(x, y, 1);

                        if (length > LINE_MIN)
                        {
                            PaintLines(x, y, COLOR_GOODLINE);
                        }
                        else
                        {
                            PaintLines(x, y, COLOR_BADLINE);
                        }
                        length = 0;
                    }
                }
            }
        }
        private uint CheckLineLength(int x, int y, uint length)
        {
            SetPixel(x, y, COLOR_DONE);

            try
            {
                if (_pixels[Neighbour(x, y, 0)] == COLOR_SKELETON)
                    length = CheckLineLength(x + 1, y, length + 1);
                if (_pixels[Neighbour(x, y, 1)] == COLOR_SKELETON)
                    length = CheckLineLength(x + 1, y - 1, length + 1);
                if (_pixels[Neighbour(x, y, 2)] == COLOR_SKELETON)
                    length = CheckLineLength(x, y - 1, length + 1);
                if (_pixels[Neighbour(x, y, 3)] == COLOR_SKELETON)
                    length = CheckLineLength(x - 1, y - 1, length + 1);
                if (_pixels[Neighbour(x, y, 4)] == COLOR_SKELETON)
                    length = CheckLineLength(x - 1, y, length + 1);
                if (_pixels[Neighbour(x, y, 5)] == COLOR_SKELETON)
                    length = CheckLineLength(x - 1, y + 1, length + 1);
                if (_pixels[Neighbour(x, y, 6)] == COLOR_SKELETON)
                    length = CheckLineLength(x, y + 1, length + 1);
                if (_pixels[Neighbour(x, y, 7)] == COLOR_SKELETON)
                    length = CheckLineLength(x + 1, y + 1, length + 1);
            }
            catch (Exception e)
            {

            }
            return length;
        }

        private void PaintLines(int x, int y, uint color)
        {
            SetPixel(x, y, color);

            try
            {
                if (_pixels[Neighbour(x, y, 0)] == COLOR_DONE) PaintLines(x + 1, y, color);
                if (_pixels[Neighbour(x, y, 1)] == COLOR_DONE) PaintLines(x + 1, y - 1, color);
                if (_pixels[Neighbour(x, y, 2)] == COLOR_DONE) PaintLines(x, y - 1, color);
                if (_pixels[Neighbour(x, y, 3)] == COLOR_DONE) PaintLines(x - 1, y - 1, color);
                if (_pixels[Neighbour(x, y, 4)] == COLOR_DONE) PaintLines(x - 1, y, color);
                if (_pixels[Neighbour(x, y, 5)] == COLOR_DONE) PaintLines(x - 1, y + 1, color);
                if (_pixels[Neighbour(x, y, 6)] == COLOR_DONE) PaintLines(x, y + 1, color);
                if (_pixels[Neighbour(x, y, 7)] == COLOR_DONE) PaintLines(x + 1, y + 1, color);
            }
            catch (Exception e)
            {
                
            }
        }

        public void MarkPoints(int distance)
        {
            int x, y;
            int length = 0;
            POINT_DIFF = distance;
            _pointsPos = POINT_DIFF - 1;

            for (y = 0; y < _height; y++)
            {
                for (x = 0; x < _width; x++)
                {
                    if (GetPixel(x, y) == COLOR_GOODLINE)
                    {
                        PaintPoints(x, y);
                    }
                }
            }
        }

        private bool PaintPoints(int x, int y)
        {
            bool result = false;

            _pointsPos++;

            if (_pointsPos == POINT_DIFF)
            {
                SetPixel(x, y, COLOR_POINT_MARK);
                _pointsPos = 0;
            }
            else SetPixel(x, y, COLOR_POINT_DONE);

            try
            {
                if (_pixels[Neighbour(x, y, 0)] == COLOR_GOODLINE) result = PaintPoints(x + 1, y);
                if (_pixels[Neighbour(x, y, 1)] == COLOR_GOODLINE) result = PaintPoints(x + 1, y - 1);
                if (_pixels[Neighbour(x, y, 2)] == COLOR_GOODLINE) result = PaintPoints(x, y - 1);
                if (_pixels[Neighbour(x, y, 3)] == COLOR_GOODLINE) result = PaintPoints(x - 1, y - 1);
                if (_pixels[Neighbour(x, y, 4)] == COLOR_GOODLINE) result = PaintPoints(x - 1, y);
                if (_pixels[Neighbour(x, y, 5)] == COLOR_GOODLINE) result = PaintPoints(x - 1, y + 1);
                if (_pixels[Neighbour(x, y, 6)] == COLOR_GOODLINE) result = PaintPoints(x, y + 1);
                if (_pixels[Neighbour(x, y, 7)] == COLOR_GOODLINE) result = PaintPoints(x + 1, y + 1);
            }
            catch (Exception e)
            {
                
            }

            if (result == false) SetPixel(x, y, COLOR_POINT_MARK);

            return true;
        }

        public void CalcAngels()
        {
            int x, y;
            int length = 0;
            _rootX = _rootY = -1;
            _allTokens = new List<LeafToken>();

            for (y = 0; y < _height; y++)
            {
                for (x = 0; x < _width; x++)
                {
                    if (GetPixel(x, y) == COLOR_POINT_MARK)
                    {
                        SearchNeighbour(x, y, true);
                    }
                }
            }

            for (int i = 0; i < _allTokens.Count; i++)
            {
                LeafToken actToken = (LeafToken)_allTokens[i];
                DrawSquare(actToken.X1, actToken.Y1, 5, COLOR_BADLINE);
            }
        }

        private void SearchNeighbour(int x, int y, bool isRoot)
        {
            if (GetPixel(x, y) == COLOR_POINT_MARK)
            {
                if (isRoot == true)
                {
                    _rootX = x;
                    _rootY = y;
                }
                else if (isRoot == false &&
                      _rootX != -1 && _rootY != -1)
                {
                    if ((_rootX + _rootY) - (x + y) == 0) return;

                    DrawLine(_rootX, _rootY, x, y, COLOR_SKELETON);

                    LeafToken ltoken = new LeafToken(_rootX, _rootY, x, y);
                    _allTokens.Add(ltoken);

                    SetPixel(_rootX, _rootY, COLOR_POINT_MARK);
                    SetPixel(x, y, COLOR_POINT_MARK);

                    return;
                }
            }
            else SetPixel(x, y, COLOR_GOODLINE);

            try
            {
                if (_pixels[Neighbour(x, y, 0)] == COLOR_POINT_DONE ||
                        _pixels[Neighbour(x, y, 0)] == COLOR_POINT_MARK)
                    SearchNeighbour(x + 1, y, false);
                if (_pixels[Neighbour(x, y, 1)] == COLOR_POINT_DONE ||
                        _pixels[Neighbour(x, y, 1)] == COLOR_POINT_MARK)
                    SearchNeighbour(x + 1, y - 1, false);
                if (_pixels[Neighbour(x, y, 2)] == COLOR_POINT_DONE ||
                        _pixels[Neighbour(x, y, 2)] == COLOR_POINT_MARK)
                    SearchNeighbour(x, y - 1, false);
                if (_pixels[Neighbour(x, y, 3)] == COLOR_POINT_DONE ||
                        _pixels[Neighbour(x, y, 3)] == COLOR_POINT_MARK)
                    SearchNeighbour(x - 1, y - 1, false);
                if (_pixels[Neighbour(x, y, 4)] == COLOR_POINT_DONE ||
                        _pixels[Neighbour(x, y, 4)] == COLOR_POINT_MARK)
                    SearchNeighbour(x - 1, y, false);
                if (_pixels[Neighbour(x, y, 5)] == COLOR_POINT_DONE ||
                        _pixels[Neighbour(x, y, 5)] == COLOR_POINT_MARK)
                    SearchNeighbour(x - 1, y + 1, false);
                if (_pixels[Neighbour(x, y, 6)] == COLOR_POINT_DONE ||
                        _pixels[Neighbour(x, y, 6)] == COLOR_POINT_MARK)
                    SearchNeighbour(x, y + 1, false);
                if (_pixels[Neighbour(x, y, 7)] == COLOR_POINT_DONE ||
                        _pixels[Neighbour(x, y, 7)] == COLOR_POINT_MARK)
                    SearchNeighbour(x + 1, y + 1, false);
            }
            catch (Exception e)
            {
               
            }

            if (isRoot == true) SetPixel(x, y, COLOR_SKELETON);
        }

        public void PaintAllLines()
        {
            _allTokens.ForEach(leafToken => {
                DrawLine(leafToken.X1, leafToken.Y1, leafToken.X2, leafToken.Y2, COLOR_GOODLINE);
                DrawSquare(leafToken.X1, leafToken.Y1, 5, COLOR_BADLINE);
            });
        }

        public List<LeafToken> GetTokens()
        {
            return _allTokens;
        }
    }
}
