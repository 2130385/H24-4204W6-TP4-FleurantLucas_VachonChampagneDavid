using Humanizer.Bytes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostHubAPI.Data;
using PostHubAPI.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Drawing;
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


        public async Task<List<int>?> GetPicturesIds(int CommentId)
        {
            Comment? comment = await _context.Comments.FindAsync(CommentId);
            if(comment == null) { return null; }
            List<Picture>? pictures = comment.Pictures;
            if(pictures == null) { return null; }
            List<int> ids = new List<int>();
            foreach (Picture picture in pictures)
            {
                ids.Add(picture.Id);
            }
            return ids;

        }

        public async Task<Picture?> GetCommentPicture(int id)
        {
            if(IsContextNull()) { return null; }
            Picture? picture = await _context.Pictures.FindAsync(id);
            if(picture == null) { return null; }
            return picture;
            //byte[] bytes = System.IO.File.ReadAllBytes(Directory.GetCurrentDirectory() + "/images/original/" + picture.FileName);
            //return File(bytes, picture.MimeType);
        }
    }
    //if (IsContextNull())
    //{
    //    return null;
    //}
    //Picture? birb = await _context.Pictures.FindAsync(id);
    //if (birb == null || birb.FileName == null || birb.MimeType == null)
    //{
    //    return NotFound(new { Message = "Ce birb n'existe pas ou n'a pas de photo. " });
    //}

    //byte[] bytes = System.IO.File.ReadAllBytes(Directory.GetCurrentDirectory() + "/images/" + "/original/" +
    //return File(bytes, birb.MimeType);
}
