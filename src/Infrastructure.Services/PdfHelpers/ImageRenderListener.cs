using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Rectangle = iText.Kernel.Geom.Rectangle;

namespace Infrastructure.Services.PdfHelpers
{
    public class ImageRenderListener : IEventListener
    {
        public List<Rectangle> ImageBboxes { get; } = new();

        public void EventOccurred(IEventData data, EventType type)
        {
            if (type == EventType.RENDER_IMAGE)
            {
                var info = (ImageRenderInfo)data;
                var matrix = info.GetImageCtm();
                var image = info.GetImage();
                ImageBboxes.Add(new Rectangle(matrix.Get(6), matrix.Get(7), image.GetWidth(), image.GetHeight()));
            }
        }

        public ICollection<EventType> GetSupportedEvents() => new[] { EventType.RENDER_IMAGE };
    }
}