using System.Collections.Generic;
using UnityEngine;

namespace Common
{
    public class PicturePoints
    {
        private Point[] _points;
        private Vector2Int _resolution;
        public PicturePoints(Point[] points, Vector2Int resolution)
        {
            _points = points;
            _resolution = resolution;
        }
        public Point[] GetPoints()
        {
            return _points;
        }
        public Vector2Int GetResolution()
        {
            return _resolution;
        }
    }
    public struct Point
    {
        public Color color;
        public Vector2 pos;
    }
    public interface IDataReceiver
    {
        public PicturePoints GetData();
    }
}