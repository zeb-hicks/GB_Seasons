using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;

using GB_Seasons.Systems;

namespace GB_Seasons.ContentHandlers {
    public class Map {
        public bool Loaded { get; private set; } = false;
        public Dictionary<string, object> Meta = new Dictionary<string, object>();
        public List<MapObject> Objects = new List<MapObject>();
        public List<MapCollider> Colliders = new List<MapCollider>();
        public List<MapVolume> Volumes = new List<MapVolume>();
        public List<MapLayer> Layers = new List<MapLayer>();

        public Map() {}
        public Map(string filename) => Load(filename);
        public Map(XmlDocument doc) => ParseXML(doc);

        public Vector2 FadePos = new Vector2();
        public float FadeAmount = 1f;

        public Effect TilemapEffect;

        public void Load(string filename) {
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);

            ParseXML(doc);

            Loaded = true;
        }

        public void ParseXML(XmlDocument doc) {
            foreach (XmlElement e in doc.SelectNodes("map/objectgroup/object")) {
                if (!e.HasAttribute("type")) Objects.Add(new MapObject(e));
            }
            foreach (XmlElement e in doc.SelectNodes("map/objectgroup[@name=\"Collision\"]/object")) {
                Colliders.Add(new MapCollider(e));
            }
            foreach (XmlElement e in doc.SelectNodes("map/objectgroup[@name=\"Volumes\"]/object")) {
                Volumes.Add(new MapVolume(e));
            }
            foreach (XmlElement e in doc.SelectNodes("map/objectgroup[@name=\"Meta\"]/object")) {
                string name = e.GetAttribute("name");
                if (e.HasAttribute("width") && e.HasAttribute("height")) {
                    // Rect
                    Meta[name] = new RectangleF(
                        float.Parse(e.GetAttribute("x")),
                        float.Parse(e.GetAttribute("y")),
                        float.Parse(e.GetAttribute("width")),
                        float.Parse(e.GetAttribute("height"))
                    );
                } else {
                    // Point
                    Meta[name] = new Vector2(
                        float.Parse(e.GetAttribute("x")),
                        float.Parse(e.GetAttribute("y"))
                    );
                }
            }

            foreach (XmlElement l in doc.SelectNodes("map/layer")) {
                MapLayer layer = new MapLayer();

                XmlElement data = (XmlElement)l.SelectSingleNode("data");
                foreach (XmlElement chunk in data.SelectNodes("chunk")) {
                    layer.Chunks.Add(new MapChunk(chunk));
                }

                Layers.Add(layer);
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D tileset, int layer, int season, int x, int y, Matrix matrix) {
            if (Layers.Count <= layer) return;
            TilemapEffect.Parameters["FadePos"]?.SetValue(FadePos);
            TilemapEffect.Parameters["FadeAmount"]?.SetValue(layer == 2 ? FadeAmount : 0);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, TilemapEffect, matrix);

            MapLayer L = Layers[layer];
            L.Draw(spriteBatch, tileset, season);

            spriteBatch.End();
        }

    }

    public class MapComponent {
        public XmlElement Element;
        public Vector2 Position;
        public RectangleF Bounds;

        public MapComponent() {}

        public MapComponent(XmlElement e) {
            Element = e;
        }
    }

    public class MapObject : MapComponent {
        public MapObject() : base() {}

        public MapObject(XmlElement e) : base(e) {
            
        }
    }

    public class MapLayer {
        public List<MapChunk> Chunks = new List<MapChunk>();

        public void Draw(SpriteBatch spriteBatch, Texture2D tileset, int season) {
            //RectangleF r = new RectangleF(0, 0, 16 * 8, 16 * 8);
            foreach (MapChunk chunk in Chunks) {
                chunk.Draw(spriteBatch, tileset, season);
            }
        }
    }

    public class MapChunk {
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public int[] Tiles;
        public bool[] FlipX;
        public bool[] FlipY;
        public bool[] FlipD;

        public MapChunk(XmlElement element) {
            int.TryParse(element.GetAttribute("x"), out X);
            int.TryParse(element.GetAttribute("y"), out Y);
            int.TryParse(element.GetAttribute("width"), out Width);
            int.TryParse(element.GetAttribute("height"), out Height);

            string[] data = element.InnerText.Split(',');
            Tiles = new int[data.Length];
            FlipX = new bool[data.Length];
            FlipY = new bool[data.Length];
            FlipD = new bool[data.Length];
            for (int i = 0; i < data.Length; i++) {
                uint td = uint.Parse(data[i]);

                FlipX[i] = (td & 0x80000000) != 0;
                FlipY[i] = (td & 0x40000000) != 0;
                FlipD[i] = (td & 0x20000000) != 0;
                Tiles[i] = (int)(td & 0x0fffffff);
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D tileset, int season) {
            int tilewidth = tileset.Width / 8;

            //Vector2 offset = Vector2.Transform(new Vector2(X * tilewidth, Y * tilewidth), m);
            //int x = (int)offset.X;
            //int y = (int)offset.Y;

            for (int tiley = 0; tiley < Height; tiley++) {
                for (int tilex = 0; tilex < Width; tilex++) {
                    int ti = tilex + tiley * Width;
                    int si = Tiles[ti] - 1;
                    bool flipx = FlipX[ti];
                    bool flipy = FlipY[ti];
                    bool flipd = FlipD[ti];

                    bool fx = flipx;
                    bool fy = flipy;
                    int rt = 0;

                    if (flipd) {
                        fx = flipx && flipy;
                        fy = !flipx && !flipy;
                        rt = (!flipx && flipy) ? 3 : 1;
                    }

                    if (si >= 0) {
                        int sx = si % tilewidth;
                        int sy = si / tilewidth;
                        sx += season * 8;
                        if (flipd) {
                            flipx = !flipx;
                        }
                        SpriteEffects se = (fx ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | (fy ? SpriteEffects.FlipVertically : SpriteEffects.None);
                        spriteBatch.Draw(
                            tileset,
                            new Rectangle(tilex * 8 + X * 8 + 4, tiley * 8 + Y * 8 + 4, 8, 8),
                            new Rectangle(sx * 8, sy * 8, 8, 8),
                            Color.White,
                            (float)(rt * Math.PI / 2f),
                            new Vector2(4, 4),
                            se,
                            1f
                        );
                    }
                }
            }
        }
    }

    public class MapVolume : MapObject {
        public Vector2[] Poly;
        public Collider Collider;
        public MapVolumeType Type;

        public MapVolume() : base() {}

        public MapVolume(XmlElement e) : base(e) {
            switch (e.GetAttribute("type")) {
                case "secret": Type = MapVolumeType.Secret; break;
                case "low_passage": Type = MapVolumeType.LowPassage; break;
                case "weather_volume": Type = MapVolumeType.Weather; break;
                default: Type = MapVolumeType.Other; break;
            }

            float.TryParse(e.Attributes["x"].Value, out float x);
            float.TryParse(e.Attributes["y"].Value, out float y);
            Position = new Vector2(x, y);
            if (e.HasChildNodes && e.SelectNodes("polygon").Count > 0) {
                // Poly
                string[] points = e.SelectSingleNode("polygon").Attributes["points"].Value.Split(' ');
                Poly = new Vector2[points.Length];
                for (int i = 0; i < points.Length; i++) {
                    string[] point = points[i].Split(',');
                    float.TryParse(point[0], out float px);
                    float.TryParse(point[1], out float py);
                    Poly[i] = new Vector2(px, py);
                }
            } else {
                // Rect
                float w = float.Parse(e.GetAttribute("width"));
                float h = float.Parse(e.GetAttribute("height"));
                Poly = new Vector2[] {
                    new Vector2(x, y),
                    new Vector2(x + w, y),
                    new Vector2(x + w, y + h),
                    new Vector2(x, y + h)
                };
            }
            Collider = new Collider(Poly, Position);
        }
    }

    public class MapCollider : MapObject {
        public Vector2[] Poly;
        public Collider Collider;
        public bool Active;
        public MapCollider() : base() {

        }

        public MapCollider(XmlElement e) : base(e) {
            float.TryParse(e.Attributes["x"].Value, out float x);
            float.TryParse(e.Attributes["y"].Value, out float y);
            Position = new Vector2(x, y);
            if (e.HasChildNodes && e.SelectNodes("polygon").Count > 0) {
                // Poly
                string[] points = e.SelectSingleNode("polygon").Attributes["points"].Value.Split(' ');
                Poly = new Vector2[points.Length];
                for (int i = 0; i < points.Length; i++) {
                    string[] point = points[i].Split(',');
                    float.TryParse(point[0], out float px);
                    float.TryParse(point[1], out float py);
                    Poly[i] = new Vector2(px, py);
                }
            } else if (e.HasChildNodes && e.SelectNodes("polyline").Count > 0) {
                // Line
                string[] points = e.SelectSingleNode("polyline").Attributes["points"].Value.Split(' ');
                Poly = new Vector2[points.Length];
                for (int i = 0; i < points.Length; i++) {
                    string[] point = points[i].Split(',');
                    float.TryParse(point[0], out float px);
                    float.TryParse(point[1], out float py);
                    Poly[i] = new Vector2(px, py);
                }
            } else {
                // Rect
                float w = float.Parse(e.GetAttribute("width"));
                float h = float.Parse(e.GetAttribute("height"));
                Poly = new Vector2[] {
                    new Vector2(0, 0),
                    new Vector2(w, 0),
                    new Vector2(w, h),
                    new Vector2(0, h)
                };
            }
            Collider = new Collider(Poly, Position);
        }
    }

    public class MapScript {
        public string Script;
        public List<string> Lines;
        public int Calls = 0;
        public int CallLimit = 1;

        public List<MapScriptAction> Actions = new List<MapScriptAction>();

        public MapScript(string script) {
            Lines = script.Split('\n').ToList();
            for (int i = 0; i < Lines.Count; i++) {
                Lines[i] = Lines[i].Trim();
                if (Lines[i] == "" || Lines[i][0] == '#') {
                    Lines.RemoveAt(i--);
                }
            }

            foreach (string line in Lines) {
                string[] w = line.Split(' ');
                var act = new MapScriptAction();

                act.Method = MethodStrings[w[0]];

                switch (act.Method) {
                    case MapScriptMethod.ControlSwitch:
                        
                        break;
                }

                Actions.Add(act);
            }
        }

        public void RunScript(GameTime gameTime) {

        }

        public static Dictionary<string, MapScriptMethod> MethodStrings = new Dictionary<string, MapScriptMethod> {
            { "control", MapScriptMethod.ControlSwitch },

            { "camera_look", MapScriptMethod.SetCameraTarget },
            { "camera_reset", MapScriptMethod.ResetCameraTarget },

            { "dialogue", MapScriptMethod.Dialogue },

            { "setanim", MapScriptMethod.SetAnimation },
            { "moveto", MapScriptMethod.MoveEntity },
            { "movealong", MapScriptMethod.MoveEntityAlong },

            { "death", MapScriptMethod.Death },
            { "victory", MapScriptMethod.Victory },
            { "gameover", MapScriptMethod.GameOver },

            { "fade_black", MapScriptMethod.FadeToBlack },
            { "fade_white", MapScriptMethod.FadeToWhite },
            { "fade_reset", MapScriptMethod.FadeReset },
        };
    }

    public class MapScriptAction {
        public MapScriptMethod Method;
        public List<string> Args = new List<string>();
    }

    public enum MapScriptMethod {
        ControlSwitch,
        SetCameraTarget,
        ResetCameraTarget,
        Dialogue,
        SetAnimation,
        MoveEntity,
        MoveEntityAlong,
        Victory,
        Death,
        GameOver,
        FadeToBlack,
        FadeToWhite,
        FadeReset
    }

    public enum MapVolumeType {
        Secret,
        Weather,
        LowPassage,
        Other
    }
}
