namespace CourseWebApiProject.Exceptions
{
    public class EventNotFoundException : Exception
    {
        public EventNotFoundException(Guid eventId) : base($"Событие с id={eventId} не найдено.") { }
    }
}
