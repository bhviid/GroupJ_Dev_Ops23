namespace MiniTwit.Server;

public class PrometheusMetrics
{
    public Counter ProcessedHttpRequests { get; }
    public Counter RegisterRequests { get; }

    public Counter AddMessageRequests { get; }
    public Histogram RequestDuration { get; } 

    public PrometheusMetrics(IServiceProvider s)
    {
        using var scope = s.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TwitContext>();

        ProcessedHttpRequests = Metrics.CreateCounter("slimtwit_processed_total", "counts_http_requests processed");
        RegisterRequests = Metrics.CreateCounter(
            "slimtwit_register_counter_total", 
            "counts the number of registered users, plus any users from the database"
            );
        AddMessageRequests = Metrics.CreateCounter("slimtwit_add_message_counter_total", "counts number of sent messages since api reset");
        RequestDuration = Metrics.CreateHistogram("slimtwit_request_duration_seconds", "Histogram of general request processing durations");

        RegisterRequests.IncTo(db.Users.Count());
        AddMessageRequests.IncTo(db.Messages.Count());
    }

}