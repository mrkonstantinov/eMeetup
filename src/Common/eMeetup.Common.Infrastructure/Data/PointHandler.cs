using System.Data;
using Dapper;
//using NetTopologySuite.IO;
//using NetTopologySuite.Geometries;

namespace eMeetup.Common.Infrastructure.Data;

//public class PointHandler : SqlMapper.TypeHandler<Point>
//{
//    public override Point Parse(System.Object value)
//    {
//        if (value == null || value is DBNull)
//        {
//            return null; // Handle nullable case appropriately
//        }

//        // Assuming the database stores Points in WKT (Well-Known Text) format
//        string wkt = value.ToString();

//        // Use WKTReader to create a Point from WKT string
//        var wktReader = new WKTReader();
//        return wktReader.Read(wkt) as Point; // AsText or equivalent parsing
//    }

//    public override void SetValue(IDbDataParameter parameter, Point value)
//    {
//        if (value == null)
//        {
//            parameter.Value = DBNull.Value; // Handle nullable case
//            return;
//        }

//        // Converts Point to WKT string
//        parameter.Value = value.AsText(); // Serializes Point to WKT
//    }
//}
