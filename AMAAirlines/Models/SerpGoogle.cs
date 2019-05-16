using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AMAAirlines.Models
{



    public class Rootobject
    {
        public Request_Info request_info { get; set; }
        public Search_Metadata search_metadata { get; set; }
        public Search_Parameters search_parameters { get; set; }
        public Search_Information search_information { get; set; }
        public Answer_Box answer_box { get; set; }
        public Related_Searches[] related_searches { get; set; }
        public Pagination pagination { get; set; }
        public Organic_Results[] organic_results { get; set; }
    }

    public class Request_Info
    {
        public bool success { get; set; }
        public int searches_used { get; set; }
        public int searches_remaining { get; set; }
    }

    public class Search_Metadata
    {
        public string id { get; set; }
        public DateTime created_at { get; set; }
        public DateTime processed_at { get; set; }
        public string google_url { get; set; }
        public bool cached { get; set; }
        public float total_time_taken { get; set; }
        public string location_auto_message { get; set; }
        public int retrieval_strategy { get; set; }
    }

    public class Search_Parameters
    {
        public string google_domain { get; set; }
        public string q { get; set; }
        public string gl { get; set; }
        public string hl { get; set; }
        public string location { get; set; }
        public string no_cache { get; set; }
        public string output { get; set; }
    }

    public class Search_Information
    {
        public bool original_query_yields_zero_results { get; set; }
        public int total_results { get; set; }
        public float time_taken_displayed { get; set; }
        public string query_displayed { get; set; }
        public string detected_location { get; set; }
    }

    public class Answer_Box
    {
        public Answer[] answers { get; set; }
    }

    public class Answer
    {
        public string answer { get; set; }
        public string category { get; set; }
        public string[] steps { get; set; }
        public Source source { get; set; }
        public string[] images { get; set; }
    }

    public class Source
    {
        public string link { get; set; }
        public string displayed_link { get; set; }
        public string title { get; set; }
    }

    public class Pagination
    {
        public string next { get; set; }
        public Other_Pages other_pages { get; set; }
        public Api_Pagination api_pagination { get; set; }
    }

    public class Other_Pages
    {
        public string _2 { get; set; }
        public string _3 { get; set; }
        public string _4 { get; set; }
        public string _5 { get; set; }
        public string _6 { get; set; }
        public string _7 { get; set; }
        public string _8 { get; set; }
        public string _9 { get; set; }
        public string _10 { get; set; }
    }

    public class Api_Pagination
    {
        public string next { get; set; }
        public Other_Pages1 other_pages { get; set; }
    }

    public class Other_Pages1
    {
        public string _2 { get; set; }
        public string _3 { get; set; }
        public string _4 { get; set; }
        public string _5 { get; set; }
        public string _6 { get; set; }
        public string _7 { get; set; }
        public string _8 { get; set; }
        public string _9 { get; set; }
        public string _10 { get; set; }
    }

    public class Related_Searches
    {
        public string query { get; set; }
        public string link { get; set; }
    }

    public class Organic_Results
    {
        public int position { get; set; }
        public string title { get; set; }
        public string link { get; set; }
        public string domain { get; set; }
        public string displayed_link { get; set; }
        public int block_position { get; set; }
        public string snippet { get; set; }
        public string cached_page_link { get; set; }
        public Sitelinks sitelinks { get; set; }
    }

    public class Sitelinks
    {
        public Inline[] inline { get; set; }
    }

    public class Inline
    {
        public string title { get; set; }
        public string link { get; set; }
    }




    
    

    

    public class Inline_Videos
    {
        public string title { get; set; }
        public string link { get; set; }
        public string length { get; set; }
        public string source { get; set; }
        public string date { get; set; }
        public int block_position { get; set; }
    }

    public class Related_Questions
    {
        public string question { get; set; }
        public string answer { get; set; }
        public Source1 source { get; set; }
        public Search search { get; set; }
    }

    public class Source1
    {
        public string link { get; set; }
        public string displayed_link { get; set; }
        public string title { get; set; }
    }

    public class Search
    {
        public string link { get; set; }
        public string title { get; set; }
    }
    

}
