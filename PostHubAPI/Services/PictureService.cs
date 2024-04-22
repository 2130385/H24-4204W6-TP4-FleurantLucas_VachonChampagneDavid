using PostHubAPI.Data;
using PostHubAPI.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Text.RegularExpressions;

namespace PostHubAPI.Services
{
    public class PictureService
    {
        private readonly PostHubAPIContext _context;

        public PictureService(PostHubAPIContext context) 
        {
            _context = context;
        }

        private bool IsContextNull() => _context.Pictures == null;

        public async Task<List<Picture>> AddPicturesAsync(List<Picture> pictures, Comment comment)
        {
            List<Picture> returnPictures = new List<Picture>();
            foreach (Picture picture in pictures)
            {
                picture.Comment = comment;
                await _context.Pictures.AddAsync(picture);
                returnPictures.Add(picture);
            }
            await _context.SaveChangesAsync();
            return returnPictures;
        }


        public async Task<List<Picture>> GetCommentPictures(int id)
        {
            return _context.Pictures.Where(i => i.CommentId == id).ToList();
        }
    }
}
