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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication8
{
    class Map
    {
        protected int _Id;
        protected String _MapFields;
        protected int _Width;
        protected int _Height;
        protected Dictionary<string, KeyObject> _KeyObjects;

        public Map(int id, String fields, int width, int height, ref KeyObject[] keys)
        {
            this._Id = id;
            this._MapFields = fields;
            this._Width = width;
            this._Height = height;
            this._KeyObjects = new Dictionary<string,KeyObject>();

            foreach (KeyObject key in keys)
            {
                this._KeyObjects.Add(key.Id.ToString(), key);
            }
        }

        public int Id
        {
            get { return _Id; }
        }

        public String MapFields
        {
            get { return _MapFields; }
            set { _MapFields = value; }
        }

        public int Width
        {
            get { return _Width; }
            set { _Width = value; }
        }

        public int Height
        {
            get { return _Height; }
            set { _Height = value; }
        }

        public Dictionary<string, KeyObject> KeyObjects
        {
            get { return _KeyObjects; }
            set { _KeyObjects = value; }
        }

        public KeyObject RemoveKey(int idx)
        {
            KeyObject obj = _KeyObjects[idx.ToString()];
            _KeyObjects.Remove(idx.ToString());
            return obj;
        }

        public void AddKey(KeyObject obj)
        {
            _KeyObjects[obj.Id.ToString()] = obj;
        }
    }
}
