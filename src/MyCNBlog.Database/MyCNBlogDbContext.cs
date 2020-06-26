using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyCNBlog.Core.Models;
using System.IO;

namespace MyCNBlog.Database
{
    public class MyCNBlogDbContext : IdentityDbContext<BlogUser, IdentityRole<int>, int>
    {
        public DbSet<Blog> Blogs { get; set; }

        public DbSet<Post> Posts { get; set; }

        public DbSet<PostComment> Comments { get; set; }

        public DbSet<Tag> Tags { get; set; }

        public DbSet<PostTag> PostTags { get; set; }

        public MyCNBlogDbContext(DbContextOptions options) : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            const string prefix = "Blog";

            builder.Entity<IdentityRole<int>>(b => b.ToTable($"{prefix}Role"));
            builder.Entity<IdentityRoleClaim<int>>(b => b.ToTable($"{prefix}RoleClaim"));
            builder.Entity<IdentityUserRole<int>>(b => b.ToTable($"{prefix}UserRole"));
            builder.Entity<IdentityUserClaim<int>>(b => b.ToTable($"{prefix}UserClaim"));
            builder.Entity<IdentityUserLogin<int>>(b => b.ToTable($"{prefix}UserLogin"));
            builder.Entity<IdentityUserToken<int>>(b => b.ToTable($"{prefix}UserToken"));
            
            builder.Entity<BlogUser>(b =>
            {
                b.ToTable(nameof(BlogUser));
                b.HasKey(x => x.Id);
                b.HasIndex(x => x.NormalizedUserName).HasName("idx_username");
                b.Property(x => x.UserName).HasMaxLength(32);
                b.Property(x => x.AvatarPath).HasMaxLength(1024);
                // BlogUser - Blog
                b.HasOne(x => x.Blog)
                .WithOne(x => x.User)
                .HasForeignKey<Blog>(x => x.UserId);
            });

            builder.Entity<Blog>(b =>
            {
                b.HasKey(x => x.Id);
                b.ToTable(nameof(Blog));
                // Blog - Posts
                b.HasMany(x => x.Posts)
                .WithOne(x => x.Blog)
                .HasForeignKey(x => x.BlogId);
            });

            builder.Entity<Post>(b =>
            {
                b.HasKey(x => x.Id);
                b.HasIndex(x => x.Title).HasName("idx_post_title");
                b.ToTable(nameof(Post));
                b.Property(x => x.Path).HasMaxLength(1024);
                b.Property(x => x.Description).HasMaxLength(512);
                b.Ignore(x => x.Tags);
                // Post - Comments
                b.HasMany(x => x.Comments)
                .WithOne(x => x.RepliedPost)
                .HasForeignKey(x => x.RepliedPostId);
                // Post - BlogUser
                b.HasOne(x => x.Author).WithMany().HasForeignKey(x => x.AuthorId);
            });

            builder.Entity<PostComment>(b =>
            {
                b.HasKey(x => x.Id);
                b.ToTable(nameof(PostComment));
                // Comment - BlogUser
                b.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.RepliedUserId);
            });

            builder.Entity<PostTag>(b =>
            {
                b.HasKey(x => x.Id);
                b.ToTable(nameof(PostTag));
                // posts - tags
                b.HasOne(x => x.Post)
                .WithMany(x => x.PostTags)
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Cascade);
                b.HasOne(x => x.Tag)
                .WithMany(x => x.PostTags)
                .HasForeignKey(x => x.TagId);
            });

            builder.Entity<Tag>(b =>
            {
                b.HasKey(x => x.Id);
                b.HasIndex(x => x.Name).HasName("idx_tag_name");
                b.ToTable(nameof(Tag));

                b.Property(x => x.Name).HasMaxLength(128);
            });
        }
    }
}
