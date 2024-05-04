using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PostHubAPI.Models;

namespace PostHubAPI.Data
{
    public class PostHubAPIContext : IdentityDbContext<User>
    {
        public PostHubAPIContext (DbContextOptions<PostHubAPIContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            var passwordHasher = new PasswordHasher<User>();
            // Nécessaire pour la relation One-To-One entre Post et Comment : EF veut savoir quelle table contiendra la clé étrangère
            modelBuilder.Entity<Post>().HasOne(p => p.MainComment).WithOne(c => c.MainCommentOf).HasForeignKey<Post>(p => p.MainCommentId);

            var roleId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid().ToString();

            modelBuilder.Entity<IdentityRole>().HasData(new IdentityRole { Id=roleId, Name = "Moderator", NormalizedName = "Moderator".ToUpper() });
            modelBuilder.Entity<User>().HasData(new User { Id=userId, Email="moderator@gmail.com", NormalizedEmail="MODERATOR@GMAIL.COM", UserName="moderator", NormalizedUserName="MODERATOR", PasswordHash=passwordHasher.HashPassword(null, "password") });
            modelBuilder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
            {
                RoleId = roleId,
                UserId = userId
            });;
        }

        public DbSet<Hub> Hubs { get; set; } = default!;
        public DbSet<Post> Posts { get; set; } = default!;
        public DbSet<Comment> Comments { get; set; } = default!;
        public DbSet<Picture> Pictures { get; set; } = default!;
    }
}
