using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication8
{
    class KeyObject
    {
        protected int _Id;
        protected float _PosX;
        protected float _PosY;
        protected String _Color;

        public KeyObject(int id, float posX, float posY, String color)
        {
            this._Id = id;
            this._PosX = posX;
            this._PosY = posY;
            this._Color = color;
        }

        public int Id
        {
            get { return _Id; }
        }

        public float PosX
        {
            get { return _PosX; }
            set { _PosX = value; }
        }

        public float PosY
        {
            get { return _PosY; }
            set { _PosY = value; }
        }

        public String Color
        {
            get { return _Color; }
        }
    }
}
