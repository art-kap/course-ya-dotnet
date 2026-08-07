namespace CourseWebApiProject.Exceptions
{
    public class EventNotFoundException : Exception
    {
        public EventNotFoundException(int eventId) : base($"Событие с id={eventId} не найдено.") { }
    }
}
