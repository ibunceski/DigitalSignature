using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Rectangle = iText.Kernel.Geom.Rectangle;

namespace Infrastructure.Services.PdfHelpers
{
    public class CustomTextExtractionStrategy : ITextExtractionStrategy
    {
        public List<Rectangle> TextBboxes { get; } = new();

        public void EventOccurred(IEventData data, EventType type)
        {
            if (type == EventType.RENDER_TEXT)
            {
                var info = (TextRenderInfo)data;
                TextBboxes.Add(info.GetBaseline().GetBoundingRectangle());
            }
        }

        public string GetResultantText() => string.Empty;
        public ICollection<EventType> GetSupportedEvents() => new[] { EventType.RENDER_TEXT };
    }
}