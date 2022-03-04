using System.Drawing;

namespace ANPRCV.Models
{
    public class PlateDetectionResult
    {
        public Point Point1 { get; set; }
        public Point Point2 { get; set; }
        public Point Point3 { get; set; }
        public Point Point4 { get; set; }
        public string Plate { get; set; }
        public bool FoundPlate { get; set; } = false;
    }
}
