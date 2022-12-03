namespace Protocol
{
    public enum TraceEventType
    {
        Info,
        Flow,        //useful 
        Detail,
        Failure,     //expected handled failure
        Error,       //unexpected error in processing
        Debug,       //very detailed information, usually superfluous
        Frequent     //an event that will be output repeatedly
    }
}
