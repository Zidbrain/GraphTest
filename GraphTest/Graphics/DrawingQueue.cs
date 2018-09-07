using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;

namespace GraphTest
{
    [Flags]
    public enum DrawingEffects
    {
        CastsShadow = 1,
        SeenThroughWindow = 2,
        BasicDrawing = 4,
        LightingEnabled = 8,
        Standart = BasicDrawing | SeenThroughWindow | CastsShadow | LightingEnabled
    }

    public interface IDrawable
    {
        DrawingEffects DrawingEffects { get; }
        void Draw();
    }

    public class DrawingQueue
    {
        private List<(IDrawable drawable, int index, bool isDrawing)> _collection;
        private bool _ordered;
        private int _times;

        public bool InsideCall { get; set; }

        public DrawingQueue() => _collection = new List<(IDrawable drawable, int index, bool isDrawing)>();

        public void Add(IDrawable value, int index)
        {
            _collection.Add((value, index, false));
            _ordered = false;
        }

        public void Draw(DrawingEffects drawingEffects)
        {
            _times++;
            if (!_ordered)
            {
                _collection.Sort(((IDrawable, int index, bool) left, (IDrawable, int index, bool) right) => left.index - right.index);
                _ordered = true;
            }

            for (int i = 0; i < _collection.Count; i++)
            {
                if (!_collection[i].isDrawing && ((_collection[i].drawable.DrawingEffects & drawingEffects) != 0))
                {
                    _collection[i] = (_collection[i].drawable, _collection[i].index, true);
                    _collection[i].drawable.Draw();
                    _collection[i] = (_collection[i].drawable, _collection[i].index, false);
                }
            }
        }

        public void Clear()
        {
            _collection.Clear();
            Console.WriteLine(_times);
            _times = 0;
        }
    }
}