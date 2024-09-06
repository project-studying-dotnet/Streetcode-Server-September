﻿using Streetcode.BLL.Dto.Media.Images;

namespace Streetcode.BLL.Dto.News
{
    public class NewsDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public int? ImageId { get; set; }
        public string URL { get; set; }
        public ImageDto? Image { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
