namespace Protocol
{
    public enum TraceEventType
    {
        Flow,        //useful for seeing normal application flow 
        Detail,      //more detailed information than flow, but less so than debug
        Failure,     //expected handled failure
        Error,       //unexpected error in processing
        Debug,       //very detailed information, usually superfluous
        Frequent     //an event that will be output repeatedly
    }
}
