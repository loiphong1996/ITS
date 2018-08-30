namespace API.ITSProject_2.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract(Name = "Comment")]
    public class CommentViewModels
    {
        [DataMember]
        public string LocationName { get; set; }

        [DataMember]
        public int LocationId { get; set; }

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Avatar { get; set; }

        [DataMember]
        public float Rating { get; set; }

        [DataMember]
        public string CreatorName { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public IEnumerable<string> Photos { get; set; }
    }
}