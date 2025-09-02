
using System.Collections.Generic;

namespace RM.Database.ResearchMantraContext
{
    public class Playlist
    {
        //public List<Introduction> Introductions { get; set; }
        public List<Chapter> Playlists { get; set; }
    }
    public class Chapter : CommonFieldForEachTable
    {
        public int Id { get; set; }
        public int? ProductId { get; set; }
        public string Description { get; set; }

        public string ChapterTitle { get; set; }
        public List<SubChapter> SubChapters { get; set; }
    }

    public class SubChapter : CommonFieldForEachTable
    {
        public int Id { get; set; }
        public int? ChapterId { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }
        public string Description { get; set; }
        public string Language { get; set; }
        public bool IsVisible { get; set; }
        public int? VideoDuration { get; set; }
    }


    public class ChapterResponseModel
    {
        public int Id { get; set; }
        public string ChapterTitle { get; set; }
        public string Description { get; set; }
        public List<SubChapterResponseModel> SubChapters { get; set; } = new List<SubChapterResponseModel>();
    }

    public class SubChapterResponseModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }
        public string Description { get; set; }
        public string Language { get; set; }
        public int VideoDuration { get; set; }
        public bool IsVisible { get; set; }
    }

    public class SubChapterRequestModel
    {
        public int? Id { get; set; }
        public int? ProductId { get; set; }
        public string? Title { get; set; }
        public string? Link { get; set; }
        public string? Description { get; set; }
        public string? Language { get; set; }
        public int? VideoDuration { get; set; }
        public bool IsVisible { get; set; }
        public bool IsActive { get; set; }
        public int? ChapterId { get; set; }
        public int? CreatedBy { get; set; }
        public string Action { get; set; }
    }

    public class GetPlayListSpModel
    {
        public int ChapterId { get; set; }
        public string ChapterTitle { get; set; }
        public string ChapterDescription { get; set; }
        public int? SubChapterId { get; set; }
        public string SubChapterTitle { get; set; }
        public string SubChapterLink { get; set; }
        public string SubChapterDescription { get; set; }
        public string SubChapterLanguage { get; set; }
        public int? VideoDuration { get; set; }
        public bool IsVisible { get; set; }
    }
}
