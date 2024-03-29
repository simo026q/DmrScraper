﻿namespace DmrScraper.Internal;

internal static class DmrUriBuilder
{
    public const string BaseUri = "https://motorregister.skat.dk/dmr-kerne/koeretoejdetaljer/visKoeretoej";

    public static Uri CreateSearch(DmrExecution execution) 
        => new($"{BaseUri}?execution={execution}&_eventId=search");

    public static Uri CreatePage(DmrExecution execution, int pageIndex) 
        => new($"{BaseUri}?execution={execution}&_eventId=customPage&_pageIndex={pageIndex}");
}
