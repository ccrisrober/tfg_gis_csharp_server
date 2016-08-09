// Copyright (c) 2015, maldicion069 (Cristian Rodríguez) <ccrisrober@gmail.con>
//
// Permission to use, copy, modify, and/or distribute this software for any
// purpose with or without fee is hereby granted, provided that the above
// copyright notice and this permission notice appear in all copies.
//
// THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
// WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
// MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
// ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
// WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
// ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
// OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.package com.example

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication8
{
    class ObjectUser
    {
        protected int _Id;
        protected float _PosX;
        protected float _PosY;
        protected int _Map;
        protected int _RollDice;
        protected HashSet<int> _Objects;

        public ObjectUser(int id, float posX, float posY, int map, int rolldice)
        {
            this._Id = id;
            this._PosX = posX;
            this._PosY = posY;
            this._Map = map;
            this._RollDice = rolldice;
            this._Objects = new HashSet<int>();
        }
        public ObjectUser(int id, float posX, float posY): this(id, posX, posY, 0, 0)
        {
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
        public int Map
        {
            get { return _Map; }
        }

        public int RollDice
        {
            get { return _RollDice; }
            set { _RollDice = value; }
        }

        public void SetPosition(float x, float y)
        {
            this._PosX = x;
            this._PosY = y;
        }

        public void AddObject(int idx)
        {
            this._Objects.Add(idx);
        }

        public void RemoveObject(int idx)
        {
            this._Objects.Remove(idx);
        }
    }
}
