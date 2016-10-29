using System;
using System.Collections.Generic;

namespace WhatsOn.DataModel
{
    public class TVGuideListingsData
    {
        public class Channel
        {
            public int FilterNumber { get; set; }
            public string FullName { get; set; }
            public object Logo { get; set; }
            public string Name { get; set; }
            public int NetworkId { get; set; }
            public string Number { get; set; }
            public int Sort { get; set; }
            public int SourceId { get; set; }
        }

        public class TVObject
        {
            public DateTime? EpisodeAirDate { get; set; }
            public string EpisodeNumber { get; set; }
            public string EpisodeSEOUrl { get; set; }
            public string SEOUrl { get; set; }
            public int SeasonNumber { get; set; }
            public int TVObjectID { get; set; }
            public string TVObjectName { get; set; }
            public int TVObjectSubType { get; set; }
            public int TVObjectType { get; set; }
        }

        public class ProgramSchedule
        {
            public int AiringAttrib { get; set; }
            public int CatFilterNum { get; set; }
            public int CatId { get; set; }
            public string CopyText { get; set; }
            public int EndTime { get; set; }
            public string EpisodeTitle { get; set; }
            public bool IsSportsEvent { get; set; }
            public int ParentProgramId { get; set; }
            public int ProgramId { get; set; }
            public string Rating { get; set; }
            public object RelatedTvObjects { get; set; }
            public int StartTime { get; set; }
            public int SubCatFilterNum { get; set; }
            public int SubCatId { get; set; }
            public TVObject TVObject { get; set; }
            public int TVObjectId { get; set; }
            public int TVObjectTypeId { get; set; }
            public string Title { get; set; }
        }

        public class RootObject
        {
            public Channel Channel { get; set; }
            public List<ProgramSchedule> ProgramSchedules { get; set; }
        }
    }
}