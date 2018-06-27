using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Xml;

namespace GB_Seasons {
    public class Map {
        private MapData mapData;
        public bool Loaded { get; set; }
        public Rectangle MapBounds { get; private set; }
        public Dictionary<string, MapObject> Meta { get { return mapData.Meta; } }
        public List<MapObject> Objects { get { return mapData.Objects; } }
        public List<Collider> Colliders { get { return mapData.Colliders; } }

        public Map() {
            Loaded = false;
        }

        public void Load(string filename) {
            mapData = new MapData(filename);

            MapBounds = mapData.Meta["map_bounds"]?.Region ?? new Rectangle(0, 0, 0, 0);
            
            Loaded = true;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D tileset, int layer, int season, int x, int y) {
            int tw = tileset.Width / 8;
            MapLayer L = mapData.Layers[layer];
            for (int ty = 0; ty < L.Height; ty++) {
                for (int tx = 0; tx < L.Width; tx++) {
                    int si = L.Tiles[tx + ty * L.Width] - 1;
                    bool flipx = L.FlipX[tx + ty * L.Width];
                    bool flipy = L.FlipY[tx + ty * L.Width];
                    bool flipd = L.FlipD[tx + ty * L.Width];
                    if (si >= 0) {
                        int sx = si % tw;
                        int sy = si / tw;
                        sx += season * 8;
                        if (flipd) {
                            flipx = !flipx;
                        }
                        SpriteEffects se = (flipx ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | (flipy ? SpriteEffects.FlipVertically : SpriteEffects.None);
                        //SpriteEffects se = SpriteEffects.None;
                        spriteBatch.Draw(
                            tileset,
                            new Rectangle(tx * 8 - x + 4, ty * 8 - y + 4, 8, 8),
                            //new Vector2(tx * 8 - x + 4, ty * 8 - y + 4),
                            new Rectangle(sx * 8, sy * 8, 8, 8),
                            Color.White,
                            (float)(flipd ? Math.PI : 0f),
                            new Vector2(4, 4),
                            se,
                            1f
                        );
                    }
                }
            }
        }
    }

    public class MapData {
        public int Width { get; set; }
        public int Height { get; set; }
        public List<MapLayer> Layers { get; private set; }
        public Dictionary<string, MapObject> Meta {get; private set; }
        public List<MapObject> Objects { get; private set; }
        public List<Collider> Colliders { get; private set; }


        public MapData(string filename) {
            Layers = new List<MapLayer>();

            XmlDocument doc = new XmlDocument();
            doc.Load(filename);
            XmlElement map = (XmlElement)doc.GetElementsByTagName("map").Item(0);
            XmlNodeList layers = map.GetElementsByTagName("layer");
            foreach (XmlElement layer in layers) {
                var l = new MapLayer();
                l.Parse(layer);
                Width = Math.Max(Width, l.Width);
                Height = Math.Max(Height, l.Height);
                Layers.Add(l);
            }

            Meta = new Dictionary<string, MapObject>();
            Objects = new List<MapObject>();
            Colliders = new List<Collider>();
            XmlNodeList objectgroups = map.GetElementsByTagName("objectgroup");
            foreach (XmlElement group in objectgroups) {
                foreach (XmlElement obj in group.GetElementsByTagName("object")) {
                    if (group.GetAttribute("name") == "Collision") {
                        Colliders.Add(new MapCollider(obj).ToCollider());
                    } else if (group.GetAttribute("name") == "Meta") {
                        Meta.Add(obj.GetAttribute("name"), new MapObject(obj));
                    } else {
                        Objects.Add(new MapObject(obj));
                    }
                }
            }
        }
    }

    public class MapLayer {
        public long[] Raw { get; private set; }
        public int[] Tiles { get; private set; }
        public bool[] FlipX { get; private set; }
        public bool[] FlipY { get; private set; }
        public bool[] FlipD { get; private set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public MapLayer() {}

        public void Parse(XmlElement layerElement) {
            string csv = layerElement.FirstChild.InnerText;
            var raws = csv.Split(',');
            Width = int.Parse(layerElement.GetAttribute("width"));
            Height = int.Parse(layerElement.GetAttribute("height"));
            Raw = new long[raws.Length];
            Tiles = new int[raws.Length];
            FlipX = new bool[raws.Length];
            FlipY = new bool[raws.Length];
            FlipD = new bool[raws.Length];
            for (int i = 0; i < raws.Length; i++) {
                Raw[i] = long.Parse(raws[i]);
                Tiles[i] = (int)(Raw[i] & 0x0fffffff);
                FlipX[i] = (int)(Raw[i] & 0x80000000) != 0;
                FlipY[i] = (int)(Raw[i] & 0x40000000) != 0;
                FlipD[i] = (int)(Raw[i] & 0x20000000) != 0;
            }
        }
    }

