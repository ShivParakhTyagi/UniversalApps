using System;
using System.Collections.Generic;

namespace Universal.Edge.Models
{
    public class DiagramDto
    {
        //public int _id { get; set; }
        public long Id { get; set; }
        public string DrawingNumber { get; set; }
        public long DrawingId { get; set; }
        public string Title { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string PageIndex { get; set; }
        public int Total { get; set; }
        public State DiagramState { get; set; }
        public int? TotalIo { get; set; }
        public int? VerifiedIo { get; set; }
        public DrawingRegistrationType Type { get; set; }
        public bool HasLocalChanges { get; set; }
        public DateTime? DownloadDateTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public DateTime ServerUpdateTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public string AssignedTo { get; set; }
        public string Color { get; set; }
        public bool DoNotIoCheck { get; set; }

        public List<DiagramPanelNo> PanelNos { get; set; }
        public DiagramContent DiagramContents { get; set; }
        public List<string> Logs { get; set; }
    }

    public class DiagramPanelNo
    {
        public long Id { get; set; }
        public long DiagramId { get; set; }
        public string PanelNumber { get; set; }
        public string Bounds { get; set; }
    }

    public enum State
    {
        All = 0,
        OnServer = 1,
        Downloaded = 2,
        HasMarkings = 3,
        Locked = 4,
        QuickMode = 5
    }

    public enum DrawingRegistrationType
    {
        SchematicIo,
        SchematicNonIo,
        NonSchematic
    }

    public class DiagramContent
    {
        public long DiagramId { get; set; }
        public string Content { get; set; }
        public List<ContentData> ContentData { get; set; }
    }

    public class ContentData
    {
        public string Text { get; set; }
        public Bounds Bounds { get; set; }
        public PageSize PageSize { get; set; }
    }

    public class Bounds
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
    }

    public class PageSize
    {
        public float Width { get; set; }
        public float Height { get; set; }
    }
}