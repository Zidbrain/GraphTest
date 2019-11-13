using System;
using System.Collections.Generic;

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
        private readonly List<(IDrawable drawable, int index, bool isDrawing)> _collection;
        private bool _ordered;

        public DrawingQueue() => _collection = new List<(IDrawable drawable, int index, bool isDrawing)>();

        public void Add(IDrawable value, int index)
        {
            _collection.Add((value, index, false));
            _ordered = false;
        }

        public bool UpdateRenderTarget { get; set; }

        public void Draw(DrawingEffects drawingEffects)
        {
            if (!_ordered)
            {
                _collection.Sort(((IDrawable, int index, bool) left, (IDrawable, int index, bool) right) => left.index - right.index);
                _ordered = true;
            }

            for (var i = 0; i < _collection.Count; i++)
            {
                if (!_collection[i].isDrawing && ((_collection[i].drawable.DrawingEffects & drawingEffects) != 0))
                {
                    _collection[i] = (_collection[i].drawable, _collection[i].index, true);
                    _collection[i].drawable.Draw();
                    _collection[i] = (_collection[i].drawable, _collection[i].index, false);

                    if (UpdateRenderTarget)
                        Program.GraphTest.Present();
                }
            }
        }

        public void Clear() => _collection.Clear();
    }
}