using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Composition;
using System.Formats.Asn1;
using System.IO.Pipelines;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using PostHubAPI.Data;
using PostHubAPI.Models;
using PostHubAPI.Models.DTOs;
using PostHubAPI.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;

namespace PostHubAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly CommentService _commentService;
        private readonly HubService _hubService;
        private readonly UserManager<User> _userManager;
        private readonly PictureService _pictureService;
        private readonly PostService _postService;

        public CommentsController(PictureService pictureService, PostService postService, HubService hubService, UserManager<User> userManager, CommentService commentService)
        {
            _pictureService = pictureService;
            _postService = postService;
            _hubService = hubService;
            _userManager = userManager;
            _commentService = commentService;
        }

        [HttpPost("{hubId}")]
        [Authorize]
        public async Task<ActionResult<PostDisplayDTO>> PostPost(int hubId)
        {
            var files = Request.Form.Files;
            var title = Request.Form["title"];
            var text = Request.Form["text"];
            List<Picture> pictures = new List<Picture>();
            if (files != null)
            {
                foreach (var formFile in files)
                {
                    if (formFile.Length > 0)
                    {
                        Image image = Image.Load(formFile.OpenReadStream());
                        Picture picture = new Picture();
                        picture.FileName = Guid.NewGuid().ToString() + Path.GetExtension(formFile.FileName);
                        picture.MimeType = formFile.ContentType;

                        image.Save(Path.Combine(Directory.GetCurrentDirectory(), "images", "original", picture.FileName));
                        pictures.Add(picture);
                    }
                }
            }



            User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (user == null) return Unauthorized();

            Hub? hub = await _hubService.GetHub(hubId);
            if (hub == null) return NotFound();

            Comment? mainComment = await _commentService.CreateComment(user, text, pictures, null);
            if (mainComment == null) return StatusCode(StatusCodes.Status500InternalServerError);

            //Ajout des images//
            //List<Picture>? picturesReturn = await _pictureService.AddPicturesAsync(pictures, mainComment);
            //if (picturesReturn.Count == 0 && pictures.Count != 0) return StatusCode(StatusCodes.Status500InternalServerError);

            Post? post = await _postService.CreatePost(title, hub, mainComment);
            if (post == null) return StatusCode(StatusCodes.Status500InternalServerError);

            bool voteToggleSuccess = await _commentService.UpvoteComment(mainComment.Id, user);
            if (!voteToggleSuccess) return StatusCode(StatusCodes.Status500InternalServerError);

            return Ok(new PostDisplayDTO(post, true, user));
        }


        [HttpPost("{parentCommentId}")]
        [Authorize]
        public async Task<ActionResult<CommentDisplayDTO>> PostComment(int parentCommentId)
        {
            var files = Request.Form.Files;
            var text = Request.Form["text"];
            List<Picture> pictures = new List<Picture>();
            if (files != null)
            {
                foreach (var formFile in files)
                {
                    Image image = Image.Load(formFile.OpenReadStream());
                    Picture picture = new Picture();
                    picture.FileName = Guid.NewGuid().ToString() + Path.GetExtension(formFile.FileName);
                    picture.MimeType = formFile.ContentType;

                    string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "images", "original", picture.FileName);
                    using (var outputStream = new FileStream(imagePath, FileMode.Create))
                    {
                        image.Save(outputStream, new PngEncoder());
                    }

                    pictures.Add(picture);
                }
            }


            User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (user == null) return Unauthorized();

            Comment? parentComment = await _commentService.GetComment(parentCommentId);
            if (parentComment == null || parentComment.User == null) return BadRequest();

            Comment? newComment = await _commentService.CreateComment(user, text, pictures, parentComment);
            if (newComment == null) return StatusCode(StatusCodes.Status500InternalServerError);

            bool voteToggleSuccess = await _commentService.UpvoteComment(newComment.Id, user);
            if (!voteToggleSuccess) return StatusCode(StatusCodes.Status500InternalServerError);


            //await _pictureService.AddPicturesAsync(pictures, newComment);
            return Ok(new CommentDisplayDTO(newComment, false, user));
        }

        [HttpGet("{tabName}/{sorting}")]
        public async Task<ActionResult<IEnumerable<PostDisplayDTO>>> GetPosts(string tabName, string sorting)
        {
            User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<Post> posts = new List<Post>();
            IEnumerable<Hub>? hubs;

            if (tabName == "myHubs" && user != null && user.Hubs != null)
            {
                hubs = user.Hubs;
            }
            else
            {
                hubs = await _hubService.GetAllHubs();
                if (hubs == null) return StatusCode(StatusCodes.Status500InternalServerError);
            }

            int postPerHub = (int)Math.Ceiling(10.0 / hubs.Count());

            foreach (Hub h in hubs)
            {
                if (sorting == "popular") posts.AddRange(GetPopularPosts(h, postPerHub));
                else posts.AddRange(GetRecentPosts(h, postPerHub));
            }

            if (sorting == "popular")
                posts = posts.OrderByDescending(p => p.MainComment?.Upvoters?.Count - p.MainComment?.Downvoters?.Count).ToList();
            else
                posts = posts.OrderByDescending(p => p.MainComment?.Date).ToList();

            return Ok(posts.Select(p => new PostDisplayDTO(p, false, null)));
        }

        [HttpGet("{searchText}/{sorting}")]
        public async Task<ActionResult<IEnumerable<PostDisplayDTO>>> SearchPosts(string searchText, string sorting)
        {
            User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<Post> posts = new List<Post>();
            IEnumerable<Hub>? hubs = await _hubService.GetAllHubs();
            if (hubs == null) return StatusCode(StatusCodes.Status500InternalServerError);

            foreach (Hub h in hubs)
            {
                h.Posts ??= new List<Post>();
                posts.AddRange(h.Posts.Where(p => p.MainComment!.Text.ToUpper().Contains(searchText.ToUpper()) || p.Title.ToUpper().Contains(searchText.ToUpper())));
            }

            if (sorting == "popular")
                posts = posts.OrderByDescending(p => p.MainComment?.Upvoters?.Count - p.MainComment?.Downvoters?.Count).ToList();
            else
                posts = posts.OrderByDescending(p => p.MainComment?.Date).ToList();

            return Ok(posts.Select(p => new PostDisplayDTO(p, false, null)));
        }

        [HttpGet("{hubId}/{sorting}")]
        public async Task<ActionResult<IEnumerable<PostDisplayDTO>>> GetHubPosts(int hubId, string sorting)
        {
            User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));

            Hub? hub = await _hubService.GetHub(hubId);
            if (hub == null) return NotFound();

            IEnumerable<PostDisplayDTO>? posts = hub.Posts?.Select(p => new PostDisplayDTO(p, false, user));
            if (sorting == "popular") posts = posts?.OrderByDescending(p => p.MainComment.Upvotes - p.MainComment.Downvotes);
            else posts = posts?.OrderByDescending(p => p.MainComment.Date);

            return Ok(posts);
        }

        [HttpGet("{postId}/{sorting}")]
        public async Task<ActionResult<PostDisplayDTO>> GetFullPost(int postId, string sorting)
        {
            User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));

            Post? post = await _postService.GetPost(postId);
            if (post == null) return NotFound();

            PostDisplayDTO postDisplayDTO = new PostDisplayDTO(post, true, user);
            if (sorting == "popular")
                postDisplayDTO.MainComment.SubComments = postDisplayDTO.MainComment!.SubComments!.OrderByDescending(c => c.Upvotes - c.Downvotes).ToList();
            else
                postDisplayDTO.MainComment.SubComments = postDisplayDTO.MainComment!.SubComments!.OrderByDescending(c => c.Date).ToList();

            return Ok(postDisplayDTO);
        }

        [HttpPut("{commentId}")]
        [Authorize]
        public async Task<ActionResult<CommentDisplayDTO>> PutComment(int commentId)
        {
            User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var files = Request.Form.Files;
            var text = Request.Form["text"];
            List<Picture> pictures = new List<Picture>();
            if (files != null)
            {
                foreach (var formFile in files)
                {
                    Image image = Image.Load(formFile.OpenReadStream());
                    Picture picture = new Picture();
                    picture.FileName = Guid.NewGuid().ToString() + Path.GetExtension(formFile.FileName);
                    picture.MimeType = formFile.ContentType;

                    string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "images", "original", picture.FileName);
                    using (var outputStream = new FileStream(imagePath, FileMode.Create))
                    {
                        image.Save(outputStream, new PngEncoder());
                    }

                    pictures.Add(picture);
                }
            }

            Comment? comment = await _commentService.GetComment(commentId);
            if (comment == null) return NotFound();

            if (user == null || comment.User != user) return Unauthorized();

            Comment? editedComment = await _commentService.EditComment(comment, text, pictures);

            if (editedComment == null) return StatusCode(StatusCodes.Status500InternalServerError);

            return Ok(new CommentDisplayDTO(editedComment, true, user));
        }

        [HttpPut("{commentId}")]
        [Authorize]
        public async Task<ActionResult> UpvoteComment(int commentId)
        {
            User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (user == null) return BadRequest();

            bool voteToggleSuccess = await _commentService.UpvoteComment(commentId, user);
            if (!voteToggleSuccess) return StatusCode(StatusCodes.Status500InternalServerError);

            return Ok(new { Message = "Vote complété." });
        }

        [HttpPut("{commentId}")]
        [Authorize]
        public async Task<ActionResult> DownvoteComment(int commentId)
        {
            User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (user == null) return BadRequest();

            bool voteToggleSuccess = await _commentService.DownvoteComment(commentId, user);
            if (!voteToggleSuccess) return StatusCode(StatusCodes.Status500InternalServerError);

            return Ok(new { Message = "Vote complété." });
        }

        [HttpDelete("{commentId}")]
        [Authorize]
        public async Task<ActionResult> DeleteComment(int commentId)
        {
            User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));

            Comment? comment = await _commentService.GetComment(commentId);
            if (comment == null) return NotFound();

            if (user == null || comment.User != user) return Unauthorized();

            do
            {
                comment.SubComments ??= new List<Comment>();

                Comment? parentComment = comment.ParentComment;

                if (comment.MainCommentOf != null && comment.GetSubCommentTotal() == 0)
                {
                    Post? deletedPost = await _postService.DeletePost(comment.MainCommentOf);
                    List<int>? ids = await _pictureService.GetPicturesIds(comment.Id);
                    if (ids != null && ids.Count > 0)
                        foreach (int id in ids)
                        {
                            Picture? picture = await _pictureService.DeletePicture(id);
                            if (picture == null) return StatusCode(StatusCodes.Status500InternalServerError);
                        }
                    if (deletedPost == null) return StatusCode(StatusCodes.Status500InternalServerError);
                }

                if (comment.GetSubCommentTotal() == 0)
                {
                    Comment? deletedComment = await _commentService.HardDeleteComment(comment);
                    List<int>? ids = await _pictureService.GetPicturesIds(comment.Id);
                    if (ids != null && ids.Count > 0)
                        foreach (int id in ids)
                        {
                            Picture? picture = await _pictureService.DeletePicture(id);
                            if (picture == null) return StatusCode(StatusCodes.Status500InternalServerError);
                        }
                    if (deletedComment == null) return StatusCode(StatusCodes.Status500InternalServerError);
                }
                else
                {
                    Comment? deletedComment = await _commentService.SoftDeleteComment(comment);
                    List<int>? ids = await _pictureService.GetPicturesIds(comment.Id);
                    if (ids != null && ids.Count > 0)
                        foreach (int id in ids)
                        {
                            Picture? picture = await _pictureService.DeletePicture(id);
                            if (picture == null) return StatusCode(StatusCodes.Status500InternalServerError);
                        }

                    if (deletedComment == null) return StatusCode(StatusCodes.Status500InternalServerError);
                    break;
                }

                comment = parentComment;

            } while (comment != null && comment.User == null && comment.GetSubCommentTotal() == 0);

            return Ok(new { Message = "Commentaire supprimé." });
        }

        [HttpGet("{CommentId}")]
        public async Task<ActionResult<List<int>>> GetPicturesIds(int CommentId)
        {
            List<int>? ids = await _pictureService.GetPicturesIds(CommentId);
            if (ids == null) { return new List<int>(); }
            return ids;
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetCommentPicture(int id)
        {
            Picture? picture = await _pictureService.GetCommentPicture(id);
            if (picture == null) { return NotFound(); }
            byte[] bytes = System.IO.File.ReadAllBytes(Directory.GetCurrentDirectory() + "/images/original/" + picture.FileName);
            return File(bytes, picture.MimeType);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePicture(int id)
        {
            await _pictureService.DeletePicture(id);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> ReportComment(int id)
        {
            if (id != null)
            {
                await _commentService.ReportComment(id);
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        private static IEnumerable<Post> GetPopularPosts(Hub hub, int qty)
        {
            return hub.Posts!.OrderByDescending(p => p.MainComment?.Upvoters?.Count - p.MainComment?.Downvoters?.Count).Take(qty);
        }

        private static IEnumerable<Post> GetRecentPosts(Hub hub, int qty)
        {
            return hub.Posts!.OrderByDescending(p => p.MainComment?.Date).Take(qty);
        }

        [HttpGet]
        [Authorize(Roles = "Moderator")]
        public async Task<IEnumerable<Comment>?> GetReportedComments()
        {
            return await _commentService.GetReportedComments();
        }
    }
}