    public class MapObject {
        public string Name { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Point Position {
            get { return new Point(x, y); }
            set { x = value.X; y = value.Y; }
        }
        public Rectangle Region {
            get { return new Rectangle(x, y, Width, Height); }
            set { x = value.X; y = value.Y; Width = value.Width; Height = value.Height; }
        }

        public MapObject(XmlElement element) {
            Name = element.GetAttribute("name");
            x = int.Parse(element.GetAttribute("x"));
            y = int.Parse(element.GetAttribute("y"));
            int.TryParse(element.GetAttribute("width"), out int w);
            int.TryParse(element.GetAttribute("height"), out int h);
            Width = w;
            Height = h;
        }
    }

    public struct MapCollider {
        public MapColliderType ColliderType;
        public int x;
        public int y;
        public int Width;
        public int Height;
        public Point[] PolyPoints;

        public Point Position {
            get { return new Point(x, y); }
        }
        public Rectangle Region {
            get { return new Rectangle(x, y, Width, Height); }
        }

        public MapCollider(XmlElement element) {
            x = int.Parse(element.GetAttribute("x"));
            y = int.Parse(element.GetAttribute("y"));
            Width = 0;
            Height = 0;
            if (element.GetElementsByTagName("polygon").Count > 0) {
                ColliderType = MapColliderType.Polygon;
            } else {
                ColliderType = MapColliderType.Rectangle;
            }
            switch (ColliderType) {
                case MapColliderType.Rectangle:
                    int.TryParse(element.GetAttribute("width"), out int w);
                    int.TryParse(element.GetAttribute("height"), out int h);
                    Width = w;
                    Height = h;
                    PolyPoints = new Point[] {
                        new Point(x        , y),
                        new Point(x + Width, y),
                        new Point(x + Width, y + Height),
                        new Point(x        , y + Height),
                    };
                    break;
                case MapColliderType.Polygon:
                    XmlElement poly = (XmlElement)element.GetElementsByTagName("polygon").Item(0);
                    string[] s_points = poly.GetAttribute("points").Split(' ');
                    PolyPoints = new Point[s_points.Length];
                    for (int i = 0; i < s_points.Length; i++) {
                        string[] s_point = s_points[i].Split(',');
                        int.TryParse(s_point[0], out int px);
                        int.TryParse(s_point[1], out int py);
                        PolyPoints[i] = new Point(px, py);
                    }
                    break;
                default:
                    ColliderType = MapColliderType.Other;
                    PolyPoints = new Point[0];
                    break;
            }
        }

        public Collider ToCollider() {
            Collider c;
            switch (ColliderType) {
                case MapColliderType.Rectangle:
                    int hw = Width / 2;
                    int hh = Height / 2;
                    c = new Collider(new Vector2[] {
                        new Vector2(-hw, -hh),
                        new Vector2( hw, -hh),
                        new Vector2( hw,  hh),
                        new Vector2(-hw,  hh)
                    }, Position.ToVector2() + new Vector2(hw, hh), new Vector2(0, 0));

                    //Utils.QueueDebugPoly(c.Points, c.Position, new Color(0, 255, 0), 10000);

                    return c;
                case MapColliderType.Polygon:
                    Vector2[] poly = new Vector2[PolyPoints.Length];
                    float mx = PolyPoints[0].X;
                    float Mx = PolyPoints[0].X;
                    float my = PolyPoints[0].Y;
                    float My = PolyPoints[0].Y;
                    for (int i = 0; i < PolyPoints.Length; i++) {
                        poly[i] = PolyPoints[i].ToVector2();
                        mx = Math.Min(mx, poly[i].X);
                        Mx = Math.Max(Mx, poly[i].X);
                        my = Math.Min(my, poly[i].Y);
                        My = Math.Max(My, poly[i].Y);
                    }
                    float pw = Mx - mx;
                    float ph = My - my;
                    c = new Collider(poly, Position.ToVector2(), new Vector2(mx + pw / 2f, my + ph / 2f));

                    //Utils.QueueDebugPoly(PolyPoints, Position.ToVector2(), new Color(255, 255, 0), 10000);
                    //Utils.QueueDebugPoly(c.Points, c.Position, new Color(0, 255, 0), 10000);

                    return c;
            }
            return new Collider(Position.ToVector2());
        }
    }

    public enum MapColliderType {
        Rectangle,
        Polygon,
        Other
    }
}
