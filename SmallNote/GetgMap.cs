using System;

namespace SmallNote
{
    public enum GoogleTileSourceType
    {
        Street,
        Hybrid,
        Satellite,
        Physical,
        PhysicalHybrid,
        StreetOverlay,
        WaterOverlay
    }

    public class GoogleTileSource : Microsoft.Phone.Controls.Maps.TileSource
    {
        public GoogleTileSource()
        {
            UriFormat = @"http://mt{0}.google.com/vt/lyrs={1}&z={2}&x={3}&y={4}";
            TileSourceType = GoogleTileSourceType.Street;
        }
        private int _servernr;
        private char _mapMode;

        private int Server
        {
            get
            {
                return _servernr = (_servernr + 1) % 4;
            }
        }

        private GoogleTileSourceType _tileSourceType;
        public GoogleTileSourceType TileSourceType
        {
            get { return _tileSourceType; }
            set
            {
                _tileSourceType = value;
                _mapMode = TypeToMapMode(value);
            }
        }

        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            if (zoomLevel > 0)
            {
                var url = string.Format(UriFormat, Server, _mapMode, zoomLevel, x, y);
                //System.Diagnostics.Debug.WriteLine(url);

                //


                //

                return new Uri(url);
            }
            return null;
        }

        private static char TypeToMapMode(GoogleTileSourceType tileSourceType)
        {
            switch (tileSourceType)
            {
                case GoogleTileSourceType.Hybrid:
                    return 'y';
                case GoogleTileSourceType.Satellite:
                    return 's';
                case GoogleTileSourceType.Street:
                    return 'm';
                case GoogleTileSourceType.Physical:
                    return 't';
                case GoogleTileSourceType.PhysicalHybrid:
                    return 'p';
                case GoogleTileSourceType.StreetOverlay:
                    return 'h';
                case GoogleTileSourceType.WaterOverlay:
                    return 'r';
            } return ' ';
        }
    }

}
